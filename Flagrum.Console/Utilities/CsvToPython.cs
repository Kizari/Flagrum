using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Newtonsoft.Json;

namespace Flagrum.Console.Utilities;

/// <summary>
///     Used for parsing materials spreadsheet to a usable format
/// </summary>
public static class CsvToPython
{
    private const int Index = 0;
    private const int Name = 1;
    private const int InputName = 2;
    private const int InputDefault = 3;
    private const int InputRelevant = 4;
    private const int TextureName = 5;
    private const int TextureDefault = 6;
    private const int TextureRelevant = 7;

    public static void Parse(string mainCsvPath, string inputImportanceCsvPath, string textureImportanceCsvPath,
        string originalNamesCsvPath)
    {
        var items = new List<MaterialDataItem>();

        using var reader = new StreamReader(mainCsvPath);
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

        using var reader2 = new StreamReader(inputImportanceCsvPath);
        using var inputCsv = new CsvReader(reader2, CultureInfo.InvariantCulture);

        while (inputCsv.Read())
        {
            var inputName = inputCsv.GetField<string>(0);

            // This is relevance, but also exists in main CSV so we ignore it
            _ = inputCsv.GetField<int>(1);

            var importance = inputCsv.GetField<int>(2);

            foreach (var item in items)
            {
                foreach (var input in item.Inputs)
                {
                    if (input.Name == inputName)
                    {
                        input.Importance = importance;
                    }
                }
            }
        }

        using var reader3 = new StreamReader(textureImportanceCsvPath);
        using var textureCsv = new CsvReader(reader3, CultureInfo.InvariantCulture);

        while (textureCsv.Read())
        {
            var textureName = textureCsv.GetField<string>(0);

            // This is relevance, but also exists in main CSV so we ignore it
            _ = textureCsv.GetField<int>(1);

            var importance = textureCsv.GetField<int>(2);

            foreach (var item in items)
            {
                foreach (var texture in item.Textures)
                {
                    if (texture.Name == textureName)
                    {
                        texture.Importance = importance;
                    }
                }
            }
        }

        using var reader4 = new StreamReader(originalNamesCsvPath);
        using var originalNamesCsv = new CsvReader(reader4, CultureInfo.InvariantCulture);

        // First 5 rows are blank
        originalNamesCsv.Read();
        originalNamesCsv.Read();
        originalNamesCsv.Read();
        originalNamesCsv.Read();
        originalNamesCsv.Read();

        while (originalNamesCsv.Read())
        {
            var originalName = originalNamesCsv.GetField<string>(4);
            var alias = originalNamesCsv.GetField<string>(7);

            foreach (var item in items)
            {
                if (item.Name == alias)
                {
                    item.OriginalName = originalName;
                }
            }
        }

        var json = JsonConvert.SerializeObject(items, Formatting.Indented);
        File.WriteAllText(mainCsvPath.Replace(".csv", ".json"), json);
        GeneratePythonClasses(Path.GetDirectoryName(mainCsvPath), items);
    }

    private static void GeneratePythonClasses(string directory, List<MaterialDataItem> items)
    {
        var template =
            @"from bpy.types import PropertyGroup
from bpy.props import (
    EnumProperty,
    FloatProperty,
    FloatVectorProperty,
    StringProperty,
    BoolProperty,
    CollectionProperty,
    IntProperty
)

from .entities import MaterialPropertyMetadata


original_name_dictionary = {
{{original_name_dictionary}}
}


material_enum = (
    ('NONE', 'None', 'No material'),
{{material_enum}}
)


material_properties = {
    'NONE': [],
{{material_properties}}
}


class FlagrumMaterialProperty(PropertyGroup):
    property_name: StringProperty()
    is_relevant: BoolProperty()
    importance: IntProperty()
    property_type: StringProperty()


class FlagrumMaterialPropertyCollection(PropertyGroup):
    material_id: StringProperty()
    property_collection: CollectionProperty(type=FlagrumMaterialProperty)

{{material_property_definitions}}

class MaterialSettings(PropertyGroup):
    def update_preset(self, context):
        match = None

        for prop in self.property_collection:
            if prop.material_id == self.preset:
                match = prop
                break

        if match is None:
            new_property_collection = self.property_collection.add()
            new_property_collection.material_id = self.preset
            for prop in material_properties[self.preset]:
                new_property = new_property_collection.property_collection.add()
                new_property.property_name = prop.property_name
                new_property.is_relevant = prop.is_relevant
                new_property.importance = prop.importance
                new_property.property_type = prop.property_type
                setattr(new_property_collection, prop.property_name, prop.default_value)

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
    property_collection: CollectionProperty(type=FlagrumMaterialPropertyCollection)
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

            propertyDefinitions +=
                $"    {(input.Name.StartsWith("_") ? "# " : "")}{input.Name}: {propertyDefinition}\n";
        }

        propertyDefinitions = uniqueTextures.Aggregate(propertyDefinitions, (current, texture) =>
            current +
            $"    {(texture.Name.StartsWith("_") ? "# " : "")}{texture.Name}: StringProperty(subtype='FILE_PATH')\n");

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

                var defaultValue = "";
                if (input.Defaults.Count > 1)
                {
                    defaultValue = input.Defaults
                        .Aggregate("[", (current, value) => current + $"{value}, ");

                    // Remove trailing comma+space
                    defaultValue = defaultValue.Remove(defaultValue.Length - 2);
                    defaultValue += "]";
                }
                else
                {
                    defaultValue = input.Defaults[0].ToString();
                }

                materialProperties +=
                    $"MaterialPropertyMetadata('{input.Name}', {(input.IsRelevant ? "True" : "False")}, {input.Importance}, 'INPUT', {defaultValue}), ";
            }

            foreach (var texture in item.Textures)
            {
                if (texture.Name.StartsWith("_"))
                {
                    continue;
                }

                materialProperties +=
                    $"MaterialPropertyMetadata('{texture.Name}', {(texture.IsRelevant ? "True" : "False")}, {texture.Importance}, 'TEXTURE', ''), ";
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

        var originalNameDictionary = "";
        foreach (var item in items)
        {
            originalNameDictionary += $"    '{item.Name.ToUpper().Replace(' ', '_')}': '{item.OriginalName}'";
            if (item != items.Last())
            {
                originalNameDictionary += ",\n";
            }
        }

        template = template
            .Replace("{{material_enum}}", materialEnum)
            .Replace("{{material_properties}}", materialProperties)
            .Replace("{{material_property_definitions}}", propertyDefinitions)
            .Replace("{{original_name_dictionary}}", originalNameDictionary);

        File.WriteAllText(directory + "\\material_data.py", template);
    }

    private class MaterialDataItem
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public List<MaterialDataInput> Inputs { get; } = new();
        public List<MaterialDataTexture> Textures { get; } = new();
    }

    private class MaterialDataInput
    {
        public string Name { get; set; }
        public List<float> Defaults { get; set; }
        public bool IsRelevant { get; set; }
        public int Importance { get; set; }
    }

    private class MaterialDataTexture
    {
        public string Name { get; set; }
        public string Default { get; set; }
        public bool IsRelevant { get; set; }
        public int Importance { get; set; }
    }
}