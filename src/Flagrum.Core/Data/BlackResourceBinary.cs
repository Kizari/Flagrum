using System;
using System.IO;
using System.Text;
using Flagrum.Core.Serialization;

namespace Flagrum.Core.Data;

public class BlackResourceBinary : BinaryReaderWriterBase
{
    public char[] Magic { get; set; } = "BdevResource".ToCharArray();
    public int Size { get; set; }
    public ResourceId ResourceId { get; set; }
    
    public override void Read(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);

        Magic = reader.ReadChars(12);
        Size = reader.ReadInt32();
        ResourceId = new ResourceId();
        ResourceId.Read(reader);
    }

    public override void Write(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        
        writer.Write(Magic);
        writer.Write(Size);
        ResourceId.Write(writer);
    }

    public static BlackResourceBinary Create(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return Create(stream);
    }

    public static BlackResourceBinary Create(byte[] buffer)
    {
        using var stream = new MemoryStream(buffer);
        return Create(stream);
    }

    public static BlackResourceBinary Create(Stream stream)
    {
        using var reader = new BinaryReader(stream);
        
        stream.Seek(16, SeekOrigin.Begin);
        var typeId = reader.ReadUInt32();
        stream.Seek(0, SeekOrigin.Begin);

        if (ResourceType.Map.TryGetValue(typeId, out var type))
        {
            var binary = (BlackResourceBinary)Activator.CreateInstance(type);
            binary.Read(stream);
            return binary;
        }

        return null;
    }
}