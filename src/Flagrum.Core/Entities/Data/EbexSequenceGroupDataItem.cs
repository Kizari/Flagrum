using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class SequenceGroupDataItem : Object
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Sequence.Group.SequenceGroupBase";

    public SequenceGroupDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType) { }
}