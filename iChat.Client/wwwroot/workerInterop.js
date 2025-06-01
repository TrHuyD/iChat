//window.sharedWorkerInterop = {
//    startWorker: function () {
//        if (!window.sharedWorkerInstance) {
//            console.log("[Interop] Starting SharedWorker...");
//            window.sharedWorkerInstance = new SharedWorker("/worker.js");

//            window.sharedWorkerInstance.port.onmessage = function (event) {
//                console.log("[Interop] Message from SharedWorker:", event.data);
//            };

//            window.sharedWorkerInstance.port.start();
//        } else {
//            console.log("[Interop] SharedWorker already started.");
//        }
//    },

//    sendMessage: function (message) {
//        if (window.sharedWorkerInstance) {
//            console.log("[Interop] Sending message to SharedWorker:", message);
//            window.sharedWorkerInstance.port.postMessage(message);
//        } else {
//            console.warn("[Interop] SharedWorker not started yet.");
//        }
//    }
//};
