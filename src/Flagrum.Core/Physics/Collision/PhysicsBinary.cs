using System.Collections.Generic;
using System.IO;
using System.Text;
using Flagrum.Core.Data.Binary;
using Flagrum.Core.Physics.Collision.PhysX;
using Flagrum.Core.Serialization;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision;

public class PhysicsBinary : SectionDataBinary
{
    public PhysicsBinary()
    {
        Subtype = ("PHB" + (char)0x00).ToCharArray();
    }

    public byte[] Reserved { get; set; } = new byte[12];
    public int BlockOffset { get; set; } = 12;
    public uint BlockCount { get; set; }
    public uint Unknown { get; set; }
    public uint Unknown2 { get; set; }
    public List<PhysicsBlock> Blocks { get; set; } = new();
    
    public uint StringBufferSize { get; set; }
    public byte[] MaybeReserved { get; set; } = new byte[12];
    public uint PhysicsCollectionBinarySize { get; set; }
    
    public string PhysxCollectionBinaryString { get; set; } = "PhysX Collection Binary";
    public byte[] Padding { get; set; } = new byte[131];

    public PhysicsCollectionBinary PhysicsCollectionBinary { get; set; } = new();
    
    public uint Unknown3 { get; set; }
    public uint Unknown4 { get; set; }
    public uint Unknown5 { get; set; }
    public ushort Unknown6 { get; set; }
    
    public StringBuffer StringBuffer { get; set; }

    public override void Read(Stream stream)
    {
        base.Read(stream);

        // This should only ever be read from DataIndexBinary, so leave the stream open
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        
        Reserved = reader.ReadBytes(12);
        BlockOffset = reader.ReadInt32();
        BlockCount = reader.ReadUInt32();
        Unknown = reader.ReadUInt32();
        Unknown2 = reader.ReadUInt32();

        for (var i = 0; i < BlockCount; i++)
        {
            var block = new PhysicsBlock();
            block.Read(reader);
            Blocks.Add(block);
        }

        StringBufferSize = reader.ReadUInt32();
        MaybeReserved = reader.ReadBytes(12);
        PhysicsCollectionBinarySize = reader.ReadUInt32();
        PhysxCollectionBinaryString = reader.ReadNullTerminatedString();
        Padding = reader.ReadBytes(131);

        var buffer = reader.ReadBytes((int)PhysicsCollectionBinarySize);
        PhysicsCollectionBinary.Read(buffer);

        Unknown3 = reader.ReadUInt32();
        Unknown4 = reader.ReadUInt32();
        Unknown5 = reader.ReadUInt32();
        Unknown6 = reader.ReadUInt16();

        buffer = reader.ReadBytes((int)StringBufferSize);
        StringBuffer = new StringBuffer(buffer);
    }

    public override void Write(Stream stream)
    {
        base.Write(stream);

        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        
        writer.Write(Reserved);
        writer.Write(BlockOffset);

        BlockCount = (uint)Blocks.Count;
        writer.Write(BlockCount);
        
        writer.Write(Unknown);
        writer.Write(Unknown2);

        foreach (var block in Blocks)
        {
            block.Write(writer);
        }

        StringBufferSize = (uint)StringBuffer.Length;
        writer.Write(StringBufferSize);
        writer.Write(MaybeReserved);

        var physxData = PhysicsCollectionBinary.Write();
        PhysicsCollectionBinarySize = (uint)physxData.Length;
        writer.Write(PhysicsCollectionBinarySize);
        writer.WriteNullTerminatedString(PhysxCollectionBinaryString);
        writer.Write(Padding);
        
        writer.Write(physxData);
        
        writer.Write(Unknown3);
        writer.Write(Unknown4);
        writer.Write(Unknown5);
        writer.Write(Unknown6);
        
        writer.Write(StringBuffer.ToArray());
    }
}