using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace Flagrum.Utilities;

public static class UpdateHelper
{
    public static async Task<bool> Update()
    {
        try
        {
            SplashViewModel.Instance.SetLoadingText("Checking for updates");

            var github = new GithubSource("https://github.com/Kizari/Flagrum", null, false);
            var manager = new UpdateManager(github);
            var newVersion = await manager.CheckForUpdatesAsync();
            
            if (newVersion != null)
            {
                SplashViewModel.Instance.SetLoadingText("Downloading updates");
                await manager.DownloadUpdatesAsync(newVersion);
                SplashViewModel.Instance.SetLoadingText("Updating Flagrum");
                manager.ApplyUpdatesAndRestart();
                return true;
            }
        }
        catch
        {
            // Not much to be done if this fails, most likely due to no internet or user blocking the update URL
            // Let Flagrum continue as normal
        }
        
        SplashViewModel.Instance.SetLoadingText("Initialising");
        return false;
    }
}