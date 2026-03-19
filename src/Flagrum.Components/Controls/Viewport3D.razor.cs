using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Flagrum.Components.Controls;

public sealed partial class Viewport3D : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private bool _isInitialized;

    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    [Parameter] public Func<Task>? OnReady { get; set; }
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public Control LeftClickAction { get; set; }
    [Parameter] public Control RightClickAction { get; set; }
    [Parameter] public Control MiddleClickAction { get; set; }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            await _module.InvokeVoidAsync("dispose");
            await _module.DisposeAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
                "./_content/Flagrum.Components/Controls/Viewport3D.razor.js");
            
            await _module.InvokeVoidAsync("initialize",
                (int)LeftClickAction, (int)MiddleClickAction, (int)RightClickAction);
            
            if (OnReady != null)
            {
                await OnReady();
            }
            
            _isInitialized = true;
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_isInitialized)
        {
            await _module!.InvokeVoidAsync("setLeftClick", LeftClickAction);
            await _module!.InvokeVoidAsync("setMiddleClick", MiddleClickAction);
            await _module!.InvokeVoidAsync("setRightClick", RightClickAction);
        }
    }

    public async Task AddMeshAsync(
        float[] vertices,
        uint[] faceIndices,
        float[] normals,
        float[] uvs,
        byte[]? diffuse,
        byte[]? normalMap)
    {
        await _module!.InvokeVoidAsync("addMesh", vertices, faceIndices, normals, uvs, diffuse, normalMap);
    }

    public async Task ClearMeshesAsync()
    {
        await _module!.InvokeVoidAsync("clearMeshes");
    }

    public async Task FrameModelAsync(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
        await _module!.InvokeVoidAsync("frameModel", minX, minY, minZ, maxX, maxY, maxZ);
    }
    
    public enum Control {None = -1, Rotate, Dolly, Pan}
}