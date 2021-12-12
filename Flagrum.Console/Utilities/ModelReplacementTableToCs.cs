using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Flagrum.Core.Archive.Binmod;

namespace Flagrum.Console.Utilities;

// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks />
[Serializable]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
[XmlRootAttribute(Namespace = "", IsNullable = false)]
public class ArrayOfModTemplateItem
{
    private ArrayOfModTemplateItemModTemplateItem[] modTemplateItemField;

    /// <remarks />
    [XmlElementAttribute("ModTemplateItem")]
    public ArrayOfModTemplateItemModTemplateItem[] ModTemplateItem
    {
        get => modTemplateItemField;
        set => modTemplateItemField = value;
    }
}

/// <remarks />
[Serializable]
[DesignerCategory("code")]
[XmlTypeAttribute(AnonymousType = true)]
public class ArrayOfModTemplateItemModTemplateItem
{
    private string modTypeField;

    private string[] origin_gmdlsField;

    private string typeField;

    /// <remarks />
    public string ModType
    {
        get => modTypeField;
        set => modTypeField = value;
    }

    /// <remarks />
    public string Type
    {
        get => typeField;
        set => typeField = value;
    }

    /// <remarks />
    [XmlArrayItemAttribute(IsNullable = false)]
    public string[] Origin_gmdls
    {
        get => origin_gmdlsField;
        set => origin_gmdlsField = value;
    }
}

public class ModelReplacementTableToCs
{
    public static void Run()
    {
        var input = "C:\\Testing\\replace_table.xml";

        var serializer = new XmlSerializer(typeof(ArrayOfModTemplateItem));
        using var reader = new FileStream(input, FileMode.Open);
        var result = (ArrayOfModTemplateItem)serializer.Deserialize(reader);

        var builder = new StringBuilder();
        builder.AppendLine("var dictionary = new Dictionary<ModelReplacementTarget, string[]>");
        builder.AppendLine("{");

        foreach (var item in result.ModTemplateItem)
        {
            if (item.ModType != "character")
            {
                continue;
            }

            var target = BinmodTypeHelper.GetBinmodTarget((int)BinmodType.Character, item.Type);
            builder.AppendLine("    {");
            builder.AppendLine(
                $"        ModelReplacementTarget.{(ModelReplacementTarget)target}, new string[{item.Origin_gmdls.Length}]");
            builder.AppendLine("        {");

            foreach (var path in item.Origin_gmdls)
            {
                builder.Append($"            \"{path}\"");

                if (path != item.Origin_gmdls.Last())
                {
                    builder.Append(',');
                }

                builder.Append('\n');
            }

            builder.Append("        }\n");
            builder.Append("    }");

            if (item != result.ModTemplateItem.Last())
            {
                builder.Append(',');
            }

            builder.Append('\n');
        }

        builder.AppendLine("};");

        File.WriteAllText("C:\\Testing\\replace_table.cs", builder.ToString());
    }
}