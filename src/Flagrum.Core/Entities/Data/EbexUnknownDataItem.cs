using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class UnknownDataItem : DataItem
{
    public UnknownDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType) { }
}