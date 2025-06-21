//messageStorage.js
class MessageStorageService {
    constructor(dbName = "ChatDB") {
        this.dbName = dbName;
        this.db = null;
        this.initialized = false;
        this.initializationPromise = null;
        this.GAP_MESSAGE_TYPE = "GAP_MARKER";
    }

    async initialize() {
        if (this.initialized) return;
        if (this.initializationPromise) return this.initializationPromise;

        this.initializationPromise = new Promise((resolve, reject) => {
            const request = indexedDB.open(this.dbName, 1);

            request.onupgradeneeded = (event) => {
                const db = event.target.result;
                if (!db.objectStoreNames.contains("Messages")) {
                    const store = db.createObjectStore("Messages", { keyPath: "id" });
                    store.createIndex("roomId", "roomId", { unique: false });
                    store.createIndex("roomId_createdAt", ["roomId", "createdAt"], { unique: false });
                    store.createIndex("messageType", "messageType", { unique: false });
                }
            };

            request.onsuccess = (event) => {
                this.db = event.target.result;
                this.initialized = true;

                this.db.onclose = () => {
                    this.db = null;
                    this.initialized = false;
                };

                this.db.onerror = (event) => {
                    console.error('Database error:', event.target.error);
                };

                resolve(this.db);
            };

            request.onerror = (event) => {
                console.error('Database opening failed:', event.target.error);
                reject(event.target.error);
            };
        });

        return this.initializationPromise;
    }

    async ensureDatabaseReady() {
        if (!this.initialized) {
            await this.initialize();
        }
        if (!this.db) {
            throw new Error('Database connection is not available');
        }
    }

    // Create a gap marker record
    createGapMarker(roomId, beforeMessageId, afterMessageId) {
        return {
            id: `${roomId}_gap_${beforeMessageId}_${afterMessageId}`,
            roomId,
            messageId: `gap_${beforeMessageId}_${afterMessageId}`,
            content: null,
            contentMedia: null,
            messageType: this.GAP_MESSAGE_TYPE,
            createdAt: new Date().toISOString(),
            senderId: null,
            gapInfo: {
                beforeMessageId,
                afterMessageId,
                isGap: true
            }
        };
    }

    async storeMessage(roomId, message) {
        await this.ensureDatabaseReady();
        const tx = this.db.transaction("Messages", "readwrite");
        const store = tx.objectStore("Messages");
        const record = {
            id: `${roomId}_${message.id}`,
            roomId,
            messageId: message.id,
            content: message.content,
            contentMedia: message.contentMedia,
            messageType: message.messageType,
            createdAt: new Date(message.createdAt).toISOString(),
            senderId: message.senderId,
        };
        store.put(record);
        return new Promise((resolve, reject) => {
            tx.oncomplete = resolve;
            tx.onerror = (event) => reject(event.target.error);
        });
    }

    async storeMessages(roomId, messages) {
        await this.ensureDatabaseReady();

        if (messages.length === 0) return;

        // Sort messages by ID (assuming snowflake IDs are sortable)
        const sortedMessages = [...messages].sort((a, b) =>
            parseInt(a.id) - parseInt(b.id)
        );

        const tx = this.db.transaction("Messages", "readwrite");
        const store = tx.objectStore("Messages");

        // Get existing messages to check for gaps
        const existingMessages = await this.getMessagesSync(roomId, store);

        // Store the new messages
        for (const message of sortedMessages) {
            const record = {
                id: `${roomId}_${message.id}`,
                roomId,
                messageId: message.id,
                content: message.content,
                contentMedia: message.contentMedia,
                messageType: message.messageType,
                createdAt: new Date(message.createdAt).toISOString(),
                senderId: message.senderId,
            };
            store.put(record);
        }

        // Update gap markers after storing messages
        await this.updateGapMarkers(roomId, sortedMessages, existingMessages, store);

        return new Promise((resolve, reject) => {
            tx.oncomplete = resolve;
            tx.onerror = (event) => reject(event.target.error);
        });
    }

    // Synchronous version for use within transactions
    async getMessagesSync(roomId, store) {
        const index = store.index("roomId");
        return new Promise((resolve, reject) => {
            const messages = [];
            const request = index.openCursor(IDBKeyRange.only(roomId));

            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor) {
                    messages.push(cursor.value);
                    cursor.continue();
                } else {
                    // Sort by message ID
                    messages.sort((a, b) => {
                        if (a.messageType === this.GAP_MESSAGE_TYPE && b.messageType !== this.GAP_MESSAGE_TYPE) return 1;
                        if (b.messageType === this.GAP_MESSAGE_TYPE && a.messageType !== this.GAP_MESSAGE_TYPE) return -1;
                        return parseInt(a.messageId) - parseInt(b.messageId);
                    });
                    resolve(messages);
                }
            };

            request.onerror = (event) => reject(event.target.error);
        });
    }

    async updateGapMarkers(roomId, newMessages, existingMessages, store) {
        if (newMessages.length === 0) return;

        // Remove gap markers that are no longer needed
        const gapsToRemove = [];

        for (const existing of existingMessages) {
            if (existing.messageType === this.GAP_MESSAGE_TYPE) {
                const gapInfo = existing.gapInfo;
                const beforeId = parseInt(gapInfo.beforeMessageId);
                const afterId = parseInt(gapInfo.afterMessageId);

                // Check if any new message fills this gap
                const fillsGap = newMessages.some(msg => {
                    const msgId = parseInt(msg.id);
                    return msgId > beforeId && msgId < afterId;
                });

                if (fillsGap) {
                    gapsToRemove.push(existing.id);
                }
            }
        }

        // Remove obsolete gap markers
        for (const gapId of gapsToRemove) {
            store.delete(gapId);
        }

        // Get all messages (existing + new, minus removed gaps)
        const allMessages = [
            ...existingMessages.filter(m =>
                m.messageType !== this.GAP_MESSAGE_TYPE ||
                !gapsToRemove.includes(m.id)
            ),
            ...newMessages
        ].sort((a, b) => parseInt(a.messageId || a.id) - parseInt(b.messageId || b.id));

        // Create new gap markers where needed
        for (let i = 0; i < allMessages.length - 1; i++) {
            const current = allMessages[i];
            const next = allMessages[i + 1];

            if (current.messageType === this.GAP_MESSAGE_TYPE ||
                next.messageType === this.GAP_MESSAGE_TYPE) continue;

            const currentId = parseInt(current.messageId || current.id);
            const nextId = parseInt(next.messageId || next.id);

            // Check if there's a gap (non-consecutive IDs)
            if (nextId - currentId > 1) {
                const gapMarker = this.createGapMarker(roomId, current.messageId || current.id, next.messageId || next.id);
                store.put(gapMarker);
            }
        }
    }

    async ensureLatestGapMarker(roomId) {
        await this.ensureDatabaseReady();

        const messages = await this.getMessages(roomId);
        if (messages.length === 0) return;

        // Find the latest actual message (not gap marker)
        const actualMessages = messages.filter(m => m.messageType !== this.GAP_MESSAGE_TYPE);
        if (actualMessages.length === 0) return;

        const latestMessage = actualMessages[actualMessages.length - 1];
        const latestMessageId = latestMessage.messageId;

        // Check if there's already a gap marker after the latest message
        const hasLatestGap = messages.some(m =>
            m.messageType === this.GAP_MESSAGE_TYPE &&
            m.gapInfo &&
            m.gapInfo.beforeMessageId === latestMessageId
        );

        if (!hasLatestGap) {
            // Create a gap marker after the latest message
            const tx = this.db.transaction("Messages", "readwrite");
            const store = tx.objectStore("Messages");

            const gapMarker = this.createGapMarker(roomId, latestMessageId, "LATEST");
            store.put(gapMarker);

            return new Promise((resolve, reject) => {
                tx.oncomplete = resolve;
                tx.onerror = (event) => reject(event.target.error);
            });
        }
    }

    async getMessages(roomId, limit = 100) {
        await this.ensureDatabaseReady();
        const tx = this.db.transaction("Messages", "readonly");
        const store = tx.objectStore("Messages");
        const index = store.index("roomId");

        return new Promise((resolve, reject) => {
            const messages = [];
            const request = index.openCursor(IDBKeyRange.only(roomId), 'prev');

            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor && (!limit || messages.length < limit)) {
                    messages.push(cursor.value);
                    cursor.continue();
                } else {
                    // Sort by message ID and filter out gap markers for return
                    const sortedMessages = messages
                        .reverse()
                        .sort((a, b) => {
                            const aId = a.messageType === this.GAP_MESSAGE_TYPE ?
                                parseInt(a.gapInfo.beforeMessageId) : parseInt(a.messageId);
                            const bId = b.messageType === this.GAP_MESSAGE_TYPE ?
                                parseInt(b.gapInfo.beforeMessageId) : parseInt(b.messageId);
                            return aId - bId;
                        })
                        .filter(m => m.messageType !== this.GAP_MESSAGE_TYPE);

                    resolve(sortedMessages);
                }
            };

            request.onerror = (event) => reject(event.target.error);
        });
    }

    async getMessagesWithGaps(roomId, limit = 100) {
        await this.ensureDatabaseReady();
        const tx = this.db.transaction("Messages", "readonly");
        const store = tx.objectStore("Messages");
        const index = store.index("roomId");

        return new Promise((resolve, reject) => {
            const messages = [];
            const request = index.openCursor(IDBKeyRange.only(roomId), 'prev');

            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor && (!limit || messages.length < limit)) {
                    messages.push(cursor.value);
                    cursor.continue();
                } else {
                    // Sort by message ID, keeping gap markers
                    const sortedMessages = messages
                        .reverse()
                        .sort((a, b) => {
                            const aId = a.messageType === this.GAP_MESSAGE_TYPE ?
                                parseInt(a.gapInfo.beforeMessageId) : parseInt(a.messageId);
                            const bId = b.messageType === this.GAP_MESSAGE_TYPE ?
                                parseInt(b.gapInfo.beforeMessageId) : parseInt(b.messageId);
                            return aId - bId;
                        });

                    resolve(sortedMessages);
                }
            };

            request.onerror = (event) => reject(event.target.error);
        });
    }

    async findGapsInRange(roomId, startMessageId, endMessageId) {
        const messagesWithGaps = await this.getMessagesWithGaps(roomId);
        const startId = parseInt(startMessageId);
        const endId = parseInt(endMessageId);

        return messagesWithGaps.filter(m => {
            if (m.messageType !== this.GAP_MESSAGE_TYPE) return false;

            const beforeId = parseInt(m.gapInfo.beforeMessageId);
            const afterId = m.gapInfo.afterMessageId === "LATEST" ?
                Infinity : parseInt(m.gapInfo.afterMessageId);

            // Gap overlaps with the requested range
            return beforeId <= endId && (afterId >= startId || afterId === Infinity);
        });
    }

    async clearMessages(roomId) {
        await this.ensureDatabaseReady();
        const tx = this.db.transaction("Messages", "readwrite");
        const store = tx.objectStore("Messages");
        const index = store.index("roomId");
        const request = index.openCursor(IDBKeyRange.only(roomId));

        return new Promise((resolve, reject) => {
            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor) {
                    cursor.delete();
                    cursor.continue();
                } else {
                    resolve();
                }
            };
            request.onerror = () => reject(request.error);
        });
    }

    async clearAllMessages() {
        await this.ensureDatabaseReady();
        const tx = this.db.transaction("Messages", "readwrite");
        const store = tx.objectStore("Messages");
        store.clear();
        return new Promise((resolve, reject) => {
            tx.oncomplete = resolve;
            tx.onerror = (event) => reject(event.target.error);
        });
    }

    async getMessageCount(roomId) {
        await this.ensureDatabaseReady();
        const tx = this.db.transaction("Messages", "readonly");
        const store = tx.objectStore("Messages");
        const index = store.index("roomId");
        return new Promise((resolve, reject) => {
            const request = index.count(IDBKeyRange.only(roomId));
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }
}