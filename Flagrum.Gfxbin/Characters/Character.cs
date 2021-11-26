using System.Collections.Generic;
using Flagrum.Gfxbin.Gmdl.Components;

namespace Flagrum.Gfxbin.Characters;

public static class Character
{
    public static List<BoneHeader> GetPreloadedBones(string character) => character switch
    {
        "PROMPTO" => PromptoData.PreloadedBones,
        _ => new List<BoneHeader>()
    };
}