using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Flagrum.Core.Scripting.Ebex.Configuration;
using Flagrum.Core.Scripting.Ebex.Data;
using Object = Flagrum.Core.Scripting.Ebex.Data.Object;

namespace Flagrum.Core.Scripting.Ebex.Type;

public class ModuleContainer : IDisposable
{
    private readonly Dictionary<Class, List<Class>> basedOnAllCache = new();

    private readonly ConcurrentDictionary<string, Lazy<DataType>> dataTypesCache = new();

    private string[] lastReadDirectory;
    private IEnumerable<Class> singletonClasses_;

    public ModuleContainer()
    {
        #if DEBUG
        readFromDirectory(new List<string> {"F:/FFXV/Builds/MOFiles_AppData/LuminousStudio/sdk/modules"});
        #else
        var path = Path.Combine(Flagrum.Core.Utilities.IOHelper.GetExecutingDirectory(), "Modules");
        readFromDirectory(new List<string> {path});
        #endif
    }

    public Dictionary<string, DataType> DataTypes { get; } = new();

    public DataType this[string name] => getCachedDataType(name);

    public int Count => DataTypes.Count;

    public Dictionary<string, KeyValuePair<Class, Field>[]> DropFileDataTypes { get; } = new();

    // public Dictionary<Class, MenuItemCommand[]> MenuItemCommands { get; } =
    //     new Dictionary<Class, MenuItemCommand[]>();

    public Dictionary<string, List<string>> FixIdTablePrefixList { get; } = new();

    public List<string> ClassUndefinedErrorTypeStrList { get; } = new();

    public Dictionary<string, string> ClassAliasDefinedTypeStrList { get; } = new();

    public Class[] ComponentClasses { get; private set; } = new Class[0];

    public void Dispose()
    {
        DataTypes.Clear();
        DropFileDataTypes.Clear();
    }

    private bool getBaseNameOf(string name, out string baseName, out string termName)
    {
        var length = name.LastIndexOf('.');
        if (length < 0)
        {
            baseName = null;
            termName = null;
            return false;
        }

        baseName = name.Substring(0, length);
        termName = name.Substring(length + 1);
        return true;
    }

    public bool GuessFullNameOfType(string partialName, out string fullName)
    {
        var str = "." + partialName;
        foreach (var dataType in DataTypes)
        {
            if (dataType.Key.Equals(partialName))
            {
                fullName = partialName;
                return true;
            }

            if (dataType.Key.EndsWith(str))
            {
                fullName = dataType.Key;
                return true;
            }
        }

        fullName = "";
        return false;
    }

    private DataType getCachedDataTypeInSuperTypes(string name)
    {
        string baseName1;
        string termName;
        if (!getBaseNameOf(name, out baseName1, out termName))
        {
            return null;
        }

        var cachedDataType1 = getCachedDataType(baseName1);
        if (cachedDataType1 == null)
        {
            return null;
        }

        if (!(cachedDataType1 is Class @class))
        {
            return null;
        }

        foreach (var baseName2 in @class.BaseNames)
        {
            var cachedDataType2 = getCachedDataType(baseName2 + "." + termName);
            if (cachedDataType2 != null)
            {
                return cachedDataType2;
            }
        }

        return null;
    }

    private DataType getCachedDataType(string name)
    {
        if (name == null)
        {
            return null;
        }

        if (DocumentInterface.ApplicationConfiguration != null &&
            DocumentInterface.ApplicationConfiguration.NamespaceAliases != null && !string.IsNullOrEmpty(name))
        {
            string namespaceName;
            if (getBaseNameOf(name, out namespaceName, out var _))
            {
                var namespaceAlias = DocumentInterface.ApplicationConfiguration.NamespaceAliases
                    .Where(val => namespaceName.StartsWith(val.Original)).FirstOrDefault();
                if (namespaceAlias != null)
                {
                    var key = name;
                    name = name.Replace(namespaceAlias.Original, namespaceAlias.Alias);
                    if (!ClassAliasDefinedTypeStrList.ContainsKey(key))
                    {
                        ClassAliasDefinedTypeStrList.Add(key, name);
                    }
                }
            }
        }

        return dataTypesCache.GetOrAdd(name, new Lazy<DataType>(() =>
        {
            DataType dataType = null;
            if (!DataTypes.TryGetValue(name, out dataType))
            {
                dataType = getCachedDataTypeInSuperTypes(name);
            }

            return dataType;
        })).Value;
    }

    public IEnumerable<Class> GetBasedOnAll(DataType dataType)
    {
        List<Class> classList = null;
        var class1 = dataType as Class;
        if (dataType != null && class1 != null)
        {
            if (!basedOnAllCache.TryGetValue(class1, out classList))
            {
                classList = new List<Class>();
                foreach (var dataType1 in DataTypes)
                {
                    if (dataType1.Value is Class class6 && class6.IsBasedOn(class1))
                    {
                        classList.Add(class6);
                    }
                }

                basedOnAllCache[class1] = classList;
            }
        }
        else
        {
            classList = new List<Class>();
        }

        return classList;
    }

    public IEnumerable<Class> GetBasedOnAll(string dataTypeFullName)
    {
        return GetBasedOnAll(this[dataTypeFullName]);
    }

    public List<Enum> GetEnumAll()
    {
        var enumList = new List<Enum>();
        foreach (var dataType in DataTypes)
        {
            if (dataType.Value is Enum enum1)
            {
                enumList.Add(enum1);
            }
        }

        return enumList;
    }

    public IEnumerable<Class> FindClassesWithAttribute(string attributeName)
    {
        return DataTypes
            .Where(kvp =>
                kvp.Value is Class && kvp.Value.ExistAttribute(attributeName))
            .Select(
                kvp => kvp.Value as Class);
    }

    public IEnumerable<Class> GetSingletonClasses()
    {
        if (singletonClasses_ == null)
        {
            singletonClasses_ = FindClassesWithAttribute("SingletonPointer");
        }

        return singletonClasses_;
    }

    public object CreateObjectFromString(DataItem parent, DataType dataType)
    {
        return CreateObjectFromString(parent, dataType, null, null, true, false);
    }

    public object CreateObjectFromString(DataItem parent, string typeName)
    {
        return CreateObjectFromString(parent, null, typeName, null, true, false);
    }

    public TObject Instantiate<TObject>(DataItem parent, string typeName)
    {
        return (TObject) CreateObjectFromString(parent, null, typeName, null, true, false);
    }

    public object CreateObjectFromString(DataItem parent, string typeName, string ebexFilePath)
    {
        return CreateObjectFromString(parent, null, typeName, null, true, false, ebexFilePath);
    }

    public object CreateObjectFromString(DataItem parent, DataType dataType, string typeName)
    {
        return CreateObjectFromString(parent, dataType, typeName, null, true, false);
    }

    public void setDataTypeInstanciators(DataType _dataType)
    {
        if (!(_dataType is Class @class))
        {
            return;
        }

        if (@class.FullName == "SQEX.Ebony.Framework.Sequence.Tray.SequenceTray")
        {
            @class.Instanciator = (parent, dataType) => new TrayDataItem(parent, dataType);
        }
        else if (@class.FullName == "SQEX.Ebony.Framework.Sequence.Tray.TemplateTray")
        {
            @class.Instanciator = (parent, dataType) => new TemplateTrayDataItem(parent, dataType);
        }
        else if (@class.FullName == "SQEX.Ebony.Framework.Sequence.Tray.PrefabTray")
        {
            @class.Instanciator = (parent, dataType) => new PrefabTrayDataItem(parent, dataType);
        }
        else if (@class.IsBasedOn(this["SQEX.Ebony.Framework.Sequence.Group.SequenceGroupBase"] as Class))
        {
            @class.Instanciator = (parent, dataType) => new SequenceGroupDataItem(parent, dataType);
        }
        else if (@class.FullName == "SQEX.Ebony.Framework.Sequence.Tray.MenuLogicTray")
        {
            @class.Instanciator = (parent, dataType) => new MenuLogicTrayDataItem(parent, dataType);
        }
        else if (@class.FullName == "Black.Sequence.Tray.SequenceDevelopingTray")
        {
            @class.Instanciator = (parent, dataType) => new ScopedTrayDataItem(parent, dataType);
        }
        else if (@class.FullName == "SQEX.Luminous.GameFramework.Sequence.SequenceDebugNameTray")
        {
            @class.Instanciator = (parent, dataType) => new SequenceDebugNameTray(parent, dataType);
        }
        else if (@class.FullName == "Black.Sequence.Action.TimeLine.SequenceActionTimeLineBlack")
        {
            @class.Instanciator = (parent, dataType) => new TimeLineDataItem2(parent, dataType);
        }
        else if (@class.FullName == "SQEX.Ebony.Framework.Sequence.Action.SequenceActionTimeLineSub")
        {
            @class.Instanciator = (parent, dataType) => new TimeLineDataItemSub(parent, dataType);
        }
        else if (@class.FullName == "SQEX.Ebony.Framework.Sequence.Action.SequenceActionTimeLineBase")
        {
            @class.Instanciator = (parent, dataType) => new TimeLineDataItem(parent, dataType);
        }
        else if (@class.FullName == "SQEX.Ebony.Framework.Sequence.Action.SequenceActionTimeLineMultiEventTrack")
        {
            @class.Instanciator = (parent, dataType) => new MultiEventDataItem(parent, dataType);
        }
        else if (@class.IsBasedOn(this["SQEX.Ebony.Framework.Sequence.Connector.SequenceConnectorBase"] as Class))
        {
            @class.Instanciator = (parent, dataType) => new ConnectorDataItem(parent, dataType);
        }
        else if (@class.IsBasedOn(this["SQEX.Luminous.GameFramework.Sequence.SequenceCSharpNode"] as Class))
        {
            @class.Instanciator = (parent, dataType) => new SequenceCSharpNodeDataItem(parent, dataType);
        }
        else if (@class.IsBasedOn(this["SQEX.Ebony.Framework.Sequence.SequenceActivatableNode"] as Class))
        {
            @class.Instanciator = (parent, dataType) => new SequenceActivatableNodeDataItem(parent, dataType);
        }
        else if (@class.IsBasedOn(this["SQEX.Ebony.Framework.Sequence.SequenceNode"] as Class))
        {
            @class.Instanciator = (parent, dataType) => new SequenceNodeDataItem(parent, dataType);
        }
        else if (@class.IsBasedOn(this["SQEX.Ebony.Framework.Entity.Prefab.Prefab"] as Class))
        {
            @class.Instanciator = (parent, dataType) => new Prefab(parent, dataType);
        }
        else if (@class.FullName == "SQEX.Ebony.Framework.Entity.EntityPackageReference")
        {
            @class.Instanciator = (parent, dataType) => new EntityPackageReference(parent, dataType);
        }
        else if (@class.FullName == "Black.Entity.Area.AreaPackage")
        {
            @class.Instanciator = (parent, dataType) => new AreaPackage(parent, dataType);
        }
        else if (@class.FullName == "Black.Entity.Area.MapPackage")
        {
            @class.Instanciator = (parent, dataType) => new MapPackage(parent, dataType);
        }
        else if (@class.FullName == "Black.Entity.Area.RotateMapPackage")
        {
            @class.Instanciator = (parent, dataType) => new RotateMapPackage(parent, dataType);
        }
        else if (@class.FullName == "Black.Entity.Area.WorldPackage")
        {
            @class.Instanciator = (parent, dataType) => new WorldPackage(parent, dataType);
        }
        else if (@class.FullName == "Black.Entity.Area.UniversalPackage")
        {
            @class.Instanciator = (parent, dataType) => new UniversalPackage(parent, dataType);
        }
        else if (@class.FullName == "Black.Entity.Data.CharacterEntry.CharaEntryPackage")
        {
            @class.Instanciator = (parent, dataType) => new CharaEntryPackage(parent, dataType);
        }
        else if (@class.FullName == "Black.Entity.CSharp.CSharpProjectPackage")
        {
            @class.Instanciator = (parent, dataType) => new CSharpProjectPackage(parent, dataType);
        }
        else if (@class.IsBasedOn(this["SQEX.Ebony.Framework.Entity.EntityPackage"] as Class))
        {
            @class.Instanciator = (parent, dataType) => new EntityPackage(parent, dataType);
        }
        else if (@class.IsBasedOn(this["SQEX.Ebony.Framework.Entity.EntityGroup"] as Class))
        {
            @class.Instanciator = (parent, dataType) => new EntityGroup(parent, dataType);
        }
        else if (@class.FullName == "SQEX.Ebony.Std.DynamicArray" ||
                 @class.FullName == "SQEX.Ebony.Std.IntrusivePointerDynamicArray")
        {
            @class.Instanciator = (parent, dataType) => new DynamicArray(parent);
        }
        else if (@class.IsBasedOn(this["SQEX.Ebony.Framework.Sequence.SequenceContainer"] as Class))
        {
            @class.Instanciator = (parent, dataType) => new SequenceContainer(parent, dataType);
        }
        else if (@class.IsBasedOn(this["SQEX.Ebony.AIGraph.Core.AIGraphContainer"] as Class))
        {
            @class.Instanciator = (parent, dataType) => new AIGraphContainer(parent, dataType);
        }
        else if (@class.IsBasedOn(this["SQEX.Ebony.Framework.Entity.Entity"] as Class))
        {
            if (CSharpEntity.IsCSharpEntity(@class))
            {
                @class.Instanciator = (parent, dataType) => new CSharpEntity(parent, dataType);
            }
            else
            {
                @class.Instanciator = (parent, dataType) => new Entity(parent, dataType);
            }
        }
        else if (@class.IsBasedOn(this["SQEX.Ebony.Framework.Node.GraphPin"] as Class))
        {
            @class.Instanciator = (parent, dataType) => new GraphPinDataItem(parent, dataType);
        }
        else if (@class.IsBasedOn(this["SQEX.Luminous.Core.Object.Object"] as Class))
        {
            @class.Instanciator = (parent, dataType) => new Object(parent, dataType);
        }
        else
        {
            @class.Instanciator = (parent, dataType) => new UnknownDataItem(parent, dataType);
        }
    }

    public IItem CreateObjectFromString(
        DataItem parent,
        DataType dataType,
        string typeName,
        string defaultValue,
        bool objectCreatable,
        bool pointer,
        string ebexFilePath = null)
    {
        if (pointer)
        {
            return new Value(PrimitiveType.Pointer, null);
        }

        var type1 = PrimitiveTypeUtility.FromNameSimple(typeName);
        if (type1 != PrimitiveType.None)
        {
            return new Value(type1, defaultValue);
        }

        dataType = dataType ?? this[typeName];
        if (dataType is Class @class && @class.Deprecated && DocumentInterface.DocumentContainer != null)
        {
            // DocumentInterface.DocumentContainer.PrintWarning("The class is deprecated（古いClassが使われています） : " +
            //                                                  @class.FullName +
            //                                                  (ebexFilePath != null
            //                                                      ? " [file=" + ebexFilePath + "]"
            //                                                      : ""));
        }

        if (dataType != null)
        {
            IItem iitem = dataType.CreateIItem(parent);
            if (iitem != null)
            {
                return iitem;
            }
        }

        if (dataType is Enum)
        {
            return new Value((Enum) dataType, defaultValue);
        }

        if (dataType is Typedef)
        {
            typeName = dataType.FullName;
        }

        if (typeName != null)
        {
            var type2 = PrimitiveTypeUtility.FromName(typeName);
            if (type2 != PrimitiveType.None)
            {
                return new Value(type2, defaultValue);
            }
        }

        if (dataType != null)
        {
            return dataType.CreateDataItem(parent);
        }

        if (DocumentInterface.DocumentContainer != null)
        {
            // DocumentInterface.DocumentContainer.PrintError("Unknown Type (old type?) : " + typeName +
            //                                                (ebexFilePath != null
            //                                                    ? " [file=" + ebexFilePath + "]"
            //                                                    : ""));
            if (!ClassUndefinedErrorTypeStrList.Contains(typeName))
            {
                ClassUndefinedErrorTypeStrList.Add(typeName);
            }
        }

        return new Value();
    }

    private bool readFromDirectory(IEnumerable<string> directories)
    {
        lastReadDirectory = directories.ToArray();
        Dispose();
        var stringSet = new HashSet<string>();
        // try
        // {
        foreach (var directory in directories)
        {
            foreach (var file in Directory.GetFiles(Path.GetFullPath(directory), "*.mtml",
                         SearchOption.AllDirectories))
            {
                if (stringSet.Add(Path.GetFileName(file)) && !file.Contains("RudeBuild_"))
                {
                    readModule(Path.Combine(directory, file));
                }
            }
        }

        matchTypes();
        foreach (var directory in directories)
        {
            readFixIdTablePrefix(directory);
        }

        foreach (var _dataType in DataTypes.Values)
        {
            setDataTypeInstanciators(_dataType);
        }

        return true;
        // }
        // catch (Exception ex)
        // {
        //     Console.Error.WriteLine(ex.Message);
        // }

        return false;
    }

    public bool Reload()
    {
        dataTypesCache.Clear();
        foreach (var dataType in DataTypes)
        {
            dataType.Value.invalidate();
        }

        return readFromDirectory(lastReadDirectory);
    }

    private bool readModule(string mtmlPath)
    {
        var xmlDocument = new XmlDocument();
        // try
        // {
        xmlDocument.Load(mtmlPath);
        var documentElement = xmlDocument.DocumentElement;
        if (documentElement != null)
        {
            foreach (var element in XmlUtility.GetElements(documentElement, "classes", "class"))
            {
                parseClass(element);
            }

            foreach (var element in XmlUtility.GetElements(documentElement, "enums", "enum"))
            {
                parseEnum(element);
            }

            foreach (var element in XmlUtility.GetElements(documentElement, "typedefs", "typedef"))
            {
                parseTypedef(element);
            }

            return true;
        }
        // }
        // catch (Exception ex)
        // {
        //     Console.Error.WriteLine(ex.Message);
        // }

        return false;
    }

    private void readFixIdTablePrefix(string directory)
    {
        var path = Path.Combine(directory, "fixid");
        if (!Directory.Exists(path))
        {
            return;
        }

        foreach (var file in Directory.GetFiles(path, "*.fixid.list"))
        {
            foreach (var readAllLine in File.ReadAllLines(file))
            {
                var chArray = new char[1] {','};
                var strArray = readAllLine.Split(chArray);
                if (strArray.Count() >= 2)
                {
                    var key = strArray[0];
                    var str = strArray[1];
                    if (!FixIdTablePrefixList.ContainsKey(key))
                    {
                        FixIdTablePrefixList.Add(key, new List<string>());
                    }

                    if (!FixIdTablePrefixList[key].Contains(str))
                    {
                        FixIdTablePrefixList[key].Add(str);
                    }
                }
            }
        }

        foreach (var key in FixIdTablePrefixList.Keys)
        {
            FixIdTablePrefixList[key].Sort();
        }
    }

    private void parseClass(XmlElement element)
    {
        var attributeText = XmlUtility.GetAttributeText(element, "name");
        if (attributeText == null)
        {
            return;
        }

        var item = new Class(attributeText);
        item.loadXml(element);
        DataTypes[item.FullName] = item;
        dataTypesCache[item.FullName] = new Lazy<DataType>(() => item);
    }

    private void parseEnum(XmlElement element)
    {
        var attributeText = XmlUtility.GetAttributeText(element, "name");
        if (attributeText == null)
        {
            return;
        }

        var item = new Enum(attributeText);
        item.loadXml(element);
        DataTypes[item.FullName] = item;
        dataTypesCache[item.FullName] = new Lazy<DataType>(() => item);
    }

    private void parseTypedef(XmlElement element)
    {
        var attributeText1 = XmlUtility.GetAttributeText(element, "name");
        if (attributeText1 == null)
        {
            return;
        }

        var attributeText2 = XmlUtility.GetAttributeText(element, "type");
        var attributeBool1 = XmlUtility.GetAttributeBool(element, "pointer", false);
        var attributeBool2 = XmlUtility.GetAttributeBool(element, "reference", false);
        var item = new Typedef(attributeText1, attributeText2, attributeBool1, attributeBool2);
        DataTypes[item.FullName] = item;
        dataTypesCache[item.FullName] = new Lazy<DataType>(() => item);
    }

    private void matchTypes()
    {
        foreach (var dataType in DataTypes)
        {
            dataType.Value.matchTypes(this);
        }

        foreach (var dataType in DataTypes)
        {
            if (dataType.Value is Class class1)
            {
                foreach (var field in class1.Fields)
                {
                    var attribute1 = field.GetAttribute("DropFile");
                    if (attribute1 != null)
                    {
                        var str = attribute1;
                        var chArray = new char[1] {';'};
                        foreach (var extension in str.Split(chArray))
                        {
                            addDropFileDataType(extension, class1, field);
                        }
                    }

                    for (var index = 0; index < 10; ++index)
                    {
                        var str = index == 0 ? "" : index.ToString();
                        var attribute2 = field.GetAttribute("MenuItemDisplayName" + str);
                        var attribute3 = field.GetAttribute("MenuItemCommand" + str);
                        if (attribute3 != null)
                        {
                            // var menuItemCommand = new MenuItemCommand();
                            // menuItemCommand.Field = field;
                            // menuItemCommand.DisplayName = attribute2 ?? attribute3;
                            // menuItemCommand.Command = attribute3;
                            // var menuItemCommandList = MenuItemCommands.ContainsKey(class1)
                            //     ? MenuItemCommands[class1].ToList()
                            //     : new List<MenuItemCommand>();
                            // menuItemCommandList.Add(menuItemCommand);
                            // MenuItemCommands[class1] = menuItemCommandList.ToArray();
                        }
                    }

                    if (field.GetAttribute("MenuItemReload") != null)
                    {
                        // var menuItemCommand = new MenuItemCommand();
                        // menuItemCommand.Field = field;
                        // menuItemCommand.DisplayName = TextResourcesJenovaData.TextReload;
                        // menuItemCommand.Command = "Reload";
                        // var menuItemCommandList = MenuItemCommands.ContainsKey(class1)
                        //     ? MenuItemCommands[class1].ToList()
                        //     : new List<MenuItemCommand>();
                        // menuItemCommandList.Add(menuItemCommand);
                        // MenuItemCommands[class1] = menuItemCommandList.ToArray();
                    }

                    resolveAutoSetAttribute(class1, field);
                    if (field.GetAttribute("DynamicLoadItem") != null)
                    {
                        class1.ContainsDynamicLoadItem = true;
                    }

                    if (field.GetAttribute("DynamicLoadPin") != null)
                    {
                        class1.ContainsDynamicLoadPin = true;
                    }

                    if (field.GetAttribute("DynamicDropDownList") != null)
                    {
                        class1.ContainsDynamicDropDownList = true;
                    }

                    var attribute4 = field.GetAttribute("StaticDropDownList");
                    if (!string.IsNullOrEmpty(attribute4))
                    {
                        readStaticDropDownListXml(Project.GetDataFullPath(attribute4), field);
                    }
                }
            }
        }

        ComponentClasses = GetBasedOnAll("SQEX.Luminous.Core.Component.Component").ToArray();
    }

    private bool readStaticDropDownListXml(string filePath, Field targetField)
    {
        return false;
        var xmlDocument = new XmlDocument();
        // try
        // {
        xmlDocument.Load(filePath);
        var documentElement = xmlDocument.DocumentElement;
        if (documentElement != null)
        {
            var elements = XmlUtility.GetElements(documentElement, "Item");
            var objectList = new List<object>();
            foreach (XmlNode xmlNode in elements)
            {
                var innerText = xmlNode["Name"].InnerText;
                objectList.Add(innerText);
            }

            targetField.StaticDropDownList = objectList;
            return true;
        }
        // }
        // catch (Exception ex)
        // {
        //     Console.Error.WriteLine(ex.Message);
        // }

        return false;
    }

    private void addDropFileDataType(string extension, Class addedClass, Field addedField)
    {
        var keyValuePair = new KeyValuePair<Class, Field>(addedClass, addedField);
        var keyValuePairList = DropFileDataTypes.ContainsKey(extension)
            ? DropFileDataTypes[extension].ToList()
            : new List<KeyValuePair<Class, Field>>();
        keyValuePairList.Add(keyValuePair);
        DropFileDataTypes[extension] = keyValuePairList.ToArray();
        foreach (var derivedType in addedClass.DerivedTypes)
        {
            addDropFileDataType(extension, derivedType, addedField);
        }
    }

    private void resolveAutoSetAttribute(Class parentClass, Field field)
    {
        var str1 = field.GetAttribute("AutoSetFromPath") ?? field.GetAttribute("AutoSetFromPathForce");
        if (str1 != null)
        {
            var name = str1;
            var length1 = str1.IndexOf(';');
            if (length1 >= 0)
            {
                name = str1.Substring(0, length1);
            }

            var length2 = name.IndexOf('@');
            if (length2 >= 0)
            {
                name = name.Substring(0, length2);
            }

            var fieldFromAllBase = parentClass.GetFieldFromAllBase(name);
            if (fieldFromAllBase != null && !fieldFromAllBase.AutoSetPropertyNameList.Contains(field.Name))
            {
                fieldFromAllBase.AutoSetPropertyNameList.Add(field.Name);
            }
        }

        var str2 = field.GetAttribute("AutoSetFromID");
        if (str2 != null)
        {
            bool flag;
            do
            {
                flag = false;
                var num = str2.IndexOf("#(");
                if (num >= 0)
                {
                    var startIndex = str2.IndexOf(')', num + 2);
                    if (startIndex >= 0)
                    {
                        var name = str2.Substring(num + 2, startIndex - (num + 2));
                        var field1 = parentClass[name];
                        if (field1 != null && !field1.AutoSetPropertyNameList.Contains(field.Name))
                        {
                            field1.AutoSetPropertyNameList.Add(field.Name);
                        }

                        str2 = str2.Substring(startIndex);
                        flag = true;
                    }
                }
            } while (flag);
        }

        var attribute = field.GetAttribute("AutoSetBoolFromValue");
        if (attribute == null)
        {
            return;
        }

        var name1 = attribute;
        var fieldFromAllBase1 = parentClass.GetFieldFromAllBase(name1);
        if (fieldFromAllBase1 == null || fieldFromAllBase1.AutoSetPropertyNameList.Contains(field.Name))
        {
            return;
        }

        fieldFromAllBase1.AutoSetPropertyNameList.Add(field.Name);
    }

    public static void ApplyUseParentAttribute(DataItem parentItem)
    {
        foreach (var child in parentItem.Children)
        {
            if (child.Field != null && child.DataType != null &&
                child.Field.GetAttribute("UseParentAttribute") != null && !child.Field.ParentAttributeResolved)
            {
                var dataItem = parentItem;
                while (!(dataItem is DynamicArray))
                {
                    dataItem = parentItem.Parent;
                    if (dataItem == null)
                    {
                        break;
                    }
                }

                if (dataItem != null)
                {
                    child.Field = new Field(child.Name, child.DataType.Name, false);
                    child.Field.ParentAttributeResolved = true;
                    foreach (var attribute in dataItem.Field.Attributes)
                    {
                        child.Field.Attributes[attribute.Key] = attribute.Value;
                    }
                }
            }
        }
    }
}