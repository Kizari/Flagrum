using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoryPack;

namespace Flagrum.Core.Persistence;

[MemoryPackable]
public partial class FixidRepository
{
    [MemoryPackIgnore] public static FixidRepository Instance { get; set; }

    public List<FixidRecord> AnimId { get; set; } = new();
    public List<FixidRecord> LinkAnimRole { get; set; } = new();
    public List<FixidRecord> Message { get; set; } = new();
    public List<FixidRecord> ModelJoint { get; set; } = new();
    public List<FixidRecord> Param { get; set; } = new();
    public List<FixidRecord> Parts { get; set; } = new();
    public List<FixidRecord> Resource { get; set; } = new();
    public List<FixidRecord> Swf { get; set; } = new();
    public List<FixidRecord> Vfx { get; set; } = new();

    public List<FixidRecord> this[string tableName] => tableName switch
    {
        "animid" => AnimId,
        "link_anim_role" => LinkAnimRole,
        "message" => Message,
        "model_joint" => ModelJoint,
        "param" => Param,
        "parts" => Parts,
        "resource" => Resource,
        "swf" => Swf,
        "vfx" => Vfx,
        _ => throw new ArgumentOutOfRangeException(nameof(tableName), tableName, "Table doesn't exist")
    };

    public List<FixidRecord> QueryTable(string tableName, string prefix, string query)
    {
        return this[tableName]
            .Where(record => ((string.IsNullOrEmpty(record.Prefix) && string.IsNullOrEmpty(prefix))
                              || record.Prefix?.Equals(prefix, StringComparison.OrdinalIgnoreCase) == true)
                             && record.Label.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public FixidRecord Get(string tableName, string prefix, string label)
    {
        return this[tableName].FirstOrDefault(record =>
            ((string.IsNullOrEmpty(record.Prefix) && string.IsNullOrEmpty(prefix))
             || record.Prefix?.Equals(prefix, StringComparison.OrdinalIgnoreCase) == true)
            && record.Label.Equals(label, StringComparison.OrdinalIgnoreCase));
    }

    public static async Task<FixidRepository> FromCsvAsync(string directory)
    {
        var repository = new FixidRepository();
        var tables = repository.GetTables();

        foreach (var (name, table) in tables)
        {
            var lines = (await File.ReadAllLinesAsync(Path.Combine(directory, $"{name}.csv")))[1..];
            foreach (var line in lines)
            {
                var record = line.Split(',');
                table.Add(new FixidRecord
                {
                    Fixid = uint.Parse(record[0]),
                    Prefix = string.IsNullOrEmpty(record[1]) ? null : record[1],
                    Label = string.IsNullOrEmpty(record[2]) ? null : record[2]
                });
            }
        }

        return repository;
    }

    public void DumpToCsvFiles(string outputDirectory)
    {
        var tables = GetTables();
        foreach (var (name, table) in tables)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Fixid,Prefix,Label");
            foreach (var record in table.OrderBy(r => r.Fixid).ThenBy(r => r.Prefix).ThenBy(r => r.Label))
            {
                builder.AppendLine($"{record.Fixid},{record.Prefix},{record.Label}");
            }

            File.WriteAllText($"{outputDirectory}/{name}.csv", builder.ToString());
        }
    }

    private Dictionary<string, List<FixidRecord>> GetTables()
    {
        return new Dictionary<string, List<FixidRecord>>
        {
            {"animid", AnimId},
            {"link_anim_role", LinkAnimRole},
            {"message", Message},
            {"model_joint", ModelJoint},
            {"param", Param},
            {"parts", Parts},
            {"resource", Resource},
            {"swf", Swf},
            {"vfx", Vfx}
        };
    }
}

[MemoryPackable]
public partial class FixidRecord
{
    public uint Fixid { get; set; }
    public string Prefix { get; set; } = null!;
    public string Label { get; set; } = null!;
}