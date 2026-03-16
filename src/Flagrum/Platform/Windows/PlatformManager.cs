using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Flagrum.Abstractions.Application;
using Flagrum.Core.Utilities;
using Flagrum.Platform.Windows.Interop;
using Injectio.Attributes;
using Microsoft.Win32;

namespace Flagrum.Platform.Windows;

/// <inheritdoc />
[RegisterTransient<IPlatformManager>]
public class PlatformManager(VersionHelper versionHelper) : IPlatformManager
{
    private const string Steam32 = @"SOFTWARE\VALVE\Steam";
    private const string Steam64 = @"SOFTWARE\Wow6432Node\Valve\Steam";
    private const string ClientIdKey =
        @"HKEY_CURRENT_USER\Software\Lucent\CLASSES\CLSID\{D81BA2C5-A176-4EE5-B8CB-3AAAE258AC8A}";

    /// <inheritdoc />
    public Guid LucentClientId
    {
        get
        {
            var clientIdString = (string?)Registry.GetValue(ClientIdKey, "", null);
            if (clientIdString == null)
            {
                clientIdString = Guid.NewGuid().ToString();
                Registry.SetValue(ClientIdKey, "", clientIdString);
            }

            return new Guid(clientIdString);
        }
    }

    /// <inheritdoc />
    public bool IsVersionSupported => versionHelper.IsCurrent();

    /// <inheritdoc />
    public void EnableTaskbarStacking() => Shell32.SetCurrentProcessExplicitAppUserModelID("Flagrum");

    /// <inheritdoc />
    public void SetFileTypeAssociation()
    {
        var flagrumPath = Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "Flagrum.exe") + " \"%1\"";
        if ((string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Classes\\.fmod", "", "Flagrum")! != flagrumPath)
        {
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\Flagrum", "", "FMOD");
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\Flagrum", "FriendlyTypeName", "Flagrum Mod");
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\Flagrum\\shell\\open\\command", "",
                flagrumPath);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\.fmod", "", "Flagrum");

            //this call notifies Windows that it needs to redo the file associations and icons
            _ = Shell32.SHChangeNotify(0x08000000, 0x2000, IntPtr.Zero, IntPtr.Zero);
        }
    }

    /// <inheritdoc />
    public bool TryGetSteamExecutablePath([NotNullWhen(true)] out string? result)
    {
        result = null;
        
        try
        {
            var key64 = Registry.LocalMachine.OpenSubKey(Steam64);
            if (key64 == null)
            {
                var key32 = Registry.LocalMachine.OpenSubKey(Steam32);
                result = key32?.GetValue("InstallPath")?.ToString();
            }
            else
            {
                result = key64.GetValue("InstallPath")?.ToString();
            }

            if (result != null)
            {
                result += @"\steam.exe";
                return true;
            }
        }
        catch
        {
            // Nothing needed here, auto-detecting this path isn't critical
        }

        return false;
    }
}