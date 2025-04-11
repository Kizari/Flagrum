using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class AIGraphContainer : Entity
{
    public new const string ClassFullName = "SQEX.Ebony.AIGraph.Core.AIGraphContainer";

    public AIGraphContainer(DataItem parent, DataType dataType)
        : base(parent, dataType) { }
}