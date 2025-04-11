using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class TimeLineDataItem2 : TimeLineDataItem
{
    public new const string ClassFullName = "Black.Sequence.Action.TimeLine.SequenceActionTimeLineBlack";

    public TimeLineDataItem2(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public override void GetTimeLineItems(DynamicArray items)
    {
        items.Children.AddRange(AllItems);
    }
}