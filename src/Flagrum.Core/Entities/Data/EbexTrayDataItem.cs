using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class TrayDataItem : SequenceNodeDataItem
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Sequence.Tray.SequenceTray";
    public static HashSet<string> set = new();
    private readonly List<int> nodeIndexListAtLoading = new();
    private int nextConnectorIndex_;
    private int nextGroupIndex_;
    private int nextNodeIndex_;

    public TrayDataItem(DataItem parent, DataType dataType)
        : base(parent, dataType)
    {
        TemplateTrayItemList = new List<TemplateTrayDataItem>();
        MenuLogicItemList = new List<DataItem>();
        createGroupArray();
        createConnectorArray();
    }

    public DynamicArray Nodes => this["nodes_"] as DynamicArray;

    public DynamicArray Groups => this["groups_"] as DynamicArray;

    public DynamicArray Connectors => this["connectors_"] as DynamicArray;

    public DynamicArray AllItems
    {
        get
        {
            var dynamicArray = new DynamicArray(null);
            if (Nodes != null)
            {
                dynamicArray.Children.AddRange(Nodes);
            }

            if (Groups != null)
            {
                dynamicArray.Children.AddRange(Groups);
            }

            if (Connectors != null)
            {
                dynamicArray.Children.AddRange(Connectors);
            }

            return dynamicArray;
        }
    }

    public List<TemplateTrayDataItem> TemplateTrayItemList { get; set; }

    public List<DataItem> MenuLogicItemList { get; set; }

    public string HeaderColorCss
    {
        get
        {
            var headerColor = GetColor("headerColorUserDefine_");
            return headerColor == default
                ? ""
                : $"background-color: rgb({headerColor.R}, {headerColor.G}, {headerColor.B}) !important;";
        }
    }

    public string BodyColorCss
    {
        get
        {
            var bodyColor = GetColor("bodyColorUserDefine_");
            return bodyColor == default
                ? ""
                : $"background-color: rgb({bodyColor.R}, {bodyColor.G}, {bodyColor.B}) !important;";
        }
    }

    public void GetTrayItems(DynamicArray items)
    {
        items.Children.AddRange(AllItems);
        foreach (var trayDataItem in Nodes.OfType<TrayDataItem>())
        {
            trayDataItem.GetTrayItems(items);
        }
    }

    public void GetTrayConnectorItems(DynamicArray items, int connectorNo = -1)
    {
        if (Connectors != null)
        {
            if (connectorNo < 0)
            {
                items.Children.AddRange(Connectors);
            }
            else
            {
                foreach (var child in Connectors.Children)
                {
                    if (((ConnectorDataItem) child).ConnectorNo == connectorNo)
                    {
                        items.Children.Add(child);
                    }
                }
            }
        }

        foreach (var trayDataItem in Nodes.OfType<TrayDataItem>())
        {
            trayDataItem.GetTrayConnectorItems(items, connectorNo);
        }
    }

    public virtual void UpdateAllIndexAtLoading()
    {
        foreach (var node in Nodes)
        {
            UpdateNodeIndexAtLoading(node);
            if (node is TrayDataItem)
            {
                ((TrayDataItem) node).UpdateAllIndexAtLoading();
            }
        }

        if (Groups != null)
        {
            foreach (var group in Groups)
            {
                UpdateGroupIndexAtLoading(group);
            }
        }

        if (Connectors != null)
        {
            foreach (var connector in Connectors)
            {
                UpdateConnectorIndexAtLoading(connector);
            }
        }

        nodeIndexListAtLoading.Clear();
    }

    public void UpdateNodeIndexAtLoading(DataItem nodeDataItem)
    {
        var num = getNameIndex(nodeDataItem.Name);
        if (DocumentInterface.Configuration != null && DocumentInterface.Configuration.IsModifyTrayNodeIndexMode)
        {
            if (nodeIndexListAtLoading.Contains(num))
            {
                if (DocumentInterface.DocumentContainer != null && ParentPackage != null)
                {
                    ParentPackage.ModifiedTrayNodeIndexAtLoading = true;
                }

                num = nextNodeIndex_;
                nodeDataItem.ChangeNameAtLoadingSequence("[" + num + "]");
                if (nodeDataItem.ParentPackage != null)
                {
                    nodeDataItem.ParentPackage.Modified = true;
                }
            }

            nodeIndexListAtLoading.Add(num);
        }

        if (num < nextNodeIndex_)
        {
            return;
        }

        nextNodeIndex_ = num + 1;
    }


    public void UpdateGroupIndexAtLoading(DataItem groupDataItem)
    {
        var nameIndex = getNameIndex(groupDataItem.Name);
        if (nameIndex < nextGroupIndex_)
        {
            return;
        }

        nextGroupIndex_ = nameIndex + 1;
    }

    public void UpdateConnectorIndexAtLoading(DataItem connectorDataItem)
    {
        var nameIndex = getNameIndex(connectorDataItem.Name);
        if (nameIndex < nextConnectorIndex_)
        {
            return;
        }

        nextConnectorIndex_ = nameIndex + 1;
    }

    private int getNameIndex(string name)
    {
        if (string.IsNullOrEmpty(name) || !name.StartsWith("["))
        {
            return -1;
        }

        return int.Parse(name.Trim('[', ']'));
    }

    public int UseNodeIndex()
    {
        return nextNodeIndex_++;
    }

    public int UseGroupIndex()
    {
        return nextGroupIndex_++;
    }

    public int UseConnectorIndex()
    {
        return nextConnectorIndex_++;
    }
}