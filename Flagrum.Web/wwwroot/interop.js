window.interop = {
    showAlert: function (message) {
        alert(message);
    },

    clickElement: function (id) {
        document.getElementById(id).click();
    }
}