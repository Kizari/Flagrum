using System.Collections.Generic;
using System.Linq;
using Flagrum.Web.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Web.Services;

public class ModelReplacementTarget
{
    public int Index { get; set; }
    public string Name { get; set; }
    public string ModmetaName { get; set; }
    public IEnumerable<string> Models { get; set; }
}

public class ModelReplacementPresets
{
    private readonly FlagrumDbContext _context;

    public ModelReplacementPresets(FlagrumDbContext context)
    {
        _context = context;
        BuildDictionary();
    }

    private List<ModelReplacementTarget> Replacements { get; set; }

    public Dictionary<int, string> GetReplacementDictionary()
    {
        return Replacements.ToDictionary(r => r.Index, r => r.Name);
    }

    public Dictionary<int, string> GetReplacementModmetaDictionary()
    {
        return Replacements.ToDictionary(r => r.Index, r => r.ModmetaName);
    }

    public IEnumerable<string> GetOriginalGmdls(int target)
    {
        return Replacements.First(r => r.Index == target).Models;
    }

    private void BuildDictionary()
    {
        // Get Flagrum presets from code
        Replacements = GetDefaultReplacements();

        // Get user-defined presets from database
        var custom = _context.ModelReplacementPresets
            .Include(p => p.ReplacementPaths)
            .ToList();

        Replacements.AddRange(custom.Select(e =>
        {
            return new ModelReplacementTarget
            {
                Index = e.Id + 100,
                Name = e.Name,
                ModmetaName = $"Custom_{e.Id + 100}",
                Models = e.ReplacementPaths.Select(p => p.Path)
            };
        }));
    }

    public List<ModelReplacementTarget> GetDefaultReplacements()
    {
        return new List<ModelReplacementTarget>
        {
            new()
            {
                Index = 0,
                Name = "Noctis",
                ModmetaName = "Noctis",
                Models = new[]
                {
                    "character/nh/nh00/model_000/nh00_000.gmdl",
                    "character/nh/nh00/model_001/nh00_001.gmdl",
                    "character/nh/nh00/model_010/nh00_010.gmdl",
                    "character/nh/nh00/model_011/nh00_011.gmdl",
                    "character/nh/nh00/model_020/nh00_020.gmdl",
                    "character/nh/nh00/model_021/nh00_021.gmdl",
                    "character/nh/nh00/model_022/nh00_022.gmdl",
                    "character/nh/nh00/model_023/nh00_023.gmdl",
                    "character/nh/nh00/model_024/nh00_024.gmdl",
                    "character/nh/nh00/model_030/nh00_030.gmdl",
                    "character/nh/nh00/model_040/nh00_040.gmdl",
                    "character/nh/nh00/model_041/nh00_041.gmdl",
                    "character/nh/nh00/model_050/nh00_050.gmdl",
                    "character/nh/nh00/model_060/nh00_060.gmdl",
                    "character/nh/nh00/model_061/nh00_061.gmdl",
                    "character/nh/nh00/model_062/nh00_062.gmdl",
                    "character/nh/nh00/model_063/nh00_063.gmdl",
                    "character/nh/nh00/model_064/nh00_064.gmdl",
                    "character/nh/nh00/model_065/nh00_065.gmdl",
                    "character/nh/nh00/model_066/nh00_066.gmdl",
                    "character/nh/nh00/model_070/nh00_070.gmdl",
                    "character/nh/nh00/model_071/nh00_071.gmdl",
                    "character/nh/nh00/model_080/nh00_080.gmdl",
                    "character/nh/nh00/model_081/nh00_081.gmdl",
                    "character/nh/nh00/model_090/nh00_090.gmdl",
                    "character/nh/nh00/model_092/nh00_092.gmdl",
                    "character/nh/nh00/model_099/nh00_099.gmdl",
                    "character/nh/nh00/model_100/nh00_100.gmdl",
                    "character/nh/nh00/model_101/nh00_101.gmdl",
                    "character/nh/nh00/model_200/nh00_200.gmdl",
                    "character/nh/nh00/model_201/nh00_201.gmdl",
                    "character/nh/nh00/model_300/nh00_300.gmdl",
                    "character/nh/nh00/model_500/nh00_500.gmdl"
                }
            },
            new()
            {
                Index = 1,
                Name = "Prompto",
                ModmetaName = "Prompto",
                Models = new[]
                {
                    "character/nh/nh02/model_000/nh02_000.gmdl",
                    "character/nh/nh02/model_010/nh02_010.gmdl",
                    "character/nh/nh02/model_020/nh02_020.gmdl",
                    "character/nh/nh02/model_030/nh02_030.gmdl",
                    "character/nh/nh02/model_040/nh02_040.gmdl",
                    "character/nh/nh02/model_041/nh02_041.gmdl",
                    "character/nh/nh02/model_042/nh02_042.gmdl",
                    "character/nh/nh02/model_043/nh02_043.gmdl",
                    "character/nh/nh02/model_050/nh02_050.gmdl",
                    "character/nh/nh02/model_060/nh02_060.gmdl",
                    "character/nh/nh02/model_070/nh02_070.gmdl",
                    "character/nh/nh02/model_081/nh02_081.gmdl",
                    "character/nh/nh02/model_090/nh02_090.gmdl",
                    "character/nh/nh02/model_092/nh02_092.gmdl",
                    "character/nh/nh02/model_100/nh02_100.gmdl",
                    "character/nh/nh02/model_200/nh02_200.gmdl",
                    "character/nh/nh02/model_300/nh02_300.gmdl"
                }
            },
            new()
            {
                Index = 2,
                Name = "Ignis",
                ModmetaName = "Ignis",
                Models = new[]
                {
                    "character/nh/nh03/model_000/nh03_000.gmdl",
                    "character/nh/nh03/model_001/nh03_001.gmdl",
                    "character/nh/nh03/model_010/nh03_010.gmdl",
                    "character/nh/nh03/model_020/nh03_020.gmdl",
                    "character/nh/nh03/model_030/nh03_030.gmdl",
                    "character/nh/nh03/model_050/nh03_050.gmdl",
                    "character/nh/nh03/model_081/nh03_081.gmdl",
                    "character/nh/nh03/model_090/nh03_090.gmdl",
                    "character/nh/nh03/model_092/nh03_092.gmdl",
                    "character/nh/nh03/model_100/nh03_100.gmdl",
                    "character/nh/nh03/model_101/nh03_101.gmdl",
                    "character/nh/nh03/model_200/nh03_200.gmdl",
                    "character/nh/nh03/model_201/nh03_201.gmdl",
                    "character/nh/nh03/model_300/nh03_300.gmdl",
                    "character/nh/nh03/model_301/nh03_301.gmdl",
                    "character/nh/nh03/model_302/nh03_302.gmdl",
                    "character/nh/nh03/model_303/nh03_303.gmdl",
                    "character/nh/nh03/model_304/nh03_304.gmdl",
                    "character/nh/nh03/model_305/nh03_305.gmdl",
                    "character/nh/nh03/model_310/nh03_310.gmdl",
                    "character/nh/nh03/model_311/nh03_311.gmdl",
                    "character/nh/nh03/model_312/nh03_312.gmdl",
                    "character/nh/nh03/model_320/nh03_320.gmdl",
                    "character/nh/nh03/model_330/nh03_330.gmdl",
                    "character/nh/nh03/model_331/nh03_331.gmdl"
                }
            },
            new()
            {
                Index = 3,
                Name = "Gladiolus",
                ModmetaName = "Gladiolus",
                Models = new[]
                {
                    "character/nh/nh01/model_000/nh01_000.gmdl",
                    "character/nh/nh01/model_010/nh01_010.gmdl",
                    "character/nh/nh01/model_011/nh01_011.gmdl",
                    "character/nh/nh01/model_020/nh01_020.gmdl",
                    "character/nh/nh01/model_030/nh01_030.gmdl",
                    "character/nh/nh01/model_050/nh01_050.gmdl",
                    "character/nh/nh01/model_081/nh01_081.gmdl",
                    "character/nh/nh01/model_090/nh01_090.gmdl",
                    "character/nh/nh01/model_092/nh01_092.gmdl",
                    "character/nh/nh01/model_100/nh01_100.gmdl",
                    "character/nh/nh01/model_200/nh01_200.gmdl",
                    "character/nh/nh01/model_300/nh01_300.gmdl"
                }
            },
            new()
            {
                Index = 4,
                Name = "Lunafreya",
                ModmetaName = "Lunafrena",
                Models = new[]
                {
                    "character/nh/nh04/model_000/nh04_000.gmdl",
                    "character/nh/nh04/model_010/nh04_010.gmdl",
                    "character/nh/nh04/model_011/nh04_011.gmdl",
                    "character/nh/nh04/model_020/nh04_020.gmdl",
                    "character/nh/nh04/model_030/nh04_030.gmdl",
                    "character/nh/nh04/model_040/nh04_040.gmdl"
                }
            },
            new()
            {
                Index = 5,
                Name = "Ardyn",
                ModmetaName = "Ardyn",
                Models = new[]
                {
                    "character/nh/nh05/model_000/nh05_000.gmdl",
                    "character/nh/nh05/model_010/nh05_010.gmdl",
                    "character/nh/nh05/model_010/nh05_100.gmdl",
                    "character/nh/nh05/model_010/nh05_101.gmdl"
                }
            },
            new()
            {
                Index = 6,
                Name = "Ravus",
                ModmetaName = "Ravus",
                Models = new[]
                {
                    "character/nh/nh08/model_000/nh08_000.gmdl",
                    "character/nh/nh08/model_001/nh08_001.gmdl",
                    "character/nh/nh08/model_100/nh08_100.gmdl"
                }
            },
            new()
            {
                Index = 7,
                Name = "Gentiana",
                ModmetaName = "Gentiana",
                Models = new[]
                {
                    "character/nh/nh09/model_000/nh09_000.gmdl",
                    "character/nh/nh09/model_010/nh09_010.gmdl",
                    "character/nh/nh09/model_020/nh09_020.gmdl",
                    "character/nh/nh09/model_030/nh09_030.gmdl"
                }
            },
            new()
            {
                Index = 8,
                Name = "Aranea",
                ModmetaName = "Aranea",
                Models = new[]
                {
                    "character/nh/nh10/model_000/nh10_000.gmdl",
                    "character/nh/nh10/model_010/nh10_010.gmdl",
                    "character/nh/nh10/model_020/nh10_020.gmdl"
                }
            },
            new()
            {
                Index = 9,
                Name = "Cid",
                ModmetaName = "Cid",
                Models = new[]
                {
                    "character/nh/nh11/model_000/nh11_000.gmdl",
                    "character/nh/nh11/model_010/nh11_010.gmdl"
                }
            },
            new()
            {
                Index = 10,
                Name = "Iris",
                ModmetaName = "Iris",
                Models = new[]
                {
                    "character/nh/nh12/model_000/nh12_000.gmdl"
                }
            },
            new()
            {
                Index = 11,
                Name = "Cor",
                ModmetaName = "Cor",
                Models = new[]
                {
                    "character/nh/nh13/model_000/nh13_000.gmdl",
                    "character/nh/nh13/model_010/nh13_010.gmdl",
                    "character/nh/nh13/model_030/nh13_030.gmdl"
                }
            },
            new()
            {
                Index = 12,
                Name = "Cindy",
                ModmetaName = "Cidney",
                Models = new[]
                {
                    "character/nh/nh19/model_000/nh19_000.gmdl"
                }
            },
            new()
            {
                Index = 13,
                Name = "Male Adult NPCs",
                ModmetaName = "NPC_MALE",
                Models = new[]
                {
                    "character/um/common/model_000/um00_000.gmdl",
                    "character/um/common/model_000/um00_000_common.gmdl",
                    "character/um/um00/model_000/um00_000.gmdl",
                    "character/um/um00/model_001/um00_001.gmdl",
                    "character/um/um00/model_010/um00_010.gmdl",
                    "character/um/um00/model_011/um00_011.gmdl",
                    "character/um/um00/model_020/um00_020.gmdl",
                    "character/um/um00/model_100/um00_100.gmdl",
                    "character/um/um00/model_410/um00_410.gmdl",
                    "character/um/um01/model_100/um01_100.gmdl",
                    "character/um/um01/model_400/um01_400.gmdl",
                    "character/um/um02/model_001/um02_001.gmdl",
                    "character/um/um02/model_002/um02_002.gmdl",
                    "character/um/um02/model_100/um02_100.gmdl",
                    "character/um/um02/model_102/um02_102.gmdl",
                    "character/um/um02/model_160/um02_160.gmdl",
                    "character/um/um02/model_400/um02_400.gmdl",
                    "character/um/um02/model_401/um02_401.gmdl",
                    "character/um/um02/model_410/um02_410.gmdl",
                    "character/um/um02/model_460/um02_460.gmdl",
                    "character/um/um04/model_001/um04_001.gmdl",
                    "character/um/um04/model_100/um04_100.gmdl",
                    "character/um/um04/model_101/um04_101.gmdl",
                    "character/um/um04/model_110/um04_110.gmdl",
                    "character/um/um04/model_111/um04_111.gmdl",
                    "character/um/um04/model_120/um04_120.gmdl",
                    "character/um/um04/model_201/um04_201.gmdl",
                    "character/um/um04/model_400/um04_400.gmdl",
                    "character/um/um04/model_401/um04_401.gmdl",
                    "character/um/um04/model_410/um04_410.gmdl",
                    "character/um/um04/model_411/um04_411.gmdl",
                    "character/um/um04/model_460/um04_460.gmdl",
                    "character/um/um04/model_501/um04_501.gmdl",
                    "character/um/um05/model_001/um05_001.gmdl",
                    "character/um/um05/model_100/um05_100.gmdl",
                    "character/um/um05/model_101/um05_101.gmdl",
                    "character/um/um05/model_102/um05_102.gmdl",
                    "character/um/um05/model_103/um05_103.gmdl",
                    "character/um/um05/model_110/um05_110.gmdl",
                    "character/um/um05/model_120/um05_120.gmdl",
                    "character/um/um05/model_130/um05_130.gmdl",
                    "character/um/um05/model_150/um05_150.gmdl",
                    "character/um/um05/model_200/um05_200.gmdl",
                    "character/um/um05/model_400/um05_400.gmdl",
                    "character/um/um05/model_401/um05_401.gmdl",
                    "character/um/um05/model_470/um05_470.gmdl",
                    "character/um/um05/model_600/um05_600.gmdl",
                    "character/um/um05/model_602/um05_602.gmdl",
                    "character/um/um05/model_650/um05_650.gmdl",
                    "character/um/um05/model_970/um05_970.gmdl",
                    "character/um/um09/model_430/um09_430.gmdl",
                    "character/um/um13/model_110/um13_110.gmdl",
                    "character/um/um13/model_410/um13_410.gmdl",
                    "character/um/um14/model_000/um14_000.gmdl",
                    "character/um/um14/model_120/um14_120.gmdl",
                    "character/um/um14/model_410/um14_410.gmdl",
                    "character/um/um20/model_001/um20_001.gmdl",
                    "character/um/um20/model_002/um20_002.gmdl",
                    "character/um/um20/model_003/um20_003.gmdl",
                    "character/um/um20/model_160/um20_160.gmdl",
                    "character/um/um20/model_170/um20_170.gmdl",
                    "character/um/um30/model_000/um30_000.gmdl",
                    "character/um/um30/model_100/um30_100.gmdl",
                    "character/um/um30/model_101/um30_101.gmdl",
                    "character/um/um30/model_400/um30_400.gmdl",
                    "character/um/um30/model_401/um30_401.gmdl",
                    "character/um/um99/model_000/um99_000.gmdl",
                    "character/ux/common/model_000/ux00_000.gmdl",
                    "character/ux/ux00/model_000/ux00_000.gmdl",
                    "character/ux/ux00/model_010/ux00_010.gmdl",
                    "character/ux/ux04/model_100/ux04_100.gmdl",
                    "character/ux/ux04/model_101/ux04_101.gmdl",
                    "character/ux/ux04/model_110/ux04_110.gmdl",
                    "character/ux/ux04/model_111/ux04_111.gmdl",
                    "character/ux/ux04/model_120/ux04_120.gmdl",
                    "character/ux/ux05/model_100/ux05_100.gmdl",
                    "character/ux/ux05/model_101/ux05_101.gmdl",
                    "character/ux/ux05/model_110/ux05_110.gmdl",
                    "character/ux/ux09/model_000/ux09_000.gmdl",
                    "character/ux/ux09/model_100/ux09_100.gmdl",
                    "character/ux/ux14/model_000/ux14_000.gmdl",
                    "character/ux/ux14/model_110/ux14_110.gmdl",
                    "character/nh/nh29/model_000/nh29_000.gmdl",
                    "character/nh/nh27/model_000/nh27_000.gmdl",
                    "character/nh/nh25/model_000/nh25_000.gmdl",
                    "character/nh/nh26/model_000/nh26_000.gmdl",
                    "character/nh/nh22/model_000/nh22_000.gmdl",
                    "character/nh/nh23/model_000/nh23_000.gmdl",
                    "character/nh/nh23/model_010/nh23_010.gmdl",
                    "character/nh/nh18/model_000/nh18_000.gmdl",
                    "character/nh/nh18/model_010/nh18_010.gmdl"
                }
            },
            new()
            {
                Index = 14,
                Name = "Male Child NPCs",
                ModmetaName = "NPC_MALE_CHILD",
                Models = new[]
                {
                    "character/uc/common/model_000/uc00_000.gmdl",
                    "character/uc/common/model_000/uc00_000_common.gmdl",
                    "character/uc/uc00/model_000/uc00_000.gmdl",
                    "character/uc/uc00/model_010/uc00_010.gmdl",
                    "character/uc/uc04/model_100/uc04_100.gmdl",
                    "character/uc/uc05/model_100/uc05_100.gmdl",
                    "character/uc/uc05/model_600/uc05_600.gmdl",
                    "character/uc/uc09/model_000/uc09_000.gmdl",
                    "character/uc/uc09/model_100/uc09_100.gmdl",
                    "character/uc/uc09/model_101/uc09_101.gmdl",
                    "character/nh/nh28/model_000/nh28_000.gmdl",
                    "character/nh/nh28/model_010/nh28_010.gmdl"
                }
            },
            new()
            {
                Index = 15,
                Name = "Female Adult NPCs",
                ModmetaName = "NPC_FEMALE",
                Models = new[]
                {
                    "character/uw/common/model_000/uw00_000.gmdl",
                    "character/uw/common/model_000/uw00_000_common.gmdl",
                    "character/uw/uw00/model_000/uw00_000.gmdl",
                    "character/uw/uw00/model_001/uw00_001.gmdl",
                    "character/uw/uw00/model_010/uw00_010.gmdl",
                    "character/uw/uw00/model_011/uw00_011.gmdl",
                    "character/uw/uw00/model_020/uw00_020.gmdl",
                    "character/uw/uw00/model_100/uw00_100.gmdl",
                    "character/uw/uw00/model_101/uw00_101.gmdl",
                    "character/uw/uw00/model_110/uw00_110.gmdl",
                    "character/uw/uw02/model_001/uw02_001.gmdl",
                    "character/uw/uw04/model_100/uw04_100.gmdl",
                    "character/uw/uw04/model_101/uw04_101.gmdl",
                    "character/uw/uw04/model_105/uw04_105.gmdl",
                    "character/uw/uw04/model_200/uw04_200.gmdl",
                    "character/uw/uw04/model_201/uw04_201.gmdl",
                    "character/uw/uw04/model_400/uw04_400.gmdl",
                    "character/uw/uw04/model_401/uw04_401.gmdl",
                    "character/uw/uw04/model_402/uw04_402.gmdl",
                    "character/uw/uw04/model_405/uw04_405.gmdl",
                    "character/uw/uw05/model_100/uw05_100.gmdl",
                    "character/uw/uw05/model_101/uw05_101.gmdl",
                    "character/uw/uw05/model_102/uw05_102.gmdl",
                    "character/uw/uw05/model_103/uw05_103.gmdl",
                    "character/uw/uw05/model_110/uw05_110.gmdl",
                    "character/uw/uw05/model_120/uw05_120.gmdl",
                    "character/uw/uw05/model_170/uw05_170.gmdl",
                    "character/uw/uw05/model_200/uw05_200.gmdl",
                    "character/uw/uw05/model_400/uw05_400.gmdl",
                    "character/uw/uw05/model_401/uw05_401.gmdl",
                    "character/uw/uw05/model_410/uw05_410.gmdl",
                    "character/uw/uw05/model_601/uw05_601.gmdl",
                    "character/uw/uw05/model_602/um05_602.gmdl",
                    "character/uw/uw14/model_100/uw14_100.gmdl",
                    "character/uw/uw14/model_120/uw14_120.gmdl",
                    "character/uw/uw14/model_400/uw14_400.gmdl",
                    "character/uw/uw14/model_401/uw14_401.gmdl",
                    "character/uw/uw20/model_160/uw20_160.gmdl",
                    "character/uw/uw30/model_100/uw30_100.gmdl",
                    "character/uw/uw30/model_400/uw30_400.gmdl",
                    "character/uy/common/model_000/uy00_000.gmdl",
                    "character/uy/uy00/model_000/uy00_000.gmdl",
                    "character/uy/uy02/model_001/uy02_001.gmdl",
                    "character/uy/uy02/model_002/uy02_002.gmdl",
                    "character/uy/uy05/model_100/uy05_100.gmdl",
                    "character/uy/uy05/model_101/uy05_101.gmdl",
                    "character/uy/uy05/model_110/uy05_110.gmdl",
                    "character/uy/uy09/model_100/uy09_100.gmdl",
                    "character/uy/uy09/model_110/uy09_110.gmdl",
                    "character/uy/uy09/model_130/uy09_130.gmdl",
                    "character/nh/nh24/model_000/nh24_000.gmdl",
                    "character/nh/nh36/model_000/nh36_000.gmdl"
                }
            },
            new()
            {
                Index = 16,
                Name = "Female Child NPCs",
                ModmetaName = "NPC_FEMALE_CHILD",
                Models = new[]
                {
                    "character/uc/uc00/model_001/uc00_001.gmdl",
                    "character/uc/uc00/model_011/uc00_011.gmdl",
                    "character/uc/uc04/model_400/uc04_400.gmdl",
                    "character/uc/uc05/model_400/uc05_400.gmdl",
                    "character/uc/uc05/model_900/uc05_900.gmdl",
                    "character/uc/uc09/model_001/uc09_001.gmdl",
                    "character/uc/uc09/model_400/uc09_400.gmdl",
                    "character/uc/uc09/model_401/uc09_401.gmdl",
                    "character/uc/uc30/model_100/uc30_100.gmdl"
                }
            },
            new()
            {
                Index = 17,
                Name = "Sword",
                ModmetaName = "Sword",
                Models = new[]
                {
                    "character/we/we01/model_000/we01_000.gmdl",
                    "character/we/we01/model_001/we01_001.gmdl",
                    "character/we/we01/model_100/we01_100.gmdl",
                    "character/we/we01/model_200/we01_200.gmdl",
                    "character/we/we01/model_201/we01_201.gmdl",
                    "character/we/we01/model_300/we01_300.gmdl",
                    "character/we/we01/model_400/we01_400.gmdl",
                    "character/we/we20/model_000/we20_000.gmdl",
                    "character/we/we20/model_001/we20_001.gmdl",
                    "character/we/we32/model_000/we32_000.gmdl",
                    "character/we/we52/model_000/we52_000.gmdl",
                    "character/we/we52/model_001/we52_001.gmdl",
                    "character/we/we56/model_000/we56_000.gmdl",
                    "character/we/we68/model_000/we68_000.gmdl"
                }
            },
            new()
            {
                Index = 18,
                Name = "Greatsword",
                ModmetaName = "LargeSword",
                Models = new[]
                {
                    "character/we/we02/model_000/we02_000.gmdl",
                    "character/we/we02/model_001/we02_001.gmdl",
                    "character/we/we02/model_100/we02_100.gmdl",
                    "character/we/we02/model_200/we02_200.gmdl",
                    "character/we/we02/model_300/we02_300.gmdl",
                    "character/we/we02/model_301/we02_301.gmdl",
                    "character/we/we02/model_400/we02_400.gmdl",
                    "character/we/we50/model_000/we50_000.gmdl",
                    "character/xg/xg18/model_000/xg18_000.gmdl"
                }
            },
            new()
            {
                Index = 19,
                Name = "Spear",
                ModmetaName = "Spear",
                Models = new[]
                {
                    "character/we/we03/model_000/we03_000.gmdl",
                    "character/we/we03/model_100/we03_100.gmdl",
                    "character/we/we03/model_200/we03_200.gmdl",
                    "character/we/we03/model_300/we03_300.gmdl",
                    "character/we/we03/model_400/we03_400.gmdl",
                    "character/we/we03/model_500/we03_500.gmdl",
                    "character/we/we53/model_000/we53_000.gmdl"
                }
            },
            new()
            {
                Index = 20,
                Name = "Dagger",
                ModmetaName = "Dagger",
                Models = new[]
                {
                    "character/we/we04/model_000/we04_000.gmdl",
                    "character/we/we04/model_100/we04_100.gmdl",
                    "character/we/we04/model_200/we04_200.gmdl",
                    "character/we/we04/model_210/we04_210.gmdl",
                    "character/we/we04/model_300/we04_300.gmdl",
                    "character/we/we04/model_400/we04_400.gmdl",
                    "character/we/we04/model_500/we04_500.gmdl",
                    "character/we/we04/model_510/we04_510.gmdl",
                    "character/we/we04/model_520/we04_520.gmdl",
                    "character/we/we04/model_600/we04_600.gmdl",
                    "character/we/we04/model_601/we04_601.gmdl",
                    "character/we/we04/model_602/we04_602.gmdl",
                    "character/we/we04/model_700/we04_700.gmdl",
                    "character/we/we51/model_000/we51_000.gmdl",
                    "character/we/we59/model_000/we59_000.gmdl",
                    "character/we/we59/model_500/we59_500.gmdl",
                    "character/we/we54/model_000/we54_000.gmdl",
                    "character/we/we54/model_001/we54_001.gmdl"
                }
            },
            new()
            {
                Index = 21,
                Name = "Gun",
                ModmetaName = "Gun",
                Models = new[]
                {
                    "character/we/we05/model_000/we05_000.gmdl",
                    "character/we/we05/model_100/we05_100.gmdl",
                    "character/we/we05/model_200/we05_200.gmdl",
                    "character/we/we05/model_300/we05_300.gmdl",
                    "character/we/we05/model_400/we05_400.gmdl",
                    "character/we/we05/model_600/we05_600.gmdl",
                    "character/we/we05/model_700/we05_700.gmdl"
                }
            },
            new()
            {
                Index = 22,
                Name = "Shield",
                ModmetaName = "Shield",
                Models = new[]
                {
                    "character/we/we06/model_000/we06_000.gmdl",
                    "character/we/we06/model_100/we06_100.gmdl",
                    "character/we/we06/model_200/we06_200.gmdl",
                    "character/we/we06/model_300/we06_300.gmdl",
                    "character/we/we06/model_400/we06_400.gmdl",
                    "character/we/we06/model_500/we06_500.gmdl"
                }
            },
            new()
            {
                Index = 23,
                Name = "Crossbow",
                ModmetaName = "Crossbow",
                Models = new[]
                {
                    "character/we/we22/model_000/we22_000.gmdl",
                    "character/we/we22/model_001/we22_001.gmdl",
                    "character/we/we22/model_100/we22_100.gmdl",
                    "character/we/we22/model_101/we22_101.gmdl",
                    "character/we/we22/model_110/we22_110.gmdl",
                    "character/we/we22/model_111/we22_111.gmdl"
                }
            },
            new()
            {
                Index = 24,
                Name = "Shuriken",
                ModmetaName = "Shuriken",
                Models = new[]
                {
                    "character/we/we25/model_000/we25_000.gmdl",
                    "character/we/we25/model_001/we25_001.gmdl",
                    "character/we/we25/model_100/we25_100.gmdl",
                    "character/we/we25/model_101/we25_101.gmdl",
                    "character/we/we25/model_110/we25_110.gmdl",
                    "character/we/we25/model_111/we25_111.gmdl"
                }
            },
            new()
            {
                Index = 25,
                Name = "Mace",
                ModmetaName = "Mace",
                Models = new[]
                {
                    "character/we/we28/model_000/we28_000.gmdl",
                    "character/we/we28/model_001/we28_001.gmdl",
                    "character/we/we28/model_500/we28_500.gmdl",
                    "character/we/we28/model_100/we28_100.gmdl",
                    "character/we/we28/model_101/we28_101.gmdl",
                    "character/we/we28/model_110/we28_110.gmdl",
                    "character/we/we28/model_111/we28_111.gmdl",
                    "character/we/we57/model_000/we57_000.gmdl",
                    "character/we/we67/model_000/we67_000.gmdl",
                    "character/we/we67/model_001/we67_001.gmdl",
                    "character/we/we67/model_002/we67_002.gmdl"
                }
            },
            new()
            {
                Index = 26,
                Name = "Katana",
                ModmetaName = "Katana",
                Models = new[]
                {
                    "character/we/we31/model_000/we31_000.gmdl",
                    "character/we/we31/model_001/we31_001.gmdl",
                    "character/we/we31/model_101/we31_101.gmdl",
                    "character/we/we31/model_200/we31_200.gmdl",
                    "character/we/we31/model_201/we31_201.gmdl",
                    "character/we/we31/model_210/we31_210.gmdl",
                    "character/we/we31/model_211/we31_211.gmdl"
                }
            },
            new()
            {
                Index = 27,
                Name = "Magitek Suit (Noctis)",
                ModmetaName = "Magitek_Noctis",
                Models = new[]
                {
                    "character/nh/nh00/model_092/nh00_092.gmdl"
                }
            },
            new()
            {
                Index = 28,
                Name = "Magitek Suit (Prompto)",
                ModmetaName = "Magitek_Prompto",
                Models = new[]
                {
                    "character/nh/nh02/model_092/nh02_092.gmdl"
                }
            },
            new()
            {
                Index = 29,
                Name = "Magitek Suit (Ignis)",
                ModmetaName = "Magitek_Ignis",
                Models = new[]
                {
                    "character/nh/nh03/model_092/nh03_092.gmdl"
                }
            },
            new()
            {
                Index = 30,
                Name = "Magitek Suit (Gladiolus)",
                ModmetaName = "Magitek_Gladiolus",
                Models = new[]
                {
                    "character/nh/nh01/model_092/nh01_092.gmdl"
                }
            }
        };
    }
}