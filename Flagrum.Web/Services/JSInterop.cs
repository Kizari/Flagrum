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
}