using System;
using System.Xml.Serialization;

namespace Flagrum.Core.Scripting.Ebex.Configuration;

[XmlRoot("NamespaceAlias")]
[Serializable]
public class NamespaceAlias
{
    public string Original { get; set; }

    public string Alias { get; set; }
}