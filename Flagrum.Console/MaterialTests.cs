using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Gfxbin.Gmtl;
using Flagrum.Gfxbin.Gmtl.Data;

namespace Flagrum.Console;

public class MaterialTests
{
    public static void BuildSkinMaterial()
    {
        var original = "C:\\Testing\\Gfxbin\\Gmtl\\arm.gmtl.gfxbin";

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

        var gfxbin = "C:\\Testing\\Gfxbin\\Gmtl\\mouth_material.gmtl.gfxbin";

        var reader = new MaterialReader(gfxbin);
        var mouthMaterial = reader.Read();
        reader = new MaterialReader(original);
        var modMaterial = reader.Read();

        mouthMaterial.Name = modMaterial.Name;
        mouthMaterial.NameHash = modMaterial.NameHash;

        foreach (var replacement in replacements)
        {
            var originalDependency = mouthMaterial.Header.Dependencies.FirstOrDefault(d => d.Path == replacement.Key);
            if (originalDependency != null)
            {
                originalDependency.Path = replacement.Value;
            }

            var texture = mouthMaterial.Textures.FirstOrDefault(t => t.Path == replacement.Key);
            if (texture != null)
            {
                texture.Path = replacement.Value;
            }
            else
            {
                throw new InvalidOperationException("BAD");
            }
        }

        foreach (var input in mouthMaterial.InterfaceInputs)
        {
            if (input.InterfaceIndex == 0)
            {
                System.Console.WriteLine(input.Name);
            }
        }

        SetUniformValues(mouthMaterial, "Occlusion1_Power", 1);
        SetUniformValues(mouthMaterial, "Occlusion0_Power", 1);
        SetUniformValues(mouthMaterial, "MagicDamages_UVScale", 4, 4);
        SetUniformValues(mouthMaterial, "DirtTextureColor", 1.5f, 1.5f, 1.5f);
        SetUniformValues(mouthMaterial, "IceColorAdd", 0.0289f, 0.0503639f, 0.1f);
        SetUniformValues(mouthMaterial, "DirtTextureUVScale", 4.2f, 6);
        SetUniformValues(mouthMaterial, "VertexColor_MaskControl", 0.8f);
        SetUniformValues(mouthMaterial, "StoneMap_UVScale", 2, 2);
        SetUniformValues(mouthMaterial, "Normal0_Power", 1.3f);
        SetUniformValues(mouthMaterial, "Roughness0_Power", 1);
        SetUniformValues(mouthMaterial, "BloodTextureColor", 2, 2, 2);
        SetUniformValues(mouthMaterial, "BloodTextureUVScale", 2, 4);
        SetUniformValues(mouthMaterial, "MagicDamageAmount", 0.3f);
        SetUniformValues(mouthMaterial, "BaseColor0", 0.8f, 0.8f, 0.8f);
        SetUniformValues(mouthMaterial, "MagicDamageMask", 0.25f);

        var writer = new MaterialWriter(mouthMaterial);
        var materialData = writer.Write();
        File.WriteAllBytes("C:\\Testing\\Gfxbin\\mod\\noctis_custom\\clean.fbxgmtl\\arm.gmtl.gfxbin", materialData);
    }

    private static void SetUniformValues(Material material, string uniformName, params float[] values)
    {
        var match = material.InterfaceInputs.FirstOrDefault(u =>
            u.ShaderGenName.ToLower() == uniformName.ToLower() && u.InterfaceIndex == 0);
        if (match == null)
        {
            throw new ArgumentException($"Input {uniformName} was not found in material {material.Name}.");
        }

        match.Values = values;
    }
}