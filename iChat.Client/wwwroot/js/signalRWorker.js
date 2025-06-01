
window.signalRInterop = {
    initialize: function (dotNetRef) {
        const worker = new SharedWorker('/js/sharedWorker.js');
        worker.port.start();

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
                case 'HEARTBEAT_CHECK':
                    worker.port.postMessage({ action: 'HEARTBEAT' });
                    break;
            }
        };

        worker.port.postMessage({
            action: 'INIT_SIGNALR'
        });

        return {
            joinRoom: function (roomId) {
                worker.port.postMessage({
                    action: 'JOIN_ROOM',
                    data: { roomId }
                });
            },
            leaveRoom: function (roomId) {
                worker.port.postMessage({
                    action: 'LEAVE_ROOM',
                    data: { roomId }
                });
            },
            sendMessage: function (roomId, message) {
                worker.port.postMessage({
                    action: 'SEND_MESSAGE',
                    data: { roomId, message }
                });
            },
            dispose: function () {
                worker.port.close();
            }
        };
    },
    scrollToBottom: function (element) {
        if (element) {
            element.scrollTop = element.scrollHeight;
        }
    }
};