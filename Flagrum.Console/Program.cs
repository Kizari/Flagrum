using System.IO;
using Flagrum.Core.Animation.InverseKinematics;
using ProtoBuf;

using var file = File.OpenRead(@"C:\Users\Kieran\Desktop\Models2\nh00\nh00.lik");
var rig = Serializer.Deserialize<IKRig>(file);

using var file2 = File.OpenWrite(@"C:\Users\Kieran\Desktop\Models2\nh00\nh00_2.lik");
Serializer.Serialize(file2, rig);

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
// var data = context.GetFileByUri("data://environment/world/heightmaps/material/terrainmaterial.tpd");
// var tpd = TerrainPaletteData.Read(data);
//
// var toleranceRange = 1.0 / tpd.Count / 2.0;
//
// Console.WriteLine("Texture\t\tValue\t\tEpsilon");
// Console.WriteLine("-----------------------------------------");
//
// foreach (var item in tpd.Items.OrderBy(i => i.TextureIndex))
// {
//     //Console.WriteLine($"{item.TextureIndex}\t\t{item.Value:0.00000}\t\t{item.Epsilon:0.00000}");
//     Console.WriteLine($"({item.TextureIndex}, {item.Value}, {item.Epsilon}),");
// }
//
// Console.WriteLine(tpd.Items.Sum(i => i.Epsilon));

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