using Flagrum.Core.Scripting.Ebex.Configuration;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class MapPackage : LoadUnitPackage
{
    public new const string ClassFullName = "Black.Entity.Area.MapPackage";
    public const string PARENT_PACKAGE_PATH_VARIABLE_NAME = "parentPackagePath_";

    public MapPackage(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public override DataItem Parent
    {
        get => base.Parent;
        set
        {
            base.Parent = value;
            if (ParentLoadUnitPackage == null)
            {
                return;
            }

            SetParentPackagePath(ParentLoadUnitPackage);
        }
    }

    public void SetParentPackagePath(LoadUnitPackage parentLoadUnitPackage)
    {
        var str1 = GetString("parentPackagePath_");
        var dataRelativePath = Project.GetDataRelativePath(ParentPackage.FullFilePath);
        var str2 = dataRelativePath;
        if (!(str1 != str2))
        {
            return;
        }

        SetStringWithoutModifiedFlag("parentPackagePath_", dataRelativePath);
    }
}