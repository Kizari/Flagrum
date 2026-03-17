using System.Threading.Tasks;
using Flagrum.Host;
using Injectio.Attributes;
using Velopack;
using Velopack.Sources;

namespace Flagrum.Services;

/// <summary>
/// Handles automatic updates for Flagrum.
/// </summary>
[RegisterSingleton<UpdateService>]
public class UpdateService(ApplicationHost application)
{
    /// <summary>
    /// Attempts to update the application.
    /// </summary>
    /// <returns><c>true</c> if an update was applied, otherwise <c>false</c>.</returns>
    public async Task<bool> TryUpdate()
    {
        try
        {
            application.SetSplashText("Checking for updates");

            // Check for updates
            var github = new GithubSource("https://github.com/Kizari/Flagrum", null, false);
            var manager = new UpdateManager(github);
            var newVersion = await manager.CheckForUpdatesAsync();

            // Download and apply updates if any were available
            if (newVersion != null)
            {
                application.SetSplashText("Downloading updates");
                await manager.DownloadUpdatesAsync(newVersion);
                application.SetSplashText("Updating Flagrum");
                manager.ApplyUpdatesAndRestart();
                return true;
            }
        }
        catch
        {
            // Not much to be done if this fails, most likely due to no internet or user blocking the update URL
            // Let Flagrum continue as normal
        }

        application.SetSplashText("Loading");
        return false;
    }
}