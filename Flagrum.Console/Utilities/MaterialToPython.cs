using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Flagrum.Gfxbin.Gmtl;

namespace Flagrum.Console.Utilities;

public static class MaterialToPython
{
    public static void Convert(string materialPath, string outputPath)
    {
        var reader = new MaterialReader(materialPath);
        var material = reader.Read();

        using var reader2 = new StreamReader("C:\\Testing\\Gfxbin\\Gmtl\\material_data_input_importance.csv");
        using var inputCsv = new CsvReader(reader2, CultureInfo.InvariantCulture);

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

        File.WriteAllText(outputPath, builder.ToString());
    }
}