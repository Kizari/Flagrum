using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Flagrum.Core.Utilities;
using Microsoft.Web.WebView2.Core;

namespace Flagrum.Utilities;

public static class WebView2Helper
{
    public static async Task EnsureInstalled()
    {
        try
        {
            CoreWebView2Environment.GetAvailableBrowserVersionString();
            
            // If WebView2 is already available, delete the installer that Flagrum ships with as it's no longer needed
            var path = Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "MicrosoftEdgeWebview2Setup.exe");
            IOHelper.DeleteFileIfExists(path);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            SplashViewModel.Instance.SetLoadingText("Installing Additional Dependencies");
            await InstallWebView2Runtime();
        }
    }

    private static async Task InstallWebView2Runtime()
    {
        var start = new ProcessStartInfo
        {
            FileName = "MicrosoftEdgeWebview2Setup.exe",
            Arguments = "/silent /install",
            WindowStyle = ProcessWindowStyle.Hidden
        };

        var process = new Process {StartInfo = start};
        process.Start();
        await process.WaitForExitAsync();
    }
}