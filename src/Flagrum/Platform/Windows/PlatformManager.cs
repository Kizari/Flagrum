using System;
using System.IO;
using Flagrum.Core.Utilities;
using Flagrum.Platform.Abstractions;
using Flagrum.Platform.Windows.Interop;
using Injectio.Attributes;
using Microsoft.Win32;

namespace Flagrum.Platform.Windows;

/// <inheritdoc />
[RegisterTransient<IPlatformManager>]
public class PlatformManager(VersionHelper versionHelper) : IPlatformManager
{
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
}