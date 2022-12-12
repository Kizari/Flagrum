using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Console.Scripts.Archive;
using Flagrum.Console.Scripts.Terrain;
using Flagrum.Console.Utilities;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Web.Persistence;
using Flagrum.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;

//IndexingScripts.DumpUniqueRelativeExtensions();

// var path = @"C:\Users\Kieran\Downloads\nh02_000_player_vr.exml";
// var output = new StringBuilder();
// Xmb2Document.Dump(File.ReadAllBytes(path), output);
// File.WriteAllText(path.Replace(".exml", ".xml"), output.ToString());

// x37_y36
// HebScripts.ReplaceMergedMaskMap(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\environment\world\heightmaps\lod00\height_x37_y36.heb",
//     @"C:\Modding\Wiz\Terrain\HebTextures\2_modified.tga");
//HebScripts.DumpImages(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\environment\world\heightmaps\lod00\height_x37_y36.heb", @"C:\Modding\Wiz\Terrain\HebTextures");
//TerrainPaletteScripts.DumpTerrainTextureTable("data://environment/world_train/heightmaps/material/terrainmaterial.tpd");
//TextureConversionScripts.DumpPngTextureArrayFromBtex("data://environment/world_train/sourceimages/terrainarraytex_xpec_00_b.tif", @"C:\Modding\Wiz\TextureDump");

// foreach (var file in Directory.EnumerateFiles(@"C:\Users\Kieran\Desktop\XMB2"))
// {
//     var builder = new StringBuilder();
//     Xmb2Document.Dump(File.ReadAllBytes(file), builder);
//     File.WriteAllText(file.Replace(".exml", ".xml"), builder.ToString());
// }

// var path = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\637650\1438266368\nh02_casgrey.ffxvbinmod";
// using var unpacker = new Unpacker(path);
// return;

// var hash = Cryptography.HashFileUri64(
//     "data://environment/duscae/props/du_ar_lhotel02/sourceimages/du_ar_lhotel02_part04_concrete_b.tif");
// Console.WriteLine(hash);
// return;

// var types = typeof(JsonSerializer).Assembly.GetTypes();
// var type = types.FirstOrDefault(p => p.Name.Contains("JsonConstants"));
// foreach (var property in type.GetField(""))
// {
//     Console.WriteLine($"{property.Name}");
// }

// var flags = ArchiveFileFlag.Autoload | ArchiveFileFlag.Compressed;
// Console.WriteLine((int)flags);
// return;


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