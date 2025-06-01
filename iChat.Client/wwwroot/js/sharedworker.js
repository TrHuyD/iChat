// sharedWorker.js
console.warn("Worker starting");
importScripts("https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js");

let hubConnection;
const roomSubscriptions = new Map(); // Map<roomId, Set<port>>
const connectedPorts = new Set();
const pendingRoomJoins = new Set(); // Track rooms waiting to be joined
let heartbeatInterval;

self.onconnect = function (e) {
    const port = e.ports[0];
    connectedPorts.add(port);

    // Handle port disconnection
    port.addEventListener('close', () => {
        connectedPorts.delete(port);
        cleanupPortSubscriptions(port);
    });

    port.onmessage = function (event) {
        const { action, data } = event.data;

        switch (action) {
            case 'INIT_SIGNALR':
                initSignalR(port);
                break;
            case 'JOIN_ROOM':
                handleJoinRoom(port, data.roomId);
                break;
            case 'LEAVE_ROOM':
                handleLeaveRoom(port, data.roomId);
                break;
            case 'SEND_MESSAGE':
                handleSendMessage(data.roomId, data.message);
                break;
            case 'HEARTBEAT':
                handleHeartbeat(port);
                break;
        }
    };

    port.start();
};

function cleanupPortSubscriptions(port) {
    for (const [roomId, ports] of roomSubscriptions.entries()) {
        if (ports.has(port)) {
            ports.delete(port);
            if (ports.size === 0) {
                // Only leave if we're actually connected
                if (hubConnection?.state === signalR.HubConnectionState.Connected) {
                    hubConnection.invoke("LeaveRoom", roomId)
                        .catch(err => console.error("LeaveRoom failed:", err));
                }
                roomSubscriptions.delete(roomId);
            }
        }
    }
    // Ensure port is removed from connectedPorts
    connectedPorts.delete(port);
}

async function handleJoinRoom(port, roomId) {
    // Initialize room subscription tracking
    if (!roomSubscriptions.has(roomId)) {
        roomSubscriptions.set(roomId, new Set());
    }
    roomSubscriptions.get(roomId).add(port);

    // Only attempt to join if connection is established
    if (hubConnection?.state === signalR.HubConnectionState.Connected) {
        try {
            await hubConnection.invoke("JoinRoom", roomId);
            pendingRoomJoins.delete(roomId);
            console.log(`Successfully joined room: ${roomId}`);
        } catch (err) {
            console.error("JoinRoom failed:", err);
            pendingRoomJoins.add(roomId); // Add to retry queue
        }
    } else {
        console.warn(`Deferred joining room ${roomId} - connection not ready`);
        pendingRoomJoins.add(roomId);
        // Ensure connection is starting if not already
        if (!hubConnection || hubConnection.state === signalR.HubConnectionState.Disconnected) {
            initSignalR();
        }
    }
}

function handleLeaveRoom(port, roomId) {
    if (roomSubscriptions.has(roomId)) {
        const ports = roomSubscriptions.get(roomId);
        ports.delete(port);

        if (ports.size === 0) {
            if (hubConnection?.state === signalR.HubConnectionState.Connected) {
                hubConnection.invoke("LeaveRoom", roomId)
                    .catch(err => console.error("LeaveRoom failed:", err));
            }
            roomSubscriptions.delete(roomId);
        }
    }
}

async function handleSendMessage(roomId, message) {
    // 1. Check connection state first
    if (!hubConnection || hubConnection.state !== signalR.HubConnectionState.Connected) {
        console.error("Cannot send message - connection not established");
        return;
    }

    // 2. Safely check room subscriptions
    const roomSubs = roomSubscriptions.get(roomId);
    if (!roomSubs || roomSubs.size === 0) {
        console.error(`Cannot send to room ${roomId} - no active subscriptions`);
        return;
    }

    // 3. Verify at least one port is still connected
    let hasActiveSubscribers = false;
    try {
        roomSubs.forEach(port => {
            if (connectedPorts.has(port)) {
                hasActiveSubscribers = true;
            }
        });
    } catch (e) {
        console.error("Error checking subscribers:", e);
        return;
    }

    if (!hasActiveSubscribers) {
        console.error(`No active subscribers in room ${roomId}`);
        return;
    }

    // 4. Send the message
    try {
        await hubConnection.invoke("SendMessage", roomId, message);
    } catch (err) {
        console.error("SendMessage failed:", err);
        // Optionally notify ports about the failure
        notifyPortsInRoom(roomId, {
            action: 'MESSAGE_ERROR',
            data: `Failed to send message: ${err.message}`
        });
    }
}

// Helper function to notify all ports in a room
function notifyPortsInRoom(roomId, message) {
    const roomSubs = roomSubscriptions.get(roomId);
    if (!roomSubs) return;

    const deadPorts = new Set();

    roomSubs.forEach(port => {
        try {
            // Double-check port is still connected
            if (port && connectedPorts.has(port)) {
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
        connectedPorts.delete(port);
        roomSubs.delete(port);
    });

    // Remove empty room subscriptions
    if (roomSubs.size === 0) {
        roomSubscriptions.delete(roomId);
    }
}
function handleHeartbeat(port) {
    try {
        port.postMessage({ action: 'HEARTBEAT_RESPONSE' });
    } catch (e) {
        connectedPorts.delete(port);
        cleanupPortSubscriptions(port);
    }
}

function initSignalR(initiatingPort = null) {
    if (hubConnection) {
        // If we already have a connection, just notify the initiating port
        if (initiatingPort) {
            notifyPortAboutConnectionState(initiatingPort);
        }
        return;
    }

    hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:6051/api/chathub", {
            transport: signalR.HttpTransportType.WebSockets,
            skipNegotiation: true,
            withCredentials: true
        })
        .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: retryContext => {
                if (retryContext.elapsedMilliseconds < 60000) {
                    return Math.random() * 2000 + 2000; // 2-4 seconds
                }
                return Math.random() * 5000 + 10000; // 10-15 seconds
            }
        })
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    // In your heartbeat interval
    heartbeatInterval = setInterval(() => {
        const deadPorts = new Set();

        connectedPorts.forEach(port => {
            try {
                port.postMessage({ action: 'HEARTBEAT_CHECK' });
            } catch (e) {
                deadPorts.add(port);
            }
        });

        // Clean up dead ports
        deadPorts.forEach(port => {
            connectedPorts.delete(port);
            cleanupPortSubscriptions(port);
        });
    }, 30000);

    // Message handler
    hubConnection.on("ReceiveMessage", (message) => {
        const roomId = '' + message.roomId;
        if (roomSubscriptions.has(roomId)) {
            const deadPorts = new Set();
            const roomSubs = roomSubscriptions.get(roomId);

            roomSubs.forEach(port => {
                try {
                    if (port && connectedPorts.has(port)) {
                        port.postMessage({
                            action: 'MESSAGE_RECEIVED',
                            data: message
                        });
                    } else {
                        deadPorts.add(port);
                    }
                } catch (e) {
                    console.error("Message delivery failed:", e);
                    deadPorts.add(port);
                }
            });

            // Clean up dead ports
            deadPorts.forEach(port => {
                connectedPorts.delete(port);
                roomSubs.delete(port);
            });
        }
    });

    // Connection state handlers
    hubConnection.onclose(() => {
        notifyAllPorts({ action: 'SIGNALR_DISCONNECTED' });
    });

    hubConnection.onreconnecting(() => {
        notifyAllPorts({ action: 'SIGNALR_RECONNECTING' });
    });

    hubConnection.onreconnected(async (connectionId) => {
        console.log("Reconnected with ID:", connectionId);
        // Rejoin all active rooms
        const rejoinPromises = [];
        roomSubscriptions.forEach((_, roomId) => {
            rejoinPromises.push(
                hubConnection.invoke("JoinRoom", roomId)
                    .catch(err => console.error("Rejoin failed:", err))
            );
        });

        await Promise.all(rejoinPromises);
        notifyAllPorts({ action: 'SIGNALR_RECONNECTED' });
    });

    // Start the connection
    hubConnection.start()
        .then(() => {
            console.log("SignalR connection established");
            // Process any pending room joins
            pendingRoomJoins.forEach(roomId => handleJoinRoom(null, roomId));
            pendingRoomJoins.clear();
            notifyAllPorts({ action: 'SIGNALR_CONNECTED' });
        })
        .catch(err => {
            console.error("Connection failed:", err);
            notifyAllPorts({
                action: 'SIGNALR_ERROR',
                data: err.toString()
            });
            // Auto-retry after delay
            setTimeout(() => initSignalR(), 5000);
        });
}

function notifyAllPorts(message) {
    connectedPorts.forEach(port => {
        try {
            port.postMessage(message);
        } catch (e) {
            connectedPorts.delete(port);
            cleanupPortSubscriptions(port);
        }
    });
}

function notifyPortAboutConnectionState(port) {
    if (!hubConnection) {
        port.postMessage({ action: 'SIGNALR_DISCONNECTED' });
        return;
    }

    switch (hubConnection.state) {
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

self.addEventListener('error', (e) => {
    console.error("SharedWorker error:", e);
    clearInterval(heartbeatInterval);
});