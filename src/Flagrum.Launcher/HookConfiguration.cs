using System.Runtime.InteropServices;

namespace Flagrum.Launcher;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HookConfiguration
{
    public byte HostType;
    public bool EnableConsole;
    public bool EnableAnselPatch;
    public bool UnlockAdditionalDlc;
    public bool IncreaseSnapshotLimit;

    public static HookConfiguration FromCommandlineArgs(string[] args)
    {
        var map = args
            .Select(a => a.Split('='))
            .ToDictionary(a => a[0], a => a[1]);

        return new HookConfiguration
        {
            HostType = byte.Parse(map["--host-type"]),
            EnableConsole = bool.Parse(map["--enable-console"]),
            EnableAnselPatch = bool.Parse(map["--enable-ansel-patch"]),
            UnlockAdditionalDlc = bool.Parse(map["--unlock-additional-dlc"]),
            IncreaseSnapshotLimit = bool.Parse(map["--increase-snapshot-limit"])
        };
    }
}