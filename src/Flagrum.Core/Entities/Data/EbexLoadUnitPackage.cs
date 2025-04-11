using System.Linq;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class LoadUnitPackage : EntityPackage
{
    public new const string ClassFullName = "Black.Entity.Area.LoadUnitPackage";

    public LoadUnitPackage(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public override bool StartupLoad
    {
        get => GetBool("startupLoad_");
        set => SetBoolWithoutModifyFlag("startupLoad_", value);
    }

    public override bool CanEditStartupLoad => true;

    public override bool StartupLoadAtSaveingAsTopPackage => false;

    public float[] AABBMin
    {
        get => GetFloat4("min_");
        set
        {
            if (GetFloat4("min_").SequenceEqual(value))
            {
                return;
            }

            SetFloat4WithoutModified("min_", value);
        }
    }

    public float[] AABBMax
    {
        get => GetFloat4("max_");
        set
        {
            if (GetFloat4("max_").SequenceEqual(value))
            {
                return;
            }

            SetFloat4WithoutModified("max_", value);
        }
    }
}