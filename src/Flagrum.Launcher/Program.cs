using System.ComponentModel;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Flagrum.Launcher;

internal static class Program
{
    private static int Main(string[] args)
    {
        var map = args
            .Select(a => a.Split('='))
            .ToDictionary(a => a[0], a => a[1]);

        // Start the game process in a suspended state
        // Need to use P/Invoke to start the process because Process.Start can't CREATE_SUSPENDED
        var startupInfo = new Kernel32.STARTUPINFO();
        if (!Kernel32.CreateProcess(
                map["--game-path"],
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
                return (int)GameLaunchResult.AccessDenied;
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // Generate the configuration for the DLL
        using var configuration = GenerateHookConfiguration(args);

        // Inject the DLL
        var process = Process.GetProcessById((int)processInfo.dwProcessId);
        Inject(process, map["--hook-path"]);

        // Now that the DLL is injected, resume the game process
        Kernel32.ResumeThread(processInfo.hThread);
        return (int)GameLaunchResult.Success;
    }

    /// <summary>
    /// Injects the DLL with the given name into the given process.
    /// </summary>
    /// <param name="process">The game process to inject the DLL into.</param>
    private static void Inject(Process process, string hookPath)
    {
        // Get address of LoadLibraryW
        var kernel32 = Kernel32.GetModuleHandle("kernel32.dll");
        var loadLibraryAddress = Kernel32.GetProcAddress(kernel32, "LoadLibraryW");
        if (loadLibraryAddress == IntPtr.Zero)
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode);
        }

        // Allocate memory for DLL path
        var dllPathBuffer = Encoding.Unicode.GetBytes(hookPath + '\0');
        var pathAddress = Kernel32.VirtualAllocEx(process.Handle, IntPtr.Zero, (uint)dllPathBuffer.Length,
            Kernel32.AllocationType.Commit | Kernel32.AllocationType.Reserve,
            Kernel32.MemoryProtection.ReadWrite);

        if (pathAddress == IntPtr.Zero)
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode);
        }

        // Write DLL path into memory in the host process
        if (!Kernel32.WriteProcessMemory(process.Handle, pathAddress, dllPathBuffer, (uint)dllPathBuffer.Length,
                out var bytesWritten))
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode);
        }

        if (bytesWritten.ToUInt64() != (ulong)dllPathBuffer.Length)
        {
            throw new ApplicationException($"Failed to write DLL path {hookPath}." +
                                           $"Only wrote {bytesWritten.ToUInt64()}/{dllPathBuffer.Length} bytes.");
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
        Kernel32.WaitForSingleObject(remoteThreadHandle, Kernel32.INFINITE);
        Kernel32.CloseHandle(remoteThreadHandle);
    }

    /// <summary>
    /// Creates the hook configuration and stores it in shared memory.
    /// </summary>
    /// <returns>
    /// The shared memory handle which should be disposed when the DLL is finished initializing.
    /// </returns>
    private static IDisposable GenerateHookConfiguration(string[] args)
    {
        // Create the configuration
        var size = Unsafe.SizeOf<HookConfiguration>();
        var configuration = HookConfiguration.FromCommandlineArgs(args);

        // Write the configuration struct to shared memory
        var memoryMappedFile = MemoryMappedFile.CreateOrOpen("DrautosConfiguration", size);
        using var accessor = memoryMappedFile.CreateViewAccessor();
        var structSpan = new Span<HookConfiguration>([configuration]);
        var buffer = MemoryMarshal.Cast<HookConfiguration, byte>(structSpan);
        accessor.WriteArray(0, buffer.ToArray(), 0, size);

        return memoryMappedFile;
    }
}