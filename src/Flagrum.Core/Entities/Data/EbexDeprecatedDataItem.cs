using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class DeprecatedDataItem : DataItem
{
    public DeprecatedDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType) { }
}