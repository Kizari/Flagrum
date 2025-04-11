using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class WorldPackage : LoadUnitPackage
{
    public new const string ClassFullName = "Black.Entity.Area.WorldPackage";

    public WorldPackage(DataItem parent, DataType dataType)
        : base(parent, dataType) { }
}