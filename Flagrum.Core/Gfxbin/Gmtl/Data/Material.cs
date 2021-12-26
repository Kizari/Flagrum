using System;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Gfxbin.Data;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Gfxbin.Gmtl.Data;

public class Material
{
    public GfxbinHeader Header { get; } = new();

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

    public string Uri { get; set; }

    public List<(string Path, string OffsetPath)> StringPropertyPaths { get; set; } = new();

    public void UpdateTexture(string textureSlot, string newPath)
    {
        var match = Textures.FirstOrDefault(t => t.ShaderGenName == textureSlot);

        if (match != null)
        {
            match.Path = newPath;
            match.PathHash = Cryptography.Hash32(newPath);
            match.ResourceFileHash = Cryptography.HashFileUri64(newPath);
        }
    }
    
    public void UpdateName(string modDirectoryName, string materialName)
    {
        Name = materialName;
        Uri = $"data://mod/{modDirectoryName}/materials/{materialName}.gmtl";
        NameHash = Cryptography.Hash32(materialName);
    }
    
    public void RegenerateDependencyTable()
    {
        var assetUri = Uri.Remove(Uri.LastIndexOf('/') + 1);
        var dependencies = new List<DependencyPath>();
        dependencies.AddRange(ShaderBinaries.Where(s => s.ResourceFileHash > 0).Select(s => new DependencyPath
            {Path = s.Path, PathHash = s.ResourceFileHash.ToString()}));
        dependencies.AddRange(Textures.Where(s => s.ResourceFileHash > 0).Select(s => new DependencyPath
            {Path = s.Path, PathHash = s.ResourceFileHash.ToString()}));
        dependencies.Add(
            new DependencyPath {PathHash = "asset_uri", Path = assetUri});
        dependencies.Add(new DependencyPath
            {PathHash = "ref", Path = Uri});
        Header.Dependencies = dependencies.DistinctBy(d => d.PathHash).ToList();
        Header.Hashes = Header.Dependencies
            .Where(d => ulong.TryParse(d.PathHash, out _))
            .Select(d => ulong.Parse(d.PathHash))
            .OrderBy(h => h)
            .ToList();
    }
}