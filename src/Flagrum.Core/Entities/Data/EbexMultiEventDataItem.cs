using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class MultiEventDataItem : TrayDataItem
{
    public new const string ClassFullName =
        "SQEX.Ebony.Framework.Sequence.Action.SequenceActionTimeLineMultiEventTrack";

    public MultiEventDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType) { }
}