using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Data;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmtl.Data;
using Flagrum.Core.Utilities;
using Newtonsoft.Json;

namespace Flagrum.Core.Gfxbin.Gmtl;

public static class MaterialBuilder
{
    public static Dictionary<string, string> DefaultTextures = new()
    {
        {"BaseColor0_Texture", "white-color.btex"},
        {"BaseColor0Texture", "white-color.btex"},
        {"BaseColorRedEye_Texture", "black.btex"},
        {"Occlusion0_Texture", "white.btex"},
        {"Occlusion0Texture", "white.btex"},
        {"MRS0_Texture", "teal.btex"},
        {"Texture0", "white.btex"},
        {"SSSMask_Texture", "white.btex"},
        {"OpacityMask0_Texture", "white.btex"},
        {"Transparency0_Texture", "white.btex"},
        {"Transparency0Texture", "white.btex"},
        {"Emissive0_Texture", "black.btex"},
        {"EmissiveColor0Texture", "black.btex"},
        {"EmissiveRedEye_Texture", "black.btex"},
        {"RefractionMask0", "white.btex"},
        {"Normal0_Texture", "blue.btex"},
        {"Normal0InnerTexture", "blue.btex"},
        {"Normal0Texture", "blue.btex"},
        {"BaseColor1_Texture", "white-color.btex"},
        {"BaseColor1_Texture0", "white-color.btex"},
        {"BaseColor2_Texture", "white-color.btex"},
        {"BloodTexture_Mask", "white.btex"},
        {"DirtField_mask", "white.btex"},
        {"DropsMask_Texture", "white.btex"},
        {"DustColorMaskTex", "white.btex"},
        {"Emissive1_Texture", "black.btex"},
        {"Emissive2_Texture", "black.btex"},
        {"Emissive3_Texture", "black.btex"},
        {"FazzMask0_Texture", "white.btex"},
        {"IceMask_Texture", "white.btex"},
        {"MuliMask_Texture_Detail", "black.btex"},
        {"MultiMask_Texture_Lip", "black.btex"},
        {"MultiMask_Texture_Manicure", "black.btex"},
        {"MultiMask_Texture_wrinkle", "black.btex"},
        {"MultiMask_Texture1", "black.btex"},
        {"MultiMask_Texture10", "black.btex"},
        {"MultiMask_Texture11", "black.btex"},
        {"MultiMask_Texture12", "black.btex"},
        {"MultiMask_Texture13", "black.btex"},
        {"MultiMask_Texture14", "black.btex"},
        {"MultiMask_Texture15", "black.btex"},
        {"MultiMask_Texture16", "black.btex"},
        {"MultiMask_Texture2", "black.btex"},
        {"MultiMask_Texture3", "black.btex"},
        {"MultiMask_Texture4", "black.btex"},
        {"MultiMask_Texture5", "black.btex"},
        {"MultiMask_Texture6", "black.btex"},
        {"MultiMask_Texture7", "black.btex"},
        {"MultiMask_Texture8", "black.btex"},
        {"MultiMask_Texture9", "black.btex"},
        {"MultiMask0_Texture", "black.btex"},
        {"MultiMask0_Texture0", "black.btex"},
        {"MultiMask1_Texture", "black.btex"},
        {"MultiMask2_Texture", "black.btex"},
        {"MultiMask3_Texture", "black.btex"},
        {"MultiMask3_Texture0", "black.btex"},
        {"MultiMask4_Texture", "black.btex"},
        {"MultiMaskRedEye_Texture", "black.btex"},
        {"Normal_Texture_Detail", "blue.btex"},
        {"Normal_Texture_wrinkle", "blue.btex"},
        {"Normal1_Texture", "blue.btex"},
        {"Normal1_Texture0", "blue.btex"},
        {"Normal2_Texture", "blue.btex"},
        {"Occlusion_Texture_Detail", "white.btex"},
        {"Occlusion1_Texture", "white.btex"},
        {"OpacityMask1_Texture", "white.btex"},
        {"StoneMask_Texture", "white.btex"},
        {"SweatTexture_Mask", "white.btex"},
        {"wipersMask_Texture", "white.btex"}
    };

    public static Material FromTemplate(string templateName, string materialName, string modDirectoryName,
        List<MaterialInputData> inputs,
        List<MaterialTextureData> textures,
        Dictionary<string, string> replacements,
        out MaterialType type,
        out Dictionary<string, byte[]> extras)
    {
        type = templateName switch
        {
            "BASIC_MATERIAL" => MaterialType.OneWeight,
            //"NAMED_HUMAN_OUTFIT" => MaterialType.SixWeights,
            //"NAMED_HUMAN_SKIN" => MaterialType.EightWeights,
            _ => MaterialType.FourWeights
        };

        Material material;
        // TODO: Remove this (keep contents of else block but remove if statement)
        // if (templateName == "BASIC_MATERIAL")
        // {
        //     var unpacker = new Unpacker("C:\\Testing\\ModelReplacements\\SinglePlayerSword\\sword_1.ffxvbinmod");
        //     extras = unpacker.UnpackFilesByQuery("data://shader");
        //     var fbxDefault = unpacker.UnpackFileByQuery("material.gmtl");
        //     var reader = new MaterialReader(fbxDefault);
        //     material = reader.Read();
        //     var oldDependency = material.Header.Dependencies.FirstOrDefault(d => d.Path.EndsWith("_d.png"));
        //     var oldIndex = material.Header.Hashes.IndexOf(ulong.Parse(oldDependency.PathHash));
        //     var oldTexture = material.Textures.FirstOrDefault(t => t.Path == oldDependency.Path);
        //     oldTexture.Path = $"data://mod/{modDirectoryName}/sourceimages/{materialName.Replace("_mat", "")}_basecolor0_texture_b.btex";
        //     oldTexture.PathHash = Cryptography.Hash32(oldTexture.Path);
        //     oldTexture.ResourceFileHash = Cryptography.HashFileUri64(oldTexture.Path);
        //     material.Header.Hashes[oldIndex] = oldTexture.ResourceFileHash;
        //     oldDependency.Path = oldTexture.Path;
        //     oldDependency.PathHash = oldTexture.ResourceFileHash.ToString();
        //     var assetUri = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri");
        //     assetUri.Path = $"data://mod/{modDirectoryName}/materials/";
        //     var reference = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref");
        //     reference.Path = $"data://mod/{modDirectoryName}/materials/{materialName}.gmtl";
        //     material.Name = materialName;
        //     material.NameHash = Cryptography.Hash32(material.Name);
        //     material.Uri = $"data://mod/{modDirectoryName}/materials/{materialName}.gmtl";
        //     return material;
        // }
        // else
        {
            extras = new Dictionary<string, byte[]>();
            var templatePath = $"{IOHelper.GetExecutingDirectory()}\\Resources\\Materials\\{templateName}.json";
            var json = File.ReadAllText(templatePath);
            material = JsonConvert.DeserializeObject<Material>(json);
        }

        material.Name = materialName;
        material.Uri = $"data://mod/{modDirectoryName}/materials/{materialName}.gmtl";
        material.NameHash = Cryptography.Hash32(materialName);

        foreach (var input in inputs)
        {
            SetInputValues(material, input.Name, input.Values);
        }

        foreach (var texture in textures)
        {
            SetTexturePath(material, texture.Name, texture.Path);
        }

        material.HighTexturePackAsset = "";
        
        foreach (var (textureId, replacementUri) in replacements)
        {
            var match = material.Textures.FirstOrDefault(t => t.ShaderGenName == textureId);
            match.Path = replacementUri;
            match.PathHash = Cryptography.Hash32(replacementUri);
            match.ResourceFileHash = Cryptography.HashFileUri64(replacementUri);
        }

        var dependencies = new List<DependencyPath>();
        dependencies.AddRange(material.ShaderBinaries.Where(s => s.ResourceFileHash > 0).Select(s => new DependencyPath { Path = s.Path, PathHash = s.ResourceFileHash.ToString() }));
        dependencies.AddRange(material.Textures.Where(s => s.ResourceFileHash > 0).Select(s => new DependencyPath { Path = s.Path, PathHash = s.ResourceFileHash.ToString() }));
        dependencies.Add(new DependencyPath { PathHash = "asset_uri", Path = $"data://mod/{modDirectoryName}/materials/"});
        dependencies.Add(new DependencyPath { PathHash = "ref", Path = $"data://mod/{modDirectoryName}/materials/{materialName}.gmtl"});
        material.Header.Dependencies = dependencies.DistinctBy(d => d.PathHash).ToList();
        material.Header.Hashes = material.Header.Dependencies
            .Where(d => ulong.TryParse(d.PathHash, out _))
            .Select(d => ulong.Parse(d.PathHash))
            .OrderBy(h => h)
            .ToList();

        return material;
    }

    private static void SetInputValues(Material material, string inputName, params float[] values)
    {
        var match = material.InterfaceInputs.FirstOrDefault(u =>
            u.ShaderGenName.ToLower() == inputName.ToLower() && u.InterfaceIndex == 0);

        if (match == null)
        {
            throw new ArgumentException($"Input {inputName} was not found in material {material.Name}.",
                nameof(inputName));
        }

        match.Values = values;
    }

    private static void SetTexturePath(Material material, string name, string path)
    {
        var match = material.Textures.FirstOrDefault(t => t.ShaderGenName == name);

        if (match == null)
        {
            return;
        }

        match.Path = path;
        match.PathHash = Cryptography.Hash32(path);
        match.ResourceFileHash = Cryptography.HashFileUri64(path);
    }

    public static byte[] GetDefaultTextureData(string name)
    {
        return File.ReadAllBytes($"{IOHelper.GetExecutingDirectory()}\\Resources\\Textures\\{name}");
    }
}

public class MaterialInputData
{
    public string Name { get; set; }
    public float[] Values { get; set; }
}

public class MaterialTextureData
{
    public string Name { get; set; }
    public string Path { get; set; }
}