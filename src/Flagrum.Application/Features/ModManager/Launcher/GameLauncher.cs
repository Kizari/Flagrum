using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Flagrum.Abstractions;
using Flagrum.Abstractions.ModManager;
using Flagrum.Core.Utilities;
using Flagrum.Generators;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Launcher.PInvoke;
using Flagrum.Application.Features.ModManager.Services;
using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Application.Features.ModManager.Launcher;

/// <summary>
/// Handles launching the game and injecting the hook DLL.
/// </summary>
[RegisterSingleton]
public class GameLauncher(
    IProfileService profile,
    IPremiumService premium,
    ModManagerServiceBase modManager)
{
    private const string HookDllName = "Drautos.dll";

    /// <summary>
    /// Launches the game and injects the hook DLL.
    /// </summary>
    /// <param name="isDebug">Whether debug options should be enabled in the DLL configuration before injecting.</param>
    /// <exception cref="Win32Exception">
    /// Thrown if any Windows errors occur during the launch or injection process.
    /// </exception>
    /// <returns><c>false</c> if the game was already running, otherwise <c>true</c>.</returns>
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

        // Start the game process in a suspended state
        // Need to use P/Invoke to start the process because Process.Start can't CREATE_SUSPENDED
        var startupInfo = new Kernel32.STARTUPINFO();

        if (!Kernel32.CreateProcess(
                profile.Current.GamePath,
                null,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                Kernel32.ProcessCreationFlags.CREATE_SUSPENDED,
                IntPtr.Zero,
                null,
                ref startupInfo,
                out var processInfo))
        {
            var error = Marshal.GetLastWin32Error();
            if (error == Kernel32.ERROR_ACCESS_DENIED)
            {
                return GameLaunchResult.AccessDenied;
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // Generate the configuration for the DLL
        using var configuration = GenerateHookConfiguration(type, isDebug);

        // Inject the DLL
        var process = Process.GetProcessById((int)processInfo.dwProcessId);
        Inject(process);

        // Now that the DLL is injected, resume the game process
        Kernel32.ResumeThread(processInfo.hThread);

        return GameLaunchResult.Success;
    }

    /// <summary>
    /// Injects the DLL with the given name into the given process.
    /// </summary>
    /// <param name="process">The game process to inject the DLL into.</param>
    private void Inject(Process process)
    {
        // Get address of LoadLibraryA
        var kernel32 = Kernel32.GetModuleHandle("kernel32.dll");
        var loadLibraryAddress = Kernel32.GetProcAddress(kernel32, "LoadLibraryA");
        if (loadLibraryAddress == IntPtr.Zero)
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new ApplicationException($"Failed to get address of LoadLibraryA. (0x{errorCode:x})");
        }

        // Allocate memory for DLL path
        var dllPath = Path.Combine(IOHelper.GetExecutingDirectory(), HookDllName);
        if (!File.Exists(dllPath))
        {
            dllPath = Path.Combine(IOHelper.GetExecutingDirectory(), "runtimes", "win-x64", "native", HookDllName);
            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException("Could not find mod loader.");
            }
        }
        
        var pathSize = (uint)((dllPath.Length + 1) * Marshal.SizeOf<char>());
        var pathAddress = Kernel32.VirtualAllocEx(process.Handle, IntPtr.Zero, pathSize,
            Kernel32.AllocationType.Commit | Kernel32.AllocationType.Reserve,
            Kernel32.MemoryProtection.ReadWrite);

        if (pathAddress == IntPtr.Zero)
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new ApplicationException($"Failed to allocate memory for DLL path. (0x{errorCode:x})");
        }

        // Write DLL path into memory in the host process
        var buffer = Encoding.Default.GetBytes(dllPath);
        if (!Kernel32.WriteProcessMemory(process.Handle, pathAddress, buffer, pathSize, out var bytesWritten)
            || bytesWritten.ToInt32() != buffer.Length + 1)
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new ApplicationException($"Failed to write the DLL path. (0x{errorCode:x})");
        }

        // Create thread in host process to load the DLL
        var remoteThreadHandle = Kernel32.CreateRemoteThread(process.Handle, IntPtr.Zero,
            0u, loadLibraryAddress, pathAddress, 0u, IntPtr.Zero);
        if (remoteThreadHandle == IntPtr.Zero)
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new ApplicationException($"Failed to load DLL on remote thread. (0x{errorCode:x})");
        }

        // Wait for the remote thread to complete
        Kernel32.WaitForSingleObject(remoteThreadHandle, 0xFFFFFFFF);
        Kernel32.CloseHandle(remoteThreadHandle);
    }

    /// <summary>
    /// Creates the hook configuration and stores it in shared memory.
    /// </summary>
    /// <returns>
    /// The shared memory handle which should be disposed when the DLL is finished initializing.
    /// </returns>
    private IDisposable GenerateHookConfiguration(GameExecutableType type, bool isDebug)
    {
        // Create the configuration
        var size = Unsafe.SizeOf<HookConfiguration>();
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

        // Write the configuration struct to shared memory
        var memoryMappedFile = MemoryMappedFile.CreateOrOpen("DrautosConfiguration", size);
        using var accessor = memoryMappedFile.CreateViewAccessor();
        var structSpan = new Span<HookConfiguration>([configuration]);
        var buffer = MemoryMarshal.Cast<HookConfiguration, byte>(structSpan);
        accessor.WriteArray(0, buffer.ToArray(), 0, size);

        return memoryMappedFile;
    }
}