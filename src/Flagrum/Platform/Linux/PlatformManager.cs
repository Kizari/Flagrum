using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Flagrum.Abstractions.Application;
using Flagrum.Generators;
using Injectio.Attributes;

namespace Flagrum.Platform.Linux;

/// <inheritdoc />
[RegisterTransient<IPlatformManager>]
public class PlatformManager : IPlatformManager
{
    /// <inheritdoc />
    public Guid LucentClientId
    {
        get
        {
            // Get the file path
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Lucent");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "registry.bin");

            // Read the ID from the file
            if (File.Exists(path))
            {
                var data = File.ReadAllBytes(path);
                if (data.Length == 16)
                {
                    return new Guid(data);
                }
                
                // File was corrupted, delete it
                File.Delete(path);
            }

            // Generate the client ID and write it to disk
            var clientId = Guid.NewGuid();
            File.WriteAllBytes($"{path}.tmp",clientId.ToByteArray());
            File.Move($"{path}.tmp", path, true);
            return clientId;
        }
    }

    /// <inheritdoc />
    public HashSet<Guid> ApplicationMigrationSteps => SteppedMigrationHelper.ApplicationSteps;
    
    /// <inheritdoc />
    public HashSet<Guid> ProfileMigrationSteps => SteppedMigrationHelper.ProfileSteps;

    /// <inheritdoc />
    public bool IsVersionSupported => true;

    /// <inheritdoc />
    public void EnableTaskbarStacking()
    {
        // Enabled by default on Linux
    }

    /// <inheritdoc />
    public void SetFileTypeAssociation()
    {
        var needsUpdate = false;
        
        // Ensure directories exist
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var mimeDirectory = Path.Combine(home, ".local", "share", "mime");
        var applicationsDirectory = Path.Combine(home, ".local", "share", "applications");
        Directory.CreateDirectory(mimeDirectory);
        Directory.CreateDirectory(applicationsDirectory);

        // Write the MIME type record
        var mimePath = Path.Combine(mimeDirectory, "packages", "flagrum.xml");
        if (!File.Exists(mimePath))
        {
            needsUpdate = true;
            File.WriteAllText(mimePath,
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <mime-info xmlns="http://www.freedesktop.org/standards/shared-mime-info">
                  <mime-type type="application/x-flagrum-mod">
                    <comment>Flagrum Mod</comment>
                    <glob pattern="*.fmod"/>
                  </mime-type>
                </mime-info>                                                                                    
                """);
        }

        // Write the desktop entry
        var executablePath = Path.Combine(home, ".local", "bin", "flagrum");
        var desktopPath = Path.Combine(applicationsDirectory, "flagrum.desktop");
        if (!File.Exists(desktopPath))
        {
            needsUpdate = true;
            File.WriteAllText(desktopPath,
                $"""
                [Desktop Entry]
                Type=Application
                Name=Flagrum
                Exec={executablePath} %f
                MimeType=application/x-flagrum-mod;
                Icon=flagrum
                Terminal=false
                """);
            
            // Make the desktop entry executable
            Process.Start("chmod", $"+x \"{desktopPath}\"");
        }
        
        // Register file type association
        if (needsUpdate)
        {
            Process.Start("xdg-mime", "default flagrum.desktop application/x-flagrum-mod");
            Process.Start("update-mime-database", mimeDirectory);
            Process.Start("update-desktop-database", applicationsDirectory);
        }
    }

    /// <inheritdoc />
    public bool TryGetSteamExecutablePath([NotNullWhen(true)] out string? result)
    {
        // Common Steam install locations on Linux
        string[] possiblePaths =
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".steam/steam/steam.sh"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".local/share/Steam/steam.sh")
        ];

        result = possiblePaths.FirstOrDefault(File.Exists);
        return result != null;
    }
}