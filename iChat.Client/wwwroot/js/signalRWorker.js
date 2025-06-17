window.signalRInterop = {
    initialize: async function (dotNetRef) {
        const worker = new SharedWorker('/js/sharedWorker.js');
        worker.port.start();

        await new Promise((resolve) => {
            const readyHandler = (event) => {
                if (event.data.action === 'WORKER_READY') {
                    worker.port.removeEventListener('message', readyHandler);
                    resolve();
                }
            };
            worker.port.addEventListener('message', readyHandler);
        });

        worker.port.onmessage = function (event) {
            const { action, data } = event.data;

            switch (action) {
                case 'MESSAGE_RECEIVED':
                    dotNetRef.invokeMethodAsync('HandleMessageReceived', JSON.stringify(data));
                    break;
                case 'SIGNALR_CONNECTED':
                    dotNetRef.invokeMethodAsync('HandleConnected');
                    break;
                case 'SIGNALR_DISCONNECTED':
                    dotNetRef.invokeMethodAsync('HandleDisconnected');
                    break;
                case 'SIGNALR_RECONNECTING':
                    dotNetRef.invokeMethodAsync('HandleReconnecting');
                    break;
                case 'SIGNALR_RECONNECTED':
                    dotNetRef.invokeMethodAsync('HandleReconnected');
                    break;
                case 'MESSAGE_HISTORY':
                    dotNetRef.invokeMethodAsync('HandleMessageHistory', data);
                    break;
                case 'MESSAGE_HISTORY_ERROR':
                    dotNetRef.invokeMethodAsync('HandleMessageHistoryError', data);
                    break;
            }
        };

        const sendToWorker = (action, data) => {
            worker.port.postMessage({ action, data });
        };

        sendToWorker('INIT_SIGNALR');

        return {
            joinRoom: (roomId) => sendToWorker('JOIN_ROOM', { roomId }),
            leaveRoom: (roomId) => sendToWorker('LEAVE_ROOM', { roomId }),
            sendMessage: (roomId, message) => sendToWorker('SEND_MESSAGE', { roomId, message }),
            getMessageHistory: (roomId, beforeMessageId) => {
                return new Promise((resolve) => {
                    const tempHandler = (e) => {
                        const { action, data } = e.data;
                        if (action === 'MESSAGE_HISTORY' || action === 'MESSAGE_HISTORY_ERROR') {
                            worker.port.removeEventListener('message', tempHandler);
                            resolve(action === 'MESSAGE_HISTORY' ? data : []);
                        }
                    };
                    worker.port.addEventListener('message', tempHandler);
                    sendToWorker('GET_MESSAGE_HISTORY', { roomId, beforeMessageId });
                });
            },
            dispose: () => worker.port.close()
        };
    }
};