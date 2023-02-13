// using System.IO;
// using System.Runtime.InteropServices;
// using DirectXTexNet;
// using Flagrum.Core.Gfxbin.Btex;
// using Flagrum.Core.Utilities.Types;
// using Flagrum.Web.Persistence;
// using Flagrum.Web.Persistence.Entities;
// using Flagrum.Web.Services;
//
// namespace Flagrum.Console.Scripts.Texture;
//
// public static class TextureConversionScripts
// {
//     /// <summary>
//     /// Outputs all images in a btex texture array to PNG files
//     /// </summary>
//     /// <param name="btexArrayUri">The URI for the texture array btex file</param>
//     /// <param name="outputDirectoryPath">The absolute path to the directory to dump the PNG files in</param>
//     public static void DumpPngTextureArrayFromBtex(string btexArrayUri, string outputDirectoryPath)
//     {
//         using var context = new FlagrumDbContext(new ProfileService());
//         var data = context.GetFileByUri(btexArrayUri);
//         data = BtexConverter.BtexToDds(data, FlagrumProfileType.FFXV);
//
//         var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
//         var pointer = pinnedData.AddrOfPinnedObject();
//
//         var image = TexHelper.Instance.LoadFromDDSMemory(pointer, data.Length, DDS_FLAGS.NONE);
//
//         pinnedData.Free();
//
//         for (var i = 0; i < image.GetMetadata().ArraySize; i++)
//         {
//             var result = image.Decompress(i * 11, DXGI_FORMAT.R8G8B8A8_UNORM);
//             using var stream = new MemoryStream();
//             using var ddsStream =
//                 result.SaveToWICMemory(0, WIC_FLAGS.FORCE_SRGB, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
//             ddsStream.CopyTo(stream);
//
//             File.WriteAllBytes($@"{outputDirectoryPath}\{i}.png", stream.ToArray());
//         }
//     }
// }