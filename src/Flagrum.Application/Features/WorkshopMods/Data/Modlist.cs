using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace Flagrum.Application.Services;

public class ModlistEntry
{
    public string Path { get; set; }
    public bool IsEnabled { get; set; }
    public int Index { get; set; }
    public bool IsWorkshopMod { get; set; }

    public static IEnumerable<ModlistEntry> FromFile(string path)
    {
        if (!File.Exists(path))
        {
            return new List<ModlistEntry>();
        }
        
        // Insert header row into the CSV in memory
        var lines = File.ReadAllLines(path).ToList();
        lines.Insert(0, "Path,IsEnabled,Index,IsWorkshopMod");

        // Convert the strings back into bytes so they can be used in a memory stream
        var final = string.Join('\n', lines);
        var bytes = Encoding.UTF8.GetBytes(final);

        // Parse the CSV
        using var stream = new MemoryStream(bytes);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<ModlistEntry>().ToList();
    }

    public static void ToFile(string path, IEnumerable<Binmod> entries, Dictionary<string, int> fixIdMap)
    {
        var orderedEntries = entries
            .OrderBy(e => e.IsWorkshopMod)
            .ThenBy(e => e.Path)
            .ToList();

        var builder = new StringBuilder();
        foreach (var entry in orderedEntries)
        {
            string modPath;
            if (entry.IsWorkshopMod)
            {
                modPath = "";
                var tokens = entry.Path.Split('\\', '/');
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

            fixIdMap.TryGetValue(modPath, out var fixId);

            builder.Append($"\"{modPath}\",");
            builder.Append($"\"{(entry.IsApplyToGame ? "True" : "False")}\",");
            builder.Append($"\"{fixId}\",");
            builder.Append($"\"{(entry.IsWorkshopMod ? "True" : "False")}\"");

            if (entry != orderedEntries.Last())
            {
                builder.Append("\r\n");
            }
        }

        File.WriteAllText(path, builder.ToString());
    }
}