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
            || textureId.Contains("mrs", StringComparison.OrdinalIgnoreCase)
            || textureId.Contains("emissive", StringComparison.OrdinalIgnoreCase))
        {
            return TextureType.Color;
        }

        return TextureType.Greyscale;
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