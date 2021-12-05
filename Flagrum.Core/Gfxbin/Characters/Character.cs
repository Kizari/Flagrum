using System.Collections.Generic;
using Flagrum.Core.Gfxbin.Gmdl.Components;

namespace Flagrum.Core.Gfxbin.Characters;

public static class Character
{
    public static List<BoneHeader> GetPreloadedBones(string character)
    {
        return character switch
        {
            "NOCTIS" => NoctisData.PreloadedBones,
            "PROMPTO" => PromptoData.PreloadedBones,
            "IGNIS" => IgnisData.PreloadedBones,
            "GLADIOLUS" => GladiolusData.PreloadedBones,
            _ => new List<BoneHeader>()
        };
    }
}