// sharedWorker.js
console.warn("Worker starting");
importScripts("https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js");
importScripts("/js/messageStorage.js");
importScripts("/js/sharedworkerSrc/connectionManager.js");

let hubConnection;
const messageStorage = new MessageStorageService();
(async () => {
    await messageStorage.initialize();
})();
let actionQueue = Promise.resolve();
self.onconnect = function (e) {
    const port = connectionManager.initializePort(e.ports[0]);

    port.onmessage = function (event) {
        const { action, data } = event.data;

        actionQueue = actionQueue.then(() => {
            console.log(`[Queue] Executing ${action}`);
            switch (action) {
                case 'INIT_SIGNALR':
                    return initSignalR(port); 
                case 'JOIN_ROOM':
                    return handleJoinRoom(port, data.roomId);
                case 'LEAVE_ROOM':
                    return handleLeaveRoom(port, data.roomId);
                case 'SEND_MESSAGE':
                    return handleSendMessage(data.roomId, data.message);
                case 'HEARTBEAT':
                    return connectionManager.handleHeartbeat(port); 
                case 'GET_MESSAGE_HISTORY':
                    return handleGetMessageHistory(port, data.roomId, data.beforeMessageId);
            }
        }).catch(err => {
            console.error("Action failed:", err);
        });
    };
};

async function handleJoinRoom(port, roomId) {
    const roomIdToJoin = connectionManager.addRoomSubscription(port, roomId);

    if (hubConnection?.state === signalR.HubConnectionState.Connected) {
        try {
            await hubConnection.invoke("JoinRoom", roomIdToJoin);
            connectionManager.clearPendingRoomJoin(roomIdToJoin);
            console.log(`Successfully joined room: ${roomIdToJoin}`);
        } catch (err) {
            console.error("JoinRoom failed:", err);
        }
    } else {
        console.warn(`Deferred joining room ${roomIdToJoin} - connection not ready`);
        if (!hubConnection || hubConnection.state === signalR.HubConnectionState.Disconnected) {
            initSignalR();
        }
    }
}

function handleLeaveRoom(port, roomId) {
    const roomIdToLeave = connectionManager.removeRoomSubscription(port, roomId);
    if (roomIdToLeave && hubConnection?.state === signalR.HubConnectionState.Connected) {
        hubConnection.invoke("LeaveRoom", roomIdToLeave)
            .catch(err => console.error("LeaveRoom failed:", err));
    }
}

async function handleSendMessage(roomId, message) {
    if (!hubConnection || hubConnection.state !== signalR.HubConnectionState.Connected) {
        console.error("Cannot send message - connection not established");
        return;
    }

    const roomSubs = connectionManager.roomSubscriptions.get(roomId);
    if (!roomSubs || roomSubs.size === 0) {
        console.error(`Cannot send to room ${roomId} - no active subscriptions`);
        return;
    }

    try {
        await hubConnection.invoke("SendMessage", roomId, message);
    } catch (err) {
        console.error("SendMessage failed:", err);
        connectionManager.notifyPortsInRoom(roomId, {
            action: 'MESSAGE_ERROR',
            data: `Failed to send message: ${err.message}`
        });
    }
}

async function handleGetMessageHistory(port, roomId, beforeMessageId) {
    try {
        await waitForSignalRConnectionReady();
        // Always get from server first
        const serverMessages = await hubConnection.invoke(
            "GetMessageHistory",
            Number(roomId),
            beforeMessageId ? Number(beforeMessageId) : null
        );

        // Format messages to ensure proper types
        const formattedMessages = serverMessages.map(msg => ({
            id: Number(msg.id),
            roomId: Number(msg.roomId),
            content: msg.content,
            contentMedia: msg.contentMedia || null,
            messageType: Number(msg.messageType),
            createdAt: msg.createdAt,
            senderId: Number(msg.senderId),
            // Add other required fields
        }));

        // Store in IndexedDB
        if (formattedMessages.length > 0) {
            await messageStorage.storeMessages(roomId, formattedMessages);
        }

        port.postMessage({
            action: 'MESSAGE_HISTORY',
            data: formattedMessages
        });
    } catch (err) {
        console.error("Error getting message history:", err);

        // Fallback to IndexedDB if server fails
        try {
            const localMessages = await messageStorage.getMessages(roomId);
            port.postMessage({
                action: 'MESSAGE_HISTORY',
                data: localMessages
            });
        } catch (dbError) {
            console.error("Error getting local messages:", dbError);
            port.postMessage({
                action: 'MESSAGE_HISTORY_ERROR',
                data: err.toString()
            });
        }
    }
}

function initSignalR(initiatingPort = null) {
    
    if (hubConnection) {
        if (initiatingPort) {
            connectionManager.notifyPortAboutConnectionState(initiatingPort, hubConnection.state);
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
                    return Math.random() * 2000 + 2000;
                }
                return Math.random() * 5000 + 10000;
            }
        })
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connectionManager.setupHeartbeat();

    hubConnection.on("ReceiveMessage", async (message) => {
        const roomId = String(message.roomId);

        try {

            await messageStorage.storeMessage(roomId, message);
        } catch (e) {
            console.error("Failed to store message:", e);
        }

        connectionManager.notifyPortsInRoom(roomId, {
            action: 'MESSAGE_RECEIVED',
            data: message,
        });
    });

    hubConnection.onclose(() => {
        connectionManager.notifyAllPorts({ action: 'SIGNALR_DISCONNECTED' });
    });

    hubConnection.onreconnecting(() => {
        connectionManager.notifyAllPorts({ action: 'SIGNALR_RECONNECTING' });
    });

    hubConnection.onreconnected(async (connectionId) => {
        console.log("Reconnected with ID:", connectionId);
        const rejoinPromises = [];

        connectionManager.roomSubscriptions.forEach((_, roomId) => {
            rejoinPromises.push(
                hubConnection.invoke("JoinRoom", roomId)
                    .catch(err => console.error("Rejoin failed:", err))
            );
        });

        await Promise.all(rejoinPromises);
        connectionManager.notifyAllPorts({ action: 'SIGNALR_RECONNECTED' });
    });

    hubConnection.start()
        .then(() => {
            console.log("SignalR connection established");
            connectionManager.getPendingRoomJoins().forEach(roomId => handleJoinRoom(null, roomId));
            connectionManager.notifyAllPorts({ action: 'SIGNALR_CONNECTED' });
        })
        .catch(err => {
            console.error("Connection failed:", err);
            connectionManager.notifyAllPorts({
                action: 'SIGNALR_ERROR',
                data: err.toString()
            });
            setTimeout(() => initSignalR(), 5000);
        });
}
function waitForSignalRConnectionReady(timeout = 10000) {
    return new Promise((resolve, reject) => {
        const start = Date.now();

        function check() {
            if (hubConnection && hubConnection.state === signalR.HubConnectionState.Connected) {
                resolve();
            } else if (Date.now() - start > timeout) {
                reject(new Error("SignalR connection timeout"));
            } else {
                setTimeout(check, 20);
            }
        }

        check();
    });
}

self.addEventListener('error', (e) => {
    console.error("SharedWorker error:", e);
    if (connectionManager.heartbeatInterval) {
        clearInterval(connectionManager.heartbeatInterval);
    }
});