using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flagrum.Gfxbin.Gmtl.Data;
using Flagrum.Gfxbin.Serialization;

namespace Flagrum.Gfxbin.Gmtl;

public class MaterialWriter
{
    private readonly Material _material;
    private readonly BinaryWriter _writer;

    public MaterialWriter(Material material)
    {
        _writer = new BinaryWriter();
        _material = material;
    }

    public byte[] Write()
    {
        _material.Header.Write(_writer);

        var stringBuffer = CreateStringBuffer();
        var inputBuffer = CreateInputBuffer();

        WriteMaterialData(inputBuffer.Length, stringBuffer.Length);
        WriteInterfaceInputs();
        WriteInterfaces();
        WriteTextures();
        WriteSamplers();
        WriteShaderBinaries();
        WriteShaderPrograms();

        _writer.WriteBin(inputBuffer.ToArray());
        _writer.WriteBin(stringBuffer.ToArray());

        return _writer.ToArray();
    }

    private byte[] CreateStringBuffer()
    {
        var stringBuffer = new StringBuffer();
        var tuples = new List<(object, PropertyInfo, string)>
            {(_material, typeof(Material).GetProperty(nameof(Material.NameOffset)), _material.Name)};

        foreach (var input in _material.InterfaceInputs)
        {
            tuples.Add((input,
                typeof(MaterialInterfaceInput).GetProperty(nameof(MaterialInterfaceInput.NameOffset)), input.Name));
            tuples.Add((input,
                typeof(MaterialInterfaceInput).GetProperty(nameof(MaterialInterfaceInput.ShaderGenNameOffset)),
                input.ShaderGenName));
        }

        foreach (var @interface in _material.Interfaces)
        {
            tuples.Add((@interface, typeof(MaterialInterface).GetProperty(nameof(MaterialInterface.NameOffset)),
                @interface.Name));
            tuples.Add((@interface,
                typeof(MaterialInterface).GetProperty(nameof(MaterialInterface.ShaderGenNameOffset)),
                @interface.ShaderGenName));
        }

        foreach (var texture in _material.Textures)
        {
            tuples.Add((texture, typeof(MaterialTexture).GetProperty(nameof(MaterialTexture.NameOffset)),
                texture.Name));
            tuples.Add((texture, typeof(MaterialTexture).GetProperty(nameof(MaterialTexture.ShaderGenNameOffset)),
                texture.ShaderGenName));
            tuples.Add((texture, typeof(MaterialTexture).GetProperty(nameof(MaterialTexture.PathOffset)),
                texture.Path));
        }

        foreach (var sampler in _material.Samplers)
        {
            tuples.Add((sampler, typeof(MaterialSampler).GetProperty(nameof(MaterialSampler.NameOffset)),
                sampler.Name));
            tuples.Add((sampler, typeof(MaterialSampler).GetProperty(nameof(MaterialSampler.ShaderGenNameOffset)),
                sampler.ShaderGenName));
        }

        foreach (var shader in _material.ShaderBinaries)
        {
            tuples.Add((shader, typeof(MaterialShaderBinary).GetProperty(nameof(MaterialShaderBinary.PathOffset)),
                shader.Path));
        }

        tuples.Add((_material, typeof(Material).GetProperty(nameof(Material.HighTexturePackAssetOffset)),
            _material.HighTexturePackAsset));

        foreach (var (instance, propertyInfo, stringValue) in tuples.OrderBy(d => d.Item3.Length))
        {
            propertyInfo.SetValue(instance, stringBuffer.Put(stringValue));
        }

        return stringBuffer.ToArray();
    }

    private byte[] CreateInputBuffer()
    {
        var floatBuffer = new FloatBuffer(_material.InputsBufferSize);
        foreach (var input in _material.InterfaceInputs.Where(u => u.InterfaceIndex == 0))
        {
            floatBuffer.Put(input.GpuOffset, input.Values);
        }

        return floatBuffer.ToArray();
    }

    private void WriteMaterialData(int inputBufferSize, int stringBufferSize)
    {
        _writer.WriteUInt(_material.NameOffset);

        // TODO: Make these dynamic
        _writer.WriteUInt(_material.InterfaceInputCount);
        _writer.WriteUInt(_material.InterfaceCount);
        _writer.WriteUInt(_material.TextureCount);
        _writer.WriteUInt(_material.SamplerCount);

        _writer.WriteUInt((ushort)_material.InterfaceInputs.Count);
        _writer.WriteUInt((ushort)_material.Interfaces.Count);
        _writer.WriteUInt((ushort)_material.Textures.Count);
        _writer.WriteUInt((ushort)_material.Samplers.Count);

        _writer.WriteUInt((ushort)_material.ShaderBinaries.Count);
        _writer.WriteUInt((ushort)_material.ShaderPrograms.Count);

        _writer.WriteUInt((ulong)inputBufferSize);
        _writer.WriteUInt((ulong)stringBufferSize);
        _writer.WriteUInt(_material.NameHash);

        _writer.WriteUInt(_material.BlendType);
        _writer.WriteFloat(_material.BlendFactor);

        _writer.WriteUInt(_material.RenderStateBits);
        _writer.WriteUInt(_material.SkinVsMaxBoneCount);
        _writer.WriteUInt(_material.BrdfType);

        // TODO: Make this optional for packs without HD
        _writer.WriteUInt(_material.HighTexturePackAssetOffset);
    }

    private void WriteInterfaceInputs()
    {
        foreach (var input in _material.InterfaceInputs)
        {
            _writer.WriteUInt(input.NameOffset);
            _writer.WriteUInt(input.ShaderGenNameOffset);
            _writer.WriteUInt(input.NameHash);
            _writer.WriteUInt(input.ShaderGenNameHash);
            _writer.WriteUInt(input.GpuOffset);
            _writer.WriteUInt(input.Size);
            _writer.WriteUInt(input.InterfaceIndex);
            _writer.WriteUInt(input.Type);
            _writer.WriteUInt(input.Flags);
        }
    }

    private void WriteInterfaces()
    {
        foreach (var @interface in _material.Interfaces)
        {
            _writer.WriteUInt(@interface.NameOffset);
            _writer.WriteUInt(@interface.ShaderGenNameOffset);
            _writer.WriteUInt(@interface.Unknown1);
            _writer.WriteUInt(@interface.NameHash);
            _writer.WriteUInt(@interface.ShaderGenNameHash);
            _writer.WriteUInt(@interface.Unknown2);
            _writer.WriteUInt(@interface.GpuOffset);
            _writer.WriteUInt(@interface.Size);
            _writer.WriteUInt(@interface.InputCount);
            _writer.WriteUInt(@interface.Flags);
        }
    }

    private void WriteTextures()
    {
        foreach (var texture in _material.Textures)
        {
            _writer.WriteUInt(texture.ResourceFileHash);
            _writer.WriteUInt(texture.NameOffset);
            _writer.WriteUInt(texture.ShaderGenNameOffset);
            _writer.WriteUInt(texture.Unknown1);
            _writer.WriteUInt(texture.PathOffset);
            _writer.WriteUInt(texture.NameHash);
            _writer.WriteUInt(texture.ShaderGenNameHash);
            _writer.WriteUInt(texture.Unknown2);
            _writer.WriteUInt(texture.PathHash);
            _writer.WriteUInt(texture.Flags);

            // TODO: Make this optional for packs without HD
            _writer.WriteSByte(texture.HighTextureStreamingLevels);
        }
    }

    private void WriteSamplers()
    {
        foreach (var sampler in _material.Samplers)
        {
            _writer.WriteUInt(sampler.NameOffset);
            _writer.WriteUInt(sampler.ShaderGenNameOffset);
            _writer.WriteUInt(sampler.Unknown); // Might be Reflection Sampler Hash Name?

            _writer.WriteSByte(sampler.MagFilter);
            _writer.WriteSByte(sampler.MinFilter);
            _writer.WriteSByte(sampler.MipFilter);
            _writer.WriteSByte(sampler.WrapS);
            _writer.WriteSByte(sampler.WrapT);
            _writer.WriteSByte(sampler.WrapR);
            _writer.WriteFloat(sampler.MipmapLodBias);
            _writer.WriteSByte(sampler.MaxAniso);

            _writer.WriteSByte(sampler.Unknown1);
            _writer.WriteSByte(sampler.Unknown2);
            _writer.WriteSByte(sampler.Unknown3);

            // Sai thinks this is BorderColor stuff
            _writer.WriteFloat(sampler.UnknownR);
            _writer.WriteFloat(sampler.UnknownG);
            _writer.WriteFloat(sampler.UnknownB);
            _writer.WriteFloat(sampler.UnknownA);

            _writer.WriteHalf(sampler.MinLod);
            _writer.WriteHalf(sampler.MaxLod);

            _writer.WriteUInt(sampler.Flags);
        }
    }

    private void WriteShaderBinaries()
    {
        foreach (var shader in _material.ShaderBinaries)
        {
            _writer.WriteUInt(shader.ResourceFileHash);
            _writer.WriteUInt(shader.PathOffset);
        }
    }

    private void WriteShaderPrograms()
    {
        foreach (var shader in _material.ShaderPrograms)
        {
            _writer.WriteUInt(shader.LowKey);
            _writer.WriteUInt(shader.HighKey);
            _writer.WriteUInt(shader.CsBinaryIndex);
            _writer.WriteUInt(shader.VsBinaryIndex);
            _writer.WriteUInt(shader.HsBinaryIndex);
            _writer.WriteUInt(shader.DsBinaryIndex);
            _writer.WriteUInt(shader.GsBinaryIndex);
            _writer.WriteUInt(shader.PsBinaryIndex);
        }
    }
}