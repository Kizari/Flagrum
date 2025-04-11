using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public abstract class ProxyTrayDataItem : TrayDataItem
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Sequence.Tray.ProxyTrayDataItem";

    public ProxyTrayDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType) { }
}