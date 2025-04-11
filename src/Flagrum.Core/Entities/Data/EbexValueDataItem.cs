using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class ValueDataItem : DataItem
{
    public ValueDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public ValueDataItem(DataItem parent, string typeFullName)
        : this(parent, typeFullName != null ? DocumentInterface.ModuleContainer[typeFullName] : null) { }
}