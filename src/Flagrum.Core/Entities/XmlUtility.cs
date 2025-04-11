using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Flagrum.Core.Scripting.Ebex.Configuration;
using Flagrum.Core.Scripting.Ebex.Data;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex;

public class XmlUtility
{
    static XmlUtility()
    {
        SlimEbexConfig.Enabled = true;

        DocumentInterface.Configuration ??= Configuration.Configuration.OpenOrNew(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SQUARE ENIX",
                "Jenova", "Config.xml"), new JenovaApplicationConfiguration());

        if (DocumentInterface.ModuleContainer != null)
        {
            return;
        }

        DocumentInterface.ModuleContainer = new ModuleContainer();
    }

    public static bool Dummy()
    {
        return true;
    }

    public static XmlElement[] GetElements(XmlElement element, string name)
    {
        var xmlElementList = new List<XmlElement>();
        foreach (XmlNode childNode in element.ChildNodes)
        {
            if (childNode is XmlElement node && node.Name == name)
            {
                xmlElementList.Add(node);
            }
        }

        return xmlElementList.ToArray();
    }

    public static XmlElement GetElement(XmlElement element, string name)
    {
        return element[name];
    }

    public static XmlElement[] GetElements(
        XmlElement element,
        string containerName,
        string itemName)
    {
        var element1 = GetElement(element, containerName);
        return element1 != null ? GetElements(element1, itemName) : new XmlElement[0];
    }

    public static string GetAttributeText(XmlElement element, string name)
    {
        return element.Attributes[name]?.Value;
    }

    public static int GetAttributeInt(XmlElement element, string name, int defaultValue)
    {
        var attribute = element.Attributes[name];
        int result;
        return attribute != null && int.TryParse(attribute.Value, out result) ? result : defaultValue;
    }

    public static bool GetAttributeBool(XmlElement element, string name, bool defaultValue)
    {
        var attribute = element.Attributes[name];
        bool result;
        return attribute != null && bool.TryParse(attribute.Value, out result) ? result : defaultValue;
    }
}