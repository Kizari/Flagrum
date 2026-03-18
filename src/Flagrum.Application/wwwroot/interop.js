window.addEventListener("click", function (e) {
    for (let i = 0; i < interop.searchSelects.length; i++) {
        let wrapper = document.getElementById(interop.searchSelects[i].wrapperId);
        let select = document.getElementById(interop.searchSelects[i].selectId);

        if (wrapper != undefined && select != undefined && !wrapper.contains(e.target) && !e.target.classList.contains("calendar-day") && !e.target.classList.contains("v-select-search-box")) {
            select.classList.remove("open");
        } else if (wrapper != undefined && select != undefined && wrapper.contains(e.target) && interop.searchSelects[i].closeOnSelect && !e.target.classList.contains("load-more-button") && !e.target.classList.contains("v-select-search-box")) {
            select.classList.remove("open");
        }
    }
});

window.interop = {
    searchSelects: [],

    monitorSearchSelect: function (selectId, wrapperId, closeOnSelect) {
        let exists = false;
        for (let i = 0; i < this.searchSelects.length; i++) {
            if (this.searchSelects[i].selectId === selectId) {
                exists = true;
            }
        }

        if (!exists) {
            this.searchSelects.push({
                selectId: selectId,
                wrapperId: wrapperId,
                closeOnSelect: closeOnSelect
            });
        }
    },

    toggleSearchSelect: function (elementId) {
        let select = document.getElementById(elementId);
        if (!select.classList.contains("open")) {
            select.classList.add("open");
        }
    },

    focusElement: function (elementId) {
        setTimeout(function () {
            document.getElementById(elementId).focus();
        }, 400);
    },

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

    applyHtmlToElement: function (id, html, dotNetObject) {
        window.interop.currentReference = dotNetObject;
        let element = document.getElementById(id);
        if (element !== null && element !== undefined) {
            element.innerHTML = html;
        }
    },

    openDotNetLink: function (uri) {
        window.interop.currentReference.invokeMethodAsync("OpenLink", uri);
    },

    scrollToElement: function (id) {
        document.getElementById(id).scrollIntoView({behavior: 'smooth'});
    },

    setFocusToElement: function (id) {
        document.getElementById(id).focus();
    },

    getElementLeftOffset: function (id) {
        return document.getElementById(id).offsetLeft;
    },

    getElementTopOffset: function (id) {
        return document.getElementById(id).offsetTop;
    },

    getElementWidth: function (id) {
        return document.getElementById(id).clientWidth;
    },

    getElementHeight: function (id) {
        return document.getElementById(id).clientHeight;
    },

    observeElementResize: function (component, elementId) {
        let element = document.getElementById(elementId);
        new ResizeObserver(function () {
            component.invokeMethodAsync("OnResize", element.offsetLeft, element.offsetTop, element.clientWidth, element.clientHeight);
        }).observe(element);
    }
}