using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Flagrum.Core.Persistence;
using Flagrum.Core.Scripting.Ebex.Data;
using Flagrum.Core.Scripting.Ebex.Type;
using Flagrum.Core.Utilities.Exceptions;
using Enum = Flagrum.Core.Scripting.Ebex.Type.Enum;
using Object = Flagrum.Core.Scripting.Ebex.Data.Object;

namespace Flagrum.Core.Scripting.Ebex;

public class EbonyEntityXmlWriter : IDisposable
{
    private readonly List<Object> _objects = new();
    private readonly EntityPackage _package;
    private readonly ItemPath _rootFullPath;
    private readonly Stream _stream;
    private readonly StreamWriter _streamWriter;
    private readonly XmlWriter _writer;
    private readonly bool _writeSlim = true;

    private int _index;

    public EbonyEntityXmlWriter(Stream stream, DataItem item)
    {
        if (item is EntityPackage package)
        {
            _package = package;
        }

        _rootFullPath = new ItemPath();
        BuildObjectListRecursive(item);

        _stream = stream;
        _streamWriter = new StreamWriter(_stream, Encoding.UTF8);
        _writer = XmlWriter.Create(_streamWriter, new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            IndentChars = "\t",
            OmitXmlDeclaration = true
        });
    }

    public EbonyEntityXmlWriter(Stream stream, EntityPackage package)
    {
        _package = package;
        _rootFullPath = new ItemPath();
        BuildObjectListRecursive(package);

        _stream = stream;
        _streamWriter = new StreamWriter(_stream, Encoding.UTF8);
        _writer = XmlWriter.Create(_streamWriter, new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            IndentChars = "\t",
            OmitXmlDeclaration = true
        });
    }

    private Class BaseInputConnectorClass =>
        DocumentInterface.ModuleContainer["SQEX.Ebony.Framework.Sequence.Connector.SequenceConnectorIn"] as Class;

    private Class BaseOutputConnectorClass =>
        DocumentInterface.ModuleContainer["SQEX.Ebony.Framework.Sequence.Connector.SequenceConnectorOut"] as Class;

    public void Dispose()
    {
        _writer?.Dispose();
        _streamWriter?.Dispose();
        _stream?.Dispose();
    }

    public static byte[] WritePackage(EntityPackage package)
    {
        using var stream = new MemoryStream();
        using (var writer = new EbonyEntityXmlWriter(stream, package))
        {
            writer.Write();
        }

        return stream.ToArray();
    }

    public void Write()
    {
        _writer.WriteStartElement("package");
        _writer.WriteAttributeString("name", _package.Name);
        WriteObjects();
        _writer.WriteEndElement();
    }

    public void WriteObjects()
    {
        _writer.WriteStartElement("objects");
        _objects.ForEach(WriteObject);
        _writer.WriteEndElement();
    }

    public void WriteObject(Object item)
    {
        _writer.WriteStartElement("object");
        _writer.WriteAttributeString("objectIndex", _index++.ToString());
        _writer.WriteAttributeString("name", item.Name);

        var typeName = item.DataType.FullName;
        var shouldWriteEntities = true;

        if (item is EntityPackageReference reference)
        {
            if (reference != _package)
            {
                _writer.WriteAttributeString("original_type", item.DataType.FullName);
                typeName = EntityPackageReference.ClassFullName;
                shouldWriteEntities = false;
            }
        }

        _writer.WriteAttributeString("type", typeName);

        var path = item.ParentPackage == null || item == _package
            ? _rootFullPath
            : item.ParentPackage.FullPath;

        _writer.WriteAttributeString("path", item.FullPath.GetRelativePath(path).ToString());
        _writer.WriteAttributeString("checked", item is not EntityPackage
                                                || item != _package
            ? item.IsChecked.ToString()
            : "True");

        var owner = GetOwner(item.Parent);
        if (owner != null)
        {
            var objectIndex = _objects.IndexOf(owner);
            if (objectIndex >= 0)
            {
                var parent = item.FullPath.GetRelativePath(owner.FullPath).GetParent();
                path = owner.ParentPackage == null || owner == _package
                    ? _rootFullPath
                    : owner.ParentPackage.FullPath;

                _writer.WriteAttributeString("owner", owner.FullPath.GetRelativePath(path).ToString());
                _writer.WriteAttributeString("ownerIndex", objectIndex.ToString());
                _writer.WriteAttributeString("ownerPath", parent.ToString());
            }
        }

        WriteDataItemChildren(item, shouldWriteEntities);
        _writer.WriteEndElement();
    }

    private void WriteDataItemChildren(DataItem item, bool shouldWriteEntities)
    {
        foreach (var child in item.Children)
        {
            if ((shouldWriteEntities || child.Name != "entities_")
                && (item is not EntityPackage package
                    || package.PrefabDiffResourcePathList != child
                    || package == _package))
            {
                if (child is not Object && child.Parent == item)
                {
                    WriteDataItem(child);
                }
                else
                {
                    WriteReference(child, item);
                }
            }
        }
    }

    private void WriteDataItem(DataItem item)
    {
        string localName;
        string typeName = null;

        if (item.Field != null)
        {
            if (item.Field.Transient)
            {
                return;
            }

            localName = item.Field.Name;
            typeName = item.Field.TypeName;
        }
        else
        {
            localName = item.DataType == null ? item.Name : item.DataType.FullName;
        }

        if (_writeSlim && CanSkip(item))
        {
            return;
        }

        if (item.Children.Count == 0 && item.Value != null)
        {
            if (item.Value is Value value)
            {
                _writer.WriteStartElement(localName);

                if (item.IsDynamic)
                {
                    _writer.WriteAttributeString("dynamic", "true");
                    _writer.WriteAttributeString("name", item.DisplayName);
                }

                switch (value.PrimitiveType)
                {
                    case PrimitiveType.String:
                        if (item.Field?.GetAttribute("FileExtension") != null
                            && item.Field.GetAttribute("CharaEntryRef") != null)
                        {
                            _writer.WriteAttributeString("CharaEntryRef", "true");
                        }

                        if (item.Field?.GetAttribute("AutoGeneratedByItemPath") != null)
                        {
                            if (item.ParentPackage is Prefab)
                            {
                                if (item.Parent is EntityDiffGroup diffGroup)
                                {
                                    var diffTarget = diffGroup.GetDiffTargetItem();
                                    var diffPath = diffTarget != null
                                        ? diffTarget.CreateAutoGeneratedItemPathFromParent()
                                        : item.CreateAutoGeneratedItemPath();

                                    item.Value = new Value(diffPath);
                                }
                                else
                                {
                                    item.Value = new Value(string.Empty);
                                }
                            }
                            else
                            {
                                item.Value = new Value(item.CreateAutoGeneratedItemPath());
                            }
                        }

                        break;
                    case PrimitiveType.Fixid:
                        if (item.Field != null)
                        {
                            item.Field.TryGetAttributeBool("FixidFlexible", out var fixidFlexible);
                            const string baseClass =
                                "SQEX.Ebony.Framework.Sequence.Variable.Primitive.SequenceFlexibleFixidData";

                            var table = string.Empty;
                            var prefix = string.Empty;
                            var isNoUpper = false;
                            var isLower = false;

                            if (fixidFlexible)
                            {
                                if (item.Parent is {DataType: Class @class}
                                    && @class.IsBasedOn((Class) DocumentInterface.ModuleContainer[baseClass]))
                                {
                                    var tableItem = item.Parent["table_"];
                                    if (tableItem is {Value: not null})
                                    {
                                        table = tableItem.Value.ToString();
                                        isNoUpper = table is "model_joint" or "swf";
                                    }

                                    var prefixItem = item.Parent["prefix_"];
                                    if (prefixItem is {Value: not null})
                                    {
                                        prefix = prefixItem.Value.ToString();
                                    }
                                }
                            }
                            else
                            {
                                table = item.Field.GetAttribute("FixidTable");
                                prefix = item.Field.GetAttribute("FixidPrefix");
                                item.Field.TryGetAttributeBool("FixidNoUpper", out isNoUpper);
                                item.Field.TryGetAttributeBool("FixidLower", out isLower);
                            }

                            if (table != null)
                            {
                                var label = value.ToString();
                                if (!string.IsNullOrEmpty(label) && label != "0" && label != "__PLACE_HOLDER__")
                                {
                                    if (isLower)
                                    {
                                        label = label.ToLower();
                                    }

                                    var fixid = GetFixid(table, prefix, label).ToString();
                                    _writer.WriteAttributeString("fixid", fixid);
                                }
                            }
                        }

                        break;
                    case PrimitiveType.Pointer:
                        WriteReferenceAttribute(value.GetPointer(), item, true);
                        break;
                    case PrimitiveType.Enum:
                        if (item.Field != null)
                        {
                            _writer.WriteAttributeString("value", ((Enum) item.Field.DataType)
                                .ValueOf(item.Value.ToString()).ToString());
                        }

                        break;
                }

                typeName = value.TypeName;
            }
            else
            {
                _writer.WriteStartElement(localName);
            }

            if (typeName != null)
            {
                _writer.WriteAttributeString("type", typeName);
            }

            _writer.WriteValue(item.Value.ToString());
        }
        else
        {
            _writer.WriteStartElement(localName);
            if (typeName != null)
            {
                if (item.IsDynamic)
                {
                    _writer.WriteAttributeString("dynamic", "true");
                    _writer.WriteAttributeString("type", typeName);
                }
            }

            WriteDataItemChildren(item, true);
        }

        _writer.WriteEndElement();
    }

    private void WriteReference(DataItem reference, DataItem referenceOwner)
    {
        _writer.WriteStartElement("reference");
        WriteReferenceAttribute(reference, referenceOwner, false);
        _writer.WriteEndElement();
    }

    private void WriteReferenceAttribute(DataItem reference, DataItem referenceOwner, bool isPointerReference)
    {
        var owner = GetOwner(reference);

        if (owner?.Name != null)
        {
            var objectIndex = _objects.IndexOf(owner);
            if (objectIndex >= 0)
            {
                WriteReferenceByObject(reference, referenceOwner, owner, objectIndex, isPointerReference);
            }
            else
            {
                _writer.WriteAttributeString("reference", "True");
                if (isPointerReference)
                {
                    if (owner.Disposed)
                    {
                        var itemPath = owner.FullPath.GetRelativePath(_rootFullPath);
                        _writer.WriteAttributeString("object", itemPath.ToString());
                    }
                    else
                    {
                        if (referenceOwner.Field is {IsFarReference: true})
                        {
                            if (owner.ParentLoadUnitPackage == null)
                            {
                                var package = owner.ParentPackageWithoutPrefab;
                                var itemPath = owner.FullPath.GetRelativePath(package.FullPath);
                                _writer.WriteAttributeString("object", itemPath.ToString());
                                _writer.WriteAttributeString("UnresolvedPointerPackageSource",
                                    EbexUtility.AbsolutePathToRelativePath(package.FullFilePath));
                            }
                        }
                        else
                        {
                            try
                            {
                                var package = owner.ParentPackageWithoutPrefab;
                                var itemPath = owner.FullPath.GetRelativePath(package.FullPath);
                                _writer.WriteAttributeString("object", itemPath.ToString());
                                _writer.WriteAttributeString("UnresolvedPointerPackageSource",
                                    EbexUtility.AbsolutePathToRelativePath(package.FullFilePath));
                            }
                            catch
                            {
                            }
                        }

                        _writer.WriteAttributeString("UseUnresolvedPointerReference", "true");
                    }
                }
                else
                {
                    var ownerPackage = owner.ParentPackage;
                    var referenceOwnerPackage = referenceOwner.ParentPackage;

                    var prefabCondition = ownerPackage is Prefab
                                          && referenceOwnerPackage.ContainsAsSubPackage(ownerPackage, true);
                    var trayCondition = GetRootTrayInTemplateTrayPackage(ownerPackage) != null
                                        || GetRootTrayInTemplateTrayPackage(referenceOwnerPackage) != null;

                    if (ownerPackage != null
                        && prefabCondition | trayCondition
                        && ownerPackage != referenceOwnerPackage)
                    {
                        if (trayCondition)
                        {
                            var itemPath = owner.FullPath.GetRelativePath(ownerPackage.FullPath);
                            _writer.WriteAttributeString("object", itemPath.ToString());
                            _writer.WriteAttributeString("PrefabOwnerPackageSource",
                                EbexUtility.AbsolutePathToRelativePath(ownerPackage.FullFilePath));

                            itemPath = referenceOwner.Parent.FullPath.GetRelativePath(referenceOwnerPackage.FullPath);
                            _writer.WriteAttributeString("PrefabConnectionSourceItemPath", itemPath.ToString());

                            if (GetRootTrayInTemplateTrayPackage(ownerPackage) != null
                                && owner is TrayDataItem tray)
                            {
                                foreach (var child in referenceOwner.Children
                                             .Where(child => child != reference
                                                             && child.Name == reference.Name
                                                             && (tray.TemplateTrayItemList.Contains(child.Parent.Parent)
                                                                 || tray.MenuLogicItemList
                                                                     .Contains(child.Parent.Parent))))
                                {
                                    itemPath = child.Parent.Parent.RelativePathFromPackage;
                                    var proxy = $"{ownerPackage.Name}({itemPath.ToString()})";
                                    _writer.WriteAttributeString("ProxyConnectionOwnerPackageName", proxy);
                                    break;
                                }
                            }

                            _writer.WriteAttributeString("UseTemplateConnection", "true");
                        }
                        else
                        {
                            var itemPath = owner.FullPath.GetRelativePath(referenceOwnerPackage.FullPath);
                            _writer.WriteAttributeString("object", itemPath.ToString());
                            _writer.WriteAttributeString("PrefabOwnerPackageSource",
                                EbexUtility.AbsolutePathToRelativePath(referenceOwnerPackage.FullFilePath));

                            itemPath = referenceOwner.Parent.FullPath.GetRelativePath(referenceOwnerPackage.FullPath);
                            _writer.WriteAttributeString("PrefabConnectionSourceItemPath", itemPath.ToString());
                            _writer.WriteAttributeString("ProxyConnectionOwnerPackageName", referenceOwnerPackage.Name);
                            _writer.WriteAttributeString("UsePrefabConnectionV20", "true");
                        }
                    }
                    else
                    {
                        var itemPath = owner.FullPath.GetRelativePath(_rootFullPath);
                        _writer.WriteAttributeString("object", itemPath.ToString());
                    }
                }

                WriteRelativeAttributeCommon(reference, owner, "relativePath", "relativeDynamicArray");
            }
        }
    }

    private void WriteReferenceByObject(DataItem reference, DataItem referenceOwner, Object item, int itemIndex,
        bool isPointerReference)
    {
        var objectType = item.DataType as Class;

        if (!isPointerReference
            && item is ConnectorDataItem connector
            && referenceOwner is DynamicArray
            && referenceOwner.Name == "connections_")
        {
            var sequenceContainer = item.ParentSequenceContainer;
            if (sequenceContainer == null)
            {
                return;
            }

            var searchTargetClass = objectType == BaseOutputConnectorClass
                ? BaseInputConnectorClass
                : BaseOutputConnectorClass;

            var counter = 0;
            foreach (var otherConnector in sequenceContainer.GetConnectorRecursive()
                         .Where(c => c.DataType is Class t && t == searchTargetClass)
                         .Cast<ConnectorDataItem>())
            {
                if (connector.ConnectorNo == otherConnector.ConnectorNo)
                {
                    var direction = searchTargetClass == BaseOutputConnectorClass ? "out_" : "in_";
                    var otherConnectorPin = otherConnector[direction];
                    var connections = otherConnectorPin?["connections_"];
                    if (connections != null && connections.Children.Count != 0)
                    {
                        foreach (var child in connections.Children)
                        {
                            var owner = GetOwner(child);
                            if (owner != null && item != owner && owner.Name != null)
                            {
                                var index = _objects.IndexOf(owner);
                                if (index >= 0)
                                {
                                    if (counter > 0)
                                    {
                                        _writer.WriteEndElement();
                                        _writer.WriteStartElement("reference");
                                    }

                                    WriteConnection(true, counter, child, owner, index, itemIndex, reference, item);
                                    counter++;
                                }
                            }
                        }
                    }
                }
            }

            if (counter == 0)
            {
                WriteConnection(true, 0, null, null, -1, itemIndex, reference, item);
            }
        }
        else
        {
            WriteConnection(false, 0, null, null, -1, itemIndex, reference, item);
        }
    }

    private void WriteConnection(bool isReferenceWithConnector, int lCreateCount, DataItem oppositeReference,
        Object oppositeObject, int oppositeObjectIndex, int objectIndex, DataItem reference, Object @object)
    {
        var objectName = isReferenceWithConnector ? "connectorObject" : "object";
        var indexName = isReferenceWithConnector ? "connectorObjectIndex" : "objectIndex";
        var relativePathName = isReferenceWithConnector ? "connectorRelativePath" : "relativePath";
        var dynamicArrayName = isReferenceWithConnector ? "connectorRelativeDynamicArray" : "relativeDynamicArray";
        var itemPath = @object.FullPath.GetRelativePath(_rootFullPath).ToString();
        _writer.WriteAttributeString("reference", "True");
        _writer.WriteAttributeString(objectName, itemPath);
        _writer.WriteAttributeString(indexName, objectIndex.ToString());
        WriteRelativeAttributeCommon(reference, @object, relativePathName, dynamicArrayName);

        if (isReferenceWithConnector)
        {
            _writer.WriteAttributeString("connectorIndex", lCreateCount.ToString());
            if (oppositeReference != null && oppositeObject != null && oppositeObjectIndex >= 0)
            {
                itemPath = oppositeObject.FullPath.GetRelativePath(_rootFullPath).ToString();
                _writer.WriteAttributeString("object", itemPath);
                _writer.WriteAttributeString("objectIndex", oppositeObjectIndex.ToString());
                WriteRelativeAttributeCommon(oppositeReference, oppositeObject, "relativePath", "relativeDynamicArray");
            }
            else
            {
                _writer.WriteAttributeString("object", "INVALID_CONNECTOR");
                _writer.WriteAttributeString("objectIndex", "-1");
            }
        }
    }

    private void WriteRelativeAttributeCommon(DataItem reference, DataItem item, string relativePathName,
        string dynamicArrayName)
    {
        var relativePath = reference.FullPath.GetRelativePath(item.FullPath);
        if (relativePath.Exists)
        {
            _writer.WriteAttributeString(relativePathName, relativePath.ToString());
        }
    }

    public void BuildObjectListRecursive(DataItem item)
    {
        if (item is Object @object)
        {
            if (_objects.Contains(@object))
            {
                return;
            }

            _objects.Add(@object);
        }

        if (item == _package || item is not EntityPackageReference)
        {
            foreach (var child in item.Children.Where(c => c.Parent == item))
            {
                BuildObjectListRecursive(child);
            }
        }
    }

    private uint GetFixid(string table, string prefix, string label)
    {
        var fixid = FixidRepository.Instance.Get(table, prefix, label);

        if (fixid != null)
        {
            return fixid.Fixid;
        }

        throw new MissingFixidException($"Fixid for {label} in table {table} was not found in the fixid database");
    }

    private Object GetOwner(DataItem item)
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

    private TrayDataItem GetRootTrayInTemplateTrayPackage(EntityPackage package)
    {
        if (package == null)
        {
            return null;
        }

        var templateTrayPackage = GetSequenceContainerInTemplateTrayPackage(package);

        var trays = templateTrayPackage?.Nodes.Where(n => n is TrayDataItem).Cast<TrayDataItem>().ToList();
        return trays?.FirstOrDefault(t => t.TemplateTrayItemList.Count > 0 || t.MenuLogicItemList.Count > 0);
    }

    private SequenceContainer GetSequenceContainerInTemplateTrayPackage(EntityPackage package)
    {
        if (package.Entities != null)
        {
            var results = new List<DataItem>();
            package.AccumulateEntitiesWithTypeFullName(results, SequenceContainer.ClassFullName, false);
            return results.Count > 0 ? results[0] as SequenceContainer : null;
        }

        return null;
    }

    private bool CanSkip(DataItem item)
    {
        if (item.Parent is DynamicArray
            || item.IsDynamic
            || item.Field?.GetAttribute("UseReferenceValue") != null
            || (item.Parent != null
                && (item.Parent.DataType is not Class dataType
                    || dataType.GetDefaultValueOverride(item.Name) != null)))
        {
            return false;
        }

        if (item is DynamicArray array)
        {
            return array.Count == 0;
        }

        var value = item.Value as Value;
        var defaultValue = item.GetDefaultValue() as Value;
        if (value == null
            || defaultValue == null
            || value.PrimitiveType == PrimitiveType.Enum
            || (value.PrimitiveType != PrimitiveType.String
                && string.IsNullOrEmpty(item.DataType.DefaultValueString)))
        {
            return false;
        }

        if (value.PrimitiveType is PrimitiveType.None or PrimitiveType.Fixid
            or PrimitiveType.Pointer or PrimitiveType.ObjectReference)
        {
            return false;
        }

        if (value.PrimitiveType == PrimitiveType.String && defaultValue.PrimitiveType == PrimitiveType.String)
        {
            if ((value.Object as string == string.Empty && defaultValue.Object == null)
                || (value.Object is not string && defaultValue.Object == null))
            {
                return true;
            }

            if (value.Object is not string)
            {
                return false;
            }
        }

        return defaultValue.Object != null && value.Equals(defaultValue);
    }
}