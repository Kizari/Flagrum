using System.IO;
using System.Threading.Tasks;
using Flagrum.Web.Features.AssetExplorer.Base;
using Flagrum.Web.Features.AssetExplorer.Data;
using Microsoft.AspNetCore.Components.Web;

namespace Flagrum.Web.Features.AssetExplorer.FileSystem;

public class FileSystemAddressBar : AddressBar
{
    public override async void NavigateToCurrentPath()
    {
        if (File.Exists(CurrentPath))
        {
            var directory = Path.GetDirectoryName(CurrentPath);
            //SetActiveDirectory(directory);
        }
        else if (Directory.Exists(CurrentPath))
        {
            //SetActiveDirectory(CurrentPath);
        }
        else
        {
            Parent.Alert.Open("Error", "Invalid Path", "Nothing was found at the given path.", null);
            return;
        }
        
        Parent.CurrentView = AssetExplorerView.GameView;
        StateHasChanged();
        await Task.Delay(100);
        Parent.CurrentView = AssetExplorerView.FileSystem;
        StateHasChanged();
    }

    protected override string GetPersistedPath() => AppState.GetCurrentAssetExplorerPath();
}