using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class SequenceContainer : Entity
{
    public new const string ClassFullName = "SQEX.Ebony.Framework.Sequence.SequenceContainer";
    private readonly List<int> nodeIndexListAtLoading = new();
    private int nextConnectorIndex_;
    private int nextGroupIndex_;
    private int nextNodeIndex_;

    public SequenceContainer(DataItem parent, DataType dataType)
        : base(parent, dataType)
    {
        createGroupArray();
        createConnectorArray();
    }

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

    public DynamicArray AllItemsRecursive
    {
        get
        {
            var allItems = AllItems;
            foreach (var node in Nodes)
            {
                if (node is TimeLineDataItem timeLineDataItem1)
                {
                    timeLineDataItem1.GetTimeLineItems(allItems);
                }
                else if (node is TrayDataItem trayDataItem)
                {
                    trayDataItem.GetTrayItems(allItems);
                }
            }

            return allItems;
        }
    }

    public DynamicArray Nodes => this["nodes_"] as DynamicArray;

    public DynamicArray Groups => this["groups_"] as DynamicArray;

    public DynamicArray Connectors => this["connectors_"] as DynamicArray;

    public bool IsTemplateInnerSequence { get; set; }

    public bool IsPrefabTopSequence
    {
        get => GetBool("bIsPrefabTopSequence_");
        set => SetBool("bIsPrefabTopSequence_", value);
    }

    public string RuntimeParentTemplateTrayNodePath { get; set; } = "";


    public DynamicArray GetConnectorRecursive(int connectorNo = -1)
    {
        var items = new DynamicArray(null);
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
                    if (((ConnectorDataItem)child).ConnectorNo == connectorNo)
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

        return items;
    }

    public void UpdateAllIndexAtLoading()
    {
        foreach (var node in Nodes)
        {
            UpdateNodeIndexAtLoading(node);
            if (node is TrayDataItem)
            {
                ((TrayDataItem)node).UpdateAllIndexAtLoading();
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
        var num = GetNameIndex(nodeDataItem.Name);
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
        var nameIndex = GetNameIndex(groupDataItem.Name);
        if (nameIndex < nextGroupIndex_)
        {
            return;
        }

        nextGroupIndex_ = nameIndex + 1;
    }

    public void UpdateConnectorIndexAtLoading(DataItem connectorDataItem)
    {
        var nameIndex = GetNameIndex(connectorDataItem.Name);
        if (nameIndex < nextConnectorIndex_)
        {
            return;
        }

        nextConnectorIndex_ = nameIndex + 1;
    }

    private int GetNameIndex(string name)
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