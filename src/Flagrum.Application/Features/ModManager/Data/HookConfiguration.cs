using System.Runtime.InteropServices;
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
}