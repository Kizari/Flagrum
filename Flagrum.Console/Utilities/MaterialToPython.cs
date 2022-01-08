using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Gfxbin.Gmtl.Data;
using Newtonsoft.Json;

namespace Flagrum.Console.Utilities;

public static class MaterialToPython
{
    public static void ConvertFromMaterialFile(string materialPath, string outputPath)
    {
        var reader = new MaterialReader(materialPath);
        var material = reader.Read();
        Convert(material, outputPath);
    }

    public static void ConvertFromJsonFile(string materialPath, string outputPath)
    {
        var json = File.ReadAllText(materialPath);
        var material = JsonConvert.DeserializeObject<Material>(json);
        Convert(material, outputPath);
    }

    private static void Convert(Material material, string outputPath)
    {
        using var inputReader = new StreamReader("C:\\Testing\\Gfxbin\\Gmtl\\material_data_input_importance.csv");
        using var inputCsv = new CsvReader(inputReader, CultureInfo.InvariantCulture);

        var list = new List<dynamic>();
        while (inputCsv.Read())
        {
            var inputName = inputCsv.GetField<string>(0);
            var relevance = inputCsv.GetField<int>(1);
            var importance = inputCsv.GetField<int>(2);
            list.Add(new
            {
                Name = inputName,
                Relevance = relevance == 1 ? "True" : "False",
                Importance = importance
            });
        }

        var builder = new StringBuilder();
        foreach (var input in material.InterfaceInputs.Where(i => i.InterfaceIndex == 0))
        {
            var defaults = "";
            if (input.Values.Length > 1)
            {
                defaults += "[";
                foreach (var value in input.Values)
                {
                    defaults += value + ", ";
                }

                defaults = defaults.Remove(defaults.Length - 2);
                defaults += "]";
            }
            else
            {
                defaults = input.Values[0].ToString();
            }

            var match = list.FirstOrDefault(l => l.Name == input.ShaderGenName);
            if (match == null)
            {
                System.Console.WriteLine(input.ShaderGenName);
                continue;
            }

            builder.Append(
                $"MaterialPropertyMetadata('{input.ShaderGenName}', {match.Relevance}, {match.Importance}, 'INPUT', {defaults}),\n");
        }

        using var textureReader = new StreamReader("C:\\Testing\\Gfxbin\\Gmtl\\material_data_texture_importance.csv");
        using var textureCsv = new CsvReader(textureReader, CultureInfo.InvariantCulture);

        list = new List<dynamic>();
        while (textureCsv.Read())
        {
            var inputName = textureCsv.GetField<string>(0);
            var relevance = textureCsv.GetField<int>(1);
            var importance = textureCsv.GetField<int>(2);
            list.Add(new
            {
                Name = inputName,
                Relevance = relevance == 1 ? "True" : "False",
                Importance = importance
            });
        }

        foreach (var texture in material.Textures.Where(t => !t.Path.EndsWith(".sb")))
        {
            var match = list.FirstOrDefault(l => l.Name == texture.ShaderGenName);
            if (match == null)
            {
                System.Console.WriteLine($"No importance data found for {texture.ShaderGenName}");
                continue;
            }

            builder.Append(
                $"MaterialPropertyMetadata('{texture.ShaderGenName}', {match.Relevance}, {match.Importance}, 'TEXTURE', ''),\n");
        }

        File.WriteAllText(outputPath, builder.ToString());
    }
}