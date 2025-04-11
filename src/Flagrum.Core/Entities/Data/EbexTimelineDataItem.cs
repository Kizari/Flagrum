using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class TimeLineDataItem : TrayDataItem
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Sequence.Action.SequenceActionTimeLineBase";
    public static readonly string TrackClassFullName = "SQEX.Ebony.Framework.TimeControl.TimeLine.Track";

    public TimeLineDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public virtual void GetTimeLineItems(DynamicArray items)
    {
        items.Children.AddRange(AllItems);
    }
}