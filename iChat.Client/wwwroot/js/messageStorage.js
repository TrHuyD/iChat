
let db;

export function initializeDb(dbName, storeName) {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(dbName, 1);

        request.onerror = (event) => {
            console.error("IndexedDB error:", event.target.error);
            reject(event.target.error);
        };

        request.onsuccess = (event) => {
            db = event.target.result;
            resolve();
        };

        request.onupgradeneeded = (event) => {
            const db = event.target.result;
            if (!db.objectStoreNames.contains(storeName)) {
                const store = db.createObjectStore(storeName, { keyPath: 'id' });
                store.createIndex('roomId', 'roomId', { unique: false });
                store.createIndex('createdAt', 'createdAt', { unique: false });
            }
        };
    });
}

export function addMessage(storeName, message) {
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(storeName, 'readwrite');
        const store = transaction.objectStore(storeName);
        const request = store.put(message);

        request.onsuccess = () => resolve();
        request.onerror = (event) => {
            console.error("Error adding message:", event.target.error);
            reject(event.target.error);
        };
    });
}

export function getMessagesByRoom(storeName, roomId, limit) {
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(storeName, 'readonly');
        const store = transaction.objectStore(storeName);
        const index = store.index('roomId');
        const request = index.getAll(IDBKeyRange.only(roomId));

        request.onsuccess = (event) => {
            let messages = event.target.result;
            // Convert to ChatMessageDto format
            messages = messages.map(m => ({
                Id: m.messageId,
                Content: m.content,
                ContentMedia: m.contentMedia,
                MessageType: m.messageType,
                CreatedAt: new Date(m.createdAt),
                SenderId: m.senderId
            }));
            resolve(messages.slice(-limit)); // Get most recent messages
        };

        request.onerror = (event) => {
            console.error("Error getting messages:", event.target.error);
            reject(event.target.error);
        };
    });
}

export function clearMessagesByRoom(storeName, roomId) {
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(storeName, 'readwrite');
        const store = transaction.objectStore(storeName);
        const index = store.index('roomId');
        const request = index.openCursor(IDBKeyRange.only(roomId));

        request.onsuccess = (event) => {
            const cursor = event.target.result;
            if (cursor) {
                cursor.delete();
                cursor.continue();
            } else {
                resolve();
            }
        };

        request.onerror = (event) => {
            console.error("Error clearing messages:", event.target.error);
            reject(event.target.error);
        };
    });
}

export function getMessageCountByRoom(storeName, roomId) {
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(storeName, 'readonly');
        const store = transaction.objectStore(storeName);
        const index = store.index('roomId');
        const request = index.count(IDBKeyRange.only(roomId));

        request.onsuccess = (event) => resolve(event.target.result);
        request.onerror = (event) => {
            console.error("Error counting messages:", event.target.error);
            reject(event.target.error);
        };
    });
}