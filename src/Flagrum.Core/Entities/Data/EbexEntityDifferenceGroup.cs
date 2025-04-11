namespace Flagrum.Core.Scripting.Ebex.Data;

public class EntityDiffGroup : DynamicArray
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Entity.EntityDiffGroup";
    public const string VersionStr = "20";

    public EntityDiffGroup(DataItem parent)
        : base(parent) { }

    public DataItem GetDiffTargetItem()
    {
        foreach (var child in Children)
        {
            if (child.Name == "diffGroupPath")
            {
                var s = child.Value.ToString();
                if (ParentPackage is Prefab parentPackage3)
                {
                    return parentPackage3.GetChild(new ItemPath(s));
                }

                break;
            }
        }

        return null;
    }
}