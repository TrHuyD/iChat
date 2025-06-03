// connectionManager.js
console.warn("Connection Manager loaded");

class ConnectionManager {
    constructor() {
        this.roomSubscriptions = new Map(); // Map<roomId, Set<port>>
        this.connectedPorts = new Set();
        this.pendingRoomJoins = new Set(); // Track rooms waiting to be joined
        this.heartbeatInterval = null;
    }

    initializePort(port) {
        this.connectedPorts.add(port);

        // Handle port disconnection
        port.addEventListener('close', () => {
            this.connectedPorts.delete(port);
            this.cleanupPortSubscriptions(port);
        });

        port.start();
        return port;
    }

    cleanupPortSubscriptions(port) {
        for (const [roomId, ports] of this.roomSubscriptions.entries()) {
            if (ports.has(port)) {
                ports.delete(port);
                if (ports.size === 0) {
                    this.roomSubscriptions.delete(roomId);
                    return roomId; // Return the roomId that needs to be left
                }
            }
        }
        return null;
    }

    handleHeartbeat(port) {
        try {
            port.postMessage({ action: 'HEARTBEAT_RESPONSE' });
        } catch (e) {
            this.connectedPorts.delete(port);
            this.cleanupPortSubscriptions(port);
        }
    }

    setupHeartbeat() {
        this.heartbeatInterval = setInterval(() => {
            const deadPorts = new Set();

            this.connectedPorts.forEach(port => {
                try {
                    port.postMessage({ action: 'HEARTBEAT_CHECK' });
                } catch (e) {
                    deadPorts.add(port);
                }
            });

            // Clean up dead ports
            deadPorts.forEach(port => {
                this.connectedPorts.delete(port);
                this.cleanupPortSubscriptions(port);
            });
        }, 30000);
    }

    notifyAllPorts(message) {
        this.connectedPorts.forEach(port => {
            try {
                port.postMessage(message);
            } catch (e) {
                this.connectedPorts.delete(port);
                this.cleanupPortSubscriptions(port);
            }
        });
    }

    notifyPortsInRoom(roomId, message) {
        const roomSubs = this.roomSubscriptions.get(roomId);
        if (!roomSubs) return;

        const deadPorts = new Set();

        roomSubs.forEach(port => {
            try {
                // Double-check port is still connected
                if (port && this.connectedPorts.has(port)) {
                    port.postMessage(message);
                } else {
                    deadPorts.add(port);
                }
            } catch (e) {
                console.error("Failed to notify port:", e);
                deadPorts.add(port);
            }
        });

        // Clean up dead ports
        deadPorts.forEach(port => {
            this.connectedPorts.delete(port);
            roomSubs.delete(port);
        });

        // Remove empty room subscriptions
        if (roomSubs.size === 0) {
            this.roomSubscriptions.delete(roomId);
        }
    }

    addRoomSubscription(port, roomId) {
        if (!this.roomSubscriptions.has(roomId)) {
            this.roomSubscriptions.set(roomId, new Set());
        }
        this.roomSubscriptions.get(roomId).add(port);
        this.pendingRoomJoins.add(roomId);
        return roomId;
    }

    removeRoomSubscription(port, roomId) {
        if (this.roomSubscriptions.has(roomId)) {
            const ports = this.roomSubscriptions.get(roomId);
            ports.delete(port);

            if (ports.size === 0) {
                this.roomSubscriptions.delete(roomId);
                return roomId; // Return roomId that needs to be left
            }
        }
        return null;
    }

    getPendingRoomJoins() {
        return this.pendingRoomJoins;
    }

    clearPendingRoomJoin(roomId) {
        this.pendingRoomJoins.delete(roomId);
    }

    notifyPortAboutConnectionState(port, connectionState) {
        switch (connectionState) {
            case signalR.HubConnectionState.Connected:
                port.postMessage({ action: 'SIGNALR_CONNECTED' });
                break;
            case signalR.HubConnectionState.Connecting:
            case signalR.HubConnectionState.Reconnecting:
                port.postMessage({ action: 'SIGNALR_RECONNECTING' });
                break;
            default:
                port.postMessage({ action: 'SIGNALR_DISCONNECTED' });
        }
    }

}

// Create a global instance
const connectionManager = new ConnectionManager();

