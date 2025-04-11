using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class SequenceDebugNameTray : TrayDataItem
{
    public new const string ClassFullName = "SQEX.Luminous.GameFramework.Sequence.SequenceDebugNameTray";

    public SequenceDebugNameTray(DataItem parent, DataType dataType)
        : base(parent, dataType) { }
}