using System;
using System.Linq;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;

// var types = typeof(JsonSerializer).Assembly.GetTypes();
// var type = types.FirstOrDefault(p => p.Name.Contains("JsonConstants"));
// foreach (var property in type.GetField(""))
// {
//     Console.WriteLine($"{property.Name}");
// }

using var context = new FlagrumDbContext(new SettingsService());
var data = context.GetFileByUri("data://environment/world/heightmaps/material/terrainmaterial.tpd");
var tpd = TerrainPaletteData.Read(data);

var toleranceRange = 1.0 / tpd.Count / 2.0;

Console.WriteLine("Texture\t\tValue\t\tEpsilon");
Console.WriteLine("-----------------------------------------");

foreach (var item in tpd.Items.OrderBy(i => i.TextureIndex))
{
    //Console.WriteLine($"{item.TextureIndex}\t\t{item.Value:0.00000}\t\t{item.Epsilon:0.00000}");
    Console.WriteLine($"({item.TextureIndex}, {item.Value}, {item.Epsilon}),");
}

Console.WriteLine(tpd.Items.Sum(i => i.Epsilon));

// foreach (var item in tpd.Items.OrderBy(i => i.TextureIndex))
// {
//     Console.WriteLine($"{item.TextureIndex}\t\t{(item.ArrayIndex / 31.0).ToString("0.00000")}\t\t{(toleranceRange / (item.MaybeToleranceDivisor == 0.0f ? 2 : item.MaybeToleranceDivisor)).ToString("0.00000")}");
// }
//
// Console.WriteLine(tpd.Items.Sum(i => toleranceRange / (i.MaybeToleranceDivisor == 0 ? 2 : i.MaybeToleranceDivisor)));


// using var context = new FlagrumDbContext(new SettingsService());
// var data = context.GetFileByUri(
//     "data://environment/world/sourceimages/terrainarraytex_displacement/terrainarraytex_00_h.png");
// data = BtexConverter.BtexToDds(data);
//
// var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
// var pointer = pinnedData.AddrOfPinnedObject();
//
// var image = TexHelper.Instance.LoadFromDDSMemory(pointer, data.Length, DDS_FLAGS.NONE);
//
// pinnedData.Free();
//
// for (var i = 0; i < 10000; i += 11)
// {
//     var result = image.Decompress(i, DXGI_FORMAT.R8G8B8A8_UNORM);
//     using var stream = new MemoryStream();
//     using var ddsStream =
//         result.SaveToWICMemory(0, WIC_FLAGS.FORCE_SRGB, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
//     ddsStream.CopyTo(stream);
//     File.WriteAllBytes($@"C:\Modding\HebTest\TerrainProject\all_displacement_textures\{i / 11}.png", stream.ToArray());
// }