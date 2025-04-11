using Microsoft.JSInterop;

namespace Flagrum.Components;

public class JSInterop(IJSRuntime jsRuntime)
{
    public async Task ClickElement(string id)
    {
        await jsRuntime.InvokeVoidAsync("interop.clickElement", id);
    }

    public async Task FadeIn(string className)
    {
        await jsRuntime.InvokeVoidAsync("interop.fadeIn", className);
    }

    public async Task FadeOut(string className)
    {
        await jsRuntime.InvokeVoidAsync("interop.fadeOut", className);
    }

    public async Task HideOverlay()
    {
        await jsRuntime.InvokeVoidAsync("interop.hideOverlay");
    }

    public async Task ShowInnerOverlay()
    {
        await jsRuntime.InvokeVoidAsync("interop.showInnerOverlay");
    }

    public async Task HideInnerOverlay()
    {
        await jsRuntime.InvokeVoidAsync("interop.hideInnerOverlay");
    }

    public async Task SetBackgroundImage(string uuid)
    {
        await jsRuntime.InvokeVoidAsync("interop.setBackgroundImage", uuid);
    }

    public async Task ApplyHtmlToElement(string id, string html, object reference)
    {
        await jsRuntime.InvokeVoidAsync("interop.applyHtmlToElement", id, html, reference);
    }

    public async Task ScrollToElement(string id)
    {
        await jsRuntime.InvokeVoidAsync("interop.scrollToElement", id);
    }

    public ValueTask SetFocusToElement(string id) => jsRuntime.InvokeVoidAsync("interop.setFocusToElement", id);

    public ValueTask<double> GetElementLeftOffset(string id) =>
        jsRuntime.InvokeAsync<double>("interop.getElementLeftOffset", id);

    public ValueTask<double> GetElementTopOffset(string id) =>
        jsRuntime.InvokeAsync<double>("interop.getElementTopOffset", id);

    public ValueTask<double> GetElementWidth(string id) => jsRuntime.InvokeAsync<double>("interop.getElementWidth", id);

    public ValueTask<double> GetElementHeight(string id) =>
        jsRuntime.InvokeAsync<double>("interop.getElementHeight", id);

    public async Task ObserveElementResize(object reference, string elementId)
    {
        await jsRuntime.InvokeVoidAsync("interop.observeElementResize", reference, elementId);
    }
}