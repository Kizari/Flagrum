using Flagrum.Core.Scripting.Ebex.Configuration;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class TemplateTrayDataItem : ProxyTrayDataItem
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Sequence.Tray.TemplateTray";

    public TemplateTrayDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public string SourcePath
    {
        get => Project.GetDataFullPath(GetString("sourcePath_"));
        set
        {
            if (!(GetString("sourcePath_") != value))
            {
                return;
            }

            SetString("sourcePath_", Project.GetDataRelativePath(value));
        }
    }

    public string SourceRelativePath => GetString("sourcePath_");

    public EntityPackage SourceEntityPackage { get; set; }

    public override void UpdateAllIndexAtLoading() { }
}