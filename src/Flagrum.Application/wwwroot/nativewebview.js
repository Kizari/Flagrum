window.__receiveMessageCallbacks = [];

window.__dispatchMessageCallback = function(message) {
    window.__receiveMessageCallbacks.forEach(function(callback) {
        callback(message);
    });
};

window.external = {
    sendMessage: function(message) {
        invokeCSharpAction(message);
    },

    receiveMessage: function(callback) {
        window.__receiveMessageCallbacks.push(callback);
    }
};