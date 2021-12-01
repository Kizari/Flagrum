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

    public async Task Alert(string message)
    {
        await _jsRuntime.InvokeVoidAsync("interop.showAlert", message);
    }

    public async Task ClickElement(string id)
    {
        await _jsRuntime.InvokeVoidAsync("interop.clickElement", id);
    }

    public async Task SetBackgroundImageBase64(string elementId, string base64)
    {
        await _jsRuntime.InvokeVoidAsync("interop.setBackgroundImageBase64", elementId, base64);
    }
}