using System.Runtime.InteropServices;
using System.Text;
using Flagrum.Abstractions.ModManager;
using Flagrum.Abstractions.ModManager.Instructions;

namespace Flagrum.Application.Features.ModManager.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HookConfiguration
{
    public GameExecutableType HostType;
    public bool EnableConsole;
    public bool EnableAnselPatch;
    public bool UnlockAdditionalDlc;
    public bool IncreaseSnapshotLimit;

    public bool this[FlagrumHookFeature feature]
    {
        set
        {
            switch (feature)
            {
                case FlagrumHookFeature.EnableConsole:
                    EnableConsole = value;
                    break;
                case FlagrumHookFeature.EnableAnsel:
                    EnableAnselPatch = value;
                    break;
                case FlagrumHookFeature.UnlockAdditionalDlc:
                    UnlockAdditionalDlc = value;
                    break;
                case FlagrumHookFeature.IncreaseSnapshotLimit:
                    IncreaseSnapshotLimit = value;
                    break;
            }
        }
    }

    public string ToCommandlineArgs()
    {
        var builder = new StringBuilder($"--host-type={(byte)HostType}");
        builder.Append($" --enable-console={EnableConsole}");
        builder.Append($" --enable-ansel-patch={EnableAnselPatch}");
        builder.Append($" --unlock-additional-dlc={UnlockAdditionalDlc}");
        builder.Append($" --increase-snapshot-limit={IncreaseSnapshotLimit}");
        return builder.ToString();
    }
}