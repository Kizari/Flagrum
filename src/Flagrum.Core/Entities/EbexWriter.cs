using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Flagrum.Core.Scripting.Ebex.Configuration;
using Flagrum.Core.Scripting.Ebex.Data;
using Flagrum.Core.Scripting.Ebex.Type;
using Flagrum.Core.Scripting.Ebex.Utility;
using Enum = Flagrum.Core.Scripting.Ebex.Type.Enum;
using Object = Flagrum.Core.Scripting.Ebex.Data.Object;

namespace Flagrum.Core.Scripting.Ebex;

public class EbexWriter : IDisposable
{
    public const string USE_UNRESOLVED_POINTER_REFERENCE_ATTR = "UseUnresolvedPointerReference";
    public const string UNRESOLVED_PACKAGE_SOURCE_ATTR = "UnresolvedPointerPackageSource";
    public const string FAR_REFERENCE_ATTR = "FarReference";
    public const string REFERENCE_SOURCE_ITEMPATH = "ReferenceSourceItemPath";
    public const string USE_PREFAB_CONNECTION_V20_ATTR = "UsePrefabConnectionV20";
    public const string PREFAB_OWNER_PACKAGE_SOURCE_ATTR = "PrefabOwnerPackageSource";
    public const string PREFAB_CONNECTION_SOURCE_ITEMPATH = "PrefabConnectionSourceItemPath";
    public const string USE_TEMPLATE_CONNECTION_ATTR = "UseTemplateConnection";
    public const string PROXY_CONNECTION_OWNER_PACKAGENAME = "ProxyConnectionOwnerPackageName";
    public const string CHARAENTRY_REF_STRING = "CharaEntryRef";
    public const string ATTR_TYPE = "type";
    public const string ATTR_ALT_TYPE = "csharp_type";
    public const string ATTR_DESERIALIZE_TYPE = "deserialize_type";
    public const string ATTR_DESERIALIZE_TYPE_CSHARP = "csharp";
    public const string ATTR_ORIGINAL_TYPE = "original_type";
    public const string ATTR_IMPORT_COMPONENT_CLASS_NAME = "import_component_class_name";
    public const string ATTR_IMPORT_COMPONENT_CLASS_HASH = "import_component_class_hash";
    public const string ATTR_IMPORT_COMPONENT_MEMBER_HASH = "import_component_member_hash";

    private static Class _baseConnectorBaseClass =
        DocumentInterface.ModuleContainer["SQEX.Ebony.Framework.Sequence.Connector.SequenceConnectorBase"] as Class;

    private static readonly Class _baseConnectorInClass =
        DocumentInterface.ModuleContainer["SQEX.Ebony.Framework.Sequence.Connector.SequenceConnectorIn"] as Class;

    private static readonly Class _baseConnectorOutClass =
        DocumentInterface.ModuleContainer["SQEX.Ebony.Framework.Sequence.Connector.SequenceConnectorOut"] as Class;

    private readonly bool atLiveEditForAddingBaseObject_;
    private readonly bool atLiveEditForAddingNode_;
    private readonly bool isUseSelectionMode_;
    private readonly MemoryStream memoryStream;
    private readonly XmlWriter output;
    private readonly ItemPath rootFullPath;
    private readonly StreamWriter streamWriter;
    private readonly bool useUnresolvedPointerReference_;
    private object fixid;
    private EntityGroup rootGroup;

    public EbexWriter(
        EntityPackage package,
        string path,
        bool isUseSelectionMode,
        bool isWriteChildPackage,
        bool atLiveEditForAddingNode = false)
    {
        atLiveEditForAddingNode_ = atLiveEditForAddingNode;
        if (atLiveEditForAddingNode_)
        {
            useUnresolvedPointerReference_ = true;
        }

        isUseSelectionMode_ = isUseSelectionMode;
        atLiveEditForAddingBaseObject_ = false;
        if (!atLiveEditForAddingNode)
        {
            //DocumentInterface.DocumentContainer.doOnEntityPackageSaving(package, path);
        }

        float[] numArray = null;
        if (package is Prefab)
        {
            numArray = package.GetFloat4("position_");
            package.SetFloat4("position_", new float[4]
            {
                0.0f,
                0.0f,
                0.0f,
                1f
            });
        }

        var flag = false;
        if (path != null)
        {
            flag = package.StartupLoad;
            package.StartupLoad = package.StartupLoadAtSaveingAsTopPackage;
            if (!(package is Prefab) || ((Prefab)package).IsNewPrefab)
            {
                var withoutExtension = Path.GetFileNameWithoutExtension(path);
                if (package.Name != withoutExtension)
                {
                    package.Name = withoutExtension;
                }
            }

            if (package.FullFilePath != path)
            {
                package.FullFilePath = path;
            }
        }

        ReferenceErrorList = new List<WriterReferenceError>();
        memoryStream = new MemoryStream();
        streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        rootGroup = package;
        output = XmlWriter.Create(streamWriter, new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            IndentChars = "\t"
        });
        rootFullPath = package.FullPath;
        output.WriteStartElement(nameof(package));
        output.WriteAttributeString("name", package.Name);
        createObjectTable(package, isWriteChildPackage);
        writeObjectTable(isWriteChildPackage);
        output.WriteEndElement();
        output.Flush();
        if (package is Prefab)
        {
            package.SetFloat4("position_", numArray);
        }

        if (path == null)
        {
            return;
        }

        package.StartupLoad = flag;
    }

    public EbexWriter(EntityGroup rootGroup)
    {
        var isWriteChildPackage = false;
        atLiveEditForAddingNode_ = false;
        atLiveEditForAddingBaseObject_ = false;
        useUnresolvedPointerReference_ = false;
        isUseSelectionMode_ = false;
        ReferenceErrorList = new List<WriterReferenceError>();
        memoryStream = new MemoryStream();
        streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        this.rootGroup = rootGroup;
        output = XmlWriter.Create(streamWriter, new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            IndentChars = "\t"
        });
        rootFullPath = new ItemPath();
        output.WriteStartElement("package");
        output.WriteAttributeString("name", rootGroup.Name);
        createObjectTable(rootGroup, isWriteChildPackage);
        writeObjectTable(isWriteChildPackage);
        output.WriteEndElement();
        output.Flush();
    }

    public EbexWriter(DataItem baseObjectDataItem, bool isNeedCreateObjectFromThisPackage)
    {
        var isWriteChildPackage = true;
        atLiveEditForAddingNode_ = false;
        atLiveEditForAddingBaseObject_ = true;
        useUnresolvedPointerReference_ = false;
        isUseSelectionMode_ = false;
        ReferenceErrorList = new List<WriterReferenceError>();
        memoryStream = new MemoryStream();
        streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        output = XmlWriter.Create(streamWriter, new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            IndentChars = "\t"
        });
        rootFullPath = new ItemPath();
        if (isNeedCreateObjectFromThisPackage)
        {
            createObjectTable(baseObjectDataItem.ParentPackage, isWriteChildPackage);
        }

        output.WriteStartElement("baseObject");
        if (baseObjectDataItem.Field.TypeName != null)
        {
            output.WriteStartElement(baseObjectDataItem.Name);
            output.WriteAttributeString("dynamic", "true");
            output.WriteAttributeString("type", baseObjectDataItem.Field.TypeName);
            writeDataItemChildren(baseObjectDataItem, isWriteChildPackage, true);
            output.WriteEndElement();
        }

        output.WriteEndElement();
        output.Flush();
    }

    public List<Object> Objects { get; } = new();

    public List<WriterReferenceError> ReferenceErrorList { get; }

    public Action<int, string> P4_EditOnChangeList { get; set; }

    public byte[] ResultData => memoryStream != null ? memoryStream.ToArray() : null;

    private object Fixid => null;

    public void Dispose()
    {
        if (streamWriter != null)
        {
            streamWriter.Dispose();
        }

        if (memoryStream != null)
        {
            memoryStream.Dispose();
        }

        if (rootGroup != null)
        {
            rootGroup = null;
        }

        if (fixid == null)
        {
            return;
        }

        //fixid.Dispose();
        fixid = null;
    }

    public byte[] Write()
    {
        return ResultData;
    }

    public bool Write(string path, int changeListID = 0)
    {
        File.WriteAllBytes(path, ResultData);
        // path = Path.GetFullPath(path);
        // var result = false;
        //
        // Directory.CreateDirectory(Path.GetDirectoryName(path));
        // Thread.Sleep(10);
        // result = P4_EditOnChangeList == null
        //     ? SourceControl.WriteIfChangedWithChangeListID(ResultData, path, false, changeListID)
        //     : SourceControl.WriteIfChangedWithCheckout(ResultData, path, changeListID, P4_EditOnChangeList);
        // if (result)
        // {
        //     rootGroup.Modified = false;
        // }

        // if (rootGroup is EntityPackage)
        // {
        //     DocumentInterface.DocumentContainer.doOnEntityPackageSaved((EntityPackage)rootGroup, path, result);
        // }

        return true;
    }

    private void createObjectTable(DataItem dataItem, bool isWriteChildPackage)
    {
        var shouldNotAdd = false;
        if (isUseSelectionMode_ && !dataItem.IsChecked && dataItem != rootGroup)
        {
            if (dataItem is EntityGroup and not EntityPackageReference)
            {
                shouldNotAdd = true;
            }
            else
            {
                if (!atLiveEditForAddingNode_)
                {
                    return;
                }

                shouldNotAdd = true;
            }
        }

        if (dataItem is Object o)
        {
            if (Objects.Contains(o))
            {
                return;
            }

            if (!shouldNotAdd)
            {
                Objects.Add(o);
            }
        }

        if (((dataItem == rootGroup ? 1 : dataItem is not EntityPackageReference ? 1 : 0) |
             (isWriteChildPackage ? 1 : 0)) == 0)
        {
            return;
        }

        foreach (var child in dataItem.Children)
        {
            if ((!isWriteChildPackage || child is not EntityPackage package || package.StartupLoad) &&
                dataItem == child.Parent)
            {
                createObjectTable(child, isWriteChildPackage);
            }
        }
    }

    private void writeObjectTable(bool isWriteChildPackage)
    {
        var num = 0;
        output.WriteStartElement("objects");
        foreach (var @object in Objects)
        {
            output.WriteStartElement("object");
            output.WriteAttributeString("objectIndex", num++.ToString());
            output.WriteAttributeString("name", @object.Name);
            var dataTypeFullName = @object.DataType.FullName;
            var noEntities = false;
            var flag = false;
            if (@object is EntityPackageReference reference)
            {
                if (reference != rootGroup && !isWriteChildPackage)
                {
                    output.WriteAttributeString("original_type", dataTypeFullName);
                    dataTypeFullName = "SQEX.Ebony.Framework.Entity.EntityPackageReference";
                    noEntities = true;
                    if (reference is EntityPackage package &&
                        GetRootTrayInTemplateTrayPackage(package) != null)
                    {
                        package.SetBoolWithoutModifyFlag("isTemplateTraySourceReference_", true);
                        flag = true;
                    }
                }

                reference.SetSourcePath(reference != rootGroup);
            }

            if (!string.IsNullOrEmpty(@object.AlternativeName))
            {
                output.WriteAttributeString("type", @object.AlternativeName);
                output.WriteAttributeString("csharp_type", dataTypeFullName);
                output.WriteAttributeString("deserialize_type", Fnv1A32.Hash("csharp").ToString());
            }
            else
            {
                output.WriteAttributeString("type", dataTypeFullName);
            }

            var basePath1 = @object.ParentPackage == null || @object == rootGroup
                ? rootFullPath
                : @object.ParentPackage.FullPath;
            output.WriteAttributeString("path", @object.FullPath.GetRelativePath(basePath1).ToString());
            output.WriteAttributeString("checked",
                @object is not EntityPackage || @object != rootGroup ? @object.IsChecked.ToString() : "True");
            var ownerObject = getOwnerObject(@object.Parent);
            if (ownerObject != null)
            {
                var objectIndex = getObjectIndex(ownerObject);
                if (objectIndex >= 0)
                {
                    var parent = @object.FullPath.GetRelativePath(ownerObject.FullPath).GetParent();
                    var basePath2 = ownerObject.ParentPackage == null || ownerObject == rootGroup
                        ? rootFullPath
                        : ownerObject.ParentPackage.FullPath;
                    output.WriteAttributeString("owner",
                        ownerObject.FullPath.GetRelativePath(basePath2).ToString());
                    output.WriteAttributeString("ownerIndex", objectIndex.ToString());
                    output.WriteAttributeString("ownerPath", parent.ToString());
                }
            }

            writeDataItemChildren(@object, isWriteChildPackage, noEntities);
            if (flag)
            {
                @object.SetBoolWithoutModifyFlag("isTemplateTraySourceReference_", false);
            }

            output.WriteEndElement();
        }

        output.WriteEndElement();
    }

    private bool canSkip(DataItem dataItem)
    {
        if (dataItem.Parent is DynamicArray
            || dataItem.IsDynamic
            || dataItem.Field?.GetAttribute("UseReferenceValue") != null
            || (dataItem.Parent != null
                && (dataItem.Parent.DataType is not Class dataType
                    || dataType.GetDefaultValueOverride(dataItem.Name) != null)))
        {
            return false;
        }

        if (dataItem is DynamicArray)
        {
            return ((DynamicArray)dataItem).Count == 0;
        }

        var obj = dataItem.Value as Value;
        var defaultValue = dataItem.GetDefaultValue() as Value;
        if (obj == null || defaultValue == null || (obj.PrimitiveType != PrimitiveType.String &&
                                                    string.IsNullOrEmpty(dataItem.DataType.DefaultValueString)))
        {
            return false;
        }

        if (obj.PrimitiveType == PrimitiveType.Enum)
        {
            var num = dataItem.DataType.DefaultValueString != defaultValue.ToString() ? 1 : 0;
            return false;
        }

        bool flag;
        switch (obj.PrimitiveType)
        {
            case PrimitiveType.Bool:
            case PrimitiveType.String:
            case PrimitiveType.Char:
            case PrimitiveType.UChar:
            case PrimitiveType.Short:
            case PrimitiveType.UShort:
            case PrimitiveType.Int:
            case PrimitiveType.UInt:
            case PrimitiveType.Long:
            case PrimitiveType.ULong:
            case PrimitiveType.Float:
            case PrimitiveType.Double:
            case PrimitiveType.Float4:
            case PrimitiveType.Color:
                flag = false;
                break;
            default:
                flag = true;
                break;
        }

        if (flag)
        {
            return false;
        }

        if (obj.PrimitiveType == PrimitiveType.String && defaultValue.PrimitiveType == PrimitiveType.String)
        {
            if ((obj.Object as string == "" && defaultValue.Object == null) ||
                (obj.Object is not string && defaultValue.Object == null))
            {
                return true;
            }

            if (obj.Object is not string)
            {
                return false;
            }
        }

        if (defaultValue.Object == null)
        {
            return false;
        }

        try
        {
            if (obj.Equals(defaultValue))
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine("canSkip " + ex.Message);
        }

        return false;
    }

    private void writeDataItem(DataItem dataItem, bool isWriteChildPackage)
    {
        string str1 = null;
        string localName;
        if (dataItem.Field != null)
        {
            if (dataItem.Field.Transient)
            {
                return;
            }

            localName = dataItem.Field.Name;
            str1 = dataItem.Field.TypeName;
        }
        else
        {
            localName = dataItem.DataType == null ? dataItem.Name : dataItem.DataType.FullName;
        }

        if (SlimEbexConfig.Enabled && canSkip(dataItem))
        {
            return;
        }

        if (dataItem.Children.Count == 0 && dataItem.Value != null)
        {
            if (dataItem.Value is Value value)
            {
                output.WriteStartElement(localName);
                if (dataItem.IsDynamic)
                {
                    output.WriteAttributeString("dynamic", "true");
                    if (dataItem.IsPropertyPin)
                    {
                        output.WriteAttributeString("pin_default_value", "true");
                    }

                    output.WriteAttributeString("name", dataItem.DisplayName);
                }

                if (dataItem.IsImportComponentProperty)
                {
                    output.WriteAttributeString("import_component_class_name", dataItem.ImportComponentClassName);
                    var output1 = output;
                    var num = CRC32.Hash(dataItem.ImportComponentClassName);
                    var str2 = num.ToString();
                    output1.WriteAttributeString("import_component_class_hash", str2);
                    var output2 = output;
                    num = Fnv1A32.Hash(dataItem.Field.Name);
                    var str3 = num.ToString();
                    output2.WriteAttributeString("import_component_member_hash", str3);
                }

                switch (value.PrimitiveType)
                {
                    case PrimitiveType.String:
                        var field1 = dataItem.Field;
                        if (field1 != null)
                        {
                            if (field1.GetAttribute("FileExtension") != null)
                            {
                                var flag = false;
                                var source = value.ToString();
                                if (source.Contains('\n'))
                                {
                                    source = source.Replace("\n", "");
                                    flag = true;
                                }

                                if (source.Contains('\r'))
                                {
                                    source = source.Replace("\r", "");
                                    flag = true;
                                }

                                if (flag)
                                {
                                    dataItem.Value = new Value(source);
                                }

                                if (field1.GetAttribute("CharaEntryRef") != null)
                                {
                                    output.WriteAttributeString("CharaEntryRef", "true");
                                }
                            }

                            if (field1.GetAttribute("AutoGeneratedByItemPath") != null)
                            {
                                if (dataItem.ParentPackage is Prefab)
                                {
                                    if (dataItem.Parent is EntityDiffGroup diffGroup)
                                    {
                                        var diffTargetItem = diffGroup.GetDiffTargetItem();
                                        string str4;
                                        if (diffTargetItem != null)
                                        {
                                            str4 = diffTargetItem.CreateAutoGeneratedItemPathFromParent();
                                        }
                                        else
                                        {
                                            str4 = dataItem.CreateAutoGeneratedItemPath();
                                            if (DocumentInterface.DocumentContainer != null)
                                            {
                                                // DocumentInterface.DocumentContainer.PrintWarning(
                                                //     "target not found for AutoGeneratedByItemPath : " + str4);
                                            }
                                        }

                                        if (value.ToString() != str4)
                                        {
                                            dataItem.Value = new Value(str4);
                                        }

                                        break;
                                    }

                                    var str5 = "";
                                    if (value.ToString() != str5)
                                    {
                                        dataItem.Value = new Value(str5);
                                    }

                                    break;
                                }

                                var generatedItemPath = dataItem.CreateAutoGeneratedItemPath();
                                if (value.ToString() != generatedItemPath)
                                {
                                    dataItem.Value = new Value(generatedItemPath);
                                }
                            }
                        }

                        break;
                    case PrimitiveType.Fixid:
                        var field2 = dataItem.Field;
                        if (field2 != null)
                        {
                            field2.TryGetAttributeBool("FixidFlexible", out var fixidFlexible);
                            var lower = value.ToString();
                            var table = string.Empty;
                            var prefix = string.Empty;
                            var isNoUpper = false;
                            var isLower = false;
                            var fixidCreate = field2.GetAttribute("FixidCreate");
                            if (fixidFlexible)
                            {
                                var parent = dataItem.Parent;
                                if (parent is {DataType: Class dataType16} &&
                                    dataType16.IsBasedOn(DocumentInterface.ModuleContainer[
                                            "SQEX.Ebony.Framework.Sequence.Variable.Primitive.SequenceFlexibleFixidData"]
                                        as Class))
                                {
                                    var tableItem = parent["table_"];
                                    if (tableItem is {Value: not null})
                                    {
                                        table = tableItem.Value.ToString();
                                        isNoUpper = table is "model_joint" or "swf";
                                    }

                                    var dataItem2 = parent["prefix_"];
                                    if (dataItem2 is {Value: not null})
                                    {
                                        prefix = dataItem2.Value.ToString();
                                    }
                                }
                            }
                            else
                            {
                                table = field2.GetAttribute("FixidTable");
                                prefix = field2.GetAttribute("FixidPrefix");
                                field2.TryGetAttributeBool("FixidNoUpper", out isNoUpper);
                                field2.TryGetAttributeBool("FixidLower", out isLower);
                            }

                            if (table != null)
                            {
                                if (!string.IsNullOrEmpty(lower))
                                {
                                    if (isLower)
                                    {
                                        lower = lower.ToLower();
                                    }

                                    // output.WriteAttributeString("fixid",
                                    //     (fixidCreate == null || fixidCreate.ToLower() == "false"
                                    //         ? getFixid(table, lower, flag2 | flag3, prefix)
                                    //         : createFixid(table, lower, flag2 | flag3, prefix))
                                    //     .ToString());
                                    output.WriteAttributeString("fixid",
                                        getFixid(table, lower, isNoUpper | isLower, prefix).ToString());
                                }

                                break;
                            }

                            Console.Error.WriteLine("Jenova: error: element {0} doesn't have FixidTable.",
                                localName);
                        }

                        break;
                    case PrimitiveType.Pointer:
                        writeReferenceAttribute(value.GetPointer(), dataItem, true);
                        break;
                    case PrimitiveType.Enum:
                        var field3 = dataItem.Field;
                        if (field3 != null)
                        {
                            if (field3.DataType is not Enum dataType17)
                            {
                                Console.Error.WriteLine("Jenova: error: {0}'s Enum is NULL.", dataItem.Name);
                                break;
                            }

                            output.WriteAttributeString("value",
                                dataType17.ValueOf(dataItem.Value.ToString()).ToString());
                        }

                        break;
                }

                str1 = value.TypeName;
            }
            else
            {
                output.WriteStartElement(localName);
            }

            if (str1 != null)
            {
                output.WriteAttributeString("type", str1);
            }

            output.WriteValue(dataItem.Value.ToString());
        }
        else
        {
            output.WriteStartElement(localName);
            if (str1 != null)
            {
                if (dataItem.IsDynamic)
                {
                    output.WriteAttributeString("dynamic", "true");
                    if (dataItem.IsPropertyPin)
                    {
                        output.WriteAttributeString("pin_default_value", "true");
                    }

                    output.WriteAttributeString("type", str1);
                }

                if (dataItem.IsImportComponentProperty)
                {
                    output.WriteAttributeString("import_component_class_name", dataItem.ImportComponentClassName);
                    var output3 = output;
                    var num = CRC32.Hash(dataItem.ImportComponentClassName);
                    var str6 = num.ToString();
                    output3.WriteAttributeString("import_component_class_hash", str6);
                    var output4 = output;
                    num = Fnv1A32.Hash(dataItem.Field.Name);
                    var str7 = num.ToString();
                    output4.WriteAttributeString("import_component_member_hash", str7);
                }
            }

            writeDataItemChildren(dataItem, isWriteChildPackage, false);
        }

        output.WriteEndElement();
    }

    private void writeDataItemChildren(
        DataItem dataItem,
        bool isWriteChildPackage,
        bool noEntities)
    {
        var templateConnectionDic = new Dictionary<DataItem, List<DataItem>>();
        foreach (var child in dataItem.Children)
        {
            if ((!isUseSelectionMode_ || child.IsChecked)
                && (!noEntities || child.Name != "entities_")
                && (dataItem is not EntityPackage package || package.PrefabDiffResourcePathList != child ||
                    package == rootGroup))
            {
                if (child is not Object && child.Parent == dataItem)
                {
                    writeDataItem(child, isWriteChildPackage);
                }
                else
                {
                    writeReference(child, dataItem, isWriteChildPackage, templateConnectionDic);
                }
            }
        }
    }

    private void writeReference(
        DataItem child,
        DataItem refOwner,
        bool isWriteChildPackage,
        Dictionary<DataItem, List<DataItem>> templateConnectionDic)
    {
        if (isWriteChildPackage && child is EntityPackage && !((EntityPackage)child).StartupLoad)
        {
            return;
        }

        output.WriteStartElement("reference");
        writeReferenceAttribute(child, refOwner, false, templateConnectionDic);
        output.WriteEndElement();
    }

    private void writeReferenceByObject(
        DataItem reference,
        DataItem refOwner,
        Object obj,
        int objectIndex,
        bool isPointerReference)
    {
        var objectType = obj.DataType as Class;
        var connectorDataItem = obj as ConnectorDataItem;
        if (!isPointerReference && connectorDataItem != null && refOwner is DynamicArray &&
            refOwner.Name == "connections_")
        {
            var num = 0;
            var sequenceContainer = obj.ParentSequenceContainer;
            if (sequenceContainer == null)
            {
                return;
            }

            var searchTargetClass =
                objectType == _baseConnectorOutClass ? _baseConnectorInClass : _baseConnectorOutClass;
            var connectorNo1 = connectorDataItem.ConnectorNo;
            foreach (var otherConnector in sequenceContainer.GetConnectorRecursive()
                         .Where(x => x.DataType is Class && x.DataType as Class == searchTargetClass))
            {
                var connectorNo2 = ((ConnectorDataItem)otherConnector).ConnectorNo;
                if (connectorNo1 == connectorNo2 && otherConnector.DataType is Class dataType6)
                {
                    var n = dataType6 == _baseConnectorOutClass ? "out_" : "in_";
                    var otherConnectorPin = otherConnector[n];
                    if (otherConnectorPin != null)
                    {
                        var connections = otherConnectorPin["connections_"];
                        if (connections != null && connections.Children.Count != 0)
                        {
                            foreach (var child in connections.Children)
                            {
                                var ownerObject = getOwnerObject(child);
                                if (ownerObject != null && obj != ownerObject && ownerObject.Name != null)
                                {
                                    var objectIndex1 = getObjectIndex(ownerObject);
                                    if (objectIndex1 >= 0)
                                    {
                                        if (num > 0)
                                        {
                                            output.WriteEndElement();
                                            output.WriteStartElement(nameof(reference));
                                        }

                                        Action(true, num, child, ownerObject, objectIndex1, objectIndex, reference,
                                            obj);
                                        ++num;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (num != 0)
            {
                return;
            }

            Action(true, 0, null, null, -1, objectIndex, reference, obj);
        }
        else
        {
            Action(false, 0, null, null, -1, objectIndex, reference, obj);
        }
    }

    private void Action(bool isReferenceWithConnector, int l_createCount, DataItem oppositeReference,
        Object oppositeObj, int oppositeObjectIndex, int objectIndex, DataItem reference, Object obj)
    {
        var objectName = !isReferenceWithConnector ? "object" : "connectorObject";
        var indexName = !isReferenceWithConnector ? nameof(objectIndex) : "connectorObjectIndex";
        var relativePathAttributeName = !isReferenceWithConnector ? "relativePath" : "connectorRelativePath";
        var relativeDynamicArrayAttributeName = !isReferenceWithConnector
            ? "relativeDynamicArray"
            : "connectorRelativeDynamicArray";
        output.WriteAttributeString(nameof(reference), "True");
        var itemPath = obj.FullPath;
        itemPath = itemPath.GetRelativePath(rootFullPath);
        var str2 = itemPath.ToString();
        output.WriteAttributeString(objectName, str2);
        output.WriteAttributeString(indexName, objectIndex.ToString());
        writeRelativeAttributeCommon(reference, obj, relativePathAttributeName, relativeDynamicArrayAttributeName);
        if (!isReferenceWithConnector)
        {
            return;
        }

        output.WriteAttributeString("connectorIndex", l_createCount.ToString());
        if (oppositeReference != null && oppositeObj != null && oppositeObjectIndex >= 0)
        {
            itemPath = oppositeObj.FullPath;
            itemPath = itemPath.GetRelativePath(rootFullPath);
            var str3 = itemPath.ToString();
            output.WriteAttributeString("object", str3);
            output.WriteAttributeString(nameof(objectIndex), oppositeObjectIndex.ToString());
            writeRelativeAttributeCommon(oppositeReference, oppositeObj, "relativePath", "relativeDynamicArray");
        }
        else
        {
            output.WriteAttributeString("object", "INVALID_CONNECTOR");
            output.WriteAttributeString(nameof(objectIndex), "-1");
        }
    }

    private void writeRelativeAttributeCommon(
        DataItem reference,
        DataItem obj,
        string relativePathAttributeName,
        string relativeDynamicArrayAttributeName)
    {
        if (atLiveEditForAddingBaseObject_)
        {
            output.WriteAttributeString(relativePathAttributeName,
                reference.RelativePathFromRootPackageForLiveEdit.ToString());
            output.WriteAttributeString(relativeDynamicArrayAttributeName,
                reference.RelativePathFromRootPackageForDynamicArray);
        }
        else
        {
            var relativePath = reference.FullPath.GetRelativePath(obj.FullPath);
            if (!relativePath.Exists)
            {
                return;
            }

            output.WriteAttributeString(relativePathAttributeName, relativePath.ToString());
        }
    }

    private void writeReferenceAttribute(
        DataItem reference,
        DataItem refOwner,
        bool isPointerReference,
        Dictionary<DataItem, List<DataItem>> templateConnectionDic = null)
    {
        if (reference == null)
        {
            return;
        }

        var ownerObject = getOwnerObject(reference);
        if (ownerObject == null)
        {
            return;
        }

        if (ownerObject.Name == null)
        {
            var str = "Jenova: error: referenceのobjectのNameがnullでした : " + ownerObject.FullPath;
            // DocumentInterface.DocumentContainer.LogAppendCurrentParagraphParse(
            //     "<Span Foreground=\"Red\">XmlWriter : writeReferenceAttribute : " + str + "</Span><LineBreak />");
            Console.Error.WriteLine(str);
        }
        else
        {
            var objectIndex = getObjectIndex(ownerObject);
            if (objectIndex >= 0)
            {
                writeReferenceByObject(reference, refOwner, ownerObject, objectIndex, isPointerReference);
            }
            else
            {
                this.output.WriteAttributeString(nameof(reference), "True");
                if (isPointerReference)
                {
                    var flag = false;
                    ItemPath itemPath;
                    if (isUseSelectionMode_ && !ownerObject.IsChecked && !useUnresolvedPointerReference_)
                    {
                        var output = this.output;
                        itemPath = ownerObject.FullPath;
                        itemPath = itemPath.GetRelativePath(rootFullPath);
                        var str = itemPath.ToString();
                        output.WriteAttributeString("object", str);
                        var referenceErrorList = ReferenceErrorList;
                        itemPath = ownerObject.FullPath;
                        var writerReferenceError = new WriterReferenceError(itemPath.ToString(), true);
                        referenceErrorList.Add(writerReferenceError);
                        flag = true;
                    }

                    if (!flag && ownerObject.Disposed)
                    {
                        var output = this.output;
                        itemPath = ownerObject.FullPath;
                        itemPath = itemPath.GetRelativePath(rootFullPath);
                        var str = itemPath.ToString();
                        output.WriteAttributeString("object", str);
                        var referenceErrorList = ReferenceErrorList;
                        itemPath = ownerObject.FullPath;
                        var writerReferenceError = new WriterReferenceError(itemPath.ToString());
                        referenceErrorList.Add(writerReferenceError);
                        flag = true;
                    }

                    if (!flag)
                    {
                        if (refOwner.Field is {IsFarReference: true})
                        {
                            var parentLoadUnitPackage = ownerObject.ParentLoadUnitPackage;
                            if (parentLoadUnitPackage != null)
                            {
                                output.WriteAttributeString("FarReference", "true");
                                var output1 = output;
                                itemPath = ownerObject.FullPath;
                                itemPath = itemPath.GetRelativePath(parentLoadUnitPackage.FullPath);
                                var str1 = itemPath.ToString();
                                output1.WriteAttributeString("object", str1);
                                output.WriteAttributeString("UnresolvedPointerPackageSource",
                                    Project.GetDataRelativePath(parentLoadUnitPackage.FullFilePath));
                                var parent = refOwner.Parent;
                                var parentPackage = refOwner.ParentPackage;
                                var output2 = output;
                                itemPath = parent.FullPath;
                                itemPath = itemPath.GetRelativePath(parentPackage.FullPath);
                                var str2 = itemPath.ToString();
                                output2.WriteAttributeString("ReferenceSourceItemPath", str2);
                            }
                            else
                            {
                                var packageWithoutPrefab = ownerObject.ParentPackageWithoutPrefab;
                                var output = this.output;
                                itemPath = ownerObject.FullPath;
                                itemPath = itemPath.GetRelativePath(packageWithoutPrefab.FullPath);
                                var str = itemPath.ToString();
                                output.WriteAttributeString("object", str);
                                this.output.WriteAttributeString("UnresolvedPointerPackageSource",
                                    Project.GetDataRelativePath(packageWithoutPrefab.FullFilePath));
                            }
                        }
                        else
                        {
                            var packageWithoutPrefab = ownerObject.ParentPackageWithoutPrefab;
                            var output = this.output;
                            itemPath = ownerObject.FullPath;
                            itemPath = itemPath.GetRelativePath(packageWithoutPrefab.FullPath);
                            var str = itemPath.ToString();
                            output.WriteAttributeString("object", str);
                            this.output.WriteAttributeString("UnresolvedPointerPackageSource",
                                Project.GetDataRelativePath(packageWithoutPrefab.FullFilePath));
                        }

                        this.output.WriteAttributeString("UseUnresolvedPointerReference", "true");
                    }
                }
                else
                {
                    var ownerPackage = ownerObject.ParentPackage;
                    var referenceOwnerPackage = refOwner.ParentPackage;
                    var flag1 = ownerPackage is Prefab &&
                                referenceOwnerPackage.ContainsAsSubPackage(ownerPackage, true);
                    var trayCondition = GetRootTrayInTemplateTrayPackage(ownerPackage) != null ||
                                        GetRootTrayInTemplateTrayPackage(referenceOwnerPackage) != null;
                    if (ownerPackage != null && flag1 | trayCondition && ownerPackage != referenceOwnerPackage)
                    {
                        if (trayCondition)
                        {
                            var output3 = output;
                            var itemPath = ownerObject.FullPath;
                            itemPath = itemPath.GetRelativePath(ownerPackage.FullPath);
                            var str3 = itemPath.ToString();
                            output3.WriteAttributeString("object", str3);
                            output.WriteAttributeString("PrefabOwnerPackageSource",
                                Project.GetDataRelativePath(ownerPackage.FullFilePath));
                            var parent = refOwner.Parent;
                            var output4 = output;
                            itemPath = parent.FullPath;
                            itemPath = itemPath.GetRelativePath(referenceOwnerPackage.FullPath);
                            var str4 = itemPath.ToString();
                            output4.WriteAttributeString("PrefabConnectionSourceItemPath", str4);
                            if (GetRootTrayInTemplateTrayPackage(ownerPackage) == null)
                            {
                                if (GetRootTrayInTemplateTrayPackage(referenceOwnerPackage) == null)
                                {
                                    ;
                                }
                            }
                            else if (ownerObject is TrayDataItem trayDataItem11)
                            {
                                foreach (var child in refOwner.Children)
                                {
                                    if (child != reference)
                                    {
                                        if (templateConnectionDic != null)
                                        {
                                            if (templateConnectionDic.ContainsKey(reference))
                                            {
                                                if (templateConnectionDic[reference].Contains(child))
                                                {
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                templateConnectionDic[reference] = new List<DataItem>();
                                            }
                                        }

                                        if (child.Name == reference.Name &&
                                            (trayDataItem11.TemplateTrayItemList.Contains(child.Parent.Parent) ||
                                             trayDataItem11.MenuLogicItemList.Contains(child.Parent.Parent)))
                                        {
                                            var output5 = output;
                                            var name = ownerPackage.Name;
                                            itemPath = child.Parent.Parent.RelativePathFromPackage;
                                            var str5 = itemPath.ToString();
                                            var str6 = name + "(" + str5 + ")";
                                            output5.WriteAttributeString("ProxyConnectionOwnerPackageName", str6);
                                            if (templateConnectionDic != null)
                                            {
                                                templateConnectionDic[reference].Add(child);
                                            }

                                            break;
                                        }
                                    }
                                }
                            }

                            output.WriteAttributeString("UseTemplateConnection", "true");
                        }
                        else
                        {
                            var output6 = output;
                            var itemPath = ownerObject.FullPath;
                            itemPath = itemPath.GetRelativePath(referenceOwnerPackage.FullPath);
                            var str7 = itemPath.ToString();
                            output6.WriteAttributeString("object", str7);
                            output.WriteAttributeString("PrefabOwnerPackageSource",
                                Project.GetDataRelativePath(referenceOwnerPackage.FullFilePath));
                            var parent = refOwner.Parent;
                            var output7 = output;
                            itemPath = parent.FullPath;
                            itemPath = itemPath.GetRelativePath(referenceOwnerPackage.FullPath);
                            var str8 = itemPath.ToString();
                            output7.WriteAttributeString("PrefabConnectionSourceItemPath", str8);
                            output.WriteAttributeString("ProxyConnectionOwnerPackageName", referenceOwnerPackage.Name);
                            output.WriteAttributeString("UsePrefabConnectionV20", "true");
                        }
                    }
                    else
                    {
                        var output = this.output;
                        var itemPath = ownerObject.FullPath;
                        itemPath = itemPath.GetRelativePath(rootFullPath);
                        var str = itemPath.ToString();
                        output.WriteAttributeString("object", str);
                        Console.Error.WriteLine(
                            "Jenova: error: reference object is not found, so objectIndex cannot be set : obj.FullPath=",
                            ownerObject.FullPath);
                        var referenceErrorList = ReferenceErrorList;
                        itemPath = ownerObject.FullPath;
                        var writerReferenceError = new WriterReferenceError(itemPath.ToString());
                        referenceErrorList.Add(writerReferenceError);
                    }
                }

                writeRelativeAttributeCommon(reference, ownerObject, "relativePath", "relativeDynamicArray");
            }
        }
    }

    private int getObjectIndex(Object obj)
    {
        var num = 0;
        foreach (var @object in Objects)
        {
            if (@object == obj)
            {
                return num;
            }

            ++num;
        }

        return -1;
    }

    private Object getOwnerObject(DataItem item)
    {
        while (true)
        {
            switch (item)
            {
                case null:
                case Object _:
                    goto label_3;
                default:
                    item = item.Parent;
                    continue;
            }
        }

        label_3:
        return item as Object;
    }

    private uint getFixid(string table, string name, bool noUpper, string prefix = null)
    {
        return GetFixidCommon(table, name, noUpper, prefix);
    }

    public static uint GetFixidCommon(
        string table,
        string name,
        bool noUpper,
        string prefix = null)
    {
        throw new NotImplementedException();
        // var repository = FixidRepository.Instance;
        // var fixid = repository[table].FirstOrDefault(r =>
        //     r.Prefix.Equals(prefix, StringComparison.OrdinalIgnoreCase)
        //     && r.Label.Equals(name, StringComparison.OrdinalIgnoreCase));
        //
        // if (fixid != null)
        // {
        //     return fixid.Fixid;
        // }
        //
        // throw new Exception($"Fixid for {name} in table {table} was not found in the fixid database");
    }

    private uint createFixid(string table, string name, bool noUpper, string prefix = null)
    {
        throw new NotImplementedException("It is not currently possible to create your own fixids");

        if (!string.IsNullOrEmpty(prefix))
        {
            name = prefix + name;
        }

        if (!noUpper)
        {
            name = name.ToUpper();
        }

        // if (!string.IsNullOrEmpty(prefix))
        // {
        //     Fixid.SetUseCache(true, true);
        //     Fixid.SetGlobalCachePrefix(prefix);
        // }
        // else
        // {
        //     Fixid.SetUseCache(false, true);
        // }
        //
        // var num = (int)Fixid.AddID(table, name);
        var num = 12345;
        if (num != 0)
        {
            return (uint)num;
        }

        // DocumentInterface.DocumentContainer.PrintWarning(
        //     string.Format("Jenova: error: fixidデータベース{0}にラベル{1}を追加できませんでした。", table, name));
        return (uint)num;
    }

    public static TrayDataItem GetRootTrayInTemplateTrayPackage(EntityPackage package)
    {
        if (package == null)
        {
            return null;
        }

        var templateTrayPackage = getSequenceContainerInTemplateTrayPackage(package);
        if (templateTrayPackage == null)
        {
            return null;
        }

        var trayDataItemList = new List<TrayDataItem>();
        foreach (var node in templateTrayPackage.Nodes)
        {
            if (node is TrayDataItem item)
            {
                trayDataItemList.Add(item);
            }
        }

        foreach (var trayDataItem in trayDataItemList)
        {
            if (trayDataItem.TemplateTrayItemList.Count > 0 || trayDataItem.MenuLogicItemList.Count > 0)
            {
                if (trayDataItemList.Count > 1)
                {
                    // DocumentInterface.DocumentContainer.LogAppendCurrentParagraphParse(
                    //     "<Span Foreground=\"Red\">XmlWriter : TemplateTray : " +
                    //     "TemplateTrayの中身のSequence直下に複数のTrayがあります。" + "</Span><LineBreak />");
                }

                return trayDataItem;
            }
        }

        return null;
    }

    private static SequenceContainer getSequenceContainerInTemplateTrayPackage(
        EntityPackage package)
    {
        var result = new List<DataItem>();
        if (package.Entities == null)
        {
            return null;
        }

        package.AccumulateEntitiesWithTypeFullName(result, "SQEX.Ebony.Framework.Sequence.SequenceContainer",
            false);
        return result.Count > 0 ? result[0] as SequenceContainer : null;
    }

    public static TrayDataItem GetRootTrayInMenuLogicTrayPackage(EntityPackage package)
    {
        if (package == null)
        {
            return null;
        }

        var templateTrayPackage = getSequenceContainerInTemplateTrayPackage(package);
        if (templateTrayPackage == null)
        {
            return null;
        }

        var trayDataItemList = new List<TrayDataItem>();
        foreach (var node in templateTrayPackage.Nodes)
        {
            if (node is TrayDataItem)
            {
                trayDataItemList.Add((TrayDataItem)node);
            }
        }

        foreach (var trayDataItem in trayDataItemList)
        {
            if (trayDataItem.MenuLogicItemList.Count > 0)
            {
                if (trayDataItemList.Count > 1)
                {
                    // DocumentInterface.DocumentContainer.LogAppendCurrentParagraphParse(
                    //     "<Span Foreground=\"Red\">XmlWriter : TemplateTray : " +
                    //     "TemplateTrayの中身のSequence直下に複数のTrayがあります。" + "</Span><LineBreak />");
                }

                return trayDataItem;
            }
        }

        return null;
    }

    public class WriterReferenceError
    {
        public WriterReferenceError(string itemPathString, bool isUncheckedReference = false)
        {
            ItemPathString = itemPathString;
            IsUncheckedReference = isUncheckedReference;
        }

        public string ItemPathString { get; }

        public bool IsUncheckedReference { get; }
    }
}