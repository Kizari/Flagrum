using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class MenuLogicTrayDataItem : TrayDataItem
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Sequence.Tray.MenuLogicTray";

    public MenuLogicTrayDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType) { }
}