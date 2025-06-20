class MessageStorageService {
    constructor(dbName = "ChatDB") {
        this.dbName = dbName;
        this.db = null;
        this.initialized = false;
        this.initializationPromise = null;
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
                }
            };

            request.onsuccess = (event) => {
                this.db = event.target.result;
                this.initialized = true;

                // Handle database closure
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
        const tx = this.db.transaction("Messages", "readwrite");
        const store = tx.objectStore("Messages");

        const promises = messages.map(message => {
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
            return new Promise((resolve, reject) => {
                const request = store.put(record);
                request.onsuccess = resolve;
                request.onerror = (event) => reject(event.target.error);
            });
        });

        await Promise.all(promises);
        return new Promise((resolve, reject) => {
            tx.oncomplete = resolve;
            tx.onerror = (event) => reject(event.target.error);
        });
    }

    async getMessages(roomId, limit = 100) {
        await this.ensureDatabaseReady();
        const tx = this.db.transaction("Messages", "readonly");
        const store = tx.objectStore("Messages");
        const index = store.index("roomId");

        return new Promise((resolve, reject) => {
            const messages = [];
            const request = index.openCursor(IDBKeyRange.only(roomId));

            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor) {
                    messages.push(cursor.value);
                    if (limit && messages.length >= limit) {
                        resolve(messages);
                    } else {
                        cursor.continue();
                    }
                } else {
                    messages.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
                    resolve(messages);
                }
            };

            request.onerror = (event) => {
                reject(event.target.error);
            };
        });
    }

    async clearMessages(roomId) {
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
        const tx = this.db.transaction("Messages", "readwrite");
        const store = tx.objectStore("Messages");
        await store.clear();
        await tx.done;
    }

    async getMessageCount(roomId) {
        const tx = this.db.transaction("Messages", "readonly");
        const store = tx.objectStore("Messages");
        const index = store.index("roomId");
        return await new Promise((resolve, reject) => {
            const request = index.count(IDBKeyRange.only(roomId));
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }
}
