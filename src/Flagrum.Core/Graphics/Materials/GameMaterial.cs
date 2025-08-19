using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Core.Graphics.Containers;
using Flagrum.Core.Serialization;
using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Materials;

public class GameMaterial : GraphicsBinary
{
    public ulong NameOffset { get; set; }
    public ushort Unknown { get; set; }

    // These are the counts of the objects in the lists that map to this material
    public ushort UniformCount { get; set; }
    public ushort BufferCount { get; set; }
    public ushort TextureCount { get; set; }
    public ushort SamplerCount { get; set; }

    // These are the counts of all objects in the lists
    public ushort TotalUniformCount { get; set; }
    public ushort TotalBufferCount { get; set; }
    public ushort TotalTextureCount { get; set; }
    public ushort TotalSamplerCount { get; set; }

    public ushort ShaderBinaryCount { get; set; }
    public ushort ShaderProgramCount { get; set; }

    public uint ValueBufferSize { get; set; }
    public uint StringBufferSize { get; set; }

    public uint NameHash { get; set; }
    public uint Unknown2 { get; set; }
    public uint BlendType { get; set; }
    public float BlendFactor { get; set; }
    public uint RenderStateBits { get; set; }
    public ushort SkinVsMaxBoneCount { get; set; }
    public ushort BrdfType { get; set; }
    public ulong HighTexturePackUriOffset { get; set; }

    public ValueBuffer ValueBuffer { get; set; }
    public StringBuffer StringBuffer { get; set; }

    public List<GameMaterialUniform> Uniforms { get; set; } = new();
    public List<GameMaterialBuffer> Buffers { get; set; } = new();
    public List<GameMaterialTexture> Textures { get; set; } = new();
    public List<GameMaterialSampler> Samplers { get; set; } = new();
    public List<GameMaterialShaderBinary> ShaderBinaries { get; set; } = new();
    public List<GameMaterialShaderProgram> ShaderPrograms { get; set; } = new();

    public string Name { get; set; }
    public string HighTexturePackUri { get; set; }

    public static GameMaterial Deserialize(byte[] buffer)
    {
        var material = new GameMaterial();
        material.Read(buffer);
        return material;
    }

    public override void Read(Stream stream)
    {
        base.Read(stream);

        using var reader = new MessagePackReader(stream);
        reader.DataVersion = Version;

        NameOffset = reader.Read<ulong>();

        if (reader.DataVersion >= 20220707)
        {
            Unknown = reader.Read<ushort>();
        }

        BufferCount = reader.Read<ushort>();
        UniformCount = reader.Read<ushort>();
        TextureCount = reader.Read<ushort>();
        SamplerCount = reader.Read<ushort>();
        TotalBufferCount = reader.Read<ushort>();
        TotalUniformCount = reader.Read<ushort>();
        TotalTextureCount = reader.Read<ushort>();
        TotalSamplerCount = reader.Read<ushort>();
        ShaderBinaryCount = reader.Read<ushort>();
        ShaderProgramCount = reader.Read<ushort>();
        ValueBufferSize = reader.Read<uint>();
        StringBufferSize = reader.Read<uint>();
        NameHash = reader.Read<uint>();

        if (reader.DataVersion >= 20220707)
        {
            Unknown2 = reader.Read<uint>();
        }

        BlendType = reader.Read<uint>();
        BlendFactor = reader.Read<float>();
        RenderStateBits = reader.Read<uint>();
        SkinVsMaxBoneCount = reader.Read<ushort>();
        BrdfType = reader.Read<ushort>();

        if (reader.DataVersion >= 20150403)
        {
            HighTexturePackUriOffset = reader.Read<ushort>();
        }

        for (var i = 0; i < TotalBufferCount; i++)
        {
            var buffer = new GameMaterialBuffer();
            buffer.Read(reader);
            Buffers.Add(buffer);
        }

        for (var i = 0; i < TotalUniformCount; i++)
        {
            var uniform = new GameMaterialUniform();
            uniform.Read(reader);
            Uniforms.Add(uniform);
        }

        for (var i = 0; i < TotalTextureCount; i++)
        {
            var texture = new GameMaterialTexture();
            texture.Read(reader);
            Textures.Add(texture);
        }

        for (var i = 0; i < TotalSamplerCount; i++)
        {
            var sampler = new GameMaterialSampler();
            sampler.Read(reader);
            Samplers.Add(sampler);
        }

        for (var i = 0; i < ShaderBinaryCount; i++)
        {
            var shaderBinary = new GameMaterialShaderBinary();
            shaderBinary.Read(reader);
            ShaderBinaries.Add(shaderBinary);
        }

        for (var i = 0; i < ShaderProgramCount; i++)
        {
            var shaderProgram = new GameMaterialShaderProgram();
            shaderProgram.Read(reader);
            ShaderPrograms.Add(shaderProgram);
        }

        if (ValueBufferSize > 0)
        {
            ValueBuffer = new ValueBuffer(reader.Read<byte[]>());
        }

        StringBuffer = new StringBuffer(reader.Read<byte[]>());

        // Read values from the value buffer
        foreach (var buffer in Buffers)
        {
            if (buffer.UniformIndex < UniformCount)
            {
                var floatCount = buffer.Size / 4;
                if (floatCount > 0)
                {
                    buffer.Values = new float[floatCount];
                    for (var i = 0u; i < floatCount; i++)
                    {
                        buffer.Values[i] = ValueBuffer.Get(buffer.Offset + i * 4);
                    }
                }
            }
        }

        // Read strings from the string buffer
        Name = StringBuffer.Get(NameOffset);
        HighTexturePackUri = StringBuffer.Get(HighTexturePackUriOffset);

        for (var i = 0; i < TotalBufferCount; i++)
        {
            Buffers[i].Name = StringBuffer.Get(Buffers[i].NameOffset);
            Buffers[i].ShaderGenName = StringBuffer.Get(Buffers[i].ShaderGenNameOffset);
        }

        for (var i = 0; i < TotalUniformCount; i++)
        {
            Uniforms[i].Name = StringBuffer.Get(Uniforms[i].NameOffset);
            Uniforms[i].ShaderGenName = StringBuffer.Get(Uniforms[i].ShaderGenNameOffset);
        }

        for (var i = 0; i < TotalTextureCount; i++)
        {
            Textures[i].Name = StringBuffer.Get(Textures[i].NameOffset);
            Textures[i].ShaderGenName = StringBuffer.Get(Textures[i].ShaderGenNameOffset);
            Textures[i].Uri = StringBuffer.Get(Textures[i].UriOffset);
        }

        for (var i = 0; i < TotalSamplerCount; i++)
        {
            Samplers[i].Name = StringBuffer.Get(Samplers[i].NameOffset);
            Samplers[i].ShaderGenName = StringBuffer.Get(Samplers[i].ShaderGenNameOffset);
        }

        for (var i = 0; i < ShaderBinaryCount; i++)
        {
            ShaderBinaries[i].Uri = StringBuffer.Get(ShaderBinaries[i].UriOffset);
        }
    }

    public override void Write(Stream stream)
    {
        UpdateStringBuffer();
        RegenerateDependencyTable();

        base.Write(stream);

        using var writer = new MessagePackWriter(stream);
        writer.DataVersion = Version;

        writer.Write(NameOffset);

        if (writer.DataVersion >= 20220707)
        {
            writer.Write(Unknown);
        }

        writer.Write(BufferCount);
        writer.Write(UniformCount);
        writer.Write(TextureCount);
        writer.Write(SamplerCount);
        writer.Write(TotalBufferCount);
        writer.Write(TotalUniformCount);
        writer.Write(TotalTextureCount);
        writer.Write(TotalSamplerCount);
        writer.Write(ShaderBinaryCount);
        writer.Write(ShaderProgramCount);
        writer.Write(ValueBufferSize);
        writer.Write(StringBufferSize);
        writer.Write(NameHash);

        if (writer.DataVersion >= 20220707)
        {
            writer.Write(Unknown2);
        }

        writer.Write(BlendType);
        writer.Write(BlendFactor);
        writer.Write(RenderStateBits);
        writer.Write(SkinVsMaxBoneCount);
        writer.Write(BrdfType);

        if (writer.DataVersion >= 20150403)
        {
            writer.Write(HighTexturePackUriOffset);
        }

        foreach (var buffer in Buffers)
        {
            buffer.Write(writer);
        }

        foreach (var uniform in Uniforms)
        {
            uniform.Write(writer);
        }

        foreach (var texture in Textures)
        {
            texture.Write(writer);
        }

        foreach (var sampler in Samplers)
        {
            sampler.Write(writer);
        }

        foreach (var shaderBinary in ShaderBinaries)
        {
            shaderBinary.Write(writer);
        }

        foreach (var shaderProgram in ShaderPrograms)
        {
            shaderProgram.Write(writer);
        }

        writer.Write(ValueBuffer.ToArray());
        writer.Write(StringBuffer.ToArray());
    }

    public void RegenerateDependencyTable()
    {
        var dependencies = new Dictionary<string, string>();
        var uri = Dependencies["ref"];
        var assetUri = uri.Remove(uri.LastIndexOf('/') + 1);

        foreach (var shaderBinary in ShaderBinaries.Where(s => s.UriHash > 0))
        {
            dependencies[shaderBinary.UriHash.ToString()] = shaderBinary.Uri;
        }

        foreach (var texture in Textures.Where(t => t.UriHash > 0))
        {
            dependencies[texture.UriHash.ToString()] = texture.Uri;
        }

        dependencies["asset_uri"] = assetUri;
        dependencies["ref"] = uri;

        Dependencies = dependencies;
        Hashes = dependencies
            .Where(d => ulong.TryParse(d.Key, out _))
            .Select(d => ulong.Parse(d.Key))
            .OrderBy(h => h)
            .ToList();
    }

    private void UpdateStringBuffer()
    {
        StringBuffer = new StringBuffer();
        NameOffset = StringBuffer.Put(Name);

        foreach (var buffer in Buffers)
        {
            buffer.NameOffset = StringBuffer.Put(buffer.Name);
            buffer.ShaderGenNameOffset = StringBuffer.Put(buffer.ShaderGenName);
        }

        foreach (var uniform in Uniforms)
        {
            uniform.NameOffset = StringBuffer.Put(uniform.Name);
            uniform.ShaderGenNameOffset = StringBuffer.Put(uniform.ShaderGenName);
        }

        foreach (var texture in Textures)
        {
            texture.NameOffset = StringBuffer.Put(texture.Name);
            texture.ShaderGenNameOffset = StringBuffer.Put(texture.ShaderGenName);
            texture.UriOffset = StringBuffer.Put(texture.Uri);
        }

        foreach (var sampler in Samplers)
        {
            sampler.NameOffset = StringBuffer.Put(sampler.Name);
            sampler.ShaderGenNameOffset = StringBuffer.Put(sampler.ShaderGenName);
        }

        foreach (var binary in ShaderBinaries)
        {
            binary.UriOffset = StringBuffer.Put(binary.Uri);
        }

        HighTexturePackUriOffset = StringBuffer.Put(HighTexturePackUri);
        StringBufferSize = (uint)StringBuffer.Length;
    }

    public void UpdateValueBuffer()
    {
        ValueBuffer = new ValueBuffer(ValueBufferSize);

        for (var i = 0; i < TotalUniformCount; i++)
        {
            var uniform = Uniforms[i];

            if ((uniform.Flags & 1) == 0)
            {
                continue;
            }

            foreach (var buffer in Buffers.Where(b => b.UniformIndex == i))
            {
                ValueBuffer.Put(Uniforms[buffer.UniformIndex].Offset + buffer.Offset, buffer.Values);
            }
        }
    }
}