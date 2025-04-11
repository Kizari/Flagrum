using System.IO;
using Flagrum.Application.Features.AssetExplorer.Base;
using Flagrum.Application.Features.AssetExplorer.Data;

namespace Flagrum.Application.Features.AssetExplorer.FileSystem;

public class FileSystemAddressBar : AddressBar
{
    protected override bool IsDisabled => false;

    public override void NavigateToCurrentPath()
    {
        CurrentPath = CurrentPath.TrimEnd('\\');

        // Fixes problem where root disk path forces the app to navigate to the executing directory
        if (CurrentPath.Length == 2)
        {
            CurrentPath += '\\';
        }

        if (string.IsNullOrWhiteSpace(CurrentPath))
        {
            AssetExplorer.FileList.SetCurrentNode(FileSystemNode.CreateRootNode());
        }
        else if (File.Exists(CurrentPath))
        {
            AssetExplorer.FileList.SetCurrentNode(new FileSystemNode(Path.GetDirectoryName(CurrentPath)));
            AssetExplorer.Preview.SetItem(new FileSystemNode(CurrentPath));
        }
        else if (Directory.Exists(CurrentPath))
        {
            AssetExplorer.FileList.SetCurrentNode(new FileSystemNode(CurrentPath));
        }
        else
        {
            if (AppState.Is3DViewerOpen)
            {
                PlatformService.Set3DViewportVisibility(false);
            }

            Parent.Alert.Open("Error", "Invalid Path", "Nothing was found at the given path.", () =>
            {
                if (AppState.Is3DViewerOpen)
                {
                    PlatformService.Set3DViewportVisibility(true);
                }
            });
            return;
        }

        StateHasChanged();
    }
}