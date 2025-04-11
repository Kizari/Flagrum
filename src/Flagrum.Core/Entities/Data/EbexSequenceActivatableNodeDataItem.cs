using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class SequenceActivatableNodeDataItem : SequenceNodeDataItem
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Sequence.SequenceActivatableNode";

    public SequenceActivatableNodeDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType) { }
}