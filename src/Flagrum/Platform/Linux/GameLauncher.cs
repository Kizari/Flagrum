using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Flagrum.Abstractions;
using Flagrum.Abstractions.ModManager;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Services;
using Flagrum.Core.Utilities;
using Injectio.Attributes;

namespace Flagrum.Platform.Linux;

/// <inheritdoc />
[RegisterSingleton<IGameLauncher>]
public class GameLauncher(
    IProfileService profile,
    IPremiumService premium,
    ModManagerServiceBase modManager) : IGameLauncher
{
    private const string HookDllName = "Drautos.dll";

    /// <inheritdoc />
    public GameLaunchResult TryLaunch(bool isDebug)
    {
        // Don't launch if the game is already running
        if (profile.IsGameRunning())
        {
            return GameLaunchResult.GameAlreadyRunning;
        }

        // Ensure the executable is supported
        var type = premium.GetGameExecutableType();
        if (type == GameExecutableType.Unknown)
        {
            return GameLaunchResult.UnsupportedExecutable;
        }

        // Launch the game
        var configuration = GenerateHookConfiguration(type, isDebug);
        StartWindowsLauncherViaProton(configuration);
        return GameLaunchResult.Success;
    }

    /// <summary>
    /// Starts Flagrum.Launcher with the launch command for FFXV.
    /// Flagrum.Launcher will handle launching the game and injecting the mod loader.
    /// </summary>
    /// <exception cref="FileNotFoundException">Thrown if the Steam compatibility programs are not found.</exception>
    private void StartWindowsLauncherViaProton(HookConfiguration configuration)
    {
        // Resolve all needed paths
        // TODO: Allow this to be configured
        //       Improve the dynamic resolution when not configured
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var steamRoot = Path.Combine(homePath, ".local", "share", "Steam");
        var compatDataPath = Path.Combine(steamRoot, "steamapps", "compatdata", "637650");
        var launchWrapperPath = Path.Combine(steamRoot, "ubuntu12_32", "steam-launch-wrapper");
        var reaperPath = Path.Combine(steamRoot, "ubuntu12_32", "reaper");
        var commonPath = Path.Combine(steamRoot, "steamapps", "common");
        var sniperPath = ResolveSniperPath(commonPath);
        // TODO: This should not be hardcoded
        var protonPath = Path.Combine(steamRoot, "compatibilitytools.d", "Proton-GE Latest", "proton");
        var launcherPath = Path.Combine(IOHelper.GetExecutingDirectory(), "Flagrum.Launcher.exe");

        // Get the DLL path
        var dllPath = Path.Combine(IOHelper.GetExecutingDirectory(), HookDllName);
        if (!File.Exists(dllPath))
        {
            dllPath = Path.Combine(IOHelper.GetExecutingDirectory(), "runtimes", "win-x64", "native", HookDllName);
            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException("Could not find mod loader.");
            }
        }

        // Start the launcher with Proton
        var args = $"-- {reaperPath} SteamLaunch AppId=637650 -- '{sniperPath}' " +
                   $"--verb=waitforexitandrun -- '{protonPath}' waitforexitandrun '{launcherPath}' " +
                   $"--game-path='{profile.Current.GamePath}' --hook-path='{dllPath}' " +
                   $"{configuration.ToCommandlineArgs()}";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{launchWrapperPath} {args}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                EnvironmentVariables =
                {
                    {"PROTON_LOG", "1"},
                    {"STEAM_COMPAT_CLIENT_INSTALL_PATH", steamRoot},
                    {"STEAM_COMPAT_DATA_PATH", compatDataPath},
                    {"SteamAppId", "637650"},
                    {"SteamGameId", "637650"}
                }
            }
        };

        process.Start();
    }

    /// <summary>
    /// Resolves the path to SteamLinuxRuntime_sniper.
    /// </summary>
    /// <param name="commonPath">Path to the common steamapps folder.</param>
    private static string ResolveSniperPath(string commonPath)
    {
        var sniperFolders = Directory.EnumerateDirectories(commonPath)
            .Where(d => d.Split(Path.DirectorySeparatorChar)[^1].StartsWith("SteamLinuxRuntime_sniper"))
            .ToArray();

        return sniperFolders.Length switch
        {
            0 => throw new FileNotFoundException("Could not find path for SteamLinuxRuntime_sniper."),
            > 1 => throw new FileNotFoundException("Could not determine which SteamLinuxRuntime_sniper to use, " +
                                                   "as multiple were detected."),
            _ => Path.Combine(sniperFolders[0], "_v2-entry-point")
        };
    }

    /// <summary>
    /// Creates the hook configuration and stores it in shared memory.
    /// </summary>
    /// <returns>
    /// The shared memory handle which should be disposed when the DLL is finished initializing.
    /// </returns>
    private HookConfiguration GenerateHookConfiguration(GameExecutableType type, bool isDebug)
    {
        // Create the configuration
        var configuration = new HookConfiguration
        {
            HostType = type,
            EnableConsole = isDebug
        };

        // Iterate over all active hook feature build instructions
        foreach (var hookInstruction in modManager.Projects
                     .Where(p => modManager.ModsState.GetActive(p.Key))
                     .SelectMany(p => p.Value.Instructions
                         .Where(i => i is EnableHookFeatureBuildInstruction))
                     .Cast<EnableHookFeatureBuildInstruction>())
        {
            // Enable the feature associated with this instruction
            configuration[hookInstruction.Feature] = true;
        }

        return configuration;
    }
}