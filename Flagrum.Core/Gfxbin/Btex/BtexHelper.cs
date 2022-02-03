using System;

namespace Flagrum.Core.Gfxbin.Btex;

public static class BtexHelper
{
    public static TextureType GetType(string textureId)
    {
        if (textureId.Contains("normal", StringComparison.OrdinalIgnoreCase))
        {
            return TextureType.Normal;
        }

        if (textureId.Contains("basecolor", StringComparison.OrdinalIgnoreCase)
            || textureId.Contains("emissive", StringComparison.OrdinalIgnoreCase))
        {
            return TextureType.BaseColor;
        }

        if (textureId.Contains("mrs", StringComparison.OrdinalIgnoreCase))
        {
            return TextureType.Mrs;
        }

        if (textureId.Contains("occlusion", StringComparison.OrdinalIgnoreCase))
        {
            return TextureType.AmbientOcclusion;
        }

        return TextureType.Undefined;
    }

    public static string GetSuffix(string textureId)
    {
        if (textureId.ToLower().Contains("normal"))
        {
            return "_n";
        }

        if (textureId.ToLower().Contains("basecolor"))
        {
            return "_b";
        }

        if (textureId.ToLower().Contains("mrs"))
        {
            return "_mrs";
        }

        if (textureId.ToLower().Contains("occlusion"))
        {
            return "_o";
        }

        if (textureId.ToLower().Contains("opacity"))
        {
            return "_a";
        }

        if (textureId.ToLower().Contains("emissive"))
        {
            return "_e";
        }

        if (textureId.ToLower().StartsWith("multimask_texture"))
        {
            var index = textureId.Substring("multimask_texture".Length);
            if (int.TryParse(index, out _))
            {
                return "_mm" + index;
            }
        }

        return "";
    }
}