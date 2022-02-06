using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Flagrum.Core.Ebex.Types;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Ebex;

public class ModuleReader
{
    private readonly Dictionary<string, EbexType> _types = new();

    public Dictionary<string, EbexType> Read()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var path = $@"{appData}\SquareEnix\FFXVMODTool\LuminousStudio\sdk\modules";

        foreach (var file in Directory.GetFiles(path, "*.mtml"))
        {
            ReadModule(file);
        }

        return _types;
    }

    private void ReadModule(string path)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.Load(path);
        var document = xmlDocument.DocumentElement;

        foreach (var element in document.GetChildContainerChildren("classes", "class"))
        {
            ParseDataType(element, "class");
        }

        foreach (var element in document.GetChildContainerChildren("enums", "enum"))
        {
            ParseDataType(element, "enum");
        }

        foreach (var element in document.GetChildContainerChildren("typedefs", "typedef"))
        {
            ParseDataType(element, "typedef");
        }
    }

    private void ParseDataType(XmlElement element, string typeName)
    {
        EbexType type = typeName switch
        {
            "class" => new EbexClass(element),
            "enum" => new EbexEnum(element),
            "typedef" => new EbexTypedef(element),
            _ => null
        };

        if (type != null)
        {
            _types[type.Name] = type;
        }
    }
}