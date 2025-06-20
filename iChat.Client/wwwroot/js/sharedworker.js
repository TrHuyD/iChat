// sharedWorker.js
console.warn("Worker starting");
importScripts("https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js");
importScripts("/js/messageStorage.js");
importScripts("/js/sharedworkerSrc/connectionManager.js");

let hubConnection;
const messageStorage = new MessageStorageService();
let isStorageInitialized = false;

self.onconnect = function (e) {
    const port = connectionManager.initializePort(e.ports[0]);

    // Initialize storage when first connection is made
    if (!isStorageInitialized) {
        messageStorage.initialize()
            .then(() => {
                isStorageInitialized = true;
                port.postMessage({ action: 'WORKER_READY' });
            })
            .catch(err => {
                console.error("Failed to initialize storage:", err);
                port.postMessage({ action: 'WORKER_ERROR', error: err.toString() });
            });
    } else {
        port.postMessage({ action: 'WORKER_READY' });
    }

    port.onmessage = async function (event) {
        const { action, data } = event.data;

        try {
            // Ensure storage is ready before handling any messages
            if (!isStorageInitialized) {
                await messageStorage.initialize();
                isStorageInitialized = true;
            }

            switch (action) {
                case 'INIT_SIGNALR':
                    initSignalR();
                    break;
                case 'SEND_MESSAGE':
                    await handleSendMessage(data.roomId, data.message);
                    break;
                case 'GET_MESSAGE_HISTORY':
                    await handleGetMessageHistory(port, data.roomId, data.beforeMessageId);
                    break;
            }
        } catch (err) {
            console.error("Error handling message:", action, err);
            port.postMessage({
                action: 'OPERATION_ERROR',
                originalAction: action,
                error: err.toString()
            });
        }
    };
};

async function handleSendMessage(roomId, message) {
    if (!hubConnection || hubConnection.state !== signalR.HubConnectionState.Connected) {
        throw new Error("Cannot send message - connection not established");
    }

    await hubConnection.invoke("SendMessage", roomId, message);
}

async function handleGetMessageHistory(port, roomId, beforeMessageId = null) {
    let localMessages = [];

    try {
        localMessages = await messageStorage.getMessages(roomId, 1);
        if (localMessages.length > 0 && !beforeMessageId) {
            beforeMessageId = Number(localMessages[0].messageId);
        }
    } catch (dbError) {
        console.warn("Failed to get local messages:", dbError);
    }

    try {
        await waitForSignalRConnectionReady();
        const serverMessages = await hubConnection.invoke(
            "GetMessageHistory",
            Number(roomId),
            beforeMessageId ? Number(beforeMessageId) : null
        );

        const formattedMessages = serverMessages.map(msg => ({
            id: Number(msg.id),
            roomId: Number(msg.roomId),
            content: msg.content,
            contentMedia: msg.contentMedia || null,
            messageType: Number(msg.messageType),
            createdAt: msg.createdAt,
            senderId: Number(msg.senderId),
        }));

        if (formattedMessages.length > 0) {
            try {
                await messageStorage.storeMessages(roomId, formattedMessages);
            } catch (storeError) {
                console.error("Failed to store messages:", storeError);
            }
        }

        port.postMessage({
            action: 'MESSAGE_HISTORY',
            data: formattedMessages,
            fromServer: true
        });
    } catch (serverError) {
        console.error("Error getting server history:", serverError);

        if (localMessages.length > 0) {
            port.postMessage({
                action: 'MESSAGE_HISTORY',
                data: localMessages,
                fromServer: false
            });
        } else {
            port.postMessage({
                action: 'MESSAGE_HISTORY_ERROR',
                data: serverError.toString()
            });
        }
    }
}

function initSignalR() {
    if (hubConnection) return;

    hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:6051/api/chathub", {
            transport: signalR.HttpTransportType.WebSockets,
            skipNegotiation: true,
            withCredentials: true
        })
        .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: retryContext =>
                retryContext.elapsedMilliseconds < 60000 ?
                    Math.random() * 2000 + 2000 :
                    Math.random() * 5000 + 10000
        })
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connectionManager.setupHeartbeat();

    hubConnection.on("ReceiveMessage", async (message) => {
        try {
            await messageStorage.storeMessage(message.roomId, message);
            connectionManager.notifyAllPorts({
                action: 'MESSAGE_RECEIVED',
                data: message
            });
        } catch (e) {
            console.error("Failed to store message:", e);
            // Still notify ports even if storage fails
            connectionManager.notifyAllPorts({
                action: 'MESSAGE_RECEIVED',
                data: message,
                storageError: e.toString()
            });
        }
    });

    hubConnection.onclose(() => {
        connectionManager.notifyAllPorts({ action: 'SIGNALR_DISCONNECTED' });
    });

    hubConnection.onreconnecting(() => {
        connectionManager.notifyAllPorts({ action: 'SIGNALR_RECONNECTING' });
    });

    hubConnection.onreconnected(async (connectionId) => {
        console.log("Reconnected with ID:", connectionId);
        connectionManager.notifyAllPorts({ action: 'SIGNALR_RECONNECTED' });
    });

    hubConnection.start()
        .then(() => {
            console.log("SignalR connection established");
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