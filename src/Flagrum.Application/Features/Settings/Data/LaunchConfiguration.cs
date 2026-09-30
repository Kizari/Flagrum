using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Flagrum.Abstractions;
using Flagrum.Application.Utilities;
using Injectio.Attributes;

namespace Flagrum.Application.Features.Settings.Data;

[RegisterSingleton<LaunchConfiguration>]
public class LaunchConfiguration(IConfiguration configuration)
{
    [Required]
    [DirectoryExists]
    public string? SteamRoot
    {
        get => configuration.TryGet<string?>(StateKey.SteamRootPath, out var value)
            ? value 
            : DetectPath(StateKey.SteamRootPath);
        set => configuration.Set(StateKey.SteamRootPath, value);
    }
    
    [Required]
    [DirectoryExists]
    public string? ProtonPrefix
    {
        get => configuration.TryGet<string?>(StateKey.ProtonPrefixPath, out var value)
            ? value 
            : DetectPath(StateKey.ProtonPrefixPath);
        set => configuration.Set(StateKey.ProtonPrefixPath, value);
    }

    [LaunchCommandFileExists("$FLAGRUM_STEAM_LAUNCH_WRAPPER")]
    public string? SteamLaunchWrapper
    {
        get => configuration.TryGet<string?>(StateKey.SteamLaunchWrapperPath, out var value)
            ? value 
            : DetectPath(StateKey.SteamLaunchWrapperPath);
        set => configuration.Set(StateKey.SteamLaunchWrapperPath, value);
    }
    
    [LaunchCommandFileExists("$FLAGRUM_STEAM_REAPER")]
    public string? SteamReaper
    {
        get => configuration.TryGet<string?>(StateKey.SteamReaperPath, out var value)
            ? value 
            : DetectPath(StateKey.SteamReaperPath);
        set => configuration.Set(StateKey.SteamReaperPath, value);
    }
    
    [LaunchCommandFileExists("$FLAGRUM_STEAM_RUNTIME")]
    public string? SteamRuntime
    {
        get => configuration.TryGet<string?>(StateKey.SteamRuntimePath, out var value)
            ? value 
            : DetectPath(StateKey.SteamRuntimePath);
        set => configuration.Set(StateKey.SteamRuntimePath, value);
    }
    
    [LaunchCommandFileExists("$FLAGRUM_PROTON_EXECUTABLE")]
    public string? Proton
    {
        get => configuration.TryGet<string?>(StateKey.ProtonPath, out var value)
            ? value 
            : DetectPath(StateKey.ProtonPath);
        set => configuration.Set(StateKey.ProtonPath, value);
    }

    [Required]
    public string? LaunchCommand
    {
        get => configuration.TryGet<string?>(StateKey.LaunchCommand, out var launchCommand) ? launchCommand :
            """
            "$FLAGRUM_STEAM_LAUNCH_WRAPPER" \
                -- "$FLAGRUM_STEAM_REAPER" SteamLaunch AppId=637650 \
                -- "$FLAGRUM_STEAM_RUNTIME" --verb=waitforexitandrun \
                -- "$FLAGRUM_PROTON_EXECUTABLE" waitforexitandrun "$FLAGRUM_LAUNCHER" \
                    --game-path="$FLAGRUM_GAME_EXECUTABLE" \
                    --hook-path="$FLAGRUM_MOD_LOADER" \
                    $FLAGRUM_LAUNCHER_CONFIG
            """;
        set => configuration.Set(StateKey.LaunchCommand, value);
    }

    private string? DetectPath(StateKey key)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var steam = Path.Combine(home, ".local", "share", "Steam");
        
        if (!Directory.Exists(steam))
        {
            return null;
        }

        var path = key switch
        {
            StateKey.SteamRootPath => steam,
            StateKey.ProtonPrefixPath => Path.Combine(steam, "steamapps", "compatdata", "637650"),
            StateKey.SteamLaunchWrapperPath => Path.Combine(steam, "ubuntu12_32", "steam-launch-wrapper"),
            StateKey.SteamReaperPath => Path.Combine(steam, "ubuntu12_32", "reaper"),
            StateKey.SteamRuntimePath => DetectRuntimePath(steam),
            StateKey.ProtonPath => DetectProtonPath(steam),
            _ => throw new NotSupportedException($"Unsupported path key '{key}'")
        };

        return key switch
        {
            StateKey.SteamRootPath or StateKey.ProtonPrefixPath => Directory.Exists(path) ? path : null,
            _ => File.Exists(path) ? path : null
        };
    }
    
    private string? DetectRuntimePath(string steam)
    {
        var common = Path.Combine(steam, "steamapps", "common");
        var sniperFolders = Directory.EnumerateDirectories(common)
            .Where(d => d.Split(Path.DirectorySeparatorChar)[^1].StartsWith("SteamLinuxRuntime_sniper"))
            .ToArray();

        if (sniperFolders.Length != 1)
        {
            return null;
        }

        var path = Path.Combine(sniperFolders[0], "_v2-entry-point");
        return File.Exists(path) ? path : null;
    }

    private string? DetectProtonPath(string steam)
    {
        var common = Path.Combine(steam, "steamapps", "common");
        var path = Path.Combine(common, "Proton - Experimental", "proton");
        return File.Exists(path) ? path : null;
    }
}