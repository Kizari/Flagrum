using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class CSharpProjectPackage : EntityPackage
{
    public new const string ClassFullName = "Black.Entity.CSharp.CSharpProjectPackage";

    public CSharpProjectPackage(DataItem parent, DataType dataType)
        : base(parent, dataType) { }
}