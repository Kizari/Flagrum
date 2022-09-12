using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Flagrum.Web.Services;

public class JSInterop
{
    private readonly IJSRuntime _jsRuntime;

    public JSInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task ClickElement(string id)
    {
        await _jsRuntime.InvokeVoidAsync("interop.clickElement", id);
    }

    public async Task FadeIn(string className)
    {
        await _jsRuntime.InvokeVoidAsync("interop.fadeIn", className);
    }

    public async Task FadeOut(string className)
    {
        await _jsRuntime.InvokeVoidAsync("interop.fadeOut", className);
    }

    public async Task HideOverlay()
    {
        await _jsRuntime.InvokeVoidAsync("interop.hideOverlay");
    }

    public async Task ShowInnerOverlay()
    {
        await _jsRuntime.InvokeVoidAsync("interop.showInnerOverlay");
    }

    public async Task HideInnerOverlay()
    {
        await _jsRuntime.InvokeVoidAsync("interop.hideInnerOverlay");
    }

    public async Task SetBackgroundImage(string uuid)
    {
        await _jsRuntime.InvokeVoidAsync("interop.setBackgroundImage", uuid);
    }

    public async Task ApplyHtmlToElement(string id, string html, object reference)
    {
        await _jsRuntime.InvokeVoidAsync("interop.applyHtmlToElement", id, html, reference);
    }

    public async Task ScrollToElement(string id)
    {
        await _jsRuntime.InvokeVoidAsync("interop.scrollToElement", id);
    }

    public ValueTask SetFocusToElement(string id)
    {
        return _jsRuntime.InvokeVoidAsync("interop.setFocusToElement", id);
    }

    public ValueTask<double> GetElementLeftOffset(string id)
    {
        return _jsRuntime.InvokeAsync<double>("interop.getElementLeftOffset", id);
    }

    public ValueTask<double> GetElementTopOffset(string id)
    {
        return _jsRuntime.InvokeAsync<double>("interop.getElementTopOffset", id);
    }

    public ValueTask<double> GetElementWidth(string id)
    {
        return _jsRuntime.InvokeAsync<double>("interop.getElementWidth", id);
    }

    public ValueTask<double> GetElementHeight(string id)
    {
        return _jsRuntime.InvokeAsync<double>("interop.getElementHeight", id);
    }

    public async Task ObserveElementResize(object reference, string elementId)
    {
        await _jsRuntime.InvokeVoidAsync("interop.observeElementResize", reference, elementId);
    }
}