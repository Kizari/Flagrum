window.__receiveMessageCallbacks = [];

window.__dispatchMessageCallback = function (message) {
    window.__receiveMessageCallbacks.forEach(cb => cb(message));
};

window.external = {
    sendMessage: function (message) {
        if (window.native && window.native.postMessage) {
            window.native.postMessage(message);
        }
    },

    receiveMessage: function (callback) {
        window.__receiveMessageCallbacks.push(callback);
    }
};

new QWebChannel(qt.webChannelTransport, function (channel) {
    window.native = channel.objects.native;
});