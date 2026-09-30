/**
 * Registers a click handler that will close the context menu if the user clicks outside of it.
 * 
 * @param element Context menu element.
 * @param dotNetHelper Allows for calling back into .NET from here.
 * @returns {{dispose: function(): void}} Cleanup function.
 */
export function registerOutsideClick(element, dotNetHelper) {
    // Click event handler
    function onClick(event) {
        if (!element.contains(event.target)) {
            dotNetHelper.invokeMethodAsync("NotifyOutsideClick");
        }
    }
    
    // Register the click event function
    document.addEventListener("click", onClick);
    
    // Return a dispose handler that .NET can call when finished
    return {
        dispose: () => document.removeEventListener("click", onClick)
    };
}