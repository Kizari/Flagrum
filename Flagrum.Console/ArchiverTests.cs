using Flagrum.Archiver;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Flagrum.Console
{
    public class ArchiverTests
    {
        public static void CreateBinMod()
        {
            var root = "C:\\Testing\\Archiver\\mod_folder";
            var shaderMetadata = "C:\\Testing\\Archiver\\shaders.json";
            var outputPath = "C:\\Testing\\Archiver\\output.ffxvbinmod";

            var archive = new Archive(root);
            AddFilesRecursively(archive, root);

            var shaders = JsonConvert.DeserializeObject<List<ShaderData>>(File.ReadAllText(shaderMetadata));
            foreach (var shader in shaders)
            {
                archive.AddFile(shader.Path);
            }

            archive.AddFile("data://$mod/temp.ebex");

            var fileStream = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            var archiveStream = archive.Pack();
            archiveStream.CopyTo(fileStream);
            fileStream.Close();
        }

        private static void AddFilesRecursively(Archive archive, string dir)
        {
            foreach (var directory in Directory.EnumerateDirectories(dir))
            {
                AddFilesRecursively(archive, directory);
            }

            foreach (var file in Directory.EnumerateFiles(dir))
            {
                var shouldAdd = true;
                var extensions = new string[] { ".clsmk", ".clsmk.dep", ".clsx" };
                foreach (var extension in extensions)
                {
                    if (file.EndsWith(extension))
                    {
                        shouldAdd = false;
                    }
                }

                if (!shouldAdd) continue;

                archive.AddFile(file);
            }
        }
    }

    public class ShaderData
    {
        public string Path { get; set; }
        public uint Size { get; set; }
        public uint ProcessedSize { get; set; }
        public uint DataStart { get; set; }
    }
}
