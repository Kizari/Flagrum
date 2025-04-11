using System.Collections.Generic;
using System.IO;

namespace Flagrum.Core.Graphics.Terrain;

public enum TerrainPaletteDataVersion
{
    Version_Unknown = 0,
    Version_Safe = 1,
    Version_Lastest = 2,
    Version_Count = 3
}

public class TerrainPaletteDataItem
{
    public uint ArrayIndex { get; set; }
    public uint TextureIndex { get; set; }
    public uint R { get; set; }
    public uint G { get; set; }
    public uint B { get; set; }
    public float UVScale { get; set; }
    public float HeightThreshold { get; set; }
    public float SlopeThreshold { get; set; }
    public float HeightDampness { get; set; }
    public uint MaybeBitflags { get; set; }
    public float AlwaysOne { get; set; }
    public float AlwaysZero { get; set; }
    public float MaybeToleranceDivisor { get; set; }
    public float Value { get; set; }
    public float Epsilon { get; set; }
}

public class TerrainPaletteExtraDataItem
{
    public uint MaybeBitflags { get; set; }
    public uint AlwaysZero { get; set; }
    public byte AlwaysOne { get; set; }
    public byte[] Reserved { get; set; }
}

public class TerrainPaletteData
{
    public char[] Magic { get; set; }
    public TerrainPaletteDataVersion Version { get; set; }
    public uint Stride { get; set; }
    public uint Count { get; set; }
    public uint DataOffset { get; set; }
    public uint ExtraDataStride { get; set; }
    public uint ExtraDataCount { get; set; }
    public uint ExtraDataOffset { get; set; }

    public List<TerrainPaletteDataItem> Items { get; set; } = new();
    public List<TerrainPaletteExtraDataItem> ExtraItems { get; set; } = new();

    public static TerrainPaletteData Read(byte[] tpd)
    {
        using var stream = new MemoryStream(tpd);
        return Read(stream);
    }

    public static TerrainPaletteData Read(Stream stream)
    {
        using var reader = new BinaryReader(stream);
        var data = new TerrainPaletteData
        {
            Magic = reader.ReadChars(4),
            Version = (TerrainPaletteDataVersion)reader.ReadInt32(),
            Stride = reader.ReadUInt32(),
            Count = reader.ReadUInt32(),
            DataOffset = reader.ReadUInt32(),
            ExtraDataStride = reader.ReadUInt32(),
            ExtraDataCount = reader.ReadUInt32(),
            ExtraDataOffset = reader.ReadUInt32()
        };

        for (var i = 0; i < data.Count; i++)
        {
            var item = new TerrainPaletteDataItem
            {
                ArrayIndex = (uint)i,
                TextureIndex = reader.ReadUInt32(),
                R = reader.ReadUInt32(),
                G = reader.ReadUInt32(),
                B = reader.ReadUInt32(),
                UVScale = reader.ReadSingle(),
                HeightThreshold = reader.ReadSingle(),
                SlopeThreshold = reader.ReadSingle(),
                HeightDampness = reader.ReadSingle(),
                MaybeBitflags = reader.ReadUInt32(),
                AlwaysOne = reader.ReadSingle(),
                AlwaysZero = reader.ReadSingle(),
                MaybeToleranceDivisor = reader.ReadSingle(),
                Value = i / (float)data.Count
            };

            var toleranceRange = 1.0f / data.Count / 2.0f;
            item.Epsilon = toleranceRange * 2; // / (item.MaybeToleranceDivisor == 0 ? 2 : item.MaybeToleranceDivisor);
            data.Items.Add(item);
        }

        for (var i = 0; i < data.ExtraDataCount; i++)
        {
            data.ExtraItems.Add(new TerrainPaletteExtraDataItem
            {
                MaybeBitflags = reader.ReadUInt32(),
                AlwaysZero = reader.ReadUInt32(),
                AlwaysOne = reader.ReadByte(),
                Reserved = reader.ReadBytes(3)
            });
        }

        return data;
    }
}