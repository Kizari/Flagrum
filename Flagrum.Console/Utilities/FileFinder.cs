// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.IO;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using Flagrum.Core.Archive;
// using Flagrum.Core.Ebex.Xmb2;
// using Flagrum.Core.Gfxbin.Gmtl;
// using Newtonsoft.Json;
//
// namespace Flagrum.Console.Utilities;
//
// public class FileData
// {
//     public string Location { get; set; }
//     public string ArchiveLocation { get; set; }
//     public string Uri { get; set; }
//     public string FileName { get; set; }
// }
//
// public class FileFinder
// {
//     private const string DataDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas";
//
//     private ConcurrentBag<FileData> _map;
//
//     public void CheckFileTypes()
//     {
//         var types = new ConcurrentDictionary<string, bool>();
//         CheckFileTypesRecursively(DataDirectory, types);
//         foreach (var (type, _) in types)
//         {
//             System.Console.WriteLine(type);
//         }
//     }
//
//     private void CheckFileTypesRecursively(string directory, ConcurrentDictionary<string, bool> types)
//     {
//         foreach (var file in Directory.EnumerateFiles(directory))
//         {
//             var extension = file[file.LastIndexOf('.')..];
//             types.TryAdd(extension, true);
//
//             if (extension == ".pfp")
//             {
//                 System.Console.WriteLine(file);
//             }
//         }
//
//         Parallel.ForEach(Directory.EnumerateDirectories(directory), d => CheckFileTypesRecursively(d, types));
//     }
//
//     public void GenerateMap()
//     {
//         System.Console.WriteLine("Started mapping...");
//         var watch = Stopwatch.StartNew();
//
//         _map = new ConcurrentBag<FileData>();
//         MapDirectory(DataDirectory);
//         Parallel.ForEach(Directory.EnumerateDirectories(DataDirectory), GenerateMapRecursively);
//
//         watch.Stop();
//         System.Console.WriteLine($"Mapping finished after {watch.ElapsedMilliseconds} milliseconds.");
//
//         File.WriteAllText(@"C:\Modding\map.json", JsonConvert.SerializeObject(_map));
//     }
//
//     private void GenerateMapRecursively(string directory)
//     {
//         MapDirectory(directory);
//
//         foreach (var subdirectory in Directory.EnumerateDirectories(directory))
//         {
//             GenerateMapRecursively(subdirectory);
//         }
//     }
//
//     private void MapDirectory(string directory)
//     {
//         foreach (var file in Directory.EnumerateFiles(directory))
//         {
//             _map.Add(new FileData
//             {
//                 FileName = file.Split('\\').Last(),
//                 Location = file
//             });
//
//             if (file.EndsWith(".earc"))
//             {
//                 using var unpacker = new Unpacker(file);
//                 foreach (var archiveFile in unpacker.Files.Where(f => !f.Flags.HasFlag(ArchiveFileFlag.Reference)))
//                 {
//                     _map.Add(new FileData
//                     {
//                         ArchiveLocation = file,
//                         FileName = archiveFile.RelativePath.Split('\\', '/').Last(),
//                         Location = file + "\\" + archiveFile.RelativePath,
//                         Uri = archiveFile.Uri
//                     });
//                 }
//             }
//         }
//     }
//
//     public void FindByQuery(Func<ArchiveFile, bool> condition, Action<ArchiveFile> onMatch, bool unpackMatchedFiles)
//     {
//         System.Console.WriteLine("Starting search...");
//         var watch = Stopwatch.StartNew();
//
//         var startDirectory = DataDirectory;
//         Parallel.ForEach(Directory.EnumerateDirectories(startDirectory),
//             directory => { FindRecursively(directory, condition, onMatch, unpackMatchedFiles); });
//
//         watch.Stop();
//         System.Console.WriteLine($"Search finished after {watch.ElapsedMilliseconds} milliseconds.");
//     }
//
//     private void FindRecursively(string directory, Func<ArchiveFile, bool> condition, Action<ArchiveFile> onMatch,
//         bool unpackMatchedFiles)
//     {
//         foreach (var file in Directory.EnumerateFiles(directory, "*.earc"))
//         {
//             using var unpacker = new Unpacker(file);
//             foreach (var archiveFile in unpacker.Files.Where(condition))
//             {
//                 if (unpackMatchedFiles)
//                 {
//                     unpacker.ReadFileData(archiveFile);
//                 }
//
//                 onMatch(archiveFile);
//             }
//         }
//
//         Parallel.ForEach(Directory.EnumerateDirectories(directory),
//             subdirectory => { FindRecursively(subdirectory, condition, onMatch, unpackMatchedFiles); });
//     }
//     
//     public static List<string> GetUrisByString(string query, string extension)
//     {
//         var list = new List<string>();
//         var finder = new FileFinder();
//         finder.FindByQuery(
//             file => file.Uri.Contains(query, StringComparison.OrdinalIgnoreCase) && file.Uri.EndsWith("." + extension),
//             file => { list.Add(file.Uri); },
//             false);
//
//         return list;
//     }
//
//     public static void FindUriByString(string query)
//     {
//         var finder = new FileFinder();
//         finder.FindByQuery(
//             file => file.Uri.Contains(query, StringComparison.OrdinalIgnoreCase) && file.Uri.EndsWith(".btex"),
//             file => { System.Console.WriteLine(file.Uri); },
//             false);
//     }
//
//     public static void FindStringInExml(string query)
//     {
//         var finder = new FileFinder();
//         finder.FindByQuery(
//             file => file.Uri.EndsWith(".ebex") || file.Uri.EndsWith(".prefab"),
//             file =>
//             {
//                 var builder = new StringBuilder();
//                 Xmb2Document.Dump(file.GetReadableData(), builder);
//                 var text = builder.ToString();
//                 if (text.Contains(query, StringComparison.OrdinalIgnoreCase))
//                 {
//                     System.Console.WriteLine(file.Uri);
//                 }
//             },
//             true);
//     }
//
//     public static void FindStringInMaterialTextures(string query)
//     {
//         var finder = new FileFinder();
//         finder.FindByQuery(
//             file => file.Uri.EndsWith(".gmtl"),
//             file =>
//             {
//                 var reader = new MaterialReader(file.GetReadableData()).Read();
//                 if (reader.Textures.Any(t => t.Path.Contains(query)))
//                 {
//                     System.Console.WriteLine(file.Uri);
//                 }
//             },
//             true);
//     }
// }