using System.Collections.Generic;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class AreaPackage : LoadUnitPackage
{
    public new const string ClassFullName = "Black.Entity.Area.AreaPackage";

    public AreaPackage(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public override string FullFilePath
    {
        get => base.FullFilePath;
        set
        {
            base.FullFilePath = value;
            var result = new List<DataItem>();
            AccumulateEntitiesWithDataType(result,
                DocumentInterface.ModuleContainer["Black.Entity.Area.MapPackage"], true);
            foreach (var dataItem in result)
            {
                if (dataItem.ParentLoadUnitPackage == this)
                {
                    ((MapPackage)dataItem).SetParentPackagePath(this);
                }
            }
        }
    }
}