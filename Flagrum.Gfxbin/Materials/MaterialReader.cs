using Flagrum.Core.Services.Logging;
using Flagrum.Gfxbin.Data;
using Flagrum.Gfxbin.Materials.Data;
using Flagrum.Gfxbin.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flagrum.Gfxbin.Materials
{
    public class MaterialReader
    {
        private readonly Logger _logger;
        private readonly Material _material;
        private readonly BinaryReader _reader;

        private byte[] _stringBuffer;

        public MaterialReader(string path)
        {
            _logger = new ConsoleLogger();
            _reader = new BinaryReader(path, out uint version);

            _material = new Material();
            _material.Header.Version = version;

            if (version < 20150713 || version > 20160705)
            {
                _logger.LogWarning($"Gfxbin Version {version}");
            }
            else
            {
                _logger.LogInformation($"Gfxbin Version {version}");
            }
        }

        public Material Read()
        {
            ReadHeader();
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

        private void ReadHeader()
        {
            var dependencyCount = _reader.ReadMapCount();

            for (var _ = 0; _ < dependencyCount; _++)
            {
                _material.Header.Dependencies.Add(new DependencyPath
                {
                    PathHash = _reader.ReadString(),
                    Path = _reader.ReadStr8()
                });
            }

            _reader.UnpackArraySize(out int hashesCount);

            for (var _ = 0; _ < hashesCount; _++)
            {
                _material.Header.Hashes.Add(_reader.ReadUint64());
            }
        }

        private void ReadMaterialData()
        {
            _reader.UnpackUInt64(out ulong nameOffset);

            _reader.UnpackUInt16(out ushort matUniformCount);
            _reader.UnpackUInt16(out ushort matBufferCount);
            _reader.UnpackUInt16(out ushort matTextureCount);
            _reader.UnpackUInt16(out ushort matSamplerCount);
            _reader.UnpackUInt16(out ushort totalUniformCount);
            _reader.UnpackUInt16(out ushort totalBufferCount);
            _reader.UnpackUInt16(out ushort totalTextureCount);

            _reader.UnpackUInt16(out ushort totalSamplerCount);
            _reader.UnpackUInt16(out ushort shaderBinaryCount);
            _reader.UnpackUInt16(out ushort shaderProgramCount);

            _reader.UnpackUInt32(out uint gpuDataSize);
            _reader.UnpackUInt32(out uint stringBufferSize);
            _reader.UnpackUInt32(out uint nameHash);
            _reader.UnpackUInt32(out uint blendType);

            _reader.UnpackFloat32(out float blendFactor);

            _reader.UnpackUInt32(out uint renderStateBits);
            _reader.UnpackUInt16(out ushort skinVsMaxBoneCount);
            _reader.UnpackUInt16(out ushort brdfType);

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
                _reader.UnpackUInt16(out ushort highTexturePackAssetOffset);
                _material.HighTexturePackAssetOffset = highTexturePackAssetOffset;
            }
        }

        private List<MaterialInterfaceInput> ReadInterfaceInputs()
        {
            var inputs = new List<MaterialInterfaceInput>();

            for (var _ = 0; _ < _material.TotalInterfaceInputCount; _++)
            {
                _reader.UnpackUInt64(out ulong nameOffset);
                _reader.UnpackUInt64(out ulong shaderGenNameOffset);
                _reader.UnpackUInt32(out uint hashName);
                _reader.UnpackUInt32(out uint hashShaderGenName);
                _reader.UnpackUInt16(out ushort offset);
                _reader.UnpackUInt16(out ushort size);
                _reader.UnpackUInt16(out ushort bufferIndex);
                _reader.UnpackUInt16(out ushort type);
                _reader.UnpackUInt32(out uint flags);

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
                _reader.UnpackUInt64(out ulong name);
                _reader.UnpackUInt64(out ulong shaderGenName);
                _reader.UnpackUInt64(out ulong u1);
                _reader.UnpackUInt32(out uint hashName);
                _reader.UnpackUInt32(out uint hashShaderGenName);
                _reader.UnpackUInt32(out uint u2);
                _reader.UnpackUInt32(out uint gpuOffset);
                _reader.UnpackUInt16(out ushort size);
                _reader.UnpackUInt16(out ushort uniformCount);
                _reader.UnpackUInt32(out uint flags);

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
                _reader.UnpackUInt64(out ulong resourceFileHash);
                _reader.UnpackUInt64(out ulong name);
                _reader.UnpackUInt64(out ulong shaderGenName);
                _reader.UnpackUInt64(out ulong u1);
                _reader.UnpackUInt64(out ulong path);
                _reader.UnpackUInt32(out uint hashName);
                _reader.UnpackUInt32(out uint hashShaderGenName);
                _reader.UnpackUInt32(out uint u2);
                _reader.UnpackUInt32(out uint hashPath);
                _reader.UnpackUInt32(out uint flags);

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
                _reader.UnpackUInt64(out ulong reflectionSamplerName);
                _reader.UnpackUInt64(out ulong reflectionSamplerShaderGenName);
                _reader.UnpackUInt64(out ulong u1); // reflectionSamplerHashName?
                _reader.UnpackInt8(out sbyte samplerStateMagFilter);

                _reader.UnpackInt8(out sbyte samplerStateMinFilter);
                _reader.UnpackInt8(out sbyte samplerStateMipFilter);
                _reader.UnpackInt8(out sbyte samplerStateWrapS);
                _reader.UnpackInt8(out sbyte samplerStateWrapT);
                _reader.UnpackInt8(out sbyte samplerStateWrapR);

                _reader.UnpackFloat32(out float mipmapLodBias);
                _reader.UnpackInt8(out sbyte maxAniso);

                _reader.UnpackInt8(out sbyte u2);
                _reader.UnpackInt8(out sbyte u3);
                _reader.UnpackInt8(out sbyte u4);

                // BorderColor shenanigans
                _reader.UnpackFloat32(out float ur);
                _reader.UnpackFloat32(out float ug);
                _reader.UnpackFloat32(out float ub);
                _reader.UnpackFloat32(out float ua);

                _reader.UnpackUInt16(out ushort minLod);
                _reader.UnpackUInt16(out ushort maxLod);

                _reader.UnpackUInt32(out uint reflectionSamplerFlags);

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
                    NameHash = 0,
                    ShaderGenNameOffset = reflectionSamplerShaderGenName,
                    ShaderGenNameHash = 0,
                    TextureFlag = 0,
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
                _reader.UnpackUInt64(out ulong resourceFileHash);
                _reader.UnpackUInt64(out ulong path);

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
                _reader.UnpackUInt16(out ushort lowKey);
                _reader.UnpackUInt16(out ushort highKey);
                _reader.UnpackUInt16(out ushort csBinaryIndex);
                _reader.UnpackUInt16(out ushort vsBinaryIndex);
                _reader.UnpackUInt16(out ushort hsBinaryIndex);
                _reader.UnpackUInt16(out ushort dsBinaryIndex);
                _reader.UnpackUInt16(out ushort gsBinaryIndex);
                _reader.UnpackUInt16(out ushort psBinaryIndex);

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
            _reader.UnpackBlob(out byte[] buffer, out uint size);

            foreach (var input in _material.InterfaceInputs)
            {
                if (input.InterfaceIndex == 0)
                {
                    var numberOfFloats = input.Size / 4;
                    if (numberOfFloats > 0)
                    {
                        input.Values = new float[numberOfFloats];
                        for (var i = 0; i < numberOfFloats; i++)
                        {
                            input.Values[i] = BitConverter.ToSingle(buffer, input.GpuOffset + (i * 4));
                        }
                    }
                }
            }
        }

        private void ReadStrings()
        {
            _reader.UnpackBlob(out byte[] buffer, out uint size);
            _stringBuffer = buffer;

            _material.Name = GetStringFromBuffer(_material.NameOffset);

            foreach (var materialInterface in _material.Interfaces)
            {
                materialInterface.Name = GetStringFromBuffer(materialInterface.NameOffset);
                materialInterface.ShaderGenName = GetStringFromBuffer(materialInterface.ShaderGenNameOffset);
            }

            foreach (var input in _material.InterfaceInputs)
            {
                input.Name = GetStringFromBuffer(input.NameOffset);
                input.ShaderGenName = GetStringFromBuffer(input.ShaderGenNameOffset);
            }

            foreach (var texture in _material.Textures)
            {
                texture.Name = GetStringFromBuffer(texture.NameOffset);
                texture.ShaderGenName = GetStringFromBuffer(texture.ShaderGenNameOffset);
                texture.Path = GetStringFromBuffer(texture.PathOffset);
            }

            foreach (var sampler in _material.Samplers)
            {
                sampler.Name = GetStringFromBuffer(sampler.NameOffset);
                sampler.ShaderGenName = GetStringFromBuffer(sampler.ShaderGenNameOffset);
            }

            foreach (var shader in _material.ShaderBinaries)
            {
                shader.Path = GetStringFromBuffer(shader.PathOffset);
            }

            if (_material.HighTexturePackAssetOffset != 0)
            {
                _material.HighTexturePackAsset = GetStringFromBuffer(_material.HighTexturePackAssetOffset);
            }
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
}
