using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class ConnectorDataItem : Object
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Sequence.Connector.SequenceConnectorBase";
    public const string ClassFullNameIn = "SQEX.Ebony.Framework.Sequence.Connector.SequenceConnectorIn";
    public const string ClassFullNameOut = "SQEX.Ebony.Framework.Sequence.Connector.SequenceConnectorOut";

    public ConnectorDataItem(DataItem parent, string typeFullName)
        : base(parent, typeFullName)
    {
        createCommentItem();
    }

    public ConnectorDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType)
    {
        createCommentItem();
    }

    public int ConnectorNo
    {
        get => GetInt("connectorNo_");
        set => SetInt("connectorNo_", value);
    }
}