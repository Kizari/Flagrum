using System.Collections.Generic;
using System.IO;

namespace Flagrum.Core.Physics.Collision;

public class PhysicsBlock
{
    public PhysicsBlockType Type { get; set; } = PhysicsBlockType.BLOCK_TYPE_STATIC_BODY;
    public uint NameOffset { get; set; }
    public uint UserStringOffset { get; set; }
    public byte IsEnabled { get; set; } = 1;
    public byte BinaryVersion { get; set; }
    public ushort Reserved { get; set; }
    public uint UserId { get; set; }
    public uint Reserved2 { get; set; }
    public int BodiesOffset { get; set; } = 28;
    public int BodiesCount { get; set; }
    public int UserVariablesOffset { get; set; }
    public int UserVariablesCount { get; set; }
    public int ExtraDataOffset { get; set; }
    public int ExtraDataCount { get; set; }
    
    public uint PartsId { get; set; }
    public List<PhysicsStaticBody> Bodies { get; set; } = new();

    public void Read(BinaryReader reader)
    {
        Type = (PhysicsBlockType)reader.ReadUInt32();
        NameOffset = reader.ReadUInt32();
        UserStringOffset = reader.ReadUInt32();
        IsEnabled = reader.ReadByte();
        BinaryVersion = reader.ReadByte();
        Reserved = reader.ReadUInt16();
        UserId = reader.ReadUInt32();
        Reserved2 = reader.ReadUInt32();
        BodiesOffset = reader.ReadInt32();
        BodiesCount = reader.ReadInt32();
        UserVariablesOffset = reader.ReadInt32();
        UserVariablesCount = reader.ReadInt32();
        ExtraDataOffset = reader.ReadInt32();
        ExtraDataCount = reader.ReadInt32();
        PartsId = reader.ReadUInt32();

        for (var i = 0; i < BodiesCount; i++)
        {
            var body = new PhysicsStaticBody();
            body.Read(reader);
            Bodies.Add(body);
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((uint)Type);
        writer.Write(NameOffset);
        writer.Write(UserStringOffset);
        writer.Write(IsEnabled);
        writer.Write(BinaryVersion);
        writer.Write(Reserved);
        writer.Write(UserId);
        writer.Write(Reserved2);
        writer.Write(BodiesOffset);

        BodiesCount = Bodies.Count;
        writer.Write(BodiesCount);
        
        writer.Write(UserVariablesOffset); // Points to before string buffer
        writer.Write(UserVariablesCount);
        writer.Write(ExtraDataOffset); // Points to before string buffer
        writer.Write(ExtraDataCount);
        writer.Write(PartsId);

        foreach (var body in Bodies)
        {
            body.Write(writer);
        }
    }
}