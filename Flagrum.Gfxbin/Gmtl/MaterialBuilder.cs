using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Core.Utilities;
using Flagrum.Gfxbin.Gmtl.Data;
using Newtonsoft.Json;

namespace Flagrum.Gfxbin.Gmtl;

public static class MaterialBuilder
{
    public static Material FromTemplate(string templateName, string meshName, string materialUri, List<MaterialInputData> inputs,
        List<MaterialTextureData> textures)
    {
        var templatePath = $"{Directory.GetCurrentDirectory()}\\Gmtl\\Templates\\{templateName}.json";
        var json = File.ReadAllText(templatePath);
        var material = JsonConvert.DeserializeObject<Material>(json);

        material.Name = meshName;
        material.NameHash = (uint)Cryptography.Hash(materialUri);
        
        foreach (var input in inputs)
        {
            SetInputValues(material, input.Name, input.Values);
        }

        foreach (var texture in textures)
        {
            SetTexturePath(material, texture.Name, texture.Path);
        }

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
            throw new ArgumentException($"Texture {name} was not found in material {material.Name}", nameof(name));
        }

        var dependencyMatch = material.Header.Dependencies.FirstOrDefault(d => d.Path == match.Path);
        
        match.Path = path;
        dependencyMatch.Path = path;
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