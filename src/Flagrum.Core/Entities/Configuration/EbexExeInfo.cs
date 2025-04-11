using System;
using System.Xml.Serialization;

namespace Flagrum.Core.Scripting.Ebex.Configuration;

[XmlRoot("ExeInfo")]
[Serializable]
public class ExeInfo
{
    public string DisplayName { get; set; }

    public string ExeRoot { get; set; }

    public string ExePath { get; set; }

    public string Platform { get; set; }

    public string Args { get; set; }
}