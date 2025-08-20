using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace Flagrum.Application.Features.ModManager.Launcher.PInvoke;

/// <summary>
/// Platform invocation methods for invoking methods from kernel32.dll.
/// </summary>
public static class Kernel32
{
    public const int ERROR_ACCESS_DENIED = 5;
    public const uint INFINITE = 0xFFFFFFFF;
    
    [Flags]
    public enum AllocationType : uint
    {
        Commit = 0x1000u,
        Reserve = 0x2000u,
        Decommit = 0x4000u,
        Release = 0x8000u,
        Reset = 0x80000u,
        Physical = 0x400000u,
        TopDown = 0x100000u,
        WriteWatch = 0x200000u,
        LargePages = 0x20000000u
    }

    [Flags]
    public enum MemoryProtection : uint
    {
        Execute = 0x10u,
        ExecuteRead = 0x20u,
        ExecuteReadWrite = 0x40u,
        ExecuteWriteCopy = 0x80u,
        NoAccess = 1u,
        ReadOnly = 2u,
        ReadWrite = 4u,
        WriteCopy = 8u,
        GuardModifierflag = 0x100u,
        NoCacheModifierflag = 0x200u,
        WriteCombineModifierflag = 0x400u
    }

    [Flags]
    public enum ProcessCreationFlags : uint
    {
        CREATE_SUSPENDED = 0x00000004
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CreateProcess(
        string lpApplicationName,
        string? lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        ProcessCreationFlags dwCreationFlags,
        IntPtr lpEnvironment,
        string? lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint ResumeThread(IntPtr hThread);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize,
        AllocationType flAllocationType, MemoryProtection flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer,
        uint dwSize, out UIntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
        IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    public struct STARTUPINFO
    {
        public uint cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }
}