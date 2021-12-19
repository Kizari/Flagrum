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
    }
}