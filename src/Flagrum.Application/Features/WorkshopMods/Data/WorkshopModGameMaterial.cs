using System;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Graphics.Materials;
using Newtonsoft.Json;

namespace Flagrum.Application.Features.WorkshopMods.Data;

public class WorkshopModGameMaterial
{
    public WorkshopModGfxbinHeader Header { get; set; }
    public string Name { get; set; }
    public uint NameHash { get; set; }
    public ulong NameOffset { get; set; }
    public ushort BrdfType { get; set; }
    public uint BlendType { get; set; }
    public float BlendFactor { get; set; }
    public bool HairNoDither { get; set; }
    public uint RenderStateBits { get; set; }
    public ushort SkinVsMaxBoneCount { get; set; }
    public string HighTexturePackAsset { get; set; }
    public ulong HighTexturePackAssetOffset { get; set; }
    public WorkshopModGmtlInterface[] Interfaces { get; set; }
    public WorkshopModGmtlInterfaceInput[] InterfaceInputs { get; set; }
    public WorkshopModGmtlTexture[] Textures { get; set; }
    public WorkshopModGmtlSampler[] Samplers { get; set; }
    public WorkshopModGmtlShaderBinary[] ShaderBinaries { get; set; }
    public WorkshopModGmtlShaderProgram[] ShaderPrograms { get; set; }
    public ushort ShaderBinaryCount { get; set; }
    public ushort ShaderProgramCount { get; set; }
    public ushort InterfaceCount { get; set; }
    public ushort InterfaceInputCount { get; set; }
    public ushort TextureCount { get; set; }
    public ushort SamplerCount { get; set; }
    public ushort TotalInterfaceCount { get; set; }
    public ushort TotalInterfaceInputCount { get; set; }
    public ushort TotalTextureCount { get; set; }
    public ushort TotalSamplerCount { get; set; }
    public uint InputsBufferSize { get; set; }
    public WorkshopModStringPropertyPath[] StringPropertyPaths { get; set; }

    public GameMaterial ToGameMaterial()
    {
        return new GameMaterial
        {
            Version = Header.Version,
            Dependencies = Header.Dependencies.ToDictionary(d => d.PathHash, d => d.Path),
            Hashes = Header.Hashes,
            NameOffset = NameOffset,
            UniformCount = InterfaceCount,
            BufferCount = InterfaceInputCount,
            TextureCount = TextureCount,
            SamplerCount = SamplerCount,
            TotalUniformCount = TotalInterfaceCount,
            TotalBufferCount = TotalInterfaceInputCount,
            TotalTextureCount = TotalTextureCount,
            TotalSamplerCount = TotalSamplerCount,
            ShaderBinaryCount = ShaderBinaryCount,
            ShaderProgramCount = ShaderProgramCount,
            ValueBufferSize = InputsBufferSize,
            NameHash = NameHash,
            BlendType = BlendType,
            BlendFactor = BlendFactor,
            RenderStateBits = RenderStateBits,
            SkinVsMaxBoneCount = SkinVsMaxBoneCount,
            BrdfType = BrdfType,
            HighTexturePackUriOffset = HighTexturePackAssetOffset,
            Uniforms = Interfaces.Select(i => new GameMaterialUniform
            {
                NameOffset = i.NameOffset,
                ShaderGenNameOffset = i.ShaderGenNameOffset,
                Unknown = i.Unknown1,
                NameHash = i.NameHash,
                ShaderGenNameHash = i.ShaderGenNameHash,
                Unknown2 = i.Unknown2,
                Offset = i.GpuOffset,
                Size = i.Size,
                BufferCount = i.InputCount,
                Flags = i.Flags,
                Name = i.Name,
                ShaderGenName = i.ShaderGenName
            }).ToList(),
            Buffers = InterfaceInputs.Select(i => new GameMaterialBuffer
            {
                NameOffset = i.NameOffset,
                ShaderGenNameOffset = i.ShaderGenNameOffset,
                NameHash = i.NameHash,
                ShaderGenNameHash = i.ShaderGenNameHash,
                Offset = i.GpuOffset,
                Size = i.Size,
                UniformIndex = i.InterfaceIndex,
                Type = i.Type,
                Flags = i.Flags,
                Name = i.Name,
                ShaderGenName = i.ShaderGenName,
                Values = i.Values
            }).ToList(),
            Textures = Textures.Select(t => new GameMaterialTexture
            {
                UriHash = t.ResourceFileHash,
                NameOffset = t.NameOffset,
                ShaderGenNameOffset = t.ShaderGenNameOffset,
                Unknown = t.Unknown1,
                UriOffset = t.PathOffset,
                NameHash = t.NameHash,
                ShaderGenNameHash = t.ShaderGenNameHash,
                Unknown2 = t.Unknown2,
                UriHash32 = t.PathHash,
                Flags = t.Flags,
                HighTextureStreamingLevels = t.HighTextureStreamingLevels,
                Name = t.Name,
                ShaderGenName = t.ShaderGenName,
                Uri = t.Path
            }).ToList(),
            Samplers = Samplers.Select(s => new GameMaterialSampler
            {
                NameOffset = s.NameOffset,
                ShaderGenNameOffset = s.ShaderGenNameOffset,
                Unknown = s.Unknown,
                StateMagFilter = s.MagFilter,
                StateMinFilter = s.MinFilter,
                StateMipFilter = s.MipFilter,
                StateWrapS = s.WrapS,
                StateWrapT = s.WrapT,
                StateWrapR = s.WrapR,
                MipmapLodBias = s.MipmapLodBias,
                MaxAnisotropy = s.MaxAniso,
                Unknown2 = s.Unknown1,
                Unknown3 = s.Unknown2,
                Unknown4 = s.Unknown3,
                BorderColour = new GameMaterialColour
                {
                    R = s.UnknownR,
                    G = s.UnknownG,
                    B = s.UnknownB,
                    A = s.UnknownA
                },
                MinLod = s.MinLod,
                MaxLod = s.MaxLod,
                Flags = s.Flags,
                Name = s.Name,
                ShaderGenName = s.ShaderGenName
            }).ToList(),
            ShaderBinaries = ShaderBinaries.Select(s => new GameMaterialShaderBinary
            {
                UriHash = s.ResourceFileHash,
                UriOffset = s.PathOffset,
                Uri = s.Path
            }).ToList(),
            ShaderPrograms = ShaderPrograms.Select(s => new GameMaterialShaderProgram
            {
                LowKey = s.LowKey,
                HighKey = s.HighKey,
                CsBinaryIndex = s.CsBinaryIndex,
                VsBinaryIndex = s.VsBinaryIndex,
                HsBinaryIndex = s.HsBinaryIndex,
                DsBinaryIndex = s.DsBinaryIndex,
                GsBinaryIndex = s.GsBinaryIndex,
                PsBinaryIndex = s.PsBinaryIndex
            }).ToList(),
            Name = Name,
            HighTexturePackUri = HighTexturePackAsset
        };
    }
}

public class WorkshopModGfxbinHeader
{
    public uint Version { get; set; }
    public WorkshopModGfxbinDependency[] Dependencies { get; set; }
    public List<ulong> Hashes { get; set; }
}

public class WorkshopModGfxbinDependency
{
    public string Path { get; set; }
    public string PathHash { get; set; }
}

public class WorkshopModGmtlInterface
{
    public string Name { get; set; }
    public uint NameHash { get; set; }
    public ulong NameOffset { get; set; }
    public string ShaderGenName { get; set; }
    public uint ShaderGenNameHash { get; set; }
    public ulong ShaderGenNameOffset { get; set; }
    public uint GpuOffset { get; set; }
    public ushort Size { get; set; }
    public ushort InputCount { get; set; }
    public uint Flags { get; set; }
    public ulong Unknown1 { get; set; }
    public uint Unknown2 { get; set; }
}

public class WorkshopModGmtlInterfaceInput
{
    public string Name { get; set; }
    public uint NameHash { get; set; }
    public ulong NameOffset { get; set; }
    public string ShaderGenName { get; set; }
    public uint ShaderGenNameHash { get; set; }
    public ulong ShaderGenNameOffset { get; set; }
    public ushort Type { get; set; }
    public ushort InterfaceIndex { get; set; }
    public ushort GpuOffset { get; set; }
    public ushort Size { get; set; }
    public uint Flags { get; set; }
    public float[] Values { get; set; }
}

public class WorkshopModGmtlTexture
{
    public ulong ResourceFileHash { get; set; }
    public string Name { get; set; }
    public uint NameHash { get; set; }
    public ulong NameOffset { get; set; }
    public string ShaderGenName { get; set; }
    public uint ShaderGenNameHash { get; set; }
    public ulong ShaderGenNameOffset { get; set; }
    public string Path { get; set; }
    public uint PathHash { get; set; }
    public ulong PathOffset { get; set; }
    public uint Flags { get; set; }
    public int HighTextureStreamingLevels { get; set; }
    public ulong Unknown1 { get; set; }
    public uint Unknown2 { get; set; }
}

public class WorkshopModGmtlSampler
{
    public string Name { get; set; }
    public ulong NameOffset { get; set; }
    public string ShaderGenName { get; set; }
    public ulong ShaderGenNameOffset { get; set; }
    public uint Flags { get; set; }
    public ulong Unknown { get; set; }
    public float MipmapLodBias { get; set; }
    public long BorderColor { get; set; }
    public byte WrapS { get; set; }
    public byte WrapT { get; set; }
    public byte WrapR { get; set; }
    [JsonIgnore] public Half MinLod { get; set; } // These are {} in the JSON, so nothing useful to read anyway
    [JsonIgnore] public Half MaxLod { get; set; } // These are {} in the JSON, so nothing useful to read anyway
    public byte MaxAniso { get; set; }
    public byte MinFilter { get; set; }
    public byte MagFilter { get; set; }
    public byte MipFilter { get; set; }
    public byte Unknown1 { get; set; }
    public byte Unknown2 { get; set; }
    public byte Unknown3 { get; set; }
    public float UnknownR { get; set; }
    public float UnknownG { get; set; }
    public float UnknownB { get; set; }
    public float UnknownA { get; set; }
}

public class WorkshopModGmtlShaderBinary
{
    public ulong ResourceFileHash { get; set; }
    public string Path { get; set; }
    public ulong PathOffset { get; set; }
}

public class WorkshopModGmtlShaderProgram
{
    public ushort LowKey { get; set; }
    public ushort HighKey { get; set; }
    public ushort CsBinaryIndex { get; set; }
    public ushort VsBinaryIndex { get; set; }
    public ushort HsBinaryIndex { get; set; }
    public ushort DsBinaryIndex { get; set; }
    public ushort GsBinaryIndex { get; set; }
    public ushort PsBinaryIndex { get; set; }
}

public class WorkshopModStringPropertyPath
{
    public string Item1 { get; set; }
    public string Item2 { get; set; }
}