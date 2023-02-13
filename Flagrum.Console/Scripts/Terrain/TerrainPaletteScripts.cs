using System.Linq;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;

namespace Flagrum.Console.Scripts.Terrain;

public static class TerrainPaletteScripts
{
    /// <summary>
    /// Outputs useful information from a TPD file to the console
    /// </summary>
    /// <param name="tpdUri">The URI of the TPD file to read out</param>
    public static void DumpTerrainTextureTable(string tpdUri)
    {
        using var context = new FlagrumDbContext(new ProfileService());
        var data = context.GetFileByUri(tpdUri);
        var tpd = TerrainPaletteData.Read(data);

        var toleranceRange = 1.0 / tpd.Count / 2.0;

        System.Console.WriteLine("Texture\t\tValue\t\tEpsilon");
        System.Console.WriteLine("-----------------------------------------");

        // foreach (var item in tpd.Items.OrderBy(i => i.TextureIndex))
        // {
        //     //Console.WriteLine($"{item.TextureIndex}\t\t{item.Value:0.00000}\t\t{item.Epsilon:0.00000}");
        //     System.Console.WriteLine($"({item.TextureIndex}, {item.Value}, {item.Epsilon}),");
        // }
        //
        // System.Console.WriteLine(tpd.Items.Sum(i => i.Epsilon));

        foreach (var item in tpd.Items.OrderBy(i => i.TextureIndex))
        {
            System.Console.WriteLine(
                $"{item.TextureIndex}\t\t{(item.ArrayIndex / 31.0).ToString("0.00000")}\t\t{(toleranceRange / (item.MaybeToleranceDivisor == 0.0f ? 2 : item.MaybeToleranceDivisor)).ToString("0.00000")}");
        }

        System.Console.WriteLine(tpd.Items.Sum(i =>
            toleranceRange / (i.MaybeToleranceDivisor == 0 ? 2 : i.MaybeToleranceDivisor)));
    }
}