using Microsoft.AspNetCore.Components;

namespace Flagrum.Web.Features.AssetExplorer.Base;

public abstract partial class AssetExplorer
{
    public AddressBar AddressBar { get; set; }
    public FileList FileList { get; set; }
    public Preview Preview { get; set; }

    protected RenderFragment AddressBarTemplate { get; set; }
    protected RenderFragment FileListTemplate { get; set; }

    protected bool IsLoading { get; set; }
    protected string LoadingMessage { get; set; }
}