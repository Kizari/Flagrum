// using System;
// using System.Diagnostics;
// using System.IO;
// using System.Linq;
// using DirectXTexNet;
// using Flagrum.Console.Utilities;
// using Flagrum.Core.Gfxbin.Btex;
// using Flagrum.Web.Services;
// using Microsoft.Extensions.Logging.Abstractions;
//
// namespace Flagrum.Console.Scripts.Terrain;
//
// public static class HebScripts
// {
//     /// <summary>
//     /// Dumps all images in a HEB file to the given directory in TGA format
//     /// </summary>
//     /// <param name="hebPath">File path of the HEB file to dump from</param>
//     /// <param name="outputDirectory">Path to the directory to dump the images in</param>
//     public static void DumpImages(string hebPath, string outputDirectory)
//     {
//         var heb = HebHeader.FromData(File.ReadAllBytes(hebPath));
//         var images = new TerrainPacker(NullLogger<TerrainPacker>.Instance, new ProfileService()).HebToImages(heb);
//
//         foreach (var imageBase in images)
//         {
//             if (imageBase is HebImageData image)
//             {
//                 File.WriteAllBytes($@"{outputDirectory}\{image.Index}.{image.Extension}", image.Data);
//             }
//         }
//     }
//     
//     /// <summary>
//     /// Replaces the merged mask map of a HEB file with a given replacement TGA file and repacks the HEB file in place
//     /// </summary>
//     /// <param name="hebPath">File path of the HEB file to edit</param>
//     /// <param name="replacementMergedMaskMapTgaPath">File path of the replacement merged mask map TGA</param>
//     public static void ReplaceMergedMaskMap(string hebPath, string replacementMergedMaskMapTgaPath)
//     {
//         var timer = Stopwatch.StartNew();
//         var backupPath = hebPath.Replace(".heb", ".backup");
//
//         Log.WriteLine($"Creating backup of HEB file at {backupPath}");
//         if (!File.Exists(backupPath))
//         {
//             File.Copy(hebPath, backupPath);
//         }
//
//         Log.WriteLine("Loading HEB data...");
//         var heb = HebHeader.FromData(File.ReadAllBytes(hebPath));
//         var mask = heb.ImageHeaders.FirstOrDefault(i => i.Type == HebImageType.TYPE_MERGED_MASK_MAP);
//
//         if (mask == null)
//         {
//             Log.WriteLine("ERROR: HEB file provided does not contain a merged mask map", ConsoleColor.Red);
//             return;
//         }
//
//         Log.WriteLine("Converting input file to DDS...");
//         var dds = new TextureConverter().TargaToDds(File.ReadAllBytes(replacementMergedMaskMapTgaPath),
//             mask.MipCount,
//             (DXGI_FORMAT)(int)BtexConverter.FormatMap[mask.TextureFormat]);
//
//         mask.DdsData = dds;
//
//         Log.WriteLine($"Repacking HEB and writing to {hebPath}");
//         File.WriteAllBytes(hebPath, HebHeader.ToData(heb));
//
//         timer.Stop();
//         Log.WriteLine($"ReplaceMergedMaskMap completed after {timer.ElapsedMilliseconds} millseconds");
//     }
// }