// using System.IO;
// using Flagrum.Core.Animation;
// using Flagrum.Core.Gfxbin.Gmdl;
// using Flagrum.Web.Persistence;
// using Flagrum.Web.Persistence.Entities;
// using Flagrum.Web.Services;
//
// namespace Flagrum.Console;
//
// public class HeightFieldEntity
// {
//     public string Name { get; set; }
//     public float[] Position { get; set; } = new float[3];
//     public int Width { get; set; }
//     public int Height { get; set; }
//     public float[] Pixels { get; set; }
// }
//
// public class Program2
// {
//     public static void Main(string[] args)
//     {
//         // using var context = new FlagrumDbContext(new SettingsService());
//         // var data = context.GetFileByUri("data://environment/common/props/co_pr_GSobj4/models/co_pr_GSobj4_objC.gmdl");
//         // var data2 = context.GetFileByUri("data://environment/common/props/co_pr_GSobj4/models/co_pr_GSobj4_objC.gpubin");
//         // var model = new ModelReader(data, data2).Read();
//         // bool x = true;
//
//         // // File path to the PKA you want to edit
//         // const string pkaFile = @"C:\Somewhere\base_move.pka";
//         //
//         // // File path where you want to save the modified PKA
//         // const string outputPath = @"C:\Somewhere\base_move_modified.pka";
//         //
//         // // Load the PKA into memory
//         // var data = File.ReadAllBytes(pkaFile);
//         // var pack = AnimationPackage.FromData(data);
//         //
//         // // Swap out the animation(s)
//         // pack.ReplaceAnimation(0, @"C:\Somewhere\new_animation_0.ani");
//         // pack.ReplaceAnimation(1, @"C:\Somewhere\new_animation_1.ani");
//         // pack.ReplaceAnimation(2, @"C:\Somewhere\new_animation_2.ani");
//         //
//         // // Repack the PKA
//         // var outputData = AnimationPackage.ToData(pack);
//         // File.WriteAllBytes(outputPath, outputData);
//
//         // using var context = new FlagrumDbContext(new SettingsService());
//         // var data = context.GetFileByUri("data://character/nh/nh00/anim/pack/base_move.pka");
//         // var pack = AnimationPackage.FromData(data);
//         // var bytes = AnimationPackage.ToData(pack);
//         // File.WriteAllBytes(@"C:\Modding\HebTest\base_move_test.pka", bytes);
//         // //var material = new MaterialReader(data).Read();
//         // bool x = true;
//
//         // using var context = new FlagrumDbContext(new SettingsService());
//         // var data = context.GetFileByUri("data://environment/world/sourceimages/terrainarraytex_00_b.tif");
//         // data = BtexConverter.BtexToDds(data);
//         // File.WriteAllBytes(@"C:\Modding\HebTest\test.dds", data);
//         //
//         // var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
//         // var pointer = pinnedData.AddrOfPinnedObject();
//         //
//         // var image = TexHelper.Instance.LoadFromDDSMemory(pointer, ((byte[])data).Length, DDS_FLAGS.NONE);
//         //
//         // pinnedData.Free();
//         //
//         // for (var i = 0; i < 10000; i++)
//         // {
//         //     var result = image.Decompress(i, DXGI_FORMAT.R8G8B8A8_UNORM);
//         //     using var stream = new MemoryStream();
//         //     using var ddsStream =
//         //         result.SaveToWICMemory(0, WIC_FLAGS.FORCE_SRGB, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
//         //     ddsStream.CopyTo(stream);
//         //     File.WriteAllBytes($@"C:\Modding\HebTest\terrain\{i}.png", stream.ToArray());
//         // }
//
//
//         // var results = context.AssetUris
//         //     .Where(a => a.Uri.StartsWith("data://level"))
//         //     .ToList()
//         //     .Select(a => a.Uri[(a.Uri.LastIndexOf('.') + 1)..])
//         //     .Distinct();
//         // foreach (var extension in results)
//         // {
//         //     System.Console.WriteLine(extension);
//         // }
//
//         // var data = context.GetFileByUri("data://environment/world/models/terrainmaterial_highspec.gmdl");
//         // var data2 = context.GetFileByUri("data://environment/world/models/terrainmaterial_highspec.gpubin");
//         // var model = new ModelReader(data, data2).Read();
//         // bool x = true;
//
//         //  var xmb2 = File.ReadAllBytes(@"C:\Modding\HebTest\area_leide_h.ebex");
//         //  using var stream = new MemoryStream(xmb2);
//         //  var package = Xmb2Document.GetRootElement(stream);
//         //  var objects = package.GetElementByName("objects");
//         //
//         //  var tiles = new List<HeightFieldEntity>();
//         //  foreach (var element in objects.GetElements())
//         //  {
//         //      var type = element.GetAttributeByName("type").GetTextValue();
//         //      if (type == "Black.Entity.Actor.HeightFieldEntity")
//         //      {
//         //          var name = element.GetAttributeByName("name").GetTextValue();
//         //          var path =
//         //              $@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\environment\world\heightmaps\lod00\{name}.heb";
//         //
//         //          if (!File.Exists(path))
//         //          {
//         //              continue;
//         //          }
//         //
//         //          var heb = new HebReader(path).Read();
//         //          foreach (var (extension, data) in BtexConverter.HebToDds(heb))
//         //          {
//         //              if (extension == "json")
//         //              {
//         //                  var position = element.GetElementByName("position_").GetFloat4Value();
//         //                  var (width, height, buffer) = ((uint, uint, float[]))data;
//         //                  tiles.Add(new HeightFieldEntity
//         //                  {
//         //                      Name = name,
//         //                      Position = new[] {position[0], position[1], position[2]},
//         //                      Width = (int)width,
//         //                      Height = (int)height,
//         //                      Pixels = buffer
//         //                  });
//         //              }
//         //          }
//         //      }
//         //  }
//         // File.WriteAllText(@"C:\Modding\HebTest\Leide.json", JsonConvert.SerializeObject(tiles));
//
//         // var reader =
//         //     new HebReader(
//         //         @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\environment\world\heightmaps\lod00\height_x40_y30.backup");
//         // var heb = reader.Read();
//         // //heb.ImageHeaders.RemoveAt(2);
//         // var image = TexHelper.Instance.LoadFromTGAFile(@"C:\Modding\HebTest\x40_y30\merged_mask_map.tga");
//         // using var stream = new MemoryStream();
//         // using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.NONE);
//         // ddsStream.CopyTo(stream);
//         // var dds = stream.ToArray();
//         // var newData = new byte[dds.Length - 128];
//         // Array.Copy(dds, 128, newData, 0, newData.Length);
//         // heb.ImageHeaders[2].DdsData = newData;
//         // var writer = new HebWriter();
//         // var result = writer.Write(heb);
//         // File.WriteAllBytes(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\environment\world\heightmaps\lod00\height_x40_y30.heb", result);
//
//
//         // var counter = 0;
//         // foreach (var (extension, data) in BtexConverter.HebToDds(heb))
//         // {
//         //     System.Console.WriteLine(counter);
//         //
//         //     if (extension == "dds")
//         //     {
//         //         var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
//         //         var pointer = pinnedData.AddrOfPinnedObject();
//         //
//         //         var image = TexHelper.Instance.LoadFromDDSMemory(pointer, ((byte[])data).Length, DDS_FLAGS.NONE);
//         //         //image = image.Convert(DXGI_FORMAT.R32G32B32A32_FLOAT, TEX_FILTER_FLAGS.CUBIC, 0.5f);
//         //
//         //         pinnedData.Free();
//         //
//         //         var metadata = image.GetMetadata();
//         //         if (metadata.Format != DXGI_FORMAT.R8G8B8A8_UNORM)
//         //         {
//         //             image = image.Decompress(DXGI_FORMAT.R8G8B8A8_UNORM);
//         //         }
//         //
//         //         using var stream = new MemoryStream();
//         //         //using var ddsStream =
//         //         //    image.SaveToWICMemory(0, WIC_FLAGS.FORCE_SRGB, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
//         //         //using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.NONE);
//         //         //using var ddsStream = image.SaveToHDRMemory(0);
//         //         using var ddsStream = image.SaveToTGAMemory(0);
//         //         ddsStream.CopyTo(stream);
//         //         File.WriteAllBytes(@$"C:\Modding\HebTest\x40_y30\{counter}.tga", stream.ToArray());
//         //     }
//         //
//         //     counter++;
//         // }
//

//
//         // var gfx = @"C:\Users\Kieran\Desktop\Environments\Altissia\al_ar_castle01_typ07c.gmdl.gfxbin";
//         // var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");
//         // var model = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu)).Read();
//         //
//         // foreach (var meshObject in model.MeshObjects)
//         // {
//         //     foreach (var mesh in meshObject.Meshes)
//         //     {
//         //         foreach (var uvMap in mesh.UVMaps)
//         //         {
//         //             foreach (var coord in uvMap.UVs)
//         //             {
//         //                 if (coord.U == Half.NaN || coord.U == Half.NegativeInfinity || coord.U == Half.PositiveInfinity
//         //                     || coord.V == Half.NaN || coord.V == Half.NegativeInfinity ||
//         //                     coord.V == Half.PositiveInfinity)
//         //                 {
//         //                     System.Console.WriteLine($"{meshObject.Name}, {mesh.Name} - U: {coord.U}, V: {coord.V}");
//         //                 }
//         //             }
//         //         }
//         //     }
//         // }
//         //
//         // var x = true;
//
//         // var finder = new FileFinder();
//         // finder.FindByQuery(
//         //     file => file.Uri.EndsWith(".amdl"),
//         //     file => System.Console.WriteLine($"{file.Uri.Split('/').Last()}\t\t{file.Uri}")
//         // );
//
//         // var input = @"C:\Modding\teal.png";
//         // var output = @"C:\Modding\teal.btex";
//         // var converter = new TextureConverter();
//         // var btex = converter.ToBtex("teal", "png", TextureType.Mrs, File.ReadAllBytes(input));
//         // File.WriteAllBytes(output, btex);
//         // var gfx = File.ReadAllBytes(@"C:\Users\Kieran\Desktop\Models2\le_ar_gqshop1\le_ar_gqshop1.gmdl.gfxbin");
//         // var gpu = File.ReadAllBytes(@"C:\Users\Kieran\Desktop\Models2\le_ar_gqshop1\le_ar_gqshop1.gpubin");
//         // var model = new ModelReader(gfx, gpu).Read();
//         // var x = true;
//         //Visit(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas");
//         // var json = File.ReadAllText(@"C:\Modding\map3.json");
//         // var map = JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<string>>>(json);
//         //
//         // var start = DateTime.Now;
//         //
//         // var root = new NamedTreeNode("data://", null);
//         // foreach (var (archive, uris) in map)
//         // {
//         //     foreach (var uri in uris)
//         //     {
//         //         var tokens = uri.Split('/');
//         //         var currentDirectory = root;
//         //         foreach (var token in tokens)
//         //         {
//         //             var subdirectory = currentDirectory.Children
//         //                 .FirstOrDefault(c => c.Name == token) ?? currentDirectory.AddChild(token);
//         //
//         //             currentDirectory = subdirectory;
//         //         }
//         //     }
//         // }
//         //
//         // var serializer = new DataContractSerializer(typeof(NamedTreeNode));
//         // using var fileStream = new FileStream(@"C:\Modding\Tree.xml", FileMode.Create, FileAccess.Write);
//         // serializer.WriteObject(fileStream, root);
//         //
//         // //File.WriteAllText(@"C:\Modding\UriDirectoryMap.json", JsonConvert.SerializeObject(root));
//         // System.Console.WriteLine((DateTime.Now - start).TotalMilliseconds);
//     }
//
//     private static void Visit(string directory)
//     {
//         foreach (var file in Directory.EnumerateFiles(directory))
//         {
//             if (!file.EndsWith(".earc") && !file.EndsWith(".heb") && !file.EndsWith(".hephysx") &&
//                 !file.EndsWith(".bk2") && !file.EndsWith(".sab"))
//             {
//                 System.Console.WriteLine(file);
//             }
//         }
//
//         foreach (var subdirectory in Directory.EnumerateDirectories(directory))
//         {
//             Visit(subdirectory);
//         }
//     }
// }

