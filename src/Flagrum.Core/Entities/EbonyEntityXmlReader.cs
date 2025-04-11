using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Flagrum.Core.Scripting.Ebex;
using Flagrum.Core.Scripting.Ebex.Data;
using Flagrum.Core.Scripting.Ebex.Type;
using Flagrum.Core.Utilities.Extensions;
using Object = Flagrum.Core.Scripting.Ebex.Data.Object;

namespace Flagrum.Core.Entities;

public class EbonyEntityXmlReader
{
    public enum ReadMode
    {
        Standard,
        Recursive
    }

    private static ConcurrentDictionary<string, bool> HasReadPackages;
    private static ConcurrentDictionary<string, Object> Packages;

    private readonly ObjectCollection _objects = new();
    private readonly List<Action> _placeholderDelegates = new();
    private readonly DataItem _rootDataItem;
    private readonly List<Task> _tasks = new();
    private ReadMode _mode;
    private List<XmlElement> _objectElements = null!;
    private string _readPath;
    private string _readVirtualPath;

    public EbonyEntityXmlReader(string dataDirectory, EntityGroup parent = null)
    {
        if (parent == null)
        {
            HasReadPackages = new ConcurrentDictionary<string, bool>();
            Packages = new ConcurrentDictionary<string, Object>();
        }

        EbexUtility.DataDirectory = dataDirectory.Replace('\\', '/').ToLower();
        _rootDataItem = parent?.Entities;
    }

    private string GetPath(string path, Func<string, string> getAlternatePath)
    {
        if (getAlternatePath == null)
        {
            return path;
        }

        var alternatePath = getAlternatePath(path);
        return File.Exists(alternatePath) ? alternatePath : path;
    }

    public Object Read(string xmlPath, ReadMode mode, bool isRootPackage = true,
        Func<string, string> getAlternatePath = null)
    {
        _readPath = xmlPath.Replace('\\', '/').ToLower();
        var stream = new FileStream(GetPath(_readPath, getAlternatePath), FileMode.Open, FileAccess.Read);
        return Read(stream, mode, isRootPackage, getAlternatePath);
    }

    public Object Read(Stream stream, ReadMode mode, bool isRootPackage, Func<string, string> getAlternatePath)
    {
        _mode = mode;

        var xmlDocument = new XmlDocument();
        xmlDocument.Load(stream);
        stream.Dispose();

        // Check that this is an Entity Package
        var objectsElement = xmlDocument.DocumentElement?.Name switch
        {
            "package" => xmlDocument.DocumentElement["objects"],
            "objects" => xmlDocument.DocumentElement,
            _ => throw new Exception("Unable to read XML document, are you sure this is an Ebony Entity XML file?")
        };

        // Get a list of all objects in the document
        _objectElements = objectsElement.GetElements("object");
        var elementList = new List<XmlElement>();

        if (_mode == ReadMode.Standard && !isRootPackage)
        {
            elementList.Add(_objectElements[0]);
        }
        else
        {
            elementList = _objectElements;
        }

        // Read all objects
        elementList.ForEach(e => ReadObject(e, getAlternatePath));

        Task.WaitAll(_tasks.ToArray());
        _placeholderDelegates.ForEach(d => d());

        // Handle dynamic objects
        for (var i = 0; i < _objects.Count; i++)
        {
            ReadDynamicObjects(elementList[i], _objects[i]);
        }

        // Handle references
        for (var i = 0; i < _objects.Count; i++)
        {
            ReadReferences(elementList[i], _objects[i]);
        }

        if (_objects.Count > 0 && _objects[0] is EntityPackage)
        {
            ResolveDifferences(_objects[0]);
        }

        return _objects[0];
    }

    private void ReadObject(XmlElement element, Func<string, string> getAlternatePath)
    {
        // Read the object attributes
        var index = element.GetIntegerAttribute("objectIndex", -1);
        var name = element.GetStringAttribute("name");
        var type = element.GetStringAttribute("type");
        var ownerIndex = element.GetIntegerAttribute("ownerIndex", -1);
        var ownerPath = element.GetStringAttribute("ownerPath");
        var csharpType = element.GetStringAttribute("csharp_type");

        // Override the C# type if present
        if (!string.IsNullOrEmpty(csharpType))
        {
            type = csharpType;
        }

        Object @object = null;

        // Handle entity package references
        var parent = _objects.GetObjectByIndexAndPath(ownerIndex, ownerPath) ?? _rootDataItem;
        if (type == EntityPackageReference.ClassFullName && parent.Parent is EntityGroup parentGroup)
        {
            var sourcePath = element["sourcePath_"]?.InnerText;
            if (sourcePath != null)
            {
                var absolutePath = EbexUtility.SourcePathToAbsolutePath(_readPath, sourcePath);
                var xmlPath = absolutePath.Replace(".ebex", ".xml").Replace(".prefab", ".xml");

                if (File.Exists(xmlPath))
                {
                    var reader = new EbonyEntityXmlReader(EbexUtility.DataDirectory, parentGroup);
                    if (CheckCircularReference(parent.ParentPackage, absolutePath))
                    {
                        @object = reader.Read(xmlPath, _mode, false, getAlternatePath);
                        if (@object is EntityPackage entityPackage)
                        {
                            entityPackage.AlreadyReadObjectsAtLoading = true;
                        }

                        if (@object is EntityPackageReference entityPackageReference)
                        {
                            entityPackageReference.FullFilePath = absolutePath;
                        }
                    }
                }
            }
        }

        if (@object == null)
        {
            @object = DocumentInterface.ModuleContainer.CreateObjectFromString(parent, type, _readPath) as Object;
            if (@object is EntityPackageReference reference && !string.IsNullOrWhiteSpace(_readPath))
            {
                if (_readVirtualPath == null)
                {
                    var extension = _objectElements[0]["sourcePath_"]?.InnerText.Split('.').Last();
                    _readVirtualPath = extension == null ? _readPath : _readPath.Replace(".xml", "." + extension);
                }

                reference.FullFilePath = _readVirtualPath;
            }
        }

        if (@object != null)
        {
            _objects.Add(@object);
            @object.ObjectIndex = index;
        }
    }

    public void ReadDynamicObjects(XmlElement element, DataItem item)
    {
        // Read item attributes
        item.IsChecked = element.GetBooleanAttribute("checked", true);
        var itemName = element.GetStringAttribute("name");
        if (!string.IsNullOrEmpty(itemName))
        {
            item.name = itemName;
        }

        // Read all children
        foreach (var node in element.ChildNodes)
        {
            // Only read children that are elements and not references
            if (node is XmlElement childElement && !childElement.GetBooleanAttribute("reference", false))
            {
                // Read child data
                var name = childElement.Name;
                var child = item[name];
                var typeName = childElement.GetStringAttribute("type");
                var type = PrimitiveTypeUtility.FromName(typeName);

                // Attempt to resolve child with renamed name
                if (child == null)
                {
                    var childByRenamedName = item.GetChildByRenamedName(name);
                    if (childByRenamedName != null)
                    {
                        child = childByRenamedName;
                    }
                }

                // Resolve dynamic children
                if (child == null && childElement.GetBooleanAttribute("dynamic", false))
                {
                    var dataType = DocumentInterface.ModuleContainer[typeName];
                    child = dataType == null
                        ? new UnknownDataItem(item, null)
                        : dataType.CreateDataItem(item);

                    child.Name = name;
                    child.Field = new Field(name, typeName, false);
                    child.IsDynamic = true;

                    if (type != PrimitiveType.None)
                    {
                        child.Field.DisplayName = childElement.GetStringAttribute("name");
                        child.Value = new Value(type, childElement.InnerText);
                    }
                }

                // Recursion
                if (child != null && type == PrimitiveType.None)
                {
                    ReadDynamicObjects(childElement, child);
                }
            }
        }
    }

    public void ReadReferences(XmlElement element, DataItem item)
    {
        foreach (var childNode in element.ChildNodes)
        {
            if (childNode is XmlElement childElement)
            {
                if (childElement.GetBooleanAttribute("reference", false))
                {
                    string objectPath, relativePath;
                    var index = childElement.GetIntegerAttribute("connectorObjectIndex", -1);
                    if (index < 0)
                    {
                        index = childElement.GetIntegerAttribute("objectIndex", -1);
                        objectPath = childElement.GetStringAttribute("object");
                        relativePath = childElement.GetStringAttribute("relativePath") ?? string.Empty;
                    }
                    else if (childElement.GetIntegerAttribute("connectorIndex", -1) < 1)
                    {
                        objectPath = childElement.GetStringAttribute("connectorObject");
                        relativePath = childElement.GetStringAttribute("connectorRelativePath") ?? string.Empty;
                    }
                    else
                    {
                        continue;
                    }

                    var other = _objects.GetObjectByIndexAndPath(index, relativePath);
                    var name = childElement.Name;
                    var type = childElement.GetStringAttribute("type");
                    if (type == "pointer")
                    {
                        var child = item[name];
                        if (child != null)
                        {
                            child.Value = new Value(PrimitiveType.Pointer, other);
                        }
                    }
                    else if (other != null)
                    {
                        if (other is TrayDataItem && item.Parent is EntityPackage)
                        {
                            item.RemoveWithoutModifyFlag(other);
                        }
                        else
                        {
                            item.Add(other);
                        }
                    }
                }
                else
                {
                    var name = childElement.Name;
                    var child = item[name];
                    var type = childElement.GetStringAttribute("type");
                    var primitiveType = PrimitiveTypeUtility.FromName(type);

                    // Temporarily attach the fixid uint value to the element so it can be grabbed for the fixid DB
                    // if (true)
                    // {
                    //     if (primitiveType == PrimitiveType.Fixid && childElement.HasAttribute("fixid") &&
                    //         child != null)
                    //     {
                    //         var fixidString = XmlUtility.GetAttributeText(childElement, "fixid");
                    //         if (uint.TryParse(fixidString, out var fixid))
                    //         {
                    //             var fixidItem = new ValueDataItem(child, "SQEX.Ebony.Std.Fixid");
                    //             fixidItem.Value = new Value(fixid);
                    //             child.Add(fixidItem);
                    //         }
                    //     }
                    // }

                    var hasRenamedName = false;
                    if (child == null)
                    {
                        var childByRenamedName = item.GetChildByRenamedName(name);
                        if (childByRenamedName != null)
                        {
                            hasRenamedName = true;

                            if (primitiveType != PrimitiveType.None)
                            {
                                if (childByRenamedName.DataType is Field field
                                    && PrimitiveTypeUtility.FromName(field.TypeName) != PrimitiveType.None)
                                {
                                    child = childByRenamedName;
                                }
                            }
                            else if (childByRenamedName.DataType is Class)
                            {
                                child = childByRenamedName;
                            }
                        }
                    }

                    if (child == null && !hasRenamedName)
                    {
                        if (item.DataType is Class deprecatedClass
                            && deprecatedClass[name] != null
                            && deprecatedClass[name].Deprecated)
                        {
                            if (!string.IsNullOrEmpty(childElement.InnerText))
                            {
                                if (deprecatedClass[name].DataType is Class @class)
                                {
                                    var baseType =
                                        (Class) DocumentInterface.ModuleContainer["SQEX.Ebony.Framework.Node.GraphPin"];
                                    if (@class.IsBasedOn(baseType))
                                    {
                                        if (childElement["connections_"] != null
                                            && childElement["connections_"].HasChildNodes)
                                        {
                                            child = new DeprecatedDataItem(item, @class);
                                            child.Name = name;
                                            child.Field = deprecatedClass[name];
                                        }
                                    }
                                    else
                                    {
                                        child = new DeprecatedDataItem(item, @class);
                                        child.Name = name;
                                        child.Field = deprecatedClass[name];
                                    }
                                }
                                else
                                {
                                    child = new DeprecatedDataItem(item, deprecatedClass[name]);
                                    child.Name = name;
                                    child.Field = deprecatedClass[name];
                                }
                            }
                        }
                        else if (primitiveType != PrimitiveType.None)
                        {
                            child = new ValueDataItem(item, type);
                            child.Name = name;
                            child.Browsable = false;
                        }
                        else if (item is Prefab && name == "differences")
                        {
                            child = new EntityDiff(item);
                            child.Name = name;
                            child.Field = new Field(child.Name, "SQEX.Ebony.Framework.Entity.EntityDiff", false);
                            child.Browsable = false;
                        }
                        else if (item is EntityDiff)
                        {
                            child = new EntityDiff(item);
                            child.Name = name;
                            child.Field = new Field(child.Name, "SQEX.Ebony.Framework.Entity.EntityDiffGroup", false);
                            child.Browsable = false;
                        }
                    }

                    if (child != null)
                    {
                        if (primitiveType == PrimitiveType.None)
                        {
                            ReadReferences(childElement, child);
                        }
                        else
                        {
                            var shouldReadFields = true;
                            if (item is EntityPackage {AlreadyReadObjectsAtLoading: true} and not Prefab)
                            {
                                if (child.Field == null)
                                {
                                    shouldReadFields = false;
                                }
                                else
                                {
                                    child.Field.TryGetAttributeBool("UseReferenceValue", out shouldReadFields);
                                }
                            }

                            if (shouldReadFields)
                            {
                                var valueString = childElement.InnerText;
                                if (child.Field != null)
                                {
                                    if (child.Field.GetAttribute("FileExtension") != null)
                                    {
                                        valueString = EbexUtility.SourcePathToRelativePath(_readPath, valueString);
                                    }

                                    if (child.Value is Value value && value.TypeName != type)
                                    {
                                        if (child.Field.GetAttribute("ConvertTypeAtLoading") != null
                                            || hasRenamedName)
                                        {
                                            type = child.Field.TypeName;
                                        }
                                    }
                                }

                                child.Value = DocumentInterface.ModuleContainer.CreateObjectFromString(null, null,
                                    type, valueString, true, false);
                            }
                        }

                        if (child.Field != null && child.Children.Count > 0)
                        {
                            ModuleContainer.ApplyUseParentAttribute(child);
                        }
                    }
                }
            }
        }
    }

    public void ResolveDifferences(DataItem parent)
    {
        foreach (var child in parent.Children)
        {
            if (child is Entity entity)
            {
                if (EntityPresetUtility.IsEnablePresetDifference(entity))
                {
                    entity = EntityPresetUtility.ApplyEntityPresetFromSavedPath(entity);
                }

                entity.ResolveDifference();
            }

            if (child.Parent == parent)
            {
                ResolveDifferences(child);
            }
        }
    }

    private bool CheckCircularReference(EntityPackage package, string sourcePath)
    {
        sourcePath = sourcePath.ToLower();
        for (; package != null && package != package.Parent; package = package.ParentPackage)
        {
            if (package.FullFilePath.ToLower() == sourcePath)
            {
                return false;
            }
        }

        return true;
    }
}