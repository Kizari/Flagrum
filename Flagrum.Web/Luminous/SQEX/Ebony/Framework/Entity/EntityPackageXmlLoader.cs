using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Ebony.Base.Serialization;
using Flagrum.Core.Ebex.Entities;
using Luminous.Core;
using Luminous.Xml;
using SQEX.Luminous.Core.Object;
using UnityEngine;
using Object = SQEX.Luminous.Core.Object.Object;
using Path = SQEX.Luminous.Core.IO.Path;

namespace SQEX.Ebony.Framework.Entity;

public class EntityPackageXmlLoader
{
    /// <summary>
    /// Offset to the root element's relative offset.
    /// </summary>
    private const int RootElementOffsetOffset = 12;

    /// <summary>
    /// The loaded objects.
    /// </summary>
    private IList<Object> Objects { get; } = new List<Object>();

    /// <summary>
    /// Maps an object's objectIndex to its index into ObjectElementList.
    /// </summary>
    private IDictionary<int, int> ObjectIndexMap { get; } = new Dictionary<int, int>();

    /// <summary>
    /// Object indices of unchecked objects.
    /// </summary>
    private IList<int> UncheckedObjectIndices { get; } = new List<int>();

    /// <summary>
    /// Object indices of unchecked objects.
    /// </summary>
    private IList<int> StartupUnloadObjectIndexes { get; } = new List<int>();

    /// <summary>
    /// Object elements.
    /// </summary>
    private IList<Xmb2Element> ObjectElementList { get; } = new List<Xmb2Element>();

    /// <summary>
    /// Paths to packages where startup loading is disabled.
    /// </summary>
    private IList<string> StartupOffPackageFilePaths { get; } = new List<string>();

    /// <summary>
    /// The load result.
    /// </summary>
    private EntityPackageCreatingResult Result { get; } = new();

    /// <summary>
    /// The current load status.
    /// </summary>
    private LOAD_STATUS LoadStatus { get; set; } = LOAD_STATUS.LOAD_ST_INIT;

    /// <summary>
    /// The XMB2 document.
    /// </summary>
    private byte[] CurrentDocument { get; set; }

    /// <summary>
    /// The number of loaded objects.
    /// </summary>
    private uint LoadedObjectCount { get; set; }

    /// <summary>
    /// Path of the source asset.
    /// </summary>
    private string SourcePath { get; set; }

    /// <summary>
    /// Unique identifier for the package.
    /// </summary>
    private string UniqueName { get; set; }

    /// <summary>
    /// Parse an XMB2 document into an EntityPackage.
    /// </summary>
    /// <param name="xmbDoc">The XMB2 document to parse.</param>
    public EntityPackage CreateEntityPackage(byte[] xmbDoc)
    {
        Debug.Assert(xmbDoc != null);
        SetCurrentDocument(xmbDoc);
        return CreatePackage();
    }

    /// <summary>
    /// Assign the XMB2 document and initialize the root elements.
    /// </summary>
    /// <param name="xmbDoc">The XMB2 document.</param>
    private void SetCurrentDocument(byte[] xmbDoc)
    {
        Debug.Assert(xmbDoc != null);

        CurrentDocument = xmbDoc;
        SetupSourceElements();
    }

    private EntityPackage CreatePackage()
    {
        EntityPackage package = null;
        if (CurrentDocument == null)
        {
            return null;
        }

        LoadStatus = LOAD_STATUS.LOAD_ST_LOAD;

        var rootElementRelativeOffset = BitConverter.ToInt32(CurrentDocument, RootElementOffsetOffset);
        var rootElement =
            Xmb2Element.FromByteArray(CurrentDocument, RootElementOffsetOffset + rootElementRelativeOffset);
        var objectsElement = rootElement.GetElementByName("objects");
        if (objectsElement == null || LoadedObjectCount >= ObjectElementList.Count)
        {
            LoadStatus = LOAD_STATUS.LOAD_ST_CREATE;
            // TODO goto LABEL_28;
        }

        // Instantiate each object.
        for (var i = (int)LoadedObjectCount; i < ObjectElementList.Count; i++)
        {
            var element = ObjectElementList[i];

            var objectIndex = 0;
            var objectIndexAttribute = element.GetAttributeByName("objectIndex");
            if (objectIndexAttribute != null)
            {
                objectIndex = objectIndexAttribute.ToInt();
            }

            ObjectIndexMap.Add(objectIndex, i);
            if (!ReadObject(element))
            {
                break;
            }
        }

        // Read each object's fields.
        for (var i = 0; i < ObjectElementList.Count; i++)
        {
            var element = ObjectElementList[i];
            var obj = Objects[i];
            if (Objects[i] is EntityPackage)
            {
                package = obj as EntityPackage;
                package.sourcePath_ = GetDataRelativePath(element, package);
            }

            File.WriteAllText($@"C:\Modding\DumbTest\{i}.txt", "test");
            ReadField(obj, element, 2166136261, null); // ""

            if (!(Objects[i] is EntityPackage))
            {
                var ownerIndexAttribute = element.GetAttributeByName("ownerIndex");
                var ownerIndex = 0;
                if (ownerIndexAttribute != null)
                {
                    ownerIndex = ownerIndexAttribute.ToInt();
                }
            }

            string name = null;
            var nameAttribute = element.GetAttributeByName("name");
            if (nameAttribute != null)
            {
                name = nameAttribute.GetTextValue();
                if (Objects[i] is EntityPackage)
                {
                    package.simpleName_ = name;
                }
            }

            string objPath = null;
            var pathAttribute = element.GetAttributeByName("path");
            if (pathAttribute != null)
            {
                objPath = pathAttribute.GetTextValue();
            }

            package.AddLoadedObject(obj, name, objPath);
        }

        LoadStatus = LOAD_STATUS.LOAD_ST_CREATE;
        return package;
    }


    /// <summary>
    /// Parse an XMB2 element into an object.
    /// </summary>
    /// <param name="element">The XMB2 element.</param>
    /// <param name="loadedObject">The parsed object.</param>
    /// <returns>True if the parse succeeded, else false.</returns>
    private bool ReadObject(Xmb2Element element)
    {
        bool through;
        var loadedObject = ReadObject(element, out through);

        var result = through;
        if (loadedObject != null)
        {
            Objects.Add(loadedObject);
            result = true;
        }
        else
        {
            // TEMP HACK
            loadedObject = new Entity();
            Objects.Add(loadedObject);
            result = true;
        }

        LoadedObjectCount++;
        return result;
    }

    private Object ReadObject(Xmb2Element element, out bool through)
    {
        through = false;
        string typeFullName = null;

        var typeAttribute = element.GetAttributeByName("type");
        if (typeAttribute != null)
        {
            typeFullName = typeAttribute.GetTextValue();
        }

        var type = ObjectType.FindByFullName(typeFullName);
        if (type == null)
        {
            return null;
        }

        /*if (type == EntityPackage.ObjectType)
        {
            return null;
        }*/

        var instance = type.ConstructFunction2() as Object;
        if (instance != null)
        {
            through = true;
        }

        return instance;
    }

    private void ReadField(BaseObject obj, Xmb2Element parentElement, uint hashedParentName, BaseObject objOffset)
    {
        if (objOffset != null)
        {
            obj = objOffset;
        }

        foreach (var element in parentElement.GetElements())
        {
            var name = element.Name;
            var hashedName = Fnv1a.Fnv1a32(name, hashedParentName);
            var type = obj.GetObjectType();
            if (type == null)
            {
                continue;
            }

            var property = type.PropertyContainer.FindByName(name);
            if (property == null)
            {
                continue;
            }

            ReadValue(obj, parentElement, element, property, hashedName, objOffset);
        }
    }

    private void ReadValue(BaseObject obj, Xmb2Element parentObject, Xmb2Element element, Property property,
        uint hashedName, BaseObject objOffset)
    {
        object value = null;
        switch (property.Type)
        {
            case Property.PrimitiveType.ClassField:
                ReadField(obj, element, Fnv1a.Fnv1a32(".", hashedName), obj.GetPropertyValue<BaseObject>(property));
                break;
            case Property.PrimitiveType.Int8:
            case Property.PrimitiveType.Int16:
            case Property.PrimitiveType.Int32:
            case Property.PrimitiveType.Int64:
            case Property.PrimitiveType.UInt8:
            case Property.PrimitiveType.UInt16:
            case Property.PrimitiveType.UInt32:
            case Property.PrimitiveType.UInt64:
            case Property.PrimitiveType.INVALID2:
            case Property.PrimitiveType.Bool:
            case Property.PrimitiveType.Float:
            case Property.PrimitiveType.Double:
            case Property.PrimitiveType.String:
            case Property.PrimitiveType.Fixid:
            case Property.PrimitiveType.Vector4:
            case Property.PrimitiveType.Color:
            case Property.PrimitiveType.Enum:
                value = ReadPrimitiveValue(element, property);
                break;
            case Property.PrimitiveType.Pointer:
                if (ResolvePointer(parentObject, element, out value))
                {
                    // TODO addPointerListReferedBySequenceIfMatchCondition
                }

                break;
            case Property.PrimitiveType.PointerArray:
            case Property.PrimitiveType.IntrusivePointerArray:
                // GOTCHA: We don't want to create a new List, because other objects can reference the list before it's instantiated
                value = obj.GetPropertyValue<IList>(property);
                ReadPointerArray(obj, element, value as IList);
                break;
            default:
                if (property.Type != Property.PrimitiveType.Array)
                {
                    throw new ArgumentException(
                        $"[EntityPackageXmlLoader]Unknown type {property.Type} for {property.Name}");
                }

                break;
        }

        if (value != null)
        {
            obj.SetPropertyValue(property, value);
        }
    }

    private void ReadPointerArray(BaseObject obj, Xmb2Element element, IList destinationArray)
    {
        var itemCount = element.ElementCount;
        for (var i = 0; i < itemCount; i++)
        {
            var child = element.GetElementByIndex(i);
            if (child.GetAttributeByName("dynamic")?.ToBool() == true)
            {
                var dynamicInstance = CreateDynamicBaseObject(child);
                ReadDynamicBaseObjectChildElement(obj, dynamicInstance, child);
                destinationArray.Add(dynamicInstance);
                continue;
            }

            object itemValue = null;
            if (ResolvePointer(element, child, out itemValue))
            {
                // TODO addPointerListReferedBySequenceIfMatchCondition
                destinationArray.Add(itemValue);
            }
        }
    }

    private void ReadDynamicBaseObjectChildElement(BaseObject parentObject, BaseObject baseObject,
        Xmb2Element childElement)
    {
        foreach (var element in childElement.GetElements())
        {
            var name = element.Name;
            var hashedName = Fnv1a.Fnv1a32(name); // ???
            var type = baseObject.GetObjectType();
            if (type == null)
            {
                continue;
            }

            var property = type.PropertyContainer.FindByName(name);
            if (property == null)
            {
                continue;
            }

            ReadValue(baseObject, childElement, element, property, hashedName, baseObject);
        }
    }

    private BaseObject CreateDynamicBaseObject(Xmb2Element childElement)
    {
        var typeName = childElement.GetAttributeByName("type").GetTextValue();
        var type = ObjectType.FindByFullName(typeName);
        return type.ConstructFunction2();
    }

    private bool ResolvePointer(Xmb2Element parentElement, Xmb2Element childElement, out object destination)
    {
        var referencedObjectIndex = 0;
        destination = ReadReference(childElement, out referencedObjectIndex);
        if (destination != null)
        {
            return true;
        }

        var isPointerType = false;
        var typeAttribute = childElement.GetAttributeByName("type");
        if (typeAttribute != null)
        {
            var type = typeAttribute.GetTextValue();
            if (type == "pointer")
            {
                isPointerType = true;
            }
        }

        /*TODO
         * bool result;
        if (!isPointerType && this.TryRegisterExternalPointerInfoAsSubPackage(destination, referencedObjectIndex, out result)
        {
            return result;
        }*/

        if (UncheckedObjectIndices.Contains(referencedObjectIndex))
        {
            return false;
        }

        if (StartupUnloadObjectIndexes.Contains(referencedObjectIndex))
        {
            return false;
        }

        var useUnresolvedPointerReferenceAttribute = childElement.GetAttributeByName("UseUnresolvedPointerReference");
        if (useUnresolvedPointerReferenceAttribute?.ToBool() != true)
        {
            var resolvedReferenceAttribute = childElement.GetAttributeByName("object");
            if (isPointerType)
            {
                Debug.Assert(resolvedReferenceAttribute == null,
                    $"[EntityPackageXmlLoader] ResolvedReference not found : object path={resolvedReferenceAttribute.GetTextValue()}");
            }
            else
            {
                var useTemplateConnectionAttribute = childElement.GetAttributeByName("UseTemplateConnection");
                var unknownFlagAttribute = childElement.GetAttributeByName(4262871454);

                if (useTemplateConnectionAttribute?.ToBool() == true || unknownFlagAttribute?.ToBool() == true)
                {
                    var prefabConnectionRelativePathAttribute = childElement.GetAttributeByName("relativePath");
                    var targetPackageSourcePathAttribute = childElement.GetAttributeByName("targetPackageSourcePath");
                    var proxyConnectionOwnerPackageNameAttribute =
                        childElement.GetAttributeByName("ProxyConnectionOwnerPackageName");

                    Debug.Assert(prefabConnectionRelativePathAttribute != null,
                        $"[EntityPackageXmlLoader] Prefab Connection : relativePath is null : object path={resolvedReferenceAttribute.GetTextValue()}, targetPackageSourcePath={targetPackageSourcePathAttribute.GetTextValue()}");
                    // Debug.Assert(
                    //     useTemplateConnectionAttribute?.ToBool() != true ||
                    //     proxyConnectionOwnerPackageNameAttribute?.ToBool() != false,
                    //     $"[EntityPackageXmlLoader] Template Connection : targetPackageName is null : object path={resolvedReferenceAttribute.GetTextValue()}, targetPackageSourcePath={targetPackageSourcePathAttribute.GetTextValue()}");

                    var templateConnectionRelativePathAttribute = childElement.GetAttributeByName("relativePath");
                    var prefabConnectionSourceItemPathAttribute =
                        childElement.GetAttributeByName("PrefabConnectionSourceItemPath");

                    ushort templateConnectionProtocol = 6;
                    if (useTemplateConnectionAttribute?.ToBool() == false)
                    {
                        templateConnectionProtocol = 4;
                    }

                    var templateConnectionExternalPointerInfo =
                        new ExternalPointerInfo(templateConnectionProtocol, destination);
                    Result.ExternalPointerInfos.Add(templateConnectionExternalPointerInfo);

                    templateConnectionExternalPointerInfo.AddKey(targetPackageSourcePathAttribute?.GetTextValue());
                    templateConnectionExternalPointerInfo.AddKey(
                        proxyConnectionOwnerPackageNameAttribute?.GetTextValue());
                    templateConnectionExternalPointerInfo.AddKey(resolvedReferenceAttribute?.GetTextValue());
                    templateConnectionExternalPointerInfo.AddKey(
                        templateConnectionRelativePathAttribute?.GetTextValue());
                    templateConnectionExternalPointerInfo.AddKey(
                        prefabConnectionSourceItemPathAttribute?.GetTextValue());
                    templateConnectionExternalPointerInfo.AddKey(SourcePath);

                    return true;
                }

                Debug.Assert(resolvedReferenceAttribute?.GetTextValue() != null,
                    "[EntityPackageXmlLoader] reference not found : object path is null");
                Debug.Assert(resolvedReferenceAttribute?.GetTextValue() != "INVALID_CONNECTOR",
                    $"[EntityPackageXmlLoader] reference not found : object path={parentElement.GetAttributeByName("path")?.GetTextValue()}, reference path={resolvedReferenceAttribute.GetTextValue()}");
            }

            return false;
        }

        var farReferenceAttribute = childElement.GetAttributeByName("FarReference");
        var objectAttribute = childElement.GetAttributeByName("object");
        var relativePathAttribute = childElement.GetAttributeByName("relativePath");
        var unknownKeyAttribute = childElement.GetAttributeByName(347410431);
        var unresolvedPointerPackageSourceAttribute = childElement.GetAttributeByName("UnresolvedPointerPackageSource");

        ushort protocol = 7;
        if (farReferenceAttribute?.ToBool() == false)
        {
            protocol = 1;
        }

        var externalPointerInfo = new ExternalPointerInfo(protocol, destination);
        Result.ExternalPointerInfos.Add(externalPointerInfo);

        externalPointerInfo.AddKey(unresolvedPointerPackageSourceAttribute?.GetTextValue());
        externalPointerInfo.AddKey(objectAttribute?.GetTextValue());
        externalPointerInfo.AddKey(relativePathAttribute?.GetTextValue());
        externalPointerInfo.AddKey(unknownKeyAttribute?.GetTextValue());
        externalPointerInfo.AddKey(SourcePath);

        if (farReferenceAttribute?.ToBool() == true)
        {
            externalPointerInfo.AddKey(childElement.GetAttributeByName(1280569575).GetTextValue());
        }

        return true;
    }

    private BaseObject ReadReference(Xmb2Element childElement, out int objectIndex)
    {
        objectIndex = -1;
        var referenceAttribute = childElement.GetAttributeByName("reference");
        if (referenceAttribute == null)
        {
            return null;
        }

        var objectIndexAttribute = childElement.GetAttributeByName("objectIndex");
        if (objectIndexAttribute != null)
        {
            objectIndex = objectIndexAttribute.ToInt(-1);
        }

        if (objectIndex < 0)
        {
            return null;
        }

        var referencedObject = GetObjectByIndex(objectIndex);
        if (referencedObject == null)
        {
            return null;
        }

        var referencedObjectElement = GetObjectElementByIndex(objectIndex);
        if (referencedObjectElement == null)
        {
            return null;
        }

        var relativePathAttribute = childElement.GetAttributeByName("relativePath");
        if (relativePathAttribute == null)
        {
            return referencedObject;
        }

        return ReadReference(relativePathAttribute.GetTextValue(), referencedObject, referencedObjectElement);
    }

    private BaseObject ReadReference(string relativePath, Object destinationObject,
        Xmb2Element destinationObjectElement)
    {
        if (relativePath == null)
        {
            return null;
        }

        var type = destinationObject.GetObjectType();
        var property = type.PropertyContainer.FindByName(relativePath);
        if (property != null)
        {
            return destinationObject.GetPropertyValue<BaseObject>(property);
        }

        var relativePathSegments = relativePath.Split('.');
        // TODO Commented out because not all types are implemented yet, causing this to misfire Debug.Assert(relativePathSegments.Length > 1, $"[EntityPackageXmlLoader]Reference relativePath not found {relativePath}");
        if (relativePathSegments.Length == 1)
        {
            return null;
        }

        property = type.PropertyContainer.FindByName(relativePathSegments[0]);
        if (property == null || property.Type != Property.PrimitiveType.ClassField &&
            property.Type != Property.PrimitiveType.PointerArray)
        {
            Debug.Fail($"[EntityPackageXmlLoader]Reference relativePath not found {relativePath}");
            return null;
        }

        if (property.Type != Property.PrimitiveType.PointerArray)
        {
            var referencedPropertyValue = destinationObject.GetPropertyValue<Object>(property);
            var referencedPropertyElement = destinationObjectElement.GetElementByName(relativePathSegments[0]);
            if (referencedPropertyElement == null)
            {
                Debug.Fail($"[EntityPackageXmlLoader]Reference relativePath not found {relativePath}");
                return null;
            }
            
            var referencedPropertyElementTypeAttribute = referencedPropertyElement.GetAttributeByName("type");
            if (referencedPropertyElementTypeAttribute == null ||
                referencedPropertyElementTypeAttribute.GetTextValue() != "enum")
            {
                return ReadReference(relativePathSegments[1], referencedPropertyValue, referencedPropertyElement);
            }

            Debug.Fail($"[EntityPackageXmlLoader]Reference relativePath not found {relativePath}");
            return null;
        }

        // TODO read pointer array item
        //return this.ReadReference(relativePath, destinationObject, destinationObjectElement);
        return null;
    }

    private Xmb2Element GetObjectElementByIndex(int objectIndex)
    {
        if (ObjectIndexMap.ContainsKey(objectIndex))
        {
            return ObjectElementList[ObjectIndexMap[objectIndex]];
        }

        throw null;
    }

    private Object GetObjectByIndex(int objectIndex)
    {
        if (ObjectIndexMap.ContainsKey(objectIndex))
        {
            return Objects[ObjectIndexMap[objectIndex]];
        }

        return null;
    }

    private object ReadPrimitiveValue(Xmb2Element element, Property property)
    {
        switch (property.Type)
        {
            case Property.PrimitiveType.Int8:
                return (sbyte)element.GetIntValue();
            case Property.PrimitiveType.Int16:
                return (short)element.GetIntValue();
            case Property.PrimitiveType.Int32:
                return element.GetIntValue();
            case Property.PrimitiveType.Int64:
                return (long)element.GetIntValue();
            case Property.PrimitiveType.UInt8:
                return (byte)element.GetUIntValue();
            case Property.PrimitiveType.UInt16:
                return (ushort)element.GetUIntValue();
            case Property.PrimitiveType.UInt32:
                return element.GetUIntValue();
            case Property.PrimitiveType.UInt64:
            case Property.PrimitiveType.INVALID2:
                return (ulong)element.GetUIntValue();
            case Property.PrimitiveType.Bool:
                return element.GetBoolValue();
            case Property.PrimitiveType.Float:
                return element.GetFloatValue();
            case Property.PrimitiveType.Double:
                return element.GetDoubleValue();
            case Property.PrimitiveType.String:
                return element.GetTextValue();
            case Property.PrimitiveType.Fixid:
            {
                var fixidAttribute = element.GetAttributeByName("fixid");
                if (fixidAttribute != null)
                {
                    return fixidAttribute.ToUInt();
                }

                return (uint)0;
            }
            case Property.PrimitiveType.Vector4:
            {
                var vector = element.GetFloat4Value();
                if (vector != null)
                {
                    return new Vector4(vector[0], vector[1], vector[2], vector[3]);
                }

                var strVector = element.GetTextValue();
                var components = strVector.Split(',').ToList();
                if (components.Count == 4)
                {
                    return new Vector4(float.Parse(components[0]), float.Parse(components[1]),
                        float.Parse(components[2]), float.Parse(components[3]));
                }

                return Vector4.zero;
            }
            case Property.PrimitiveType.Color:
            {
                var vector = element.GetFloat4Value();
                if (vector != null)
                {
                    return new Color(vector[0], vector[1], vector[2], vector[3]);
                }

                var strVector = element.GetTextValue();
                var components = strVector.Split(',').ToList();
                if (components.Count == 4)
                {
                    return new Color(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]),
                        float.Parse(components[3]));
                }

                return Color.black;
            }
            case Property.PrimitiveType.Enum:
            {
                var valueAttribute = element.GetAttributeByName("value");
                if (valueAttribute != null)
                {
                    return valueAttribute.ToInt();
                }

                return (uint)0;
            }
            default:
                return null;
        }
    }

    /// <summary>
    /// Find the root elements and process their metadata.
    /// </summary>
    private void SetupSourceElements()
    {
        var rootElementRelativeOffset = BitConverter.ToInt32(CurrentDocument, RootElementOffsetOffset);
        var rootElement =
            Xmb2Element.FromByteArray(CurrentDocument, RootElementOffsetOffset + rootElementRelativeOffset);
        Debug.Assert(rootElement != null);

        var objectsElement = rootElement.GetElementByName("objects");
        Debug.Assert(objectsElement != null);

        objectsElement.GetElements("object", ObjectElementList);
        if (ObjectElementList.Count > 0)
        {
            var firstElement = ObjectElementList[0];
            var sourcePathElement = firstElement.GetElementByName("sourcePath_");
            if (sourcePathElement != null)
            {
                SourcePath = sourcePathElement.GetTextValue();
            }

            var nameElement = firstElement.GetElementByName("name_");
            if (nameElement != null)
            {
                UniqueName = nameElement.GetTextValue();
            }
        }

        UpdateListsByChecked();
        UpdateListsByReference();
    }

    /// <summary>
    /// Find unchecked objects and move them from ObjectElementList to UncheckedObjectIndices.
    /// </summary>
    private void UpdateListsByChecked()
    {
        var isCheckedTable = new Dictionary<int, bool>();
        for (var i = 0; i < ObjectElementList.Count; i++)
        {
            isCheckedTable.Add(i, true);
        }

        for (var i = 0; i < ObjectElementList.Count; i++)
        {
            var element = ObjectElementList[i];
            var objectIndexAttribute = element.GetAttributeByName("objectIndex");
            if (objectIndexAttribute == null)
            {
                continue;
            }

            var objectIndex = objectIndexAttribute.ToInt();
            if (objectIndex == 0)
            {
                continue;
            }

            var ownerIndex = 0;
            var ownerIndexAttribute = element.GetAttributeByName("ownerIndex");
            if (ownerIndexAttribute != null)
            {
                ownerIndex = ownerIndexAttribute.ToInt();
            }

            var isOwnerChecked = true;
            if (ownerIndex != 0)
            {
                isOwnerChecked = isCheckedTable[ownerIndex];
            }

            var isChecked = false;
            var checkedAttribute = element.GetAttributeByName("checked");
            if (checkedAttribute != null)
            {
                isChecked = checkedAttribute.ToBool();
            }

            if (isChecked && isOwnerChecked)
            {
                continue;
            }

            isCheckedTable[i] = false;
            UncheckedObjectIndices.Add(i);
        }

        /*var elementsToRemove = from kvp in isCheckedTable
                               where !kvp.Value
                               select this.ObjectElementList[kvp.Key];

        foreach (var elementToRemove in elementsToRemove)
        {
            this.ObjectElementList.Remove(elementToRemove);
        }*/
    }

    /// <summary>
    /// Find startup unload objects and move them from ObjectElementList to StartupUnloadObjectIndexes. Also find startup off
    /// packages and register them.
    /// </summary>
    private void UpdateListsByReference()
    {
        var isNotStartupUnloadObjectTable = new Dictionary<int, bool>();
        for (var i = 0; i < ObjectElementList.Count; i++)
        {
            isNotStartupUnloadObjectTable.Add(i, true);
        }

        for (var i = 0; i < ObjectElementList.Count; i++)
        {
            var element = ObjectElementList[i];

            var typeAttribute = element.GetAttributeByName("type");
            if (typeAttribute == null)
            {
                continue;
            }

            var typeName = typeAttribute.GetTextValue();
            if (typeName != "SQEX.Ebony.Framework.Entity.EntityPackageReference" &&
                typeName != "Black.Entity.Data.CharacterEntry.CharaEntryPackage")
            {
                continue;
            }

            var startupLoadElement = element.GetElementByName("startupLoad_");
            if (startupLoadElement?.GetBoolValue() == false)
            {
                isNotStartupUnloadObjectTable[i] = false;
            }

            var isTemplateTraySourceReferenceElement = element.GetElementByName("isTemplateTraySourceReference_");
            if (isTemplateTraySourceReferenceElement?.GetBoolValue() == true)
            {
                isNotStartupUnloadObjectTable[i] = false;
            }

            var dataRelativeSourcePath = GetDataRelativePath(element, null);
            StartupOffPackageFilePaths.Add(dataRelativeSourcePath);

            var triggerTypeElement = element.GetElementByName("triggerType_");
            if (triggerTypeElement == null)
            {
                continue;
            }

            var triggerTypeValueAttribute = triggerTypeElement.GetAttributeByName("value");

            var triggerType = 0;
            if (triggerTypeValueAttribute != null)
            {
                triggerType = triggerTypeValueAttribute.ToInt();
            }

            if (triggerType == 0)
            {
                continue;
            }

            var externalPointerInfo = new ExternalPointerInfo(9, null);
            externalPointerInfo.AddKey(dataRelativeSourcePath);

            Result.ExternalPointerInfos.Add(externalPointerInfo);

            var fixid = 0u;
            var packageSearchLabelIdElement = element.GetElementByName("packageSearchLabelId_");
            if (packageSearchLabelIdElement != null)
            {
                var fixidAttribute = packageSearchLabelIdElement.GetAttributeByName("fixid");
                if (fixidAttribute != null)
                {
                    fixid = fixidAttribute.ToUInt();
                }
            }

            float[] position = null;
            var positionElement = element.GetElementByName("position_");
            if (positionElement != null)
            {
                position = positionElement.GetFloat4Value();
            }

            float[] rotation = null;
            var rotationElement = element.GetElementByName("rotation_");
            if (rotationElement != null)
            {
                rotation = rotationElement.GetFloat4Value();
            }

            var triggerEnableElement = element.GetElementByName("triggerEnable_");
            var triggerEnable = true;
            if (triggerEnableElement != null)
            {
                triggerEnable = triggerEnableElement.GetBoolValue();
            }

            var isNotLoadedAtFlying = false;
            var isNotLoadedAtFlyingElement = element.GetElementByName("isNotLoadedAtFlying_");
            if (isNotLoadedAtFlyingElement != null)
            {
                isNotLoadedAtFlying = isNotLoadedAtFlyingElement.GetBoolValue();
            }

            var invalidateInitForceLoadRadiusFlag = false;
            var invalidateInitForceLoadRadiusFlagElement =
                element.GetElementByName("invalidateInitForceLoadRadiusFlag_");
            if (invalidateInitForceLoadRadiusFlagElement != null)
            {
                invalidateInitForceLoadRadiusFlag = invalidateInitForceLoadRadiusFlagElement.GetBoolValue();
            }

            var noWaitLoadingAtFlying = false;
            var noWaitLoadingAtFlyingElement = element.GetElementByName("noWaitLoadingAtFlying_");
            if (noWaitLoadingAtFlyingElement != null)
            {
                noWaitLoadingAtFlying = noWaitLoadingAtFlyingElement.GetBoolValue();
            }

            var importantLoading = false;
            var importantLoadingElement = element.GetElementByName("importantLoading_");
            if (importantLoadingElement != null)
            {
                importantLoading = importantLoadingElement.GetBoolValue();
            }

            var triggerRadius = 0.0f;
            var triggerRadiusElement = element.GetElementByName("triggerRadius_");
            if (triggerRadiusElement != null)
            {
                triggerRadius = triggerRadiusElement.GetFloatValue();
            }

            var triggerHeight = 1000.0f;
            var triggerHeightElement = element.GetElementByName("triggerHeight_");
            if (triggerHeightElement != null)
            {
                triggerHeight = triggerHeightElement.GetFloatValue();
            }

            var triggerWidth = 0.0f;
            var triggerWidthElement = element.GetElementByName("triggerWidth_");
            if (triggerWidthElement != null)
            {
                triggerWidth = triggerWidthElement.GetFloatValue();
            }

            var triggerDepth = 0.0f;
            var triggerDepthElement = element.GetElementByName("triggerDepth_");
            if (triggerDepthElement != null)
            {
                triggerDepth = triggerDepthElement.GetFloatValue();
            }

            var unloadMargin = 0.0f;
            var unloadMarginElement = element.GetElementByName("unloadMargin_");
            if (unloadMarginElement != null)
            {
                unloadMargin = unloadMarginElement.GetFloatValue();
            }

            var forceNowLoadingRatio = 0.0f;
            var forceNowLoadingRatioElement = element.GetElementByName("forceNowLoadingRatio_");
            if (forceNowLoadingRatioElement != null)
            {
                forceNowLoadingRatio = forceNowLoadingRatioElement.GetFloatValue();
            }

            float[] triggerOffset = null;
            var triggerOffsetElement = element.GetElementByName("triggerOffset_");
            if (triggerOffsetElement != null)
            {
                triggerOffset = triggerOffsetElement.GetFloat4Value();
            }

            string lodLowPackageName = null;
            var lodLowPackageNameElement = element.GetElementByName("lodLowPackageName_");
            if (lodLowPackageNameElement != null)
            {
                lodLowPackageName = lodLowPackageNameElement.GetTextValue();
                externalPointerInfo.AddKey(lodLowPackageName);
            }
            else
            {
                externalPointerInfo.AddKey(null);
            }

            string loadDependentPackageName = null;
            var loadDependentPackageNameElement = element.GetElementByName("loadDependentPackageName_");
            if (loadDependentPackageNameElement != null)
            {
                loadDependentPackageName = loadDependentPackageNameElement.GetTextValue();
                externalPointerInfo.AddKey(loadDependentPackageName);
            }
            else
            {
                externalPointerInfo.AddKey(null);
            }

            var isLowPackageLoad = false;
            var isLowPackageLoadElement = element.GetElementByName("isLowPackageLoad_");
            if (isLowPackageLoadElement != null)
            {
                isLowPackageLoad = isLowPackageLoadElement.GetBoolValue();
            }

            var name = element.GetAttributeByName("name");
            externalPointerInfo.AddKey(name.GetTextValue());

            var path = element.GetAttributeByName("path");
            externalPointerInfo.AddKey(path.GetTextValue());

            var paramsBuffer = externalPointerInfo.GetParamsBuffer();
            paramsBuffer.Add(fixid);
            paramsBuffer.Add(position);
            paramsBuffer.Add(rotation);
            paramsBuffer.Add(triggerType);
            paramsBuffer.Add(triggerEnable);
            paramsBuffer.Add(isNotLoadedAtFlying);
            paramsBuffer.Add(invalidateInitForceLoadRadiusFlag);
            paramsBuffer.Add(noWaitLoadingAtFlying);
            paramsBuffer.Add(importantLoading);
            paramsBuffer.Add(triggerRadius);
            paramsBuffer.Add(triggerHeight);
            paramsBuffer.Add(triggerWidth);
            paramsBuffer.Add(triggerDepth);
            paramsBuffer.Add(lodLowPackageName);
            paramsBuffer.Add(forceNowLoadingRatio);
            paramsBuffer.Add(triggerOffset);
            paramsBuffer.Add(isLowPackageLoad);
        }

        foreach (var entry in isNotStartupUnloadObjectTable)
        {
            if (entry.Value)
            {
                continue;
            }

            var objectIndex = 0;
            var element = ObjectElementList[entry.Key];
            var objectIndexAttribute = element.GetAttributeByName("objectIndex");
            if (objectIndexAttribute != null)
            {
                objectIndex = objectIndexAttribute.ToInt();
            }

            StartupUnloadObjectIndexes.Add(objectIndex);
        }

        /*foreach (var entry in isNotStartupUnloadObjectTable)
        {
            if (entry.Value)
            {
                continue;
            }

            var objToRemove = this.ObjectElementList[entry.Key];
            this.ObjectElementList.Remove(objToRemove);
        }*/
    }

    /// <summary>
    /// Get the canonicalized source path of an element.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>The canonicalized source path.</returns>
    private string GetDataRelativePath(Xmb2Element element, EntityPackage parentPackage)
    {
        var sourcePathElement = element.GetElementByName("sourcePath_");
        if (sourcePathElement == null)
        {
            return null;
        }

        var isSharedElement = element.GetElementByName("isShared_");
        if (isSharedElement?.GetBoolValue() == true)
        {
            return null;
        }

        var sourcePath = sourcePathElement.GetTextValue();
        if (sourcePath == null)
        {
            return null;
        }

        //return sourcePath;

        // TODO Properly implement and debug
        if (parentPackage != null)
        {
            return Path.ResolveRelativePath(parentPackage.sourcePath_, sourcePath);
        }

        return Path.ResolveRelativePath(SourcePath, sourcePath);
    }

    private enum LOAD_STATUS
    {
        LOAD_ST_INIT = 0x0,
        LOAD_ST_LOAD = 0x1,
        LOAD_ST_CREATE = 0x2,
        LOAD_ST_END = 0x3
    }
}