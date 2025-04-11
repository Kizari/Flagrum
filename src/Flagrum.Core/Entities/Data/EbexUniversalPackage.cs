using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class UniversalPackage : LoadUnitPackage
{
    public new const string ClassFullName = "Black.Entity.Area.UniversalPackage";

    public UniversalPackage(DataItem parent, DataType dataType)
        : base(parent, dataType) { }
}