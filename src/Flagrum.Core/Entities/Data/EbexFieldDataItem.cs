using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class FieldDataItem : DataItem
{
    public FieldDataItem(DataItem parent, Field field)
        : base(parent, field) { }
}