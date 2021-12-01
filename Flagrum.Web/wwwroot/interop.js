window.interop = {
    showAlert: function (message) {
        alert(message);
    },

    clickElement: function (id) {
        document.getElementById(id).click();
    },

    setBackgroundImageBase64: function (id, base64) {
        let element = document.getElementById(id);
        element.style.backgroundImage = "url('data:image/png;base64," + base64 + "')";
    }
}