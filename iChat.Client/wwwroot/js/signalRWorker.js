window.signalRInterop = {
    initialize: function (csSignalRServiceRef) {
        const worker = new SharedWorker('/js/sharedWorker.js');
        worker.port.start();

        worker.port.onmessage = function (event) {
            const { action, data } = event.data;
            console.log('[SignalRInterop] Received from worker:', action, data);

            switch (action) {
                case 'MESSAGE_RECEIVED':
                    csSignalRServiceRef.invokeMethodAsync('HandleMessageReceived', JSON.stringify(data));
                    break;
                case 'SIGNALR_CONNECTED':
                    csSignalRServiceRef.invokeMethodAsync('HandleConnected');
                    break;
                case 'SIGNALR_DISCONNECTED':
                    csSignalRServiceRef.invokeMethodAsync('HandleDisconnected');
                    break;
                case 'SIGNALR_RECONNECTING':
                    csSignalRServiceRef.invokeMethodAsync('HandleReconnecting');
                    break;
                case 'SIGNALR_RECONNECTED':
                    csSignalRServiceRef.invokeMethodAsync('HandleReconnected');
                    break;
                case 'HEARTBEAT_CHECK':
                    console.log('[SignalRInterop] Sending HEARTBEAT back to worker');
                    worker.port.postMessage({ action: 'HEARTBEAT' });
                    break;
                case 'MESSAGE_HISTORY':
                    csSignalRServiceRef.invokeMethodAsync('HandleMessageHistory', data);
                    break;
                case 'MESSAGE_HISTORY_ERROR':
                    csSignalRServiceRef.invokeMethodAsync('HandleMessageHistoryError', data);
                    break;
            }
        };

        const sendToWorker = (action, data) => {
            console.log('[SignalRInterop] Sending to worker:', action, data);
            worker.port.postMessage({ action, data });
        };

        sendToWorker('INIT_SIGNALR');

        return {
            joinRoom: function (roomId) {
                sendToWorker('JOIN_ROOM', { roomId });
            },
            leaveRoom: function (roomId) {
                sendToWorker('LEAVE_ROOM', { roomId });
            },
            sendMessage: function (roomId, message) {
                sendToWorker('SEND_MESSAGE', { roomId, message });
            },
            getMessageHistory: function (roomId, beforeMessageId) {
                return new Promise((resolve) => {
                    const tempHandler = (e) => {
                        const { action, data } = e.data;
                        console.log('[SignalRInterop] TempHandler received:', action, data);

                        if (action === 'MESSAGE_HISTORY' || action === 'MESSAGE_HISTORY_ERROR') {
                            worker.port.removeEventListener('message', tempHandler);
                            if (action === 'MESSAGE_HISTORY_ERROR') {
                                console.error('[SignalRInterop] Error getting message history:', data);
                                resolve([]);
                            } else {
                                resolve(data);
                            }
                        }
                    };
                    worker.port.addEventListener('message', tempHandler);

                    sendToWorker('GET_MESSAGE_HISTORY', { roomId, beforeMessageId });
                });
            },
            dispose: function () {
                console.log('[SignalRInterop] Disposing worker');
                worker.port.close();
            }
        };
    },

    setupInfiniteScroll: function (element, dotNetRef) {
        element.addEventListener('scroll', async function () {
            const scrollInfo = {
                ScrollTop: element.scrollTop,
                ScrollHeight: element.scrollHeight,
                ClientHeight: element.clientHeight
            };

            console.log('[SignalRInterop] Scroll event:', scrollInfo);

            if (element.scrollTop < 100) {
                await dotNetRef.invokeMethodAsync('LoadMoreMessages');
            }
        });
    },

    getScrollInfo: function (element) {
        const info = {
            ScrollTop: element.scrollTop,
            ScrollHeight: element.scrollHeight,
            ClientHeight: element.clientHeight,
            IsNearTop: element.scrollTop < 100
        };
        console.log('[SignalRInterop] getScrollInfo:', info);
        return info;
    },

    scrollToBottom: function (element) {
        if (element) {
            console.log('[SignalRInterop] Scrolling to bottom');
            element.scrollTop = element.scrollHeight;
        }
    }
};
