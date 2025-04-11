using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Flagrum.Core.Scripting.Ebex.Configuration;
using Flagrum.Core.Scripting.Ebex.Data;
using Flagrum.Core.Scripting.Ebex.Type;
using Object = Flagrum.Core.Scripting.Ebex.Data.Object;

namespace Flagrum.Core.Scripting.Ebex;

public class EbexReader : IDisposable
{
    public enum ReadMode
    {
        RM_NEW_PACKAGE_RECURSIVE,
        RM_EXISTING_PACKAGE_ONLY
    }

    private const int ExpectedTotalFilesCount = 0;

    private static readonly bool DBG_USE_CACHE = true;

    private static readonly ConcurrentDictionary<string, Lazy<Tuple<bool, XmlDocument>>> documentCache = new();

    private static int _loadedFilesCount;

    private readonly List<Object> _objects = new();
    private readonly DataItem _rootDataItem;
    private XmlDocument _document;
    private string _loadingFilePath;
    private bool _recursiveImport;

    public EbexReader() { }

    public EbexReader(
        EntityGroup parent,
        ReadMode readMode = ReadMode.RM_NEW_PACKAGE_RECURSIVE,
        EntityPackageReference targetEntityPackageReference = null,
        bool isLazyLoadRecursive = false)
    {
        CurrentReadMode = readMode;
        TargetEntityPackageReference = targetEntityPackageReference;
        IsLazyLoadRecursive = isLazyLoadRecursive;
        UnresolvedReferenceInfo = new List<ReaderUnresolvedReferenceInfo>();
        PacakgeReferenceErrorList = new List<PacakgeReferenceError>();
        if (parent != null)
        {
            _rootDataItem = parent.Entities;
        }
    }

    public bool AttachFixidValuesForFixidDatabaseGenerator { get; set; }

    public List<string> SubPackagePathList { get; set; }

    public List<ReaderUnresolvedReferenceInfo> UnresolvedReferenceInfo { get; set; }

    public ReadMode CurrentReadMode { get; set; }

    private EntityPackageReference TargetEntityPackageReference { get; }

    public bool IsRootPackage { get; set; }

    public List<PacakgeReferenceError> PacakgeReferenceErrorList { get; }

    private bool IsLazyLoadRecursive { get; }

    public static Func<int, int, string, bool> ProgressReporter { get; set; }

    public void Dispose()
    {
        _document = null;
    }

    public DataItem Read(string path, bool isRootPackage = true)
    {
        IsRootPackage = isRootPackage;
        if (isRootPackage)
        {
            _loadedFilesCount = 0;
        }

        DataItem result = null;
        TryRead(path, out result);
        if (DBG_USE_CACHE && IsRootPackage)
        {
            documentCache.Clear();
        }

        return result;
    }

    public bool TryRead(string path, out DataItem result, bool noCache = false)
    {
        _loadingFilePath = path;
        result = null;
        // try
        // {
        if (noCache)
        {
            Lazy<Tuple<bool, XmlDocument>> lazy = null;
            documentCache.TryRemove(path, out lazy);
        }

        var orAdd = documentCache.GetOrAdd(path, new Lazy<Tuple<bool, XmlDocument>>(() =>
        {
            var flag = false;
            var xmlDocument = new XmlDocument();
            // try
            // {
            if ((File.GetAttributes(path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                flag = true;
            }
            // }
            // catch { }

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                xmlDocument.Load(fileStream);
                if (xmlDocument.DocumentElement!.Name != "package")
                {
                    Console.WriteLine($"[E] File was not an Entity Package at {path}");
                    return Tuple.Create(flag, (XmlDocument)null);
                }
            }

            return Tuple.Create(flag, xmlDocument);
        }));
        var document = orAdd.Value.Item2;

        if (document == null)
        {
            return false;
        }

        var flag1 = orAdd.Value.Item1;
        _recursiveImport = true;
        ++_loadedFilesCount;
        if (ProgressReporter != null)
        {
            var num = ProgressReporter(_loadedFilesCount, ExpectedTotalFilesCount, path) ? 1 : 0;
        }

        SubPackagePathList = new List<string>();
        if (!TryRead(document, out result))
        {
            return false;
        }

        if (!flag1)
        {
            result.EarcModified = true;
        }

        if (result is EntityPackage)
        {
            ((EntityPackage)result).EarcCopyguard =
                XmlUtility.GetAttributeBool(document.DocumentElement, "copyguard", false);
        }

        return true;
        // }
        // catch (Exception ex)
        // {
        //     Console.Error.WriteLine("Jenova: error: {0},{1}", ex.Message, ex.StackTrace);
        //     return false;
        // }
    }

    public DataItem Read(byte[] data)
    {
        try
        {
            _recursiveImport = false;
            using (var memoryStream = new MemoryStream(data))
            {
                _document = new XmlDocument();
                _document.Load(memoryStream);
                return Read(_document);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Jenova: error: {0},{1}", ex.Message, ex.StackTrace);
        }

        return null;
    }

    public DataItem ReadFromTemporaryBuffer(byte[] data, string parentFilePath)
    {
        try
        {
            _recursiveImport = true;
            _loadingFilePath = parentFilePath;
            SubPackagePathList = new List<string>();
            using (var memoryStream = new MemoryStream(data))
            {
                _document = new XmlDocument();
                _document.Load(memoryStream);
                return Read(_document);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Jenova: error: {0},{1}", ex.Message, ex.StackTrace);
        }

        return null;
    }

    public DataItem Read(XmlDocument document)
    {
        DataItem result = null;
        TryRead(document, out result);
        return result;
    }

    public bool TryRead(XmlDocument document, out DataItem result)
    {
        result = null;
        // try
        // {
        var documentElement = document.DocumentElement;
        if (documentElement == null)
        {
            return true;
        }

        readObjects(XmlUtility.GetElement(documentElement, "objects"));
        foreach (DataItem dataItem in _objects)
        {
            if (dataItem != null)
            {
                dataItem.Modified = false;
            }
        }

        if (_objects.Count > 0)
        {
            if (_objects[0] != null)
            {
                _objects[0].EarcModified = false;
                onReadEnd(_objects[0]);
            }

            result = _objects[0];
            return true;
        }
        // }
        // catch (Exception ex)
        // {
        //     Console.Error.WriteLine("Jenova: error: {0},{1}", ex.Message, ex.StackTrace);
        //     // var num = (int)DocumentInterface.DocumentContainer.ShowMessageBox(
        //     //     string.Format("{0}\n{1}", ex.Message, ex.StackTrace), "Jenova Error");
        //     result = null;
        //     return false;
        // }

        return true;
    }

    private void readObjects(XmlElement container)
    {
        var flag = CurrentReadMode == ReadMode.RM_NEW_PACKAGE_RECURSIVE || IsRootPackage;
        var xmlElementList = new List<XmlElement>();
        var elements = XmlUtility.GetElements(container, "object");
        if (elements.Length != 0)
        {
            if (CurrentReadMode == ReadMode.RM_EXISTING_PACKAGE_ONLY &&
                XmlUtility.GetAttributeText(elements[0], "type") == "Black.Entity.Prefab.Prefab")
            {
                flag = true;
            }

            if (flag)
            {
                xmlElementList.AddRange(elements);
            }
            else
            {
                xmlElementList.Add(elements[0]);
            }

            if (elements.Length == 1)
            {
                flag = true;
            }
        }

        foreach (var element in xmlElementList)
        {
            Object readDataItem = null;
            if (TryReadObject(element, out readDataItem))
            {
                _objects.Add(readDataItem);
            }
        }

        for (var index = 0; index < xmlElementList.Count; ++index)
        {
            readDynamicObjects(xmlElementList[index], _objects[index]);
        }

        for (var index = 0; index < xmlElementList.Count; ++index)
        {
            readReferences(xmlElementList[index], _objects[index]);
        }

        if (!(_objects[0] is EntityPackageReference packageReference))
        {
            return;
        }

        packageReference.FullFilePath = _loadingFilePath.Replace('\\', '/');
        packageReference.IsLoaded = flag;
    }

    public bool TryReadObject(XmlElement element, out Object readDataItem)
    {
        readDataItem = null;
        var attributeInt1 = XmlUtility.GetAttributeInt(element, "objectIndex", -1);
        var attributeText1 = XmlUtility.GetAttributeText(element, "name");
        var typeName = XmlUtility.GetAttributeText(element, "type");
        var attributeText2 = XmlUtility.GetAttributeText(element, "path");
        XmlUtility.GetAttributeText(element, "owner");
        var ownerIndex = XmlUtility.GetAttributeInt(element, "ownerIndex", -1);
        var ownerPath = XmlUtility.GetAttributeText(element, "ownerPath");
        var attributeText4 = XmlUtility.GetAttributeText(element, "csharp_type");
        if (!string.IsNullOrEmpty(attributeText4))
        {
            typeName = attributeText4;
        }

        if ((IsLazyLoadRecursive ? 1 : CurrentReadMode == ReadMode.RM_EXISTING_PACKAGE_ONLY ? 1 : 0) != 0 &&
            TargetEntityPackageReference != null && attributeInt1 == 0)
        {
            _objects.Add(TargetEntityPackageReference);
            return false;
        }

        var parent1 = GetObjectDataItem(ownerIndex, ownerPath) ?? _rootDataItem;
        if (string.IsNullOrEmpty(attributeText1))
        {
            //Console.WriteLine("XmlReader : readObjects: element name is invalid : path=" + attributeText2 +
            //                  " ownerPath=" + attributeText3);
        }

        Object @object = null;
        var parent2 = parent1?.Parent;
        string sourceFullPath = null;
        if (_recursiveImport && typeName == "SQEX.Ebony.Framework.Entity.EntityPackageReference" &&
            parent2 is EntityGroup)
        {
            var innerText = XmlUtility.GetElement(element, "sourcePath_")?.InnerText;
            if (innerText != null)
            {
                var relativePath = getDataRelativeSourcePath(innerText);
                var dataFullPath = Project.GetDataFullPath(relativePath);
                var xmlPath = dataFullPath.Replace(".ebex", ".xml").Replace(".prefab", ".xml");
                if (!File.Exists(xmlPath))
                {
                    var text = "XmlReader[E] : Package File not found : " + xmlPath;
                    Console.Error.WriteLine(text);
                    if (DocumentInterface.DocumentContainer != null)
                    {
                        //DocumentInterface.DocumentContainer.PrintWarning(text);
                    }

                    sourceFullPath = dataFullPath;
                }
                else
                {
                    using (var xmlReader = new EbexReader(parent2 as EntityGroup, CurrentReadMode))
                    {
                        if (checkCircularReference(parent1.ParentPackage, dataFullPath))
                        {
                            SubPackagePathList.Add(dataFullPath);
                            @object = xmlReader.Read(xmlPath, false) as Object;
                            if (@object is EntityPackage entityPackage8)
                            {
                                entityPackage8.AlreadyReadObjectsAtLoading = true;
                            }

                            if (@object is EntityPackageReference packageReference8)
                            {
                                packageReference8.FullFilePath = dataFullPath;
                            }

                            UnresolvedReferenceInfo.AddRange(xmlReader.UnresolvedReferenceInfo);
                            PacakgeReferenceErrorList.AddRange(xmlReader.PacakgeReferenceErrorList);
                        }
                    }
                }
            }
        }

        if (@object == null)
        {
            @object =
                DocumentInterface.ModuleContainer.CreateObjectFromString(parent1, typeName, _loadingFilePath) as
                    Object;
            if (@object is EntityPackageReference packageReference11 && !string.IsNullOrWhiteSpace(_loadingFilePath))
            {
                packageReference11.FullFilePath = _loadingFilePath.Replace("\\", "/");
            }
        }

        if (sourceFullPath != null)
        {
            PacakgeReferenceErrorList.Add(new PacakgeReferenceError(@object, sourceFullPath));
        }

        readDataItem = @object;
        return true;
    }

    private bool checkCircularReference(EntityPackage p, string sourcepath)
    {
        var entityPackage = p;
        sourcepath = sourcepath.ToLower();
        for (; p != null && p != p.Parent; p = p.ParentPackage)
        {
            if (p.FullFilePath.ToLower() == sourcepath)
            {
                // var num = (int)DocumentInterface.DocumentContainer.ShowMessageBox("!!データを修正して下さい!!\n" + sourcepath +
                //     " が循環参照しています。\n参照しているパッケージは " + entityPackage.FullFilePath + " です。");
                return false;
            }
        }

        return true;
    }

    private void readDynamicObjects(XmlElement element, DataItem dataItem)
    {
        if (dataItem == null)
        {
            return;
        }

        if (CurrentReadMode != ReadMode.RM_EXISTING_PACKAGE_ONLY || TargetEntityPackageReference != dataItem)
        {
            var attributeText = XmlUtility.GetAttributeText(element, "name");
            if (!string.IsNullOrEmpty(attributeText))
            {
                dataItem.name = attributeText;
            }

            dataItem.IsChecked = XmlUtility.GetAttributeBool(element, "checked", true);
        }

        foreach (XmlNode childNode in element.ChildNodes)
        {
            if (childNode is XmlElement childElement && !XmlUtility.GetAttributeBool(childElement, "reference", false))
            {
                var name = childElement.Name;
                var child = dataItem[name];
                var typeName = XmlUtility.GetAttributeText(childElement, "type");
                var type = PrimitiveTypeUtility.FromName(typeName);
                if (child == null)
                {
                    var childByRenamedName = dataItem.GetChildByRenamedName(name);
                    if (childByRenamedName != null)
                    {
                        child = childByRenamedName;
                    }
                }

                if (child == null && XmlUtility.GetAttributeBool(childElement, "dynamic", false))
                {
                    var dataType = DocumentInterface.ModuleContainer[typeName];
                    child = dataType == null
                        ? new UnknownDataItem(dataItem, dataType)
                        : dataType.CreateDataItem(dataItem);
                    child.Name = name;
                    child.Field = new Field(name, typeName, false);
                    child.IsDynamic = true;
                    if (type != PrimitiveType.None && childElement.InnerText != null)
                    {
                        child.Field.DisplayName = XmlUtility.GetAttributeText(childElement, "name");
                        child.Value = new Value(type, childElement.InnerText);
                    }
                }

                if (child != null && type == PrimitiveType.None)
                {
                    readDynamicObjects(childElement, child);
                }
            }
        }
    }

    private void readReferences(XmlElement element, DataItem dataItem)
    {
        if (dataItem == null)
        {
            return;
        }

        var flag1 = dataItem is EntityPackage {AlreadyReadObjectsAtLoading: true} and not Prefab;

        if (CurrentReadMode != ReadMode.RM_EXISTING_PACKAGE_ONLY || TargetEntityPackageReference != dataItem)
        {
            var attributeText = XmlUtility.GetAttributeText(element, "name");
            if (!string.IsNullOrEmpty(attributeText))
            {
                dataItem.name = attributeText;
            }

            dataItem.IsChecked = XmlUtility.GetAttributeBool(element, "checked", true);
        }

        foreach (XmlNode childNode in element.ChildNodes)
        {
            if (childNode is XmlElement childElement)
            {
                if (XmlUtility.GetAttributeBool(childElement, "reference", false))
                {
                    var index = XmlUtility.GetAttributeInt(childElement, "connectorObjectIndex", -1);
                    string objectPath;
                    string relativePath;
                    if (index < 0)
                    {
                        index = XmlUtility.GetAttributeInt(childElement, "objectIndex", -1);
                        objectPath = XmlUtility.GetAttributeText(childElement, "object");
                        relativePath = XmlUtility.GetAttributeText(childElement, "relativePath") ?? string.Empty;
                    }
                    else if (XmlUtility.GetAttributeInt(childElement, "connectorIndex", -1) < 1)
                    {
                        objectPath = XmlUtility.GetAttributeText(childElement, "connectorObject");
                        relativePath = XmlUtility.GetAttributeText(childElement, "connectorRelativePath") ??
                                       string.Empty;
                    }
                    else
                    {
                        continue;
                    }

                    var other = GetObjectDataItem(index, relativePath);
                    var name = childElement.Name;
                    var typeName = XmlUtility.GetAttributeText(childElement, "type");
                    if (name != null && typeName == "pointer")
                    {
                        var child = dataItem[name];
                        if (child != null)
                        {
                            child.Value = new Value(PrimitiveType.Pointer, other);
                            if (other == null)
                            {
                                var unresolvedPointerPackageSource =
                                    XmlUtility.GetAttributeText(childElement, "UnresolvedPointerPackageSource");
                                UnresolvedReferenceInfo.Add(new ReaderUnresolvedReferenceInfo(child,
                                    objectPath, relativePath, unresolvedPointerPackageSource, null));
                            }
                        }
                    }
                    else if (other != null)
                    {
                        var flag2 = true;
                        if (other is TrayDataItem && dataItem.Parent is EntityPackage)
                        {
                            flag2 = false;
                            dataItem.RemoveWithoutModifyFlag(other);
                            if (DocumentInterface.DocumentContainer != null)
                            {
                                // DocumentInterface.DocumentContainer.PrintWarning(
                                //     "XmlReader : Tray removed : Package has Tray directly!");
                            }

                            //Console.WriteLine("Jenova: error : Package has Tray directly!");
                        }

                        if (flag2)
                        {
                            dataItem.Add(other);
                        }
                    }
                    else if (XmlUtility.GetAttributeBool(childElement, "UsePrefabConnectionV20", false) |
                             XmlUtility.GetAttributeBool(childElement, "UseTemplateConnection", false))
                    {
                        var attributeText4 = XmlUtility.GetAttributeText(childElement, "PrefabOwnerPackageSource");
                        var attributeText5 =
                            XmlUtility.GetAttributeText(childElement, "ProxyConnectionOwnerPackageName");
                        UnresolvedReferenceInfo.Add(new ReaderUnresolvedReferenceInfo(dataItem, objectPath,
                            relativePath,
                            attributeText4, attributeText5));
                    }
                    else if (index != -1 && !string.IsNullOrEmpty(relativePath) &&
                             DocumentInterface.DocumentContainer != null)
                    {
                        // DocumentInterface.DocumentContainer.PrintWarning("Jenova: warning: Index" + attributeInt +
                        //                                                  "要素" + str + "が見つかりません。\n");
                    }
                }
                else
                {
                    var name = childElement.Name;
                    if (name != null)
                    {
                        var child = dataItem[name];
                        var type = XmlUtility.GetAttributeText(childElement, "type");
                        var primitiveType = PrimitiveTypeUtility.FromName(type);

                        // Temporarily attach the fixid uint value to the element so it can be grabbed for the fixid DB
                        if (AttachFixidValuesForFixidDatabaseGenerator)
                        {
                            if (primitiveType == PrimitiveType.Fixid && childElement.HasAttribute("fixid") &&
                                child != null)
                            {
                                var fixidString = XmlUtility.GetAttributeText(childElement, "fixid");
                                if (uint.TryParse(fixidString, out var fixid))
                                {
                                    child.CenterText = fixid.ToString();
                                    // var fixidItem = new ValueDataItem(child, "SQEX.Ebony.Std.Fixid");
                                    // fixidItem.Value = new Value(fixid);
                                    // child.Add(fixidItem);
                                }
                            }
                        }

                        var hasRenamedName = false;
                        if (child == null)
                        {
                            var childByRenamedName = dataItem.GetChildByRenamedName(name);
                            if (childByRenamedName != null)
                            {
                                hasRenamedName = true;
                                if (primitiveType != PrimitiveType.None)
                                {
                                    if (childByRenamedName.DataType is Field dataType30 &&
                                        PrimitiveTypeUtility.FromName(dataType30.TypeName) != PrimitiveType.None)
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
                            var flag4 = false;
                            Class deprecatedType = null;
                            if (dataItem.DataType is Class dt && dt[name] != null &&
                                dt[name].Deprecated)
                            {
                                deprecatedType = dt;
                                flag4 = true;
                            }

                            if (flag4)
                            {
                                if (!string.IsNullOrEmpty(childElement.InnerText))
                                {
                                    if (deprecatedType[name].DataType is Class @class)
                                    {
                                        var baseType =
                                            DocumentInterface.ModuleContainer["SQEX.Ebony.Framework.Node.GraphPin"]
                                                as Class;
                                        if (@class.IsBasedOn(baseType))
                                        {
                                            if (childElement["connections_"] != null &&
                                                childElement["connections_"].HasChildNodes)
                                            {
                                                child = new DeprecatedDataItem(dataItem, @class);
                                                child.Name = name;
                                                child.Field = deprecatedType[name];
                                            }
                                        }
                                        else
                                        {
                                            child = new DeprecatedDataItem(dataItem, @class);
                                            child.Name = name;
                                            child.Field = deprecatedType[name];
                                        }
                                    }
                                    else
                                    {
                                        child = new DeprecatedDataItem(dataItem, deprecatedType[name]);
                                        child.Name = name;
                                        child.Field = deprecatedType[name];
                                    }
                                }
                            }
                            else if (primitiveType != PrimitiveType.None)
                            {
                                child = new ValueDataItem(dataItem, type);
                                child.Name = name;
                                child.Browsable = false;
                            }
                            else if (dataItem is Prefab && name == "differences")
                            {
                                child = new EntityDiff(dataItem);
                                child.Name = name;
                                child.Field = new Field(child.Name,
                                    "SQEX.Ebony.Framework.Entity.EntityDiff", false);
                                child.Browsable = false;
                            }
                            else if (dataItem is EntityDiff)
                            {
                                child = new EntityDiffGroup(dataItem);
                                child.Name = name;
                                child.Field = new Field(child.Name,
                                    "SQEX.Ebony.Framework.Entity.EntityDiffGroup", false);
                                child.Browsable = false;
                            }
                        }

                        if (child != null)
                        {
                            if (primitiveType == PrimitiveType.None)
                            {
                                readReferences(childElement, child);
                            }
                            else if (CurrentReadMode != ReadMode.RM_EXISTING_PACKAGE_ONLY ||
                                     TargetEntityPackageReference != dataItem)
                            {
                                var flag5 = true;
                                if (flag1)
                                {
                                    if (child.Field == null)
                                    {
                                        flag5 = false;
                                    }
                                    else
                                    {
                                        child.Field.TryGetAttributeBool("UseReferenceValue", out flag5);
                                    }
                                }

                                if (flag5)
                                {
                                    var str2 = childElement.InnerText;
                                    if (child.Field != null)
                                    {
                                        if (child.Field.GetAttribute("FileExtension") != null)
                                        {
                                            str2 = getDataRelativeSourcePath(str2);
                                        }

                                        if (child.Value != null && child.Value is Value obj15 &&
                                            obj15.TypeName != type)
                                        {
                                            if (child.Field.GetAttribute("ConvertTypeAtLoading") != null)
                                            {
                                                type = child.Field.TypeName;
                                            }

                                            if (hasRenamedName)
                                            {
                                                type = child.Field.TypeName;
                                            }
                                        }
                                    }

                                    child.Value =
                                        DocumentInterface.ModuleContainer.CreateObjectFromString(null, null, type,
                                            str2, true, false);
                                }
                            }

                            if (child.Field != null && child.Children.Count > 0)
                            {
                                ModuleContainer.ApplyUseParentAttribute(child);
                            }
                        }
                    }
                    //Console.WriteLine("Jenova: warning: childName is not found...?");
                }
            }
        }
    }

    private DataItem GetObjectDataItem(int objectIndex, string relativePath = null)
    {
        if (objectIndex < 0 || objectIndex >= _objects.Count)
        {
            return null;
        }

        DataItem child = _objects[objectIndex];
        var relativePath1 = new ItemPath(relativePath);
        if (child != null && relativePath1.Exists)
        {
            child = child.GetChild(relativePath1);
        }

        return child;
    }

    private void onReadEnd(DataItem dataItem)
    {
        resolveDifferences(dataItem);
        if (DocumentInterface.DocumentContainer == null)
        {
            return;
        }

        DocumentInterface.DocumentContainer.doOnEntityPackageOpened(dataItem as EntityPackage);
    }

    private string getDataRelativeSourcePath(string origPath)
    {
        return !origPath.StartsWith(".")
            ? origPath
            : Project.GetDataRelativePath(new Uri(new Uri(Path.GetDirectoryName(_loadingFilePath) + "/"), origPath)
                .AbsolutePath.Replace('\\', '/').Replace("%20", " "));
    }

    private void resolveDifferences(DataItem parentDataItem)
    {
        foreach (var child in parentDataItem.Children)
        {
            if (child is Entity)
            {
                var entity = (Entity)child;
                if (EntityPresetUtility.IsEnablePresetDifference(entity))
                {
                    entity = EntityPresetUtility.ApplyEntityPresetFromSavedPath(entity);
                }

                entity.ResolveDifference();
            }

            if (child.Parent == parentDataItem)
            {
                resolveDifferences(child);
            }
        }
    }

    public class ReaderUnresolvedReferenceInfo
    {
        public DataItem DataItem_;
        public string ItemPathString_;
        public string PackageSourcePathString_;
        public string ProxyConnectionOwnerPackageName_;
        public string RelativePathString_;

        public ReaderUnresolvedReferenceInfo(
            DataItem dataItem,
            string itemPathString,
            string relativePathString,
            string packageSourcePathString,
            string proxyConnectionOwnerPackageName)
        {
            DataItem_ = dataItem;
            ItemPathString_ = itemPathString;
            RelativePathString_ = relativePathString;
            PackageSourcePathString_ = packageSourcePathString;
            ProxyConnectionOwnerPackageName_ = proxyConnectionOwnerPackageName;
        }
    }

    public class PacakgeReferenceError
    {
        public PacakgeReferenceError(DataItem item, string sourceFullPath)
        {
            Item = item;
            SourceFullPath = sourceFullPath;
        }

        public DataItem Item { get; }

        public string SourceFullPath { get; }
    }
}