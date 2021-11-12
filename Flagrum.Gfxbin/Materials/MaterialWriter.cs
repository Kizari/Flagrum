using Flagrum.Gfxbin.Materials.Data;
using Flagrum.Gfxbin.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flagrum.Gfxbin.Materials
{
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
            WriteHeader();

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

        private void WriteHeader()
        {
            _writer.Write(_material.Header.Version);
            _writer.WriteMapCount((uint)_material.Header.Dependencies.Count);

            foreach (var dependency in _material.Header.Dependencies)
            {
                _writer.WriteStringX(dependency.PathHash);
                _writer.WriteString8(dependency.Path);
            }

            _writer.WriteArraySize((uint)_material.Header.Hashes.Count);

            foreach (var hash in _material.Header.Hashes)
            {
                _writer.WriteInt(hash);
            }
        }

        private byte[] CreateStringBuffer()
        {
            var stringBuffer = new StringBuffer();
            var tuples = new List<(object, PropertyInfo, string)>();

            tuples.Add((_material, typeof(Material).GetProperty(nameof(Material.NameOffset)), _material.Name));

            foreach (var input in _material.InterfaceInputs)
            {
                tuples.Add((input, typeof(MaterialInterfaceInput).GetProperty(nameof(MaterialInterfaceInput.NameOffset)), input.Name));
                tuples.Add((input, typeof(MaterialInterfaceInput).GetProperty(nameof(MaterialInterfaceInput.ShaderGenNameOffset)), input.ShaderGenName));
            }

            foreach (var @interface in _material.Interfaces)
            {
                tuples.Add((@interface, typeof(MaterialInterface).GetProperty(nameof(MaterialInterface.NameOffset)), @interface.Name));
                tuples.Add((@interface, typeof(MaterialInterface).GetProperty(nameof(MaterialInterface.ShaderGenNameOffset)), @interface.ShaderGenName));
            }

            foreach (var texture in _material.Textures)
            {
                tuples.Add((texture, typeof(MaterialTexture).GetProperty(nameof(MaterialTexture.NameOffset)), texture.Name));
                tuples.Add((texture, typeof(MaterialTexture).GetProperty(nameof(MaterialTexture.ShaderGenNameOffset)), texture.ShaderGenName));
                tuples.Add((texture, typeof(MaterialTexture).GetProperty(nameof(MaterialTexture.PathOffset)), texture.Path));
            }

            foreach (var sampler in _material.Samplers)
            {
                tuples.Add((sampler, typeof(MaterialSampler).GetProperty(nameof(MaterialSampler.NameOffset)), sampler.Name));
                tuples.Add((sampler, typeof(MaterialSampler).GetProperty(nameof(MaterialSampler.ShaderGenNameOffset)), sampler.ShaderGenName));
            }

            foreach (var shader in _material.ShaderBinaries)
            {
                tuples.Add((shader, typeof(MaterialShaderBinary).GetProperty(nameof(MaterialShaderBinary.PathOffset)), shader.Path));
            }

            tuples.Add((_material, typeof(Material).GetProperty(nameof(Material.HighTexturePackAssetOffset)), _material.HighTexturePackAsset));

            foreach (var record in tuples.OrderBy(d => d.Item3.Length))
            {
                record.Item2.SetValue(record.Item1, stringBuffer.Put(record.Item3));
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
            _writer.WriteInt(_material.NameOffset);

            // TODO: Make these dynamic
            _writer.WriteInt(_material.InterfaceInputCount);
            _writer.WriteInt(_material.InterfaceCount);
            _writer.WriteInt(_material.TextureCount);
            _writer.WriteInt(_material.SamplerCount);

            _writer.WriteInt((ushort)_material.InterfaceInputs.Count);
            _writer.WriteInt((ushort)_material.Interfaces.Count);
            _writer.WriteInt((ushort)_material.Textures.Count);
            _writer.WriteInt((ushort)_material.Samplers.Count);

            _writer.WriteInt((ushort)_material.ShaderBinaries.Count);
            _writer.WriteInt((ushort)_material.ShaderPrograms.Count);

            _writer.WriteInt((ulong)inputBufferSize);
            _writer.WriteInt((ulong)stringBufferSize);
            _writer.WriteInt(_material.NameHash);

            _writer.WriteInt(_material.BlendType);
            _writer.WriteFloat(_material.BlendFactor);

            _writer.WriteInt(_material.RenderStateBits);
            _writer.WriteInt(_material.SkinVsMaxBoneCount);
            _writer.WriteInt(_material.BrdfType);

            // TODO: Make this optional for packs without HD
            _writer.WriteInt(_material.HighTexturePackAssetOffset);
        }

        private void WriteInterfaceInputs()
        {
            foreach (var input in _material.InterfaceInputs)
            {
                _writer.WriteInt(input.NameOffset);
                _writer.WriteInt(input.ShaderGenNameOffset);
                _writer.WriteInt(input.NameHash);
                _writer.WriteInt(input.ShaderGenNameHash);
                _writer.WriteInt(input.GpuOffset);
                _writer.WriteInt(input.Size);
                _writer.WriteInt(input.InterfaceIndex);
                _writer.WriteInt(input.Type);
                _writer.WriteInt(input.Flags);
            }
        }

        private void WriteInterfaces()
        {
            foreach (var @interface in _material.Interfaces)
            {
                _writer.WriteInt(@interface.NameOffset);
                _writer.WriteInt(@interface.ShaderGenNameOffset);
                _writer.WriteInt(@interface.Unknown1);
                _writer.WriteInt(@interface.NameHash);
                _writer.WriteInt(@interface.ShaderGenNameHash);
                _writer.WriteInt(@interface.Unknown2);
                _writer.WriteInt(@interface.GpuOffset);
                _writer.WriteInt(@interface.Size);
                _writer.WriteInt(@interface.InputCount);
                _writer.WriteInt(@interface.Flags);
            }
        }

        private void WriteTextures()
        {
            foreach (var texture in _material.Textures)
            {
                _writer.WriteInt(texture.ResourceFileHash);
                _writer.WriteInt(texture.NameOffset);
                _writer.WriteInt(texture.ShaderGenNameOffset);
                _writer.WriteInt(texture.Unknown1);
                _writer.WriteInt(texture.PathOffset);
                _writer.WriteInt(texture.NameHash);
                _writer.WriteInt(texture.ShaderGenNameHash);
                _writer.WriteInt(texture.Unknown2);
                _writer.WriteInt(texture.PathHash);
                _writer.WriteInt(texture.Flags);

                // TODO: Make this optional for packs without HD
                _writer.WriteSByte(texture.HighTextureStreamingLevels);
            }
        }

        private void WriteSamplers()
        {
            foreach (var sampler in _material.Samplers)
            {
                _writer.WriteInt(sampler.NameOffset);
                _writer.WriteInt(sampler.ShaderGenNameOffset);
                _writer.WriteInt(sampler.Unknown);  // Might be Reflection Sampler Hash Name?

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

                _writer.WriteInt(sampler.MinLod);
                _writer.WriteInt(sampler.MaxLod);

                _writer.WriteInt(sampler.Flags);
            }
        }

        private void WriteShaderBinaries()
        {
            foreach (var shader in _material.ShaderBinaries)
            {
                _writer.WriteInt(shader.ResourceFileHash);
                _writer.WriteInt(shader.PathOffset);
            }
        }

        private void WriteShaderPrograms()
        {
            foreach (var shader in _material.ShaderPrograms)
            {
                _writer.WriteInt(shader.LowKey);
                _writer.WriteInt(shader.HighKey);
                _writer.WriteInt(shader.CsBinaryIndex);
                _writer.WriteInt(shader.VsBinaryIndex);
                _writer.WriteInt(shader.HsBinaryIndex);
                _writer.WriteInt(shader.DsBinaryIndex);
                _writer.WriteInt(shader.GsBinaryIndex);
                _writer.WriteInt(shader.PsBinaryIndex);
            }
        }
    }
}
