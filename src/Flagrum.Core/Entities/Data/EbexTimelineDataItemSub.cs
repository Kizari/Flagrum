using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class TimeLineDataItemSub : TimeLineDataItem
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Sequence.Action.SequenceActionTimeLineSub";

    public TimeLineDataItemSub(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public override void GetTimeLineItems(DynamicArray items)
    {
        items.Children.AddRange(AllItems);
    }
}