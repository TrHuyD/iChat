class StorageHelper {
    static setItem(key, value, expireMinutes) {
        const now = new Date().getTime();
        const item = {
            value: value,
            expiry: now + expireMinutes * 60 * 1000,
        };
        store.set(key, item);
    }

    static getItem(key) {
        const item = store.get(key);
        if (!item) return null;

        const now = new Date().getTime();
        if (now > item.expiry) {
            store.remove(key);
            return null;
        }
        return item.value;
    }

    static removeItem(key) {
        store.remove(key);
    }
}
