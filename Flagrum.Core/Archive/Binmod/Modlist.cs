using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace Flagrum.Core.Archive.Binmod;

public class ModlistEntry
{
    public string Path { get; set; }
    public bool IsEnabled { get; set; }
    public int Index { get; set; }
    public bool IsWorkshopMod { get; set; }

    public static IEnumerable<ModlistEntry> FromFile(string path)
    {
        var entries = new List<ModlistEntry>();

        using (var reader = new StreamReader(path))
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                while (csv.Read())
                {
                    entries.Add(new ModlistEntry
                    {
                        Path = csv.GetField<string>(0),
                        IsEnabled = csv.GetField<bool>(1),
                        Index = csv.GetField<int>(2),
                        IsWorkshopMod = csv.GetField<bool>(3)
                    });
                }
            }
        }

        return entries;
    }

    public static void ToFile(string path, IEnumerable<Binmod> entries)
    {
        var orderedEntries = entries
            .OrderBy(e => e.IsWorkshopMod)
            .ThenBy(e => e.Path);

        var builder = new StringBuilder();
        foreach (var entry in orderedEntries)
        {
            string modPath;
            if (entry.IsWorkshopMod)
            {
                modPath = "";
                var tokens = entry.Path.Split('\\');
                for (var i = 0; i < tokens.Length; i++)
                {
                    if (i > 0 && i < tokens.Length - 1)
                    {
                        modPath += '\\';
                    }
                    else if (i > 0)
                    {
                        modPath += '/';
                    }

                    modPath += tokens[i];
                }
            }
            else
            {
                modPath = entry.Path.Split('\\').Last();
            }

            builder.Append($"\"{modPath}\",");
            builder.Append($"\"{(entry.IsApplyToGame ? "True" : "False")}\",");
            builder.Append($"\"{entry.Index}\",");
            builder.Append($"\"{(entry.IsWorkshopMod ? "True" : "False")}\"");

            if (entry != orderedEntries.Last())
            {
                builder.Append("\r\n");
            }
        }

        File.WriteAllText(path, builder.ToString());
    }
}