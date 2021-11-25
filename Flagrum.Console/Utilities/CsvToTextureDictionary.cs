using System.Globalization;
using System.IO;
using CsvHelper;

namespace Flagrum.Console.Utilities;

public static class CsvToTextureDictionary
{
    public static void Run()
    {
        using var reader = new StreamReader("C:\\Testing\\Gfxbin\\Gmtl\\material_data_input_importance2.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var dictionary = "";

        while (csv.Read())
        {
            var name = csv.GetField<string>(0);
            var texture = csv.GetField<string>(4);

            if (!string.IsNullOrEmpty(texture) && texture != "material")
            {
                dictionary += "{\"" + name + "\", \"" + texture + ".btex\"},\n";
            }
        }

        File.WriteAllText("C:\\Testing\\Gfxbin\\Gmtl\\material_data_input_importance2.cs", dictionary);
    }
}