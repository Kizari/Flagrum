using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Archiver;
using Flagrum.Gfxbin.Gmtl;
using Flagrum.Gfxbin.Gmtl.Data;
using Newtonsoft.Json;

namespace Flagrum.Console;

public class ArchiverTests
{
    public static void ReplaceMaterialAndRepack()
    {
        var replacements = new Dictionary<string, string>
        {
            {
                "data://character/nh/nh00/model_000/sourceimages/nh00_000_skin_02_b.tif",
                "data://mod/noctis_custom/arm_b.png"
            },
            {
                "data://character/nh/nh00/model_000/sourceimages/nh00_000_skin_02_n.tif",
                "data://mod/noctis_custom/arm_n.png"
            },
            {
                "data://character/nh/nh00/model_000/sourceimages/nh00_000_skin_02_mrs.tif",
                "data://mod/noctis_custom/arm_mrs.png"
            },
            {
                "data://character/nh/nh00/model_000/sourceimages/nh00_000_skin_02_o.tif",
                "data://mod/noctis_custom/arm_o.png"
            }
        };

        var reader = new MaterialReader("C:\\Testing\\Archiver\\mouth_material_original.gmtl.gfxbin");
        var baseMaterial = reader.Read();

        reader = new MaterialReader("C:\\Testing\\Archiver\\noctis_custom\\clean.fbxgmtl\\arm.gmtl.gfxbin");
        var modMaterial = reader.Read();

        baseMaterial.Name = modMaterial.Name;
        baseMaterial.NameHash = modMaterial.NameHash;

        foreach (var replacement in replacements)
        {
            var originalDependency =
                baseMaterial.Header.Dependencies.FirstOrDefault(d => d.Path == replacement.Key);
            if (originalDependency != null)
            {
                originalDependency.Path = replacement.Value;
            }
            else
            {
                throw new InvalidOperationException("BAD");
            }

            var texture = baseMaterial.Textures.FirstOrDefault(t => t.Path == replacement.Key);
            if (texture != null)
            {
                texture.Path = replacement.Value;
            }
            else
            {
                throw new InvalidOperationException("BAD");
            }
        }

        SetInputValues(baseMaterial, "Occlusion1_Power", 1);
        SetInputValues(baseMaterial, "Occlusion0_Power", 0);
        SetInputValues(baseMaterial, "MagicDamages_UVScale", 4, 4);
        SetInputValues(baseMaterial, "DirtTextureColor", 1.5f, 1.5f, 1.5f);
        SetInputValues(baseMaterial, "IceColorAdd", 0.0289f, 0.0503639f, 0.1f);
        SetInputValues(baseMaterial, "DirtTextureUVScale", 4.2f, 6);
        SetInputValues(baseMaterial, "VertexColor_MaskControl", 0.8f);
        SetInputValues(baseMaterial, "StoneMap_UVScale", 2, 2);
        SetInputValues(baseMaterial, "Normal0_Power", 1.3f);
        SetInputValues(baseMaterial, "Roughness0_Power", 1);
        SetInputValues(baseMaterial, "BloodTextureColor", 2, 2, 2);
        SetInputValues(baseMaterial, "BloodTextureUVScale", 2, 4);
        SetInputValues(baseMaterial, "MagicDamageAmount", 0.3f);
        SetInputValues(baseMaterial, "BaseColor0", 0.8f, 0.8f, 0.8f);
        SetInputValues(baseMaterial, "MagicDamageMask", 0.25f);

        var writer = new MaterialWriter(baseMaterial);
        var data = writer.Write();
        File.WriteAllBytes("C:\\Testing\\Archiver\\arm_material_modified.gmtl.gfxbin", data);

        replacements = new Dictionary<string, string>
        {
            {
                "C:\\Testing\\Archiver\\noctis_custom\\clean.fbxgmtl\\arm.gmtl.gfxbin",
                "C:\\Testing\\Archiver\\arm_material_modified.gmtl.gfxbin"
            }
        };

        CreateBinMod(replacements);
    }

    public static void CreateBinMod1(Dictionary<string, string> replacements)
    {
        var root = "C:\\Testing\\Gfxbin\\mod\\noctis_custom";
        var shaderMetadata = "C:\\Testing\\Archiver\\shaders.json";
        var outputPath = "C:\\Testing\\Gfxbin\\d090b917-d422-41ed-a641-0047de5fea48.ffxvbinmod";

        var packer = new Packer(root);
        packer.AddFile("data://$mod/temp.ebex");
        AddFilesRecursively(packer, root, replacements);

        var shaders = JsonConvert.DeserializeObject<List<ShaderData>>(File.ReadAllText(shaderMetadata));
        foreach (var shader in shaders)
        {
            packer.AddFile(shader.Path);
        }

        packer.WriteToFile(outputPath);
    }

    public static void CreateBinMod(Dictionary<string, string> replacements)
    {
        var root = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2";
        //var root = "C:\\Testing\\Gfxbin\\mod\\magic_cube";
        var shaderMetadata = "C:\\Testing\\Archiver\\shaders.json";
        var outputPath = "C:\\Testing\\Gfxbin\\de81d8a4-53d8-4ca9-bcf0-f9397e82db81.ffxvbinmod";
        //var outputPath = "C:\\Testing\\Gfxbin\\7e96495e-8336-4cbb-bc44-4ab826591644.ffxvbinmod";

        var packer = new Packer(root);
        packer.AddFile("data://$mod/temp.ebex");
        AddFilesRecursively(packer, root, replacements);

        var shaders = JsonConvert.DeserializeObject<List<ShaderData>>(File.ReadAllText(shaderMetadata));
        foreach (var shader in shaders)
        {
            packer.AddFile(shader.Path);
        }

        packer.WriteToFile(outputPath);
    }

    private static void AddFilesRecursively(Packer packer, string dir, Dictionary<string, string> replacements)
    {
        foreach (var directory in Directory.EnumerateDirectories(dir))
        {
            AddFilesRecursively(packer, directory, replacements);
        }

        foreach (var file in Directory.EnumerateFiles(dir))
        {
            var shouldAdd = true;
            var extensions = new[] {".clsmk", ".clsmk.dep", ".clsx"};
            foreach (var extension in extensions)
            {
                if (file.EndsWith(extension))
                {
                    shouldAdd = false;
                }
            }

            if (!shouldAdd)
            {
                continue;
            }

            if (replacements.TryGetValue(file, out var replacement))
            {
                packer.AddFile(file, replacement);
            }
            else
            {
                packer.AddFile(file);
            }
        }
    }

    private static void SetInputValues(Material material, string inputName, params float[] values)
    {
        var match = material.InterfaceInputs.FirstOrDefault(u =>
            u.ShaderGenName.ToLower() == inputName.ToLower() && u.InterfaceIndex == 0);
        if (match == null)
        {
            throw new ArgumentException($"Input {inputName} was not found in material {material.Name}.");
        }

        match.Values = values;
    }
}

public class ShaderData
{
    public string Path { get; set; }
    public uint Size { get; set; }
    public uint ProcessedSize { get; set; }
    public uint DataStart { get; set; }
}