using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Flagrum.Core.Scripting.Ebex.Type;
using Enum = Flagrum.Core.Scripting.Ebex.Type.Enum;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class DataItemExpandableConverter : ExpandableObjectConverter
{
    public override PropertyDescriptorCollection GetProperties(
        ITypeDescriptorContext context,
        object value,
        Attribute[] attributes)
    {
        var properties = base.GetProperties(context, value, attributes);
        var propertyDescriptorList = new List<PropertyDescriptor>(properties.Count);
        foreach (var obj in properties)
        {
            var flag = true;
            var propertyDescriptor = obj as PropertyDescriptor;
            if (flag)
            {
                propertyDescriptorList.Add(propertyDescriptor);
            }
        }

        return new PropertyDescriptorCollection(propertyDescriptorList.ToArray());
    }
}

[TypeConverter(typeof(DataItemExpandableConverter))]
public class DataItem : IItem, IDisposable, IEnumerable<DataItem>, IEnumerable
{
    public enum BreakPointType
    {
        BPT_ON_VALID,
        BPT_ON_INVALID,
        BPT_OFF
    }

    public enum RuntimeOverrideType
    {
        ROT_NONE,
        ROT_TRUE,
        ROT_FALSE
    }

    private BreakPointType breakPointType_ = BreakPointType.BPT_OFF;
    private bool browsable = true;
    protected DataType dataType;
    protected bool disposed;
    private bool earcModified_;
    protected Field field;
    private bool isDefault = true;
    private bool isExpanded;
    private bool modifed;
    private bool modifiedLocked_;
    public string name;
    private DataItem parent;
    private RuntimeOverrideType runtimeBrowsable;
    private PrimitiveType runtimeOverridePrimitiveType_;
    private Field.AutoExpandPropertyState runtimePropertyExpandedState;
    private RuntimeOverrideType runtimeReadOnly;
    private IItem value;

    protected DataItem(DataItem parent, DataType dataType)
    {
        this.dataType = dataType;
        this.parent = parent;
        setupDefaultValue(this.dataType);
        setupBrowsable();
        parent?.Add(this);
    }

    public ILucentGraphItem GraphItem { get; set; }

    public bool Disposed => disposed;

    public string Name
    {
        get => name;
        set
        {
            if (this is Entity && string.IsNullOrEmpty(value))
            {
                // DocumentInterface.DocumentContainer.LogAppendCurrentParagraphParse(
                //     "<Span Foreground=\"Red\">Entityの名前は空にできません</Span><LineBreak />");
            }
            else if (name == null || !name.StartsWith("["))
            {
                if (this is Entity && DocumentInterface.DocumentContainer != null)
                {
                    changeUniqueEntityName(value);
                }
                else
                {
                    if (!(name != value))
                    {
                        return;
                    }

                    name = value;
                    Modified = true;
                }
            }
            // DocumentInterface.DocumentContainer.LogAppendCurrentParagraphParse(
            //     "<Span Foreground=\"Red\">EbonyEditor : Setting new Name [X] : name=" + name + " value=" +
            //     value + "</Span><LineBreak />");
        }
    }

    public float[] Position
    {
        get
        {
            var result = GetFloat4("position_");
            return result == null ? new[] {0f, 0f, 0f} : result[..3];
        }
    }

    public float[] Rotation
    {
        get
        {
            var result = GetFloat4("rotation_");
            return result == null ? new[] {0f, 0f, 0f} : result[..3];
        }
    }

    public float Scaling
    {
        get
        {
            var result = GetFloat("scaling_");
            return result == 0f ? 1f : result;
        }
    }

    public string DisplayName
    {
        get
        {
            if (field != null)
            {
                return field.DisplayName;
            }

            return name.StartsWith('[') ? DataTypeDisplayName : name;
        }
        set
        {
            if (field == null)
            {
                return;
            }

            field.DisplayName = value;
        }
    }

    public string Category
    {
        get
        {
            if (field != null && field.Category != null)
            {
                return field.Category;
            }

            return dataType != null ? dataType.Category : null;
        }
    }

    public string Description
    {
        get
        {
            if (field != null && field.Description != null)
            {
                return field.Description;
            }

            return dataType != null ? dataType.Description : null;
        }
    }

    public string IconName
    {
        get
        {
            string str = null;
            if (DataType != null)
            {
                str = DataType.IconName;
            }

            return str ?? "Document";
        }
    }

    public IItem Value
    {
        get => value;
        set
        {
            var obj1 = this.value as Value;
            this.value = value;
            if (obj1 != null && obj1.PrimitiveType == PrimitiveType.Pointer && obj1.Object is Entity entity1)
            {
                entity1.RemoveReferencedItem(this);
            }

            if (!(value is Value))
            {
                return;
            }

            var obj2 = value as Value;
            if (obj2.PrimitiveType != PrimitiveType.Pointer || !(obj2.Object is Entity entity2))
            {
                return;
            }

            entity2.AddReferencedItem(this);
        }
    }

    public IItem DefaultValue => dataType != null ? dataType.DefaultValue : null;

    public IItem RuntimeOverrideMaxValue { get; set; }

    public IItem RuntimeOverrideMinValue { get; set; }

    public PrimitiveType RuntimeOverridePrimitiveType
    {
        get => runtimeOverridePrimitiveType_;
        set
        {
            if (runtimeOverridePrimitiveType_ == value)
            {
                return;
            }

            runtimeOverridePrimitiveType_ = value;
        }
    }

    public DataType DataType => dataType;

    public Field Field
    {
        get => field;
        set
        {
            field = value;
            browsable = true;
            setupBrowsable();
        }
    }

    public bool IsDynamic { get; set; }

    public bool IsPropertyPin { get; set; }

    public bool IsImportComponentProperty { get; set; }

    public string ImportComponentClassName { get; set; } = "";

    public bool ReadOnly
    {
        get
        {
            if (RuntimeReadOnly != RuntimeOverrideType.ROT_NONE)
            {
                return RuntimeReadOnly == RuntimeOverrideType.ROT_TRUE;
            }

            if (field != null && field.ReadOnly)
            {
                return true;
            }

            return dataType != null && dataType.ReadOnly;
        }
        set
        {
            if (field == null)
            {
                return;
            }

            field.ReadOnly = value;
        }
    }

    public RuntimeOverrideType RuntimeReadOnly
    {
        get => runtimeReadOnly;
        set
        {
            if (runtimeReadOnly == value)
            {
                return;
            }

            runtimeReadOnly = value;
        }
    }

    public bool Browsable
    {
        get
        {
            if (RuntimeBrowsable == RuntimeOverrideType.ROT_NONE)
            {
                return browsable;
            }

            return RuntimeBrowsable == RuntimeOverrideType.ROT_TRUE;
        }
        set
        {
            browsable = value;
            setupBrowsable();
        }
    }

    public RuntimeOverrideType RuntimeBrowsable
    {
        get => runtimeBrowsable;
        set
        {
            if (runtimeBrowsable == value)
            {
                return;
            }

            runtimeBrowsable = value;
        }
    }

    public bool IsChecked { get; set; } = true;

    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (isExpanded == value)
            {
                return;
            }

            isExpanded = value;
        }
    }

    public DynamicArray TriInPorts => this["triInPorts_"] as DynamicArray;

    public DynamicArray TriOutPorts => this["triOutPorts_"] as DynamicArray;

    public DynamicArray RefInPorts => this["refInPorts_"] as DynamicArray;

    public DynamicArray RefOutPorts => this["refOutPorts_"] as DynamicArray;

    public DataItem[] AllPorts
    {
        get
        {
            var dataItemList = new List<DataItem>();
            if (RefInPorts != null && RefOutPorts != null)
            {
                dataItemList.AddRange(RefInPorts);
                dataItemList.AddRange(RefOutPorts);
                if (TriInPorts != null && TriOutPorts != null)
                {
                    dataItemList.AddRange(TriInPorts);
                    dataItemList.AddRange(TriOutPorts);
                }
            }

            return dataItemList.ToArray();
        }
    }

    public string CenterTextTarget
    {
        get
        {
            if (Browsable && field != null)
            {
                var attribute = field.GetAttribute(nameof(CenterTextTarget));
                if (attribute != null)
                {
                    return attribute;
                }
            }

            var dataType = this.dataType;
            return null;
        }
    }

    public string CenterTextBoolTrue
    {
        get
        {
            if (Browsable && field != null)
            {
                var attribute = field.GetAttribute(nameof(CenterTextBoolTrue));
                if (attribute != null)
                {
                    return attribute;
                }
            }

            var dataType = this.dataType;
            return null;
        }
    }

    public string CenterTextCommand
    {
        get
        {
            if (field != null && Browsable && field.CenterTextCommand != null)
            {
                return field.CenterTextCommand;
            }

            var dataType = this.dataType;
            return null;
        }
    }

    public string CenterText { get; set; }

    public string Comment
    {
        get
        {
            var str1 = "";
            var flag = false;
            var str2 = "";
            foreach (var child in Children)
            {
                if (child.SpecialType == nameof(Comment))
                {
                    if (flag)
                    {
                        str2 += "\n";
                    }

                    str2 += ((Value) child.value).ToString();
                    flag = true;
                }
            }

            var dataItem = this["comment_"];
            if (dataItem != null)
            {
                str1 = ((Value) dataItem.Value).ToString();
                if (flag)
                {
                    str1 = str1 + "\n" + str2;
                }
            }

            return str1;
        }
    }

    public BreakPointType BreakPoint
    {
        get => breakPointType_;
        set
        {
            if (breakPointType_ == value)
            {
                return;
            }

            breakPointType_ = value;
        }
    }

    public Field.AutoExpandPropertyState RuntimePropertyExpandedState
    {
        get => runtimePropertyExpandedState;
        set
        {
            if (runtimePropertyExpandedState == value)
            {
                return;
            }

            runtimePropertyExpandedState = value;
        }
    }

    public virtual string AlternativeName => "";

    public bool IsDependencyPathItem => DependencyPath || DependencyFolderPath;

    public bool DependencyPath => field != null && field.DependencyPath;

    public bool DependencyFolderPath => field != null && field.DependencyFolderPath;

    public string SpecialType
    {
        get
        {
            if (field != null && field.SpecialType != null)
            {
                return field.SpecialType;
            }

            var dataType = this.dataType;
            return null;
        }
    }

    public bool IsDefault
    {
        get => isDefault;
        set
        {
            isDefault = value;
            if (parent == null || (!(parent is DynamicArray) && !parent.IsDynamic))
            {
                return;
            }

            if (!value)
            {
                parent.IsDefault = false;
            }
            else
            {
                var flag = true;
                foreach (var child in parent.Children)
                {
                    flag &= child.isDefault;
                }

                if (!flag)
                {
                    return;
                }

                parent.IsDefault = true;
            }
        }
    }

    public bool Modified
    {
        get => modifed;
        set => setModifiedInternal(value);
    }

    public bool EarcModified
    {
        get => earcModified_;
        set
        {
            if (disposed || ModifiedLocked)
            {
                return;
            }

            if (value)
            {
                if (value == earcModified_)
                {
                    return;
                }

                earcModified_ = value;
                if (this is EntityPackage && !(this is Prefab))
                {
                    if (DocumentInterface.DocumentContainer == null)
                    {
                        return;
                    }

                    DocumentInterface.DocumentContainer.doOnEntityPackageEarcModifiedChanged(this as EntityPackage);
                }
                else
                {
                    if (parent == null)
                    {
                        return;
                    }

                    parent.EarcModified = value;
                }
            }
            else
            {
                foreach (var child in Children)
                {
                    if (child.Parent == this && (!(child is EntityPackage) || child is Prefab))
                    {
                        child.EarcModified = value;
                    }
                }

                if (value == earcModified_)
                {
                    return;
                }

                earcModified_ = value;
                if (!(this is EntityPackage) || this is Prefab || DocumentInterface.DocumentContainer == null)
                {
                    return;
                }

                DocumentInterface.DocumentContainer.doOnEntityPackageEarcModifiedChanged(this as EntityPackage);
            }
        }
    }

    public bool ModifiedLocked
    {
        get => modifiedLocked_;
        set
        {
            if (value == modifiedLocked_ || disposed)
            {
                return;
            }

            modifiedLocked_ = value;
            foreach (var child in Children)
            {
                if (!(child is EntityPackage) || child is Prefab)
                {
                    child.ModifiedLocked = value;
                }
            }
        }
    }

    public bool ModifiedOnVCS { get; set; }

    public bool IsNameModified { get; private set; }

    public int WakeUpCurveEditorByDoubleClick
    {
        get
        {
            if (field != null)
            {
                return field.WakeUpCurveEditorByDoubleClick;
            }

            return dataType != null ? dataType.WakeUpCurveEditorByDoubleClick : -1;
        }
    }

    private bool hasTimeLine
    {
        get
        {
            if (field != null)
            {
                return field.TimeLine;
            }

            return dataType != null && dataType.TimeLine;
        }
    }

    public bool TimeLineExposable => field != null && field.TimeLineExposable;

    public bool DataGrid
    {
        get
        {
            if (field != null)
            {
                return field.DataGrid;
            }

            return dataType != null && dataType.DataGrid;
        }
    }

    public virtual DataItem Parent
    {
        get => parent;
        set
        {
            if (parent == this)
            {
                return;
            }

            parent = value;
        }
    }

    public ItemPath ParentItemPath => parent == null ? new ItemPath() : parent.FullPath;

    public EntityPackage ParentPackage
    {
        get
        {
            if (Parent == null)
            {
                return null;
            }

            return Parent is EntityPackage ? (EntityPackage) Parent : Parent.ParentPackage;
        }
    }

    public EntityGroup ParentGroup
    {
        get
        {
            if (Parent == null)
            {
                return null;
            }

            return parent is EntityGroup ? (EntityGroup) Parent : Parent.ParentGroup;
        }
    }

    public Entity ParentEntity
    {
        get
        {
            if (Parent == null)
            {
                return null;
            }

            return Parent is Entity ? (Entity) Parent : Parent.ParentEntity;
        }
    }

    public EntityPackage ParentPackageWithoutPrefab
    {
        get
        {
            if (Parent == null)
            {
                return null;
            }

            return Parent is EntityPackage && !(Parent is Prefab)
                ? (EntityPackage) Parent
                : Parent.ParentPackageWithoutPrefab;
        }
    }

    public LoadUnitPackage ParentLoadUnitPackage
    {
        get
        {
            if (Parent == null)
            {
                return null;
            }

            return Parent is LoadUnitPackage ? (LoadUnitPackage) Parent : Parent.ParentLoadUnitPackage;
        }
    }

    public Prefab ParentPrefab
    {
        get
        {
            if (Parent == null)
            {
                return null;
            }

            return Parent is Prefab ? (Prefab) Parent : Parent.ParentPrefab;
        }
    }

    public SequenceContainer ParentSequenceContainer
    {
        get
        {
            if (Parent == null)
            {
                return null;
            }

            return Parent is SequenceContainer ? Parent as SequenceContainer : Parent.ParentSequenceContainer;
        }
    }

    public Object ParentObject
    {
        get
        {
            if (Parent == null)
            {
                return null;
            }

            return Parent is Object ? Parent as Object : Parent.ParentObject;
        }
    }

    public DataItem ParentNodeContainer
    {
        get
        {
            if (Parent == null)
            {
                return null;
            }

            return Parent as TrayDataItem ?? Parent as SequenceContainer ?? Parent.ParentNodeContainer;
        }
    }

    public DataItem ParentTimeLineTrack
    {
        get
        {
            if (Parent == null)
            {
                return null;
            }

            if (!(Parent.DataType is Class dataType))
            {
                return null;
            }

            if (dataType.FullName == TimeLineDataItem.TrackClassFullName)
            {
                if (Parent.GetEnum("trackType_") == "TT_TRACK")
                {
                    return Parent;
                }

                if (Parent is Object)
                {
                    return null;
                }
            }

            return Parent.ParentTimeLineTrack;
        }
    }

    public DataItem this[string n]
    {
        get
        {
            foreach (var child in Children)
            {
                if (child.Name == n)
                {
                    return child;
                }
            }

            return null;
        }
    }

    public DataItem this[Field f]
    {
        get
        {
            if (f != null)
            {
                foreach (var child in Children)
                {
                    if (child.Field == f)
                    {
                        return child;
                    }
                }
            }

            return null;
        }
    }

    public List<DataItem> Children { get; } = new();

    public ItemPath FullPath
    {
        get
        {
            var itemPath = new ItemPath(Name);
            if (Parent != null)
            {
                itemPath = Parent.FullPath + itemPath;
            }

            return itemPath;
        }
    }

    public ItemPath FullPathWithoutRoot
    {
        get
        {
            var itemPath = new ItemPath(Name);
            if (Parent != null && Parent.Parent != null && !(Parent.Parent is IDocumentContainer))
            {
                itemPath = Parent.FullPathWithoutRoot + itemPath;
            }

            return itemPath;
        }
    }

    public ItemPath RelativePathFromPackage
    {
        get
        {
            var itemPath = new ItemPath(Name);
            if (Parent != null && !(Parent is EntityPackage))
            {
                itemPath = Parent.RelativePathFromPackage + itemPath;
            }

            return itemPath;
        }
    }

    public ItemPath RelativePathFromPackageWithoutPrefab
    {
        get
        {
            var itemPath = new ItemPath(Name);
            if (Parent != null && (!(Parent is EntityPackage) || Parent is Prefab))
            {
                itemPath = Parent.RelativePathFromPackageWithoutPrefab + itemPath;
            }

            return itemPath;
        }
    }

    public ItemPath RelativePathFromRootPackageForLiveEdit
    {
        get
        {
            var itemPath = new ItemPath(Name);
            if (IsAreaPackageChanged() || this is WorldPackage || this is UniversalPackage || ParentPackage == null)
            {
                return itemPath;
            }

            itemPath = Parent.RelativePathFromRootPackageForLiveEdit + itemPath;
            return itemPath;
        }
    }

    public string RelativePathFromRootPackageForDynamicArray => GetRelativePathFromRootPackageForDynamicArray();

    public bool HasDuplicatedName
    {
        get
        {
            var sequenceContainer = ParentSequenceContainer;
            if (sequenceContainer != null)
            {
                foreach (var node in sequenceContainer.Nodes)
                {
                    if (node != this)
                    {
                        var fullPath1 = node.FullPath;
                        var fullPath2 = fullPath1.FullPath;
                        fullPath1 = FullPath;
                        var fullPath3 = fullPath1.FullPath;
                        if (fullPath2 == fullPath3)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    public IEnumerable<GraphPinDataItem> AllTriggerInputs
    {
        get
        {
            foreach (var item in Children)
            {
                if (item is GraphPinDataItem pin
                    && pin.DataType?.FullName?.Contains("In") == true
                    && pin.DataType?.FullName?.Contains("Trigger") == true)
                {
                    yield return pin;
                }
            }

            if (TriInPorts != null)
            {
                foreach (var port in TriInPorts)
                {
                    yield return (GraphPinDataItem) port;
                }
            }
        }
    }

    public IEnumerable<GraphPinDataItem> AllVariableInputs
    {
        get
        {
            foreach (var item in Children)
            {
                if (item is GraphPinDataItem pin
                    && pin.DataType?.FullName?.Contains("In") == true
                    && pin.DataType?.FullName?.Contains("Variable") == true)
                {
                    yield return pin;
                }
            }

            if (RefInPorts != null)
            {
                foreach (var port in RefInPorts)
                {
                    yield return (GraphPinDataItem) port;
                }
            }
        }
    }

    public IEnumerable<GraphPinDataItem> AllTriggerOutputs
    {
        get
        {
            foreach (var item in Children)
            {
                if (item is GraphPinDataItem pin
                    && pin.DataType?.FullName?.Contains("Out") == true
                    && pin.DataType?.FullName?.Contains("Trigger") == true)
                {
                    yield return pin;
                }
            }

            if (TriOutPorts != null)
            {
                foreach (var port in TriOutPorts)
                {
                    yield return (GraphPinDataItem) port;
                }
            }
        }
    }

    public IEnumerable<GraphPinDataItem> AllVariableOutputs
    {
        get
        {
            foreach (var item in Children)
            {
                if (item is GraphPinDataItem pin
                    && pin.DataType?.FullName?.Contains("Out") == true
                    && pin.DataType?.FullName?.Contains("Variable") == true)
                {
                    yield return pin;
                }
            }

            if (RefOutPorts != null)
            {
                foreach (var port in RefOutPorts)
                {
                    yield return (GraphPinDataItem) port;
                }
            }
        }
    }

    public string MaterialIconName => this switch
    {
        EntityPackageReference => "widgets",
        SequenceContainer => "share",
        TrayDataItem => "tab",
        GraphPinDataItem => "gps_fixed",
        SequenceNodeDataItem => "commit",
        ConnectorDataItem => "stream",
        EntityGroup => "square",
        Entity => "token",
        _ => "question_mark"
    };

    public string DataTypeDisplayName => DataType?.DisplayName ?? Field?.TypeName ?? "Unknown";
    public string DataTypeFullName => DataType?.FullName ?? Field?.TypeName ?? "Unknown";
    public string DataTypeShortName => DataType?.ShortName ?? Field?.TypeName ?? "Unknown";

    public IEnumerator<DataItem> GetEnumerator()
    {
        return Children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Children.GetEnumerator();
    }

    public virtual void Dispose()
    {
        try
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            if (OnDisposing != null)
            {
                OnDisposing();
            }

            if (DocumentInterface.DocumentContainer == null)
            {
                return;
            }

            DocumentInterface.DocumentContainer.SelectRemove(this, true);
            DocumentInterface.DocumentContainer.beginDisposeDataItem(this);
            if (value != null)
            {
                if (value is DataItem)
                {
                    ((DataItem) value).Dispose();
                    value = null;
                }
                else
                {
                    value = null;
                }
            }

            if ((this is Entity && !(this is SequenceContainer)) || this is DynamicArray)
            {
                ClearChild();
            }

            if (parent != null)
            {
                parent.Remove(this);
                parent = null;
            }

            DocumentInterface.DocumentContainer.endDisposeDataItem(this);
        }
        catch (Exception ex)
        {
            // DocumentInterface.DocumentContainer.LogAppendCurrentParagraphParse(
            //     "<Span Foreground=\"Red\">DataItem.Dispose : " + ex.Message + "</Span><LineBreak />");
        }
    }

    public override string ToString()
    {
        return DataType?.Name switch
        {
            "SQEX.Ebony.Framework.Sequence.Connector.SequenceConnectorIn"
                or "SQEX.Ebony.Framework.Sequence.Connector.SequenceConnectorOut"
                => $"Portal (Group {GetInt("connectorNo_")})",
            _ => this is TrayDataItem
                ? GetString("trayname_") ?? DisplayName
                : this is SequenceNodeDataItem
                    ? $"{Name} {DataType.DisplayName}"
                    : DisplayName
        };
    }

    public event Action OnDisposing;

    public void ChangeName(string newName)
    {
        if (!(name != newName))
        {
            return;
        }

        name = newName;
        Modified = true;
    }

    public void ChangeNameAtLoadingSequence(string newName)
    {
        if (!(name != newName))
        {
            return;
        }

        name = newName;
    }

    public void SetUniqueEntityNameAtInitializing(string value)
    {
        changeUniqueEntityName(value, false);
    }

    private void changeUniqueEntityName(string value, bool setModifyFlag = true)
    {
        if (DocumentInterface.DocumentAction != null)
        {
            value = DocumentInterface.DocumentAction.GetAvailableName(this, value);
        }

        if (!(name != value))
        {
            return;
        }

        name = value;
        if (setModifyFlag)
        {
            Modified = true;
            IsNameModified = true;
        }

        DocumentInterface.DocumentContainer.doOnEntityNameChanged(this as Entity);
    }

    public void CheckAndChangeUniqueEntityName(bool setModifyFlag = true)
    {
        if (!(this is Entity) || DocumentInterface.DocumentContainer == null ||
            DocumentInterface.DocumentAction.IsUniqueName(this, name))
        {
            return;
        }

        changeUniqueEntityName(name, setModifyFlag);
    }

    public IItem GetDefaultValue()
    {
        if (this.dataType == null)
        {
            return null;
        }

        return !(this.dataType is Field dataType) ? this.dataType.DefaultValue : dataType.ConstructValue(Parent);
    }

    public void SetRuntimeReadOnlyRecursive(RuntimeOverrideType overrideType)
    {
        foreach (var dataItem in Children.Where(x => x.Browsable))
        {
            dataItem.RuntimeReadOnly = overrideType;
            dataItem.SetRuntimeReadOnlyRecursive(overrideType);
        }
    }

    public void AutoCreateDynamicArrayItem()
    {
        foreach (var dataItem in Children.Where(x => x.Browsable))
        {
            if (dataItem is DynamicArray dynamicArray2)
            {
                dynamicArray2.AutoCreateItem();
            }

            dataItem.AutoCreateDynamicArrayItem();
        }
    }

    public DataItem FindChildSpecialType(string specialTypeName)
    {
        foreach (var child in Children)
        {
            if (child.SpecialType == specialTypeName)
            {
                return child;
            }
        }

        return null;
    }

    public TChild FindFirstOfTypeRecursive<TChild>()
    {
        var myChildren = Children.Where(c => c.Parent == this).ToList();

        foreach (var child in myChildren)
        {
            if (child is TChild result)
            {
                return result;
            }
        }

        foreach (var child in myChildren)
        {
            var result = child.FindFirstOfTypeRecursive<TChild>();
            if (result != null)
            {
                return result;
            }
        }

        return default;
    }

    public void FindChildSpecialTypeListRecursive(
        string specialTypeName,
        List<DataItem> list,
        bool isStartWith = false)
    {
        foreach (var child in Children)
        {
            if (child.DataType is Class dataType1)
            {
                if (!(dataType1.FullName == "SQEX.Ebony.Std.DynamicArray"))
                {
                    child.FindChildSpecialTypeListRecursive(specialTypeName, list, isStartWith);
                }
            }
            else if ((!isStartWith ? child.SpecialType == specialTypeName ? 1 : 0 :
                         child.SpecialType.StartsWith(specialTypeName) ? 1 : 0) != 0)
            {
                list.Add(child);
            }
        }
    }

    public Value FindChildSpecialTypeValue(string specialTypeName)
    {
        var childSpecialType = FindChildSpecialType(specialTypeName);
        return childSpecialType != null ? (Value) childSpecialType.Value : null;
    }

    public int FindChildSpecialTypeInt(string categoryName)
    {
        var specialTypeValue = FindChildSpecialTypeValue(categoryName);
        return specialTypeValue == null ? 0 : specialTypeValue.GetInt();
    }

    public string FindChildSpecialTypeString(string categoryName)
    {
        var specialTypeValue = FindChildSpecialTypeValue(categoryName);
        return specialTypeValue == null ? "" : specialTypeValue.ToString();
    }

    public string FindChildSpecialTypeString2(string categoryName)
    {
        var specialTypeValue = FindChildSpecialTypeValue(categoryName);
        return specialTypeValue == null ? "" : specialTypeValue.ToStringWithFormat(Field, null);
    }

    public DynamicArray FindChildSpecialTypeArray(string specialTypeName)
    {
        foreach (var child in Children)
        {
            if (child.SpecialType == specialTypeName)
            {
                return child as DynamicArray;
            }
        }

        return null;
    }

    private void setModifiedInternal(bool value)
    {
        if (value == modifed || disposed || ModifiedLocked)
        {
            return;
        }

        modifed = value;
        if (value)
        {
            EarcModified = true;
            if (this is EntityPackage && !(this is Prefab))
            {
                if (DocumentInterface.DocumentContainer == null)
                {
                    return;
                }

                DocumentInterface.DocumentContainer.doOnEntityPackageModifiedChanged(this as EntityPackage);
            }
            else
            {
                if (this is Entity && DocumentInterface.DocumentContainer != null)
                {
                    DocumentInterface.DocumentContainer.doOnEntityModifiedChanged((Entity) this);
                }

                if (parent == null)
                {
                    return;
                }

                parent.setModifiedInternal(value);
            }
        }
        else
        {
            foreach (var child in Children)
            {
                if (!(child is EntityPackage) || child is Prefab)
                {
                    child.setModifiedInternal(value);
                }
            }

            if (this is EntityPackage && !(this is Prefab) && DocumentInterface.DocumentContainer != null)
            {
                DocumentInterface.DocumentContainer.doOnEntityPackageModifiedChanged(this as EntityPackage);
            }

            if (!(this is Entity))
            {
                return;
            }

            IsNameModified = false;
        }
    }

    public bool CheckDifference()
    {
        var parentPackage = ParentPackage as Prefab;
        var parentEntity = ParentEntity;
        if (parentPackage != null)
        {
            return parentPackage.AddDifference(this);
        }

        return parentEntity != null && EntityPresetUtility.IsEnablePresetDifference(parentEntity) &&
               parentEntity.AddDifference(this);
    }

    public int GetFCurveEditor()
    {
        var editorByDoubleClick1 = WakeUpCurveEditorByDoubleClick;
        if (editorByDoubleClick1 > 0)
        {
            return editorByDoubleClick1;
        }

        foreach (var child in Children)
        {
            var editorByDoubleClick2 = child.WakeUpCurveEditorByDoubleClick;
            if (editorByDoubleClick2 > 0)
            {
                return editorByDoubleClick2;
            }
        }

        return -1;
    }

    public bool HasTimeLine()
    {
        if (hasTimeLine)
        {
            return true;
        }

        foreach (var child in Children)
        {
            if (child.hasTimeLine)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasDataGrid()
    {
        if (DataGrid)
        {
            return true;
        }

        foreach (var child in Children)
        {
            if (child.DataGrid)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsParentOf(DataItem item)
    {
        for (; item != null; item = item.Parent)
        {
            if (item == this)
            {
                return true;
            }
        }

        return false;
    }

    public virtual DataItem GetChild(ItemPath relativePath, bool isAllowLazyLoading = true)
    {
        DataItem dataItem = null;
        var l_name = relativePath.PopFront();
        if (!string.IsNullOrEmpty(l_name))
        {
            if (l_name != null && l_name.StartsWith("["))
            {
                using (var enumerator = Children.Where(x => x.Name == l_name).GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        dataItem = enumerator.Current;
                    }
                }
            }
            else
            {
                dataItem = this[l_name];
            }
        }
        else
        {
            dataItem = this;
        }

        if (dataItem != null && relativePath.Exists)
        {
            dataItem = dataItem.GetChild(relativePath, isAllowLazyLoading);
        }

        return dataItem;
    }

    public DataItem GetChildByRenamedName(string childName)
    {
        foreach (var child in Children)
        {
            if (child.Field != null && Array.IndexOf(child.Field.Renameds, childName) >= 0)
            {
                return child;
            }
        }

        return null;
    }

    public void Add(DataItem item)
    {
        if (Children.Contains(item))
        {
            return;
        }

        Children.Add(item);
        Modified = true;
    }

    public void AddForce(DataItem item)
    {
        Children.Add(item);
        Modified = true;
    }

    public void AddWithoutModifyFlag(DataItem item)
    {
        if (Children.Contains(item))
        {
            return;
        }

        Children.Add(item);
    }

    public void AddForceWithoutModifyFlag(DataItem item)
    {
        Children.Add(item);
    }

    public void Insert(int insertPosition, DataItem item)
    {
        if (Children.Contains(item))
        {
            return;
        }

        if (insertPosition < 0)
        {
            insertPosition = Children.Count();
        }

        Children.Insert(insertPosition, item);
        Modified = true;
    }

    public void Remove(DataItem item)
    {
        if (!Children.Contains(item))
        {
            return;
        }

        Children.Remove(item);
        Modified = true;
    }

    public void RemoveWithoutModifyFlag(DataItem item)
    {
        if (!Children.Contains(item))
        {
            return;
        }

        Children.Remove(item);
    }

    public void ReplaceChildren(DataItem[] items)
    {
        Children.Clear();
        Children.AddRange(items);
    }

    public void ClearChild()
    {
        clearChildImpl(true);
    }

    public void ClearChildWithoutModifyFlag()
    {
        clearChildImpl(false);
    }

    private void clearChildImpl(bool setModifyFlag)
    {
        if (Children.Count <= 0)
        {
            return;
        }

        foreach (var dataItem in Children.ToArray())
        {
            if (dataItem.parent == this)
            {
                dataItem.parent = null;
                dataItem.Dispose();
            }
        }

        Children.Clear();
        if (!setModifyFlag)
        {
            return;
        }

        Modified = true;
    }

    public string GetRelativePathFromRootPackageForDynamicArray(DataItem relativeItem = null)
    {
        var str = string.Empty;
        if (Parent.DataType is Class dataType && dataType.FullName == "SQEX.Ebony.Std.DynamicArray")
        {
            var num = 0;
            foreach (var dataItem in Parent)
            {
                if (dataItem == this)
                {
                    str = num.ToString();
                    break;
                }

                ++num;
            }
        }

        if (IsAreaPackageChanged() || ParentPackage == null ||
            (relativeItem != null && ParentPackage == relativeItem))
        {
            return str;
        }

        str = Parent.RelativePathFromRootPackageForDynamicArray + "." + str;
        return str;
    }

    private bool IsAreaPackageChanged()
    {
        return this is AreaPackage && !((EntityPackage) this).StartupLoad;
    }

    public string CreateAutoGeneratedItemPath()
    {
        return parent != null ? parent.CreateAutoGeneratedItemPathFromParent() : "";
    }

    public string CreateAutoGeneratedItemPathFromParent()
    {
        return ("(ItemPath)" + FullPath).Replace("root.entities_.", "").Replace("entities_.", "")
            .Replace("nodes_.", "");
    }

    /// <param name="overrideParentDataItem">
    ///     If given, the clone will be parented to this DataItem instead of the original
    ///     parent
    /// </param>
    public DataItem Clone(DataItem overrideParentDataItem = null)
    {
        var newData = overrideParentDataItem == null
            ? DocumentInterface.ModuleContainer.CreateObjectFromString(Parent, DataType) as DataItem
            : DocumentInterface.ModuleContainer.CreateObjectFromString(overrideParentDataItem,
                DataType) as DataItem;
        copyField(newData);
        return newData;
    }

    public DataItem Convert(string typeFullName, DataItem overrideParentDataItem = null)
    {
        var newData = overrideParentDataItem == null
            ? DocumentInterface.ModuleContainer.CreateObjectFromString(Parent, typeFullName) as DataItem
            : DocumentInterface.ModuleContainer.CreateObjectFromString(overrideParentDataItem, typeFullName) as
                DataItem;
        copyField(newData);
        return newData;
    }

    private void copyField(DataItem newData, bool copyDynamicArray = true)
    {
        for (var index1 = 0; index1 < newData.Children.Count(); ++index1)
        {
            var index2 = -1;
            for (var index3 = 0; index3 < Children.Count(); ++index3)
            {
                if (newData.Children[index1].Name == Children[index3].Name)
                {
                    index2 = index3;
                    break;
                }
            }

            if (index2 >= 0)
            {
                if (newData.Children[index1].dataType is Class)
                {
                    if (newData.Children[index1] is DynamicArray array)
                    {
                        if (copyDynamicArray)
                        {
                            Children[index2].copyDynamicArray((DynamicArray) newData.Children[index1]);
                        }
                    }
                    else
                    {
                        Children[index2].copyField(newData.Children[index1], copyDynamicArray);
                    }
                }
                else if (newData.Children[index1].dataType is Field)
                {
                    newData.Children[index1].value = new Value((Value) Children[index2].Value);
                    ModuleContainer.ApplyUseParentAttribute(newData.Children[index1]);
                }
            }
        }
    }

    private void copyDynamicArray(DynamicArray newData)
    {
        var index1 = 0;
        var index2 = 0;
        while (index1 < Children.Count())
        {
            if (Children[index1] is SequenceContainer)
            {
                --index2;
            }
            else if (Children[index1].Parent == this)
            {
                var objectFromString =
                    DocumentInterface.ModuleContainer.CreateObjectFromString(newData, Children[index1].dataType) as
                        DataItem;
                objectFromString.name = Children[index1].name;
                if (!(Children[index1] is Entity))
                {
                    objectFromString.Field =
                        new Field(Children[index1].name, Children[index1].dataType.FullName, false);
                }

                objectFromString.IsDynamic = Children[index1].IsDynamic;
                if (newData.Children[index2].dataType is Class)
                {
                    if (objectFromString is DynamicArray)
                    {
                        Children[index1].copyDynamicArray((DynamicArray) objectFromString);
                    }
                    else
                    {
                        Children[index1].copyField(objectFromString);
                    }

                    ModuleContainer.ApplyUseParentAttribute(newData.Children[index2]);
                }
                else if (objectFromString.dataType is Field)
                {
                    objectFromString.value = new Value((Value) Children[index1].Value);
                    ModuleContainer.ApplyUseParentAttribute(newData.Children[index2]);
                }
            }

            ++index1;
            ++index2;
        }
    }

    public void CloneLinks(DataItem clone, List<DataItem> allClones)
    {
        foreach (var child in clone.Children)
        {
            var index2 = -1;
            for (var index3 = 0; index3 < Children.Count; ++index3)
            {
                if (child.Name == Children[index3].Name)
                {
                    index2 = index3;
                    break;
                }
            }

            if (index2 >= 0)
            {
                if (child.dataType is Class)
                {
                    if (child is DynamicArray array)
                    {
                        Children[index2].CloneDynamicArrayLinks(array, allClones);
                    }
                    else
                    {
                        Children[index2].CloneLinks(child, allClones);
                    }
                }
            }
        }
    }

    private void CloneDynamicArrayLinks(DynamicArray clone, List<DataItem> allClones)
    {
        var index1 = 0;
        var index2 = 0;

        while (index1 < Children.Count)
        {
            if (Children[index1] is SequenceContainer)
            {
                --index2;
            }
            else if (Children[index1].Parent == this)
            {
                var cloneChild = clone.Children[index1];

                if (clone.Children[index2].dataType is Class)
                {
                    if (cloneChild is DynamicArray array)
                    {
                        Children[index1].CloneDynamicArrayLinks(array, allClones);
                    }
                    else
                    {
                        Children[index1].CloneLinks(cloneChild, allClones);
                    }
                }
            }
            else
            {
                var matchingNode = allClones.FirstOrDefault(c => c.Name == Children[index1].ParentObject.Name);
                if (matchingNode != null)
                {
                    var matchingPinArray = matchingNode.FirstOrDefault(c => c.Name == Children[index1].Parent.Name);
                    var matchingPin = matchingPinArray == null
                        ? matchingNode.FirstOrDefault(c => c.Name == Children[index1].Name)
                        : matchingPinArray.FirstOrDefault(c => c.Name == Children[index1].Name);

                    if (matchingPin != null)
                    {
                        clone.Add(matchingPin);
                    }
                }
            }

            ++index1;
            ++index2;
        }
    }

    public Value GetValue(string name)
    {
        var dataItem = this[name];
        return dataItem is {Value: Data.Value} ? (Value) dataItem.Value : null;
    }

    public string GetString(string name)
    {
        return GetValue(name)?.ToString();
    }

    public DataItem SetString(string name, string value)
    {
        if (this[name] == null)
        {
            var valueDataItem = new ValueDataItem(this, "string");
            valueDataItem.Name = name;
            valueDataItem.Browsable = false;
        }

        this[name].Value = new Value(value);
        Modified = true;
        return this[name];
    }

    public DataItem SetStringWithoutModifiedFlag(string name, string value)
    {
        if (this[name] == null)
        {
            var valueDataItem = new ValueDataItem(this, "string");
            valueDataItem.Name = name;
            valueDataItem.Browsable = false;
        }

        this[name].Value = new Value(value);
        return this[name];
    }

    public bool GetBool(string name, bool defaultValue = false)
    {
        var obj = GetValue(name);
        return obj != null ? obj.GetBool() : defaultValue;
    }

    public void SetBool(string name, bool value)
    {
        setBoolImpl(name, value, true);
    }

    public void SetBoolWithoutModifyFlag(string name, bool value)
    {
        setBoolImpl(name, value, false);
    }

    private void setBoolImpl(string name, bool value, bool modify)
    {
        if (this[name] == null)
        {
            var valueDataItem = new ValueDataItem(this, "bool");
            valueDataItem.Name = name;
            valueDataItem.Browsable = false;
        }

        this[name].Value = new Value(value);
        if (!modify)
        {
            return;
        }

        Modified = true;
    }

    public int GetInt(string name)
    {
        var obj = GetValue(name);
        return obj != null ? obj.GetInt() : 0;
    }

    public void SetInt(string name, int value)
    {
        if (this[name] == null)
        {
            var valueDataItem = new ValueDataItem(this, "int");
            valueDataItem.Name = name;
            valueDataItem.Browsable = false;
        }

        this[name].Value = new Value(value);
        Modified = true;
    }

    public float GetFloat(string name)
    {
        var obj = GetValue(name);
        return obj != null ? obj.GetFloat() : 0.0f;
    }

    public void SetFloat(string name, float value)
    {
        if (this[name] == null)
        {
            var valueDataItem = new ValueDataItem(this, "float");
            valueDataItem.Name = name;
            valueDataItem.Browsable = false;
        }

        this[name].Value = new Value(value);
        Modified = true;
    }

    public double GetDouble(string name)
    {
        var obj = GetValue(name);
        return obj != null ? obj.GetDouble() : 0.0;
    }

    public void SetDouble(string name, double value)
    {
        if (this[name] == null)
        {
            var valueDataItem = new ValueDataItem(this, "double");
            valueDataItem.Name = name;
            valueDataItem.Browsable = false;
        }

        this[name].Value = new Value(value);
        Modified = true;
    }

    public float[] GetFloat4(string name)
    {
        return GetValue(name)?.GetFloatArray();
    }

    public void SetFloat4(string name, float[] value)
    {
        SetFloat4Impl(name, value, true);
    }

    public void SetFloat4WithoutModified(string name, float[] value)
    {
        SetFloat4Impl(name, value, false);
    }

    public void SetFloat4Impl(string name, float[] value, bool updateModifed)
    {
        if (this[name] == null)
        {
            var valueDataItem = new ValueDataItem(this, "float4");
            valueDataItem.Name = name;
            valueDataItem.Browsable = false;
        }

        this[name].Value = new Value(value[0], value[1], value[2], value[3]);
        if (!updateModifed)
        {
            return;
        }

        Modified = true;
    }

    public string GetEnum(string name)
    {
        return GetValue(name)?.ToString();
    }

    public void SetEnum(string name, Enum e, string value)
    {
        SetEnumImpl(name, e, value, true);
    }

    public void SetEnumWithoutModified(string name, Enum e, string value)
    {
        SetEnumImpl(name, e, value, false);
    }

    public void SetEnumImpl(string name, Enum e, string value, bool modify)
    {
        if (this[name] == null)
        {
            var valueDataItem = new ValueDataItem(this, "enum");
            valueDataItem.Name = name;
            valueDataItem.Browsable = false;
        }

        this[name].Value = new Value(e, value);
        if (!modify)
        {
            return;
        }

        Modified = true;
    }

    public DataItem GetPointer(string name)
    {
        return GetValue(name)?.GetPointer();
    }

    public void SetFixid(string name, string label)
    {
        if (this[name] == null)
        {
            var valueDataItem = new ValueDataItem(this, "Fixid");
            valueDataItem.Name = name;
            valueDataItem.Browsable = false;
        }

        this[name].Value = new Value(PrimitiveType.Fixid, label);
        Modified = true;
    }

    public void SetFixidWithNewField(
        string name,
        string label,
        string table,
        string prefix,
        bool isFixidCreate,
        bool isFixidNoUpper)
    {
        if (this[name] == null)
        {
            var valueDataItem = new ValueDataItem(this, "Fixid");
            valueDataItem.Name = name;
            valueDataItem.Browsable = false;
        }

        var dataItem = this[name];
        dataItem.Value = new Value(PrimitiveType.Fixid, label);
        dataItem.Field = new Field(dataItem.Field.Name, dataItem.Field.TypeName, false);
        dataItem.Field.Attributes["FixidTable"] = table;
        dataItem.Field.Attributes["FixidPrefix"] = prefix;
        dataItem.Field.Attributes["FixidCreate"] = isFixidCreate ? "true" : "false";
        dataItem.Field.Attributes["FixidNoUpper"] = isFixidNoUpper ? "true" : "false";
        Modified = true;
    }

    public Color GetColor(string name)
    {
        var obj = GetValue(name);
        return obj != null ? obj.GetColor() : new Color();
    }

    public void AddDynamicArray(string name, DataItem item)
    {
        if (!(this[name] is DynamicArray dynamicArray))
        {
            CreateDynamicArray(name);
            dynamicArray = this[name] as DynamicArray;
        }

        dynamicArray.Add(item);
    }

    public void ClearDynamicArray(string name)
    {
        if (!(this[name] is DynamicArray dynamicArray))
        {
            CreateDynamicArray(name);
            dynamicArray = this[name] as DynamicArray;
        }

        dynamicArray.ClearChild();
    }

    public void CreateDynamicArray(string name)
    {
        if (this[name] == null)
        {
            var dynamicArray = new DynamicArray(this);
            dynamicArray.Name = name;
            dynamicArray.Browsable = false;
            this[name].Add(dynamicArray);
        }

        Modified = true;
    }

    public DataItem GetClass(string name)
    {
        return this[name] ?? null;
    }

    private void setupDefaultValue_(DataType dataType)
    {
        if (!(dataType is Class class1))
        {
            return;
        }

        foreach (DataType baseType in class1.BaseTypes)
        {
            setupDefaultValue_(baseType);
        }

        foreach (var field in class1.Fields)
        {
            if (!field.Deprecated)
            {
                DataItem dataItem = null;
                if (field.DataType != null && !field.Pointer)
                {
                    dataItem =
                        DocumentInterface.ModuleContainer.CreateObjectFromString(this, field.DataType) as DataItem;
                }

                if (dataItem == null)
                {
                    dataItem = new FieldDataItem(this, field);
                    dataItem.value = field.ConstructValue(this);
                }

                dataItem.name = field.Name;
                dataItem.Field = field;
                var baseType =
                    DocumentInterface.ModuleContainer["SQEX.Ebony.Framework.Node.GraphPropertyVariableInputPin"] as
                        Class;
                var class2 = field.Class;
                if ((class2 != null ? class2.IsBasedOn(baseType) ? 1 : 0 : 0) != 0)
                {
                    string constructTypeName = null;
                    if (field.TryGetAttributeString("PinValueType", out constructTypeName) &&
                        constructTypeName != string.Empty)
                    {
                        var fieldDataItem = new FieldDataItem(this, field);
                        fieldDataItem.Value = field.ConstructValueFromString(this, constructTypeName);
                        fieldDataItem.Name = field.Name;
                        fieldDataItem.Field = field;
                        fieldDataItem.IsDynamic = true;
                        fieldDataItem.IsPropertyPin = true;
                    }
                }
            }
        }

        if (!class1.IsBasedOn(
                DocumentInterface.ModuleContainer["SQEX.Ebony.Framework.Entity.PropertyImportableEntity"] as Class))
        {
            return;
        }

        importComponentPropertyFromClassMember_(class1.Name);
        var className = "";
        if (!class1.TryGetAttributeString("ImportComponentProperty", out className))
        {
            return;
        }

        importComponentPropertyFromClassMember_(className);
    }

    private void importComponentPropertyFromClassMember_(string className)
    {
        var baseType = DocumentInterface.ModuleContainer["SQEX.Luminous.Core.Component.Component"] as Class;
        var @class = DocumentInterface.ModuleContainer[className] as Class;
        if (baseType == null || @class == null)
        {
            return;
        }

        foreach (var field in @class.Fields)
        {
            if (field.Pointer && DocumentInterface.ModuleContainer[field.TypeName] is Class componentClass1 &&
                componentClass1.IsBasedOn(baseType))
            {
                importComponentProperty_(componentClass1);
            }
        }
    }

    private void importComponentProperty_(Class componentClass)
    {
        if (componentClass == null)
        {
            return;
        }

        foreach (var componentPropertyField in componentClass.ComponentPropertyFields)
        {
            if (!componentPropertyField.Deprecated)
            {
                DataItem dataItem = null;
                if (componentPropertyField.DataType != null && !componentPropertyField.Pointer)
                {
                    dataItem = DocumentInterface.ModuleContainer.CreateObjectFromString(this,
                        componentPropertyField.DataType) as DataItem;
                }

                if (dataItem == null)
                {
                    dataItem = new FieldDataItem(this, componentPropertyField);
                    dataItem.value = componentPropertyField.ConstructValue(this);
                }

                dataItem.name = componentPropertyField.Name;
                dataItem.Field = componentPropertyField;
                dataItem.IsImportComponentProperty = true;
                dataItem.ImportComponentClassName = componentClass.Name;
            }
        }
    }

    protected virtual void setupDefaultValue(DataType dataType)
    {
        setupDefaultValue_(dataType);
    }

    private void setupBrowsable()
    {
        if (!browsable)
        {
            return;
        }

        if (IsPropertyPin)
        {
            browsable = true;
        }
        else if (field != null)
        {
            if (Parent != null && Parent.DataType is Class dataType3)
            {
                var attributeRecursive = dataType3.GetAttributeRecursive("BrowsableOverride");
                if (attributeRecursive != null && attributeRecursive.Contains(field.Name))
                {
                    var str1 = attributeRecursive;
                    var chArray1 = new char[1] {';'};
                    foreach (var str2 in str1.Split(chArray1))
                    {
                        var chArray2 = new char[1] {'/'};
                        var strArray = str2.Split(chArray2);
                        if (strArray.Length == 2)
                        {
                            var str3 = strArray[0];
                            var str4 = strArray[1];
                            if (str3 == Name)
                            {
                                bool.TryParse(str4, out browsable);
                                return;
                            }
                        }
                    }
                }
            }

            browsable = field.Browsable;
        }
        else
        {
            if (dataType == null)
            {
                return;
            }

            browsable = dataType.Browsable;
        }
    }

    public bool IsChildOf(DataItem other)
    {
        if (other.Children.Contains(this))
        {
            return true;
        }

        foreach (var child in other.Children.Where(c => c.Parent == other))
        {
            var result = IsChildOf(child);
            if (result)
            {
                return true;
            }
        }

        return false;
    }

    public void SetPackageDirty()
    {
        var package = this as EntityPackage ?? ParentPackage;
        package.IsDirty = true;
    }
}