class MessageStorageService {
    constructor(dbName = "ChatDB") {
        this.dbName = dbName;
        this.db = null;
        this.ranOnce = false;
    }

    async initialize() {
        if (this.ranOnce) return;
        this.ranOnce = true;

        this.db = await new Promise((resolve, reject) => {
            const request = indexedDB.open(this.dbName, 1);

            request.onupgradeneeded = (event) => {
                const db = event.target.result;
                if (!db.objectStoreNames.contains("Messages")) {
                    const store = db.createObjectStore("Messages", { keyPath: "id" });
                    store.createIndex("roomId", "roomId", { unique: false });
                }
            };

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    async storeMessage(roomId, message) {
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
        await tx.done;
    }

    async storeMessages(roomId, messages) {
        const tx = this.db.transaction("Messages", "readwrite");
        const store = tx.objectStore("Messages");

        for (const message of messages) {
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
        await tx.done;
    }

    async getMessages(roomId, limit = 100) {
        const tx = this.db.transaction("Messages", "readonly");
        const store = tx.objectStore("Messages");
        const index = store.index("roomId");

        const messages = [];
        const request = index.openCursor(IDBKeyRange.only(roomId));

        return new Promise((resolve, reject) => {
            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor) {
                    messages.push(cursor.value);
                    cursor.continue();
                } else {
                    messages.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
                    resolve(messages.slice(-limit));
                }
            };
            request.onerror = () => reject(request.error);
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
