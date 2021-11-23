using System.Collections.Generic;
using Flagrum.Core.Services.Logging;
using Flagrum.Gfxbin.Serialization;

namespace Flagrum.Gfxbin.Data;

public class GfxbinHeader
{
    private readonly Logger _logger;

    public GfxbinHeader()
    {
        _logger = new ConsoleLogger();
    }

    public uint Version { get; set; }
    public List<DependencyPath> Dependencies { get; set; } = new();
    public List<ulong> Hashes { get; set; } = new();

    public void Read(BinaryReader reader)
    {
        Version = (uint)reader.Read();

        if (Version is < 20150713 or > 20160705)
        {
            _logger.LogWarning($"Gfxbin Version {Version}");
        }
        else
        {
            _logger.LogInformation($"Gfxbin Version {Version}");
        }

        var dependencyCount = reader.ReadMapCount();

        for (var _ = 0; _ < dependencyCount; _++)
        {
            Dependencies.Add(new DependencyPath
            {
                PathHash = reader.ReadString(),
                Path = reader.ReadString()
            });
        }

        reader.UnpackArraySize(out var hashesCount);

        for (var _ = 0; _ < hashesCount; _++)
        {
            Hashes.Add(reader.ReadUint64());
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.WriteMapCount((uint)Dependencies.Count);

        foreach (var dependency in Dependencies)
        {
            writer.WriteStringX(dependency.PathHash);
            writer.WriteString8(dependency.Path);
        }

        writer.WriteArraySize((uint)Hashes.Count);

        foreach (var hash in Hashes)
        {
            writer.WriteUInt(hash);
        }
    }
}