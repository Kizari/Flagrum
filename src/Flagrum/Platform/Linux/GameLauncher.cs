using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Flagrum.Abstractions;
using Flagrum.Abstractions.ModManager;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Services;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Core.Utilities;
using Injectio.Attributes;

namespace Flagrum.Platform.Linux;

/// <inheritdoc />
[RegisterSingleton<IGameLauncher>]
public partial class GameLauncher(
    IProfileService profile,
    IPremiumService premium,
    ModManagerServiceBase modManager,
    LaunchConfiguration launchConfig) : IGameLauncher
{
    private const string HookDllName = "Drautos.dll";

    /// <inheritdoc />
    public GameLaunchResult TryLaunch(bool isDebug, string? command = null)
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
        
        // Validate launch configuration if applicable
        if (command == null)
        {
            var context = new ValidationContext(launchConfig);
            if (!Validator.TryValidateObject(launchConfig, context, null, true))
            {
                return GameLaunchResult.InvalidLaunchConfiguration;
            }
        }

        // Launch the game
        var configuration = GenerateHookConfiguration(type, isDebug);
        StartWindowsLauncherViaProton(configuration, command == null
            ? launchConfig.LaunchCommand!
            : AlterSteamCommand(command));
        
        return GameLaunchResult.Success;
    }

    /// <summary>
    /// Starts Flagrum.Launcher with the launch command for FFXV.
    /// Flagrum.Launcher will handle launching the game and injecting the mod loader.
    /// </summary>
    /// <exception cref="FileNotFoundException">Thrown if the Steam compatibility programs are not found.</exception>
    private void StartWindowsLauncherViaProton(HookConfiguration configuration, string command)
    {
        // Get paths to Flagrum components
        var launcherPath = Path.Combine(IOHelper.GetExecutingDirectory(), "Flagrum.Launcher.exe");
        var dllPath = Path.Combine(IOHelper.GetExecutingDirectory(), HookDllName);
        if (!File.Exists(dllPath))
        {
            dllPath = Path.Combine(IOHelper.GetExecutingDirectory(),
                "runtimes", "win-x64", "native", HookDllName);
            
            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException("Could not find mod loader.");
            }
        }

        // Start the launcher with Proton
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-", // Read from stdin
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                EnvironmentVariables =
                {
                    //["PROTON_LOG"] = "1",
                    ["STEAM_COMPAT_CLIENT_INSTALL_PATH"] = launchConfig.SteamRoot,
                    ["STEAM_COMPAT_DATA_PATH"] = launchConfig.ProtonPrefix,
                    ["SteamAppId"] = "637650",
                    ["SteamGameId"] = "637650",
                    ["FLAGRUM_STEAM_LAUNCH_WRAPPER"] = launchConfig.SteamLaunchWrapper,
                    ["FLAGRUM_STEAM_REAPER"] = launchConfig.SteamReaper,
                    ["FLAGRUM_STEAM_RUNTIME"] = launchConfig.SteamRuntime,
                    ["FLAGRUM_PROTON_EXECUTABLE"] = launchConfig.Proton,
                    ["FLAGRUM_LAUNCHER"] = launcherPath,
                    ["FLAGRUM_GAME_EXECUTABLE"] = profile.Current.GamePath,
                    ["FLAGRUM_MOD_LOADER"] = dllPath,
                    ["FLAGRUM_LAUNCHER_CONFIG"] = configuration.ToCommandlineArgs()
                }
            }
        };

        // Send the command via stdin, done this way to prevent nightmares with nested quotes if using `-c "{command}"`
        process.Start();
        process.StandardInput.Write(command);
        process.StandardInput.Close();
    }

    /// <summary>
    /// Creates the hook configuration.
    /// </summary>
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

    /// <summary>
    /// Replaces the game executable path in the Steam launch %command% with the Flagrum Launcher
    /// executable path and respective commandline arguments.
    /// </summary>
    private static string AlterSteamCommand(string command) => ExecutablePathRegex()
        .Replace(command, "\"$FLAGRUM_LAUNCHER\" " +
                          "--game-path=\"$FLAGRUM_GAME_EXECUTABLE\" " +
                          "--hook-path=\"$FLAGRUM_MOD_LOADER\" " +
                          "$FLAGRUM_LAUNCHER_CONFIG");

    [GeneratedRegex(@"'([^']+\.exe)'$")]
    private static partial Regex ExecutablePathRegex();
}