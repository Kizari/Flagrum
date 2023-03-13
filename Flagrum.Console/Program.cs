using System;
using System.IO;
using System.Linq;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Types;
using Flagrum.Web.Features.ModManager.Data;
using Flagrum.Web.Services;
using K4os.Compression.LZ4.Streams;

var profile = new ProfileService();
using var archive = new EbonyArchive(@$"{profile.GameDataDirectory}\level\dlc_ex\tera2\env\tb2_c_invisible - Copy.earc");
archive.RemoveFile("data://environment/world/curve/collision/area_tb2_malmalm_base_c.tcm");
archive.WriteToFile(@$"{profile.GameDataDirectory}\level\dlc_ex\tera2\env\tb2_c_invisible.earc", LuminousGame.FFXV);
return;

var fragment = new FmodFragment();
fragment.Read(@"C:\Users\Kieran\AppData\Local\Flagrum\earc\d52176d1-2d22-4311-be79-5ff8831976ed\2\2.ffg");

var stream = new MemoryStream(fragment.Data);
using var lz4Stream = LZ4Stream.Decode(stream);
using var destinationStream = new MemoryStream();
lz4Stream.CopyTo(destinationStream);
stream.Dispose();
var buffer = destinationStream.ToArray();

File.WriteAllBytes(@"C:\Users\Kieran\Downloads\test.btex", buffer);
return;

Console.WriteLine(((LuminousGame)75759744).HasFlag((LuminousGame)67108864));
return;

var uri = "data://asset/character/hu/hu000/model_000/sourceimages/tshirts/hu000_000_inner_shirts_1001_b_$m1.tif";
Console.WriteLine(Cryptography.HashFileUri64(uri));
return;

var converter = new TextureConverter(LuminousGame.Forspoken);
var btex = converter.ToBtex(new BtexBuildRequest
{
    Name = "Terada",
    PixelFormat = BtexFormat.BC1_UNORM,
    ImageFlags = 121,
    MipLevels = 0,
    SourceFormat = BtexSourceFormat.Wic,
    SourceData = File.ReadAllBytes(@"C:\Users\Kieran\Downloads\hu000_000_inner_shirts_1001_b_h_EvilTerada.png"),
    AddSedbHeader = false
});

File.WriteAllBytes(@"C:\Users\Kieran\Downloads\hu000_000_inner_shirts_1001_b_h_EvilTerada.btex", btex);
return;

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