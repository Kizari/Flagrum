window.interop = {
    clickElement: function (id) {
        document.getElementById(id).click();
    },

    fadeIn: function (className) {
        let elements = document.getElementsByClassName(className);
        for (let i = 0; i < elements.length; i++) {
            // setTimeout(function () {
            //     elements[i].classList.add("transition-opacity");
            //     elements[i].classList.remove("opacity-0");
            // }, 20 * i);
            //elements[i].classList.add("transition-opacity");
            elements[i].classList.remove("opacity-0");
        }
    },

    fadeOut: function (className) {
        let elements = document.getElementsByClassName(className);
        for (let i = 0; i < elements.length; i++) {
            //elements[i].classList.remove("transition-opacity");
            //elements[i].classList.add("opacity-0");
        }
    },

    hideOverlay: function () {
        let overlay = document.getElementById("overlay");
        overlay.classList.add("opacity-0");
        setTimeout(function () {
            overlay.remove();
        }, 200);
    },

    hideInnerOverlay: function () {
        let overlay = document.getElementById("inner-overlay");
        overlay.classList.add("opacity-0");
        setTimeout(function () {
            overlay.classList.add("hidden");
        }, 500);
    },

    showInnerOverlay: function () {
        let overlay = document.getElementById("inner-overlay");
        overlay.classList.remove("hidden");
        setTimeout(function () {
            overlay.classList.remove("opacity-0");
        }, 20);
    },
    
    setBackgroundImage: function (modId) {
        let container = document.getElementById(modId);
        let image = new Image();
        image.onload = function () {
            if (container !== null) {
                container.style.backgroundImage = `url('images/${modId}.png')`;
                container.classList.remove("opacity-0");
            }
        }

        image.src = `images/${modId}.png`;
    },

    initialiseBmcButton: function (id) {
        let button = document.getElementById(id);
        let C = document.querySelector('script[data-name="bmc-button"]');
        button.innerHTML = bmcBtnWidget(
            C.attributes["data-text"].value,
            C.attributes["data-slug"].value,
            C.attributes["data-color"].value,
            C.attributes["data-emoji"].value,
            C.attributes["data-font"].value,
            C.attributes["data-font-color"]
                ? C.attributes["data-font-color"].value
                : void 0,
            C.attributes["data-outline-color"]
                ? C.attributes["data-outline-color"].value
                : void 0,
            C.attributes["data-coffee-color"]
                ? C.attributes["data-coffee-color"].value
                : void 0
        );
    },

    applyHtmlToElement: function (id, html, dotNetObject) {
        window.interop.currentReference = dotNetObject;
        let element = document.getElementById(id);
        element.innerHTML = html;
    },

    openDotNetLink: function (uri) {
        window.interop.currentReference.invokeMethodAsync("OpenLink", uri);
    }
}