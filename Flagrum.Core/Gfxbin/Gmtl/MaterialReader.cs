using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Gfxbin.Gmtl.Data;
using Flagrum.Core.Utilities;
using BinaryReader = Flagrum.Core.Gfxbin.Serialization.BinaryReader;

namespace Flagrum.Core.Gfxbin.Gmtl;

public class MaterialReader
{
    private readonly Material _material;
    private readonly BinaryReader _reader;

    private byte[] _stringBuffer;

    public MaterialReader(string filePath) : this(File.ReadAllBytes(filePath)) { }

    public MaterialReader(byte[] materialData)
    {
        _reader = new BinaryReader(materialData);
        _material = new Material();
    }

    public Material Read()
    {
        _material.Header.Read(_reader);

        ReadMaterialData();

        _material.InterfaceInputs = ReadInterfaceInputs();
        _material.Interfaces = ReadInterfaces();
        _material.Textures = ReadTextures();
        _material.Samplers = ReadSamplers();
        _material.ShaderBinaries = ReadShaderBinaries();
        _material.ShaderPrograms = ReadShaderPrograms();

        ReadInterfaceInputParameters();
        ReadStrings();

        return _material;
    }

    private void ReadMaterialData()
    {
        _reader.UnpackUInt64(out var nameOffset);

        _reader.UnpackUInt16(out var matUniformCount);
        _reader.UnpackUInt16(out var matBufferCount);
        _reader.UnpackUInt16(out var matTextureCount);
        _reader.UnpackUInt16(out var matSamplerCount);
        _reader.UnpackUInt16(out var totalUniformCount);
        _reader.UnpackUInt16(out var totalBufferCount);
        _reader.UnpackUInt16(out var totalTextureCount);

        _reader.UnpackUInt16(out var totalSamplerCount);
        _reader.UnpackUInt16(out var shaderBinaryCount);
        _reader.UnpackUInt16(out var shaderProgramCount);

        _reader.UnpackUInt32(out var gpuDataSize);
        _reader.UnpackUInt32(out _); // String buffer size
        _reader.UnpackUInt32(out var nameHash);
        _reader.UnpackUInt32(out var blendType);

        _reader.UnpackFloat32(out var blendFactor);

        _reader.UnpackUInt32(out var renderStateBits);
        _reader.UnpackUInt16(out var skinVsMaxBoneCount);
        _reader.UnpackUInt16(out var brdfType);

        _material.NameOffset = nameOffset;
        _material.NameHash = nameHash;
        _material.BlendFactor = blendFactor;
        _material.BlendType = blendType;
        _material.RenderStateBits = renderStateBits;
        _material.SkinVsMaxBoneCount = skinVsMaxBoneCount;
        _material.BrdfType = brdfType;
        _material.InterfaceInputCount = matUniformCount;
        _material.InterfaceCount = matBufferCount;
        _material.TextureCount = matTextureCount;
        _material.SamplerCount = matSamplerCount;
        _material.TotalInterfaceCount = totalBufferCount;
        _material.TotalInterfaceInputCount = totalUniformCount;
        _material.TotalSamplerCount = totalSamplerCount;
        _material.TotalTextureCount = totalTextureCount;
        _material.ShaderBinaryCount = shaderBinaryCount;
        _material.ShaderProgramCount = shaderProgramCount;
        _material.InputsBufferSize = gpuDataSize;

        if (_material.Header.Version >= 20150403)
        {
            _reader.UnpackUInt16(out var highTexturePackAssetOffset);
            _material.HighTexturePackAssetOffset = highTexturePackAssetOffset;
        }
    }

    private List<MaterialInterfaceInput> ReadInterfaceInputs()
    {
        var inputs = new List<MaterialInterfaceInput>();

        for (var _ = 0; _ < _material.TotalInterfaceInputCount; _++)
        {
            _reader.UnpackUInt64(out var nameOffset);
            _reader.UnpackUInt64(out var shaderGenNameOffset);
            _reader.UnpackUInt32(out var hashName);
            _reader.UnpackUInt32(out var hashShaderGenName);
            _reader.UnpackUInt16(out var offset);
            _reader.UnpackUInt16(out var size);
            _reader.UnpackUInt16(out var bufferIndex);
            _reader.UnpackUInt16(out var type);
            _reader.UnpackUInt32(out var flags);

            inputs.Add(new MaterialInterfaceInput
            {
                GpuOffset = offset,
                InterfaceIndex = bufferIndex,
                NameHash = hashName,
                NameOffset = nameOffset,
                ShaderGenNameHash = hashShaderGenName,
                ShaderGenNameOffset = shaderGenNameOffset,
                Size = size,
                Type = type,
                Flags = flags
            });
        }

        return inputs;
    }

    private List<MaterialInterface> ReadInterfaces()
    {
        var interfaces = new List<MaterialInterface>();

        for (var _ = 0; _ < _material.TotalInterfaceCount; _++)
        {
            _reader.UnpackUInt64(out var name);
            _reader.UnpackUInt64(out var shaderGenName);
            _reader.UnpackUInt64(out var u1);
            _reader.UnpackUInt32(out var hashName);
            _reader.UnpackUInt32(out var hashShaderGenName);
            _reader.UnpackUInt32(out var u2);
            _reader.UnpackUInt32(out var gpuOffset);
            _reader.UnpackUInt16(out var size);
            _reader.UnpackUInt16(out var uniformCount);
            _reader.UnpackUInt32(out var flags);

            interfaces.Add(new MaterialInterface
            {
                Flags = flags,
                GpuOffset = gpuOffset,
                InputCount = uniformCount,
                NameHash = hashName,
                NameOffset = name,
                ShaderGenNameHash = hashShaderGenName,
                ShaderGenNameOffset = shaderGenName,
                Size = size,
                Unknown1 = u1,
                Unknown2 = u2
            });
        }

        return interfaces;
    }

    private List<MaterialTexture> ReadTextures()
    {
        var textures = new List<MaterialTexture>();

        for (var _ = 0; _ < _material.TotalTextureCount; _++)
        {
            sbyte highTextureStreamingLevels = -1;
            _reader.UnpackUInt64(out var resourceFileHash);
            _reader.UnpackUInt64(out var name);
            _reader.UnpackUInt64(out var shaderGenName);
            _reader.UnpackUInt64(out var u1);
            _reader.UnpackUInt64(out var path);
            _reader.UnpackUInt32(out var hashName);
            _reader.UnpackUInt32(out var hashShaderGenName);
            _reader.UnpackUInt32(out var u2);
            _reader.UnpackUInt32(out var hashPath);
            _reader.UnpackUInt32(out var flags);

            if (_material.Header.Version > 20150508)
            {
                _reader.UnpackInt8(out highTextureStreamingLevels);
            }

            textures.Add(new MaterialTexture
            {
                Flags = flags,
                HighTextureStreamingLevels = highTextureStreamingLevels,
                NameHash = hashName,
                NameOffset = name,
                PathHash = hashPath,
                PathOffset = path,
                ResourceFileHash = resourceFileHash,
                ShaderGenNameHash = hashShaderGenName,
                ShaderGenNameOffset = shaderGenName,
                Unknown1 = u1,
                Unknown2 = u2
            });
        }

        return textures;
    }

    private List<MaterialSampler> ReadSamplers()
    {
        var samplers = new List<MaterialSampler>();

        for (var _ = 0; _ < _material.TotalSamplerCount; _++)
        {
            _reader.UnpackUInt64(out var reflectionSamplerName);
            _reader.UnpackUInt64(out var reflectionSamplerShaderGenName);
            _reader.UnpackUInt64(out var u1); // reflectionSamplerHashName?
            _reader.UnpackInt8(out var samplerStateMagFilter);

            _reader.UnpackInt8(out var samplerStateMinFilter);
            _reader.UnpackInt8(out var samplerStateMipFilter);
            _reader.UnpackInt8(out var samplerStateWrapS);
            _reader.UnpackInt8(out var samplerStateWrapT);
            _reader.UnpackInt8(out var samplerStateWrapR);

            _reader.UnpackFloat32(out var mipmapLodBias);
            _reader.UnpackInt8(out var maxAniso);

            _reader.UnpackInt8(out var u2);
            _reader.UnpackInt8(out var u3);
            _reader.UnpackInt8(out var u4);

            // BorderColor shenanigans
            _reader.UnpackFloat32(out var ur);
            _reader.UnpackFloat32(out var ug);
            _reader.UnpackFloat32(out var ub);
            _reader.UnpackFloat32(out var ua);

            //var minLod = _reader.UnpackHalf();
            //var maxLod = _reader.UnpackHalf();
            _reader.UnpackUInt16(out var minLodUshort);
            _reader.UnpackUInt16(out var maxLodUshort);
            var minLodBytes = BitConverter.GetBytes(minLodUshort);
            var maxLodBytes = BitConverter.GetBytes(maxLodUshort);
            var minLod = BitConverter.ToHalf(minLodBytes);
            var maxLod = BitConverter.ToHalf(maxLodBytes);

            Console.WriteLine(minLod);
            Console.WriteLine(maxLod);

            _reader.UnpackUInt32(out var reflectionSamplerFlags);

            samplers.Add(new MaterialSampler
            {
                MinLod = minLod,
                MaxLod = maxLod,
                MaxAniso = maxAniso,
                BorderColor = 0xFFFFFFFF,
                Flags = reflectionSamplerFlags,
                MagFilter = samplerStateMagFilter,
                MinFilter = samplerStateMinFilter,
                MipFilter = samplerStateMipFilter,
                MipmapLodBias = mipmapLodBias,
                NameOffset = reflectionSamplerName,
                ShaderGenNameOffset = reflectionSamplerShaderGenName,
                WrapS = samplerStateWrapS,
                WrapT = samplerStateWrapT,
                WrapR = samplerStateWrapR,
                UnknownR = ur,
                UnknownG = ug,
                UnknownB = ub,
                UnknownA = ua,
                Unknown = u1,
                Unknown1 = u2,
                Unknown2 = u3,
                Unknown3 = u4
            });
        }

        return samplers;
    }

    private List<MaterialShaderBinary> ReadShaderBinaries()
    {
        var shaderBinaries = new List<MaterialShaderBinary>();

        for (var _ = 0; _ < _material.ShaderBinaryCount; _++)
        {
            _reader.UnpackUInt64(out var resourceFileHash);
            _reader.UnpackUInt64(out var path);

            shaderBinaries.Add(new MaterialShaderBinary
            {
                ResourceFileHash = resourceFileHash,
                PathOffset = path
            });
        }

        return shaderBinaries;
    }

    private List<MaterialShaderProgram> ReadShaderPrograms()
    {
        var shaderPrograms = new List<MaterialShaderProgram>();

        for (var _ = 0; _ < _material.ShaderProgramCount; _++)
        {
            _reader.UnpackUInt16(out var lowKey);
            _reader.UnpackUInt16(out var highKey);
            _reader.UnpackUInt16(out var csBinaryIndex);
            _reader.UnpackUInt16(out var vsBinaryIndex);
            _reader.UnpackUInt16(out var hsBinaryIndex);
            _reader.UnpackUInt16(out var dsBinaryIndex);
            _reader.UnpackUInt16(out var gsBinaryIndex);
            _reader.UnpackUInt16(out var psBinaryIndex);

            shaderPrograms.Add(new MaterialShaderProgram
            {
                LowKey = lowKey,
                HighKey = highKey,
                CsBinaryIndex = csBinaryIndex,
                VsBinaryIndex = vsBinaryIndex,
                HsBinaryIndex = hsBinaryIndex,
                DsBinaryIndex = dsBinaryIndex,
                GsBinaryIndex = gsBinaryIndex,
                PsBinaryIndex = psBinaryIndex
            });
        }

        return shaderPrograms;
    }

    private void ReadInterfaceInputParameters()
    {
        _reader.UnpackBlob(out var buffer, out _);

        foreach (var input in _material.InterfaceInputs)
        {
            if (input.InterfaceIndex < _material.InterfaceCount)
            {
                var numberOfFloats = input.Size / 4;
                if (numberOfFloats > 0)
                {
                    input.Values = new float[numberOfFloats];
                    for (var i = 0; i < numberOfFloats; i++)
                    {
                        input.Values[i] = BitConverter.ToSingle(buffer, input.GpuOffset + i * 4);
                    }
                }
            }
        }
    }

    private void ReadStrings()
    {
        var propertyPaths = new List<(ulong offset, string path, string offsetPath)>();
        
        _reader.UnpackBlob(out var buffer, out _);
        _stringBuffer = buffer;

        _material.Name = GetStringFromBuffer(_material.NameOffset);
        propertyPaths.Add((_material.NameOffset, nameof(Material.Name), nameof(Material.NameOffset)));

        for (var i = 0; i < _material.Interfaces.Count; i++)
        {
            var materialInterface = _material.Interfaces[i];
            materialInterface.Name = GetStringFromBuffer(materialInterface.NameOffset);
            materialInterface.ShaderGenName = GetStringFromBuffer(materialInterface.ShaderGenNameOffset);
            var i1 = i;
            propertyPaths.Add((materialInterface.NameOffset, $"{nameof(Material.Interfaces)}[{i1}].{nameof(MaterialInterface.Name)}", $"{nameof(Material.Interfaces)}[{i1}].{nameof(MaterialInterface.NameOffset)}"));
            propertyPaths.Add((materialInterface.ShaderGenNameOffset, $"{nameof(Material.Interfaces)}[{i1}].{nameof(MaterialInterface.ShaderGenName)}", $"{nameof(Material.Interfaces)}[{i1}].{nameof(MaterialInterface.ShaderGenNameOffset)}"));
        }

        for (var i = 0; i < _material.InterfaceInputs.Count; i++)
        {
            var input = _material.InterfaceInputs[i];
            input.Name = GetStringFromBuffer(input.NameOffset);
            input.ShaderGenName = GetStringFromBuffer(input.ShaderGenNameOffset);
            var i1 = i;
            propertyPaths.Add((input.NameOffset, $"{nameof(Material.InterfaceInputs)}[{i1}].{nameof(MaterialInterfaceInput.Name)}", $"{nameof(Material.InterfaceInputs)}[{i1}].{nameof(MaterialInterfaceInput.NameOffset)}"));
            propertyPaths.Add((input.ShaderGenNameOffset, $"{nameof(Material.InterfaceInputs)}[{i1}].{nameof(MaterialInterfaceInput.ShaderGenName)}", $"{nameof(Material.InterfaceInputs)}[{i1}].{nameof(MaterialInterfaceInput.ShaderGenNameOffset)}"));
        }

        for (var i = 0; i < _material.Textures.Count; i++)
        {
            var texture = _material.Textures[i];
            texture.Name = GetStringFromBuffer(texture.NameOffset);
            texture.ShaderGenName = GetStringFromBuffer(texture.ShaderGenNameOffset);
            texture.Path = GetStringFromBuffer(texture.PathOffset);
            var i1 = i;
            propertyPaths.Add((texture.NameOffset, $"{nameof(Material.Textures)}[{i1}].{nameof(MaterialTexture.Name)}", $"{nameof(Material.Textures)}[{i1}].{nameof(MaterialTexture.NameOffset)}"));
            propertyPaths.Add((texture.ShaderGenNameOffset, $"{nameof(Material.Textures)}[{i1}].{nameof(MaterialTexture.ShaderGenName)}", $"{nameof(Material.Textures)}[{i1}].{nameof(MaterialTexture.ShaderGenNameOffset)}"));
            propertyPaths.Add((texture.PathOffset, $"{nameof(Material.Textures)}[{i1}].{nameof(MaterialTexture.Path)}", $"{nameof(Material.Textures)}[{i1}].{nameof(MaterialTexture.PathOffset)}"));
        }

        for (var i = 0; i < _material.Samplers.Count; i++)
        {
            var sampler = _material.Samplers[i];
            sampler.Name = GetStringFromBuffer(sampler.NameOffset);
            sampler.ShaderGenName = GetStringFromBuffer(sampler.ShaderGenNameOffset);
            var i1 = i;
            propertyPaths.Add((sampler.NameOffset, $"{nameof(Material.Samplers)}[{i1}].{nameof(MaterialSampler.Name)}", $"{nameof(Material.Samplers)}[{i1}].{nameof(MaterialSampler.NameOffset)}"));
            propertyPaths.Add((sampler.ShaderGenNameOffset, $"{nameof(Material.Samplers)}[{i1}].{nameof(MaterialSampler.ShaderGenName)}", $"{nameof(Material.Samplers)}[{i1}].{nameof(MaterialSampler.ShaderGenNameOffset)}"));
        }

        for (var i = 0; i < _material.ShaderBinaries.Count; i++)
        {
            var shader = _material.ShaderBinaries[i];
            shader.Path = GetStringFromBuffer(shader.PathOffset);
            var i1 = i;
            propertyPaths.Add((shader.PathOffset, $"{nameof(Material.ShaderBinaries)}[{i1}].{nameof(MaterialShaderBinary.Path)}", $"{nameof(Material.ShaderBinaries)}[{i1}].{nameof(MaterialShaderBinary.PathOffset)}"));
        }

        if (_material.HighTexturePackAssetOffset != 0)
        {
            _material.HighTexturePackAsset = GetStringFromBuffer(_material.HighTexturePackAssetOffset);
            propertyPaths.Add((_material.HighTexturePackAssetOffset, nameof(Material.HighTexturePackAsset), nameof(Material.HighTexturePackAssetOffset)));
        }

        _material.StringPropertyPaths = propertyPaths.OrderBy(p => p.offset).Select(p => (p.path, p.offsetPath)).ToList();
    }
    
    private string GetStringFromBuffer(ulong offset)
    {
        var sb = new StringBuilder();

        for (var c = (char)_stringBuffer[offset]; c != 0;)
        {
            sb.Append(c);
            offset++;
            c = (char)_stringBuffer[offset];
        }

        return sb.ToString();
    }
}