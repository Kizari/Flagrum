using System;
using Flagrum.Core.Serialization.MessagePack;
using Flagrum.Application.Features.WorkshopMods.Data;
using Newtonsoft.Json;

namespace Flagrum.Core.Graphics.Materials;

public class GameMaterialSampler
{
    public ulong NameOffset { get; set; }
    public ulong ShaderGenNameOffset { get; set; }
    public ulong Unknown { get; set; }

    public byte StateMagFilter { get; set; }
    public byte StateMinFilter { get; set; }
    public byte StateMipFilter { get; set; }
    public byte StateWrapS { get; set; }
    public byte StateWrapT { get; set; }
    public byte StateWrapR { get; set; }

    public float MipmapLodBias { get; set; }
    public byte MaxAnisotropy { get; set; }

    public byte Unknown2 { get; set; }
    public byte Unknown3 { get; set; }
    public byte Unknown4 { get; set; }

    public GameMaterialColour BorderColour { get; set; }

    [JsonIgnore] public Half MinLod { get; set; }
    [JsonIgnore] public Half MaxLod { get; set; }

    public uint Flags { get; set; }

    public string Name { get; set; }
    public string ShaderGenName { get; set; }

    public void Read(MessagePackReader reader)
    {
        NameOffset = reader.Read<ulong>();
        ShaderGenNameOffset = reader.Read<ulong>();
        Unknown = reader.Read<ulong>();

        StateMagFilter = reader.Read<byte>();
        StateMinFilter = reader.Read<byte>();
        StateMipFilter = reader.Read<byte>();
        StateWrapS = reader.Read<byte>();
        StateWrapT = reader.Read<byte>();
        StateWrapR = reader.Read<byte>();

        MipmapLodBias = reader.Read<float>();
        MaxAnisotropy = reader.Read<byte>();

        Unknown2 = reader.Read<byte>();
        Unknown3 = reader.Read<byte>();
        Unknown4 = reader.Read<byte>();

        BorderColour = new GameMaterialColour();
        BorderColour.Read(reader);

        var minLod = reader.Read<ushort>();
        var maxLod = reader.Read<ushort>();
        MinLod = BitConverter.ToHalf(BitConverter.GetBytes(minLod));
        MaxLod = BitConverter.ToHalf(BitConverter.GetBytes(maxLod));

        Flags = reader.Read<uint>();
    }

    public void Write(MessagePackWriter writer)
    {
        writer.Write(NameOffset);
        writer.Write(ShaderGenNameOffset);
        writer.Write(Unknown);

        writer.Write(StateMagFilter);
        writer.Write(StateMinFilter);
        writer.Write(StateMipFilter);
        writer.Write(StateWrapS);
        writer.Write(StateWrapT);
        writer.Write(StateWrapR);

        writer.Write(MipmapLodBias);
        writer.Write(MaxAnisotropy);

        writer.Write(Unknown2);
        writer.Write(Unknown3);
        writer.Write(Unknown4);

        BorderColour.Write(writer);

        var minLod = BitConverter.ToUInt16(BitConverter.GetBytes(MinLod));
        var maxLod = BitConverter.ToUInt16(BitConverter.GetBytes(MaxLod));
        writer.Write(minLod);
        writer.Write(maxLod);

        writer.Write(Flags);
    }
}