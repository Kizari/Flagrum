using Flagrum.Gfxbin.Data;
using System.Collections.Generic;

namespace Flagrum.Gfxbin.Materials.Data
{
    public class Material
    {
        public GfxbinHeader Header { get; } = new GfxbinHeader();

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

        public List<MaterialInterface> Interfaces { get; set; }
        public List<MaterialInterfaceInput> InterfaceInputs { get; set; }
        public List<MaterialTexture> Textures { get; set; }
        public List<MaterialSampler> Samplers { get; set; }
        public List<MaterialShaderBinary> ShaderBinaries { get; set; }
        public List<MaterialShaderProgram> ShaderPrograms { get; set; }

        public ushort ShaderBinaryCount { get; set; }
        public ushort ShaderProgramCount { get; set; }

        // These are the counts of the objects in the lists that map to this material
        public ushort InterfaceCount { get; set; }
        public ushort InterfaceInputCount { get; set; }
        public ushort TextureCount { get; set; }
        public ushort SamplerCount { get; set; }

        // These are the counts of all objects in the lists
        public ushort TotalInterfaceCount { get; set; }
        public ushort TotalInterfaceInputCount { get; set; }
        public ushort TotalTextureCount { get; set; }
        public ushort TotalSamplerCount { get; set; }

        public uint InputsBufferSize { get; set; }
    }
}
