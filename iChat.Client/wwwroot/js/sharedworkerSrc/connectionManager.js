// connectionManager.js
console.warn("Connection Manager loaded");

class ConnectionManager {
    constructor() {
        this.connectedPorts = new Set();
        this.heartbeatInterval = null;
    }

    initializePort(port) {
        this.connectedPorts.add(port);

        port.addEventListener('close', () => {
            this.connectedPorts.delete(port);
        });

        port.start();
        return port;
    }

    handleHeartbeat(port) {
        try {
            port.postMessage({ action: 'HEARTBEAT_RESPONSE' });
        } catch (e) {
            this.connectedPorts.delete(port);
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

            deadPorts.forEach(port => this.connectedPorts.delete(port));
        }, 30000);
    }

    notifyAllPorts(message) {
        this.connectedPorts.forEach(port => {
            try {
                port.postMessage(message);
            } catch (e) {
                this.connectedPorts.delete(port);
            }
        });
    }

    notifyPortAboutConnectionState(port, connectionState) {
        try {
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
        } catch (e) {
            this.connectedPorts.delete(port);
        }
    }
}

const connectionManager = new ConnectionManager();  