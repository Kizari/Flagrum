using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class PrefabTrayDataItem : ProxyTrayDataItem
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Sequence.Tray.PrefabTray";

    public PrefabTrayDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType) { }

    public Prefab TargetPrefab => GetPointer("entityPointer_") as Prefab;

    public override void UpdateAllIndexAtLoading() { }
}