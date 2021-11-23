using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Newtonsoft.Json;

namespace Flagrum.Console.Utilities;

/// <summary>
/// Used for parsing materials spreadsheet to a usable format
/// </summary>
public class CsvToJson
{
    private const int Index = 0;
    private const int Name = 1;
    private const int InputName = 2;
    private const int InputDefault = 3;
    private const int InputRelevant = 4;
    private const int TextureName = 5;
    private const int TextureDefault = 6;
    private const int TextureRelevant = 7;
    
    public static void Parse(string inputCsvPath)
    {
        var items = new List<MaterialDataItem>();
        
        using var reader = new StreamReader(inputCsvPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        MaterialDataItem currentRecord = null;
        
        // Skip headers
        csv.Read();

        while (csv.Read())
        {
            var index = csv.GetField<int?>(Index);
            var name = csv.GetField<string>(Name);

            if (!string.IsNullOrEmpty(name))
            {
                if (currentRecord != null)
                {
                    items.Add(currentRecord);
                }

                currentRecord = new MaterialDataItem
                {
                    Index = index ?? 0,
                    Name = name
                };
            }

            var inputName = csv.GetField<string>(InputName);
            var inputDefaults = csv.GetField<string>(InputDefault);
            var inputRelevant = csv.GetField<int?>(InputRelevant);

            if (!string.IsNullOrEmpty(inputName))
            {
                var floatStrings = inputDefaults.Split(',');
                var parsedInputDefaults = floatStrings.Select(float.Parse).ToList();

                currentRecord.Inputs.Add(new MaterialDataInput
                {
                    Name = inputName,
                    Defaults = parsedInputDefaults,
                    IsRelevant = inputRelevant == 1
                });
            }

            var textureName = csv.GetField<string>(TextureName);
            var textureDefault = csv.GetField<string>(TextureDefault);
            var textureRelevant = csv.GetField<int?>(TextureRelevant);

            if (!string.IsNullOrEmpty(textureName))
            {
                currentRecord.Textures.Add(new MaterialDataTexture
                {
                    Name = textureName,
                    Default = textureDefault,
                    IsRelevant = textureRelevant == 1
                });
            }
        }

        var json = JsonConvert.SerializeObject(items, Formatting.Indented);
        File.WriteAllText(inputCsvPath.Replace(".csv", ".json"), json);
        GeneratePythonClasses(Path.GetDirectoryName(inputCsvPath), items);
    }

    private static void GeneratePythonClasses(string directory, List<MaterialDataItem> items)
    {
        var template = @"from bpy.props import EnumProperty, FloatProperty, FloatVectorProperty, StringProperty, BoolProperty
from bpy.types import PropertyGroup


material_enum = (
    ('NONE', 'None', 'No material'),
{{material_enum}}
)


material_properties = {
    'NONE': [],
{{material_properties}}
}


class MaterialSettings(PropertyGroup):
    def update_preset(self, context):
        context.view_layer.objects.active.flagrum_material_properties.clear()
        for prop in material_properties[self.preset]:
            context.view_layer.objects.active.flagrum_material_properties.append(prop)

    preset: EnumProperty(
        items=material_enum,
        name='Material',
        description='Choose a material to apply to this mesh',
        default='NONE',
        options={'ANIMATABLE'},
        update=update_preset,
        get=None,
        set=None
    )

    show_advanced: BoolProperty(name='Show Advanced Options', default=False)

{{material_property_definitions}}
";
        
        var uniqueInputs = items
            .SelectMany(i => i.Inputs
                .OrderByDescending(input => input.IsRelevant)
                .ThenBy(input => input.Name))
            .DistinctBy(i => i.Name);

        var uniqueTextures = items
            .SelectMany(i => i.Textures
                .OrderByDescending(t => t.IsRelevant)
                .ThenBy(t => t.Name))
            .DistinctBy(t => t.Name);

        var propertyDefinitions = "";
        foreach (var input in uniqueInputs)
        {
            var propertyDefinition = "";
            if (input.Defaults.Count > 1)
            {
                propertyDefinition += $"FloatVectorProperty(size={input.Defaults.Count}, default=[";
                propertyDefinition = input.Defaults
                    .Aggregate(propertyDefinition, (current, value) => current + $"{value}, ");

                propertyDefinition = propertyDefinition.Remove(propertyDefinition.Length - 2);
                propertyDefinition += "])";
            }
            else
            {
                propertyDefinition = $"FloatProperty(default={input.Defaults[0]})";
            }

            propertyDefinitions += $"    {(input.Name.StartsWith("_") ? "# " : "")}{input.Name}: {propertyDefinition}\n";
        }

        propertyDefinitions = uniqueTextures.Aggregate(propertyDefinitions, (current, texture) =>
            current + $"    {(texture.Name.StartsWith("_") ? "# " : "")}{texture.Name}: StringProperty(subtype='FILE_PATH')\n");

        var materialProperties = "";
        foreach (var item in items)
        {
            materialProperties += $"    '{item.Name.ToUpper().Replace(' ', '_')}': [";
            foreach (var input in item.Inputs)
            {
                if (input.Name.StartsWith("_"))
                {
                    continue;
                }
                
                materialProperties += $"('{input.Name}', {(input.IsRelevant ? "True" : "False")}), ";
            }

            foreach (var texture in item.Textures)
            {
                if (texture.Name.StartsWith("_"))
                {
                    continue;
                }
                
                materialProperties += $"('{texture.Name}', {(texture.IsRelevant ? "True" : "False")}), ";
            }

            materialProperties = materialProperties.Remove(materialProperties.Length - 2);
            materialProperties += "]";

            if (item != items.Last())
            {
                materialProperties += ",\n";
            }
        }

        var materialEnum = "";
        foreach (var item in items)
        {
            materialEnum += $"    ('{item.Name.ToUpper().Replace(' ', '_')}', '{item.Name}', '')";

            if (item != items.Last())
            {
                materialEnum += ",\n";
            }
        }

        template = template
            .Replace("{{material_enum}}", materialEnum)
            .Replace("{{material_properties}}", materialProperties)
            .Replace("{{material_property_definitions}}", propertyDefinitions);
        
        File.WriteAllText(directory + "\\material_data.py", template);
    }

    private class MaterialDataItem
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public List<MaterialDataInput> Inputs { get; set; } = new();
        public List<MaterialDataTexture> Textures { get; set; } = new();
    }

    private class MaterialDataInput
    {
        public string Name { get; set; }
        public List<float> Defaults { get; set; }
        public bool IsRelevant { get; set; }
    }

    private class MaterialDataTexture
    {
        public string Name { get; set; }
        public string Default { get; set; }
        public bool IsRelevant { get; set; }
    }
}