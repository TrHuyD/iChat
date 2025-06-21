// Enhanced sharedWorker.js with gap tracking
console.warn("Worker starting");
importScripts("https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js");
importScripts("/js/messageStorage.js");
importScripts("/js/sharedworkerSrc/connectionManager.js");

let hubConnection;
const messageStorage = new MessageStorageService();
let isStorageInitialized = false;
const DEFAULT_MESSAGE_LIMIT = 50; // Default number of messages to fetch

self.onconnect = function (e) {
    const port = connectionManager.initializePort(e.ports[0]);

    // Initialize storage when first connection is made
    if (!isStorageInitialized) {
        messageStorage.initialize()
            .then(async () => {
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
                    await initSignalR();
                    break;
                case 'SEND_MESSAGE':
                    await handleSendMessage(data.roomId, data.message);
                    break;
                case 'GET_MESSAGE_HISTORY':
                    await handleGetMessageHistory(port, data.roomId, data.beforeMessageId, data.limit);
                    break;
                case 'ENSURE_LATEST_GAP':
                    await handleEnsureLatestGap(data.roomId);
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

async function handleEnsureLatestGap(roomId) {
    try {
        await messageStorage.ensureLatestGapMarker(roomId);
        connectionManager.notifyAllPorts({
            action: 'LATEST_GAP_ENSURED',
            data: { roomId }
        });
    } catch (error) {
        console.error("Failed to ensure latest gap:", error);
        connectionManager.notifyAllPorts({
            action: 'LATEST_GAP_ERROR',
            data: { roomId, error: error.toString() }
        });
    }
}

async function handleGetMessageHistory(port, roomId, beforeMessageId = null, requestedLimit = DEFAULT_MESSAGE_LIMIT) {
    let localMessages = [];
    let needsServerFetch = true;
    let fetchBeforeId = beforeMessageId;

    try {
        // Get local messages first
        localMessages = await messageStorage.getMessages(roomId, requestedLimit * 2); // Get more to account for gaps

        if (localMessages.length > 0) {
            if (!beforeMessageId) {
                // If no beforeMessageId specified, try to get latest messages
                const latestMessages = localMessages.slice(-requestedLimit);

                // Check if we have enough messages and no gaps in this range
                if (latestMessages.length >= requestedLimit) {
                    const oldestInRange = latestMessages[0];
                    const newestInRange = latestMessages[latestMessages.length - 1];

                    const gaps = await messageStorage.findGapsInRange(
                        roomId,
                        oldestInRange.messageId,
                        newestInRange.messageId
                    );

                    if (gaps.length === 0) {
                        // Return local messages if no gaps
                        port.postMessage({
                            action: 'MESSAGE_HISTORY',
                            data: latestMessages,
                            fromServer: false,
                            hasGaps: false
                        });
                        return;
                    }
                }

                // Set fetchBeforeId to oldest local message if not specified
                fetchBeforeId = localMessages[0].messageId;
            } else {
                // Find messages before the specified ID
                const beforeIndex = localMessages.findIndex(m => m.messageId === beforeMessageId);
                if (beforeIndex > 0) {
                    const requestedMessages = localMessages.slice(0, beforeIndex).slice(-requestedLimit);

                    if (requestedMessages.length >= requestedLimit) {
                        const oldestInRange = requestedMessages[0];
                        const newestInRange = requestedMessages[requestedMessages.length - 1];

                        const gaps = await messageStorage.findGapsInRange(
                            roomId,
                            oldestInRange.messageId,
                            newestInRange.messageId
                        );

                        if (gaps.length === 0) {
                            port.postMessage({
                                action: 'MESSAGE_HISTORY',
                                data: requestedMessages,
                                fromServer: false,
                                hasGaps: false
                            });
                            return;
                        }
                    }
                }
            }
        }
    } catch (dbError) {
        console.warn("Failed to get local messages:", dbError);
    }

    // Fetch from server via HTTP API to fill gaps or get initial messages
    try {
        let allFetchedMessages = [];
        let currentBeforeId = fetchBeforeId;
        let attempts = 0;
        const maxAttempts = 5;

        // Keep fetching until we have enough messages or no more available
        while (allFetchedMessages.length < requestedLimit && attempts < maxAttempts) {
            const url = new URL(`https://localhost:6051/api/Chat/${roomId}/history`);
            if (currentBeforeId) {
                url.searchParams.append('beforeMessageId', currentBeforeId);
            }

            const response = await fetch(url.toString(), {
                method: 'GET',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const serverMessages = await response.json();

            if (!serverMessages || serverMessages.length === 0) {
                break; // No more messages available
            }

            const formattedMessages = serverMessages.map(msg => ({
                id: msg.id,
                roomId: msg.roomId,
                content: msg.content,
                contentMedia: msg.contentMedia || null,
                messageType: msg.messageType,
                createdAt: msg.createdAt,
                senderId: msg.senderId,
            }));

            // Sort by ID to ensure proper order
            formattedMessages.sort((a, b) => parseInt(a.id) - parseInt(b.id));

            // Add to our collection
            allFetchedMessages = [...formattedMessages, ...allFetchedMessages];

            // Update currentBeforeId for next iteration
            if (formattedMessages.length > 0) {
                currentBeforeId = formattedMessages[0].id;
            }

            attempts++;
        }

        if (allFetchedMessages.length > 0) {
            try {
                // Store messages and update gap markers
                await messageStorage.storeMessages(roomId, allFetchedMessages);

                // Get the requested number of messages from the stored data
                const finalMessages = await messageStorage.getMessages(roomId, requestedLimit);

                port.postMessage({
                    action: 'MESSAGE_HISTORY',
                    data: finalMessages.slice(-requestedLimit), // Get the most recent ones
                    fromServer: true,
                    hasGaps: false,
                    totalFetched: allFetchedMessages.length
                });
            } catch (storeError) {
                console.error("Failed to store messages:", storeError);
                // Still return the fetched messages even if storage fails
                port.postMessage({
                    action: 'MESSAGE_HISTORY',
                    data: allFetchedMessages.slice(-requestedLimit),
                    fromServer: true,
                    storageError: storeError.toString()
                });
            }
        } else {
            // No server messages, return local messages if available
            if (localMessages.length > 0) {
                port.postMessage({
                    action: 'MESSAGE_HISTORY',
                    data: localMessages.slice(-requestedLimit),
                    fromServer: false,
                    hasGaps: true
                });
            } else {
                port.postMessage({
                    action: 'MESSAGE_HISTORY',
                    data: [],
                    fromServer: true
                });
            }
        }
    } catch (serverError) {
        console.error("Error getting server history:", serverError);

        // Fallback to local messages
        if (localMessages.length > 0) {
            const fallbackMessages = beforeMessageId ?
                localMessages.filter(m => parseInt(m.messageId) < parseInt(beforeMessageId)).slice(-requestedLimit) :
                localMessages.slice(-requestedLimit);

            port.postMessage({
                action: 'MESSAGE_HISTORY',
                data: fallbackMessages,
                fromServer: false,
                hasGaps: true,
                serverError: serverError.toString()
            });
        } else {
            port.postMessage({
                action: 'MESSAGE_HISTORY_ERROR',
                data: serverError.toString()
            });
        }
    }
}

async function initSignalR() {
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

        // Ensure latest gap markers are in place after reconnection
        // This could be expanded to handle multiple rooms if needed
    });

    hubConnection.start()
        .then(async () => {
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