namespace Flagrum.Core.Scripting.Ebex.Data;

public class EntityDiff : DynamicArray
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Entity.EntityDiff";

    public EntityDiff(DataItem parent)
        : base(parent) { }
}