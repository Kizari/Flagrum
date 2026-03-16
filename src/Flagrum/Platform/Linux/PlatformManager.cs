using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Flagrum.Abstractions.Application;
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
    public bool IsVersionSupported => true;

    /// <inheritdoc />
    public void EnableTaskbarStacking()
    {
        // Enabled by default on Linux
    }

    /// <inheritdoc />
    public void SetFileTypeAssociation()
    {
        // TODO: Implement this
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