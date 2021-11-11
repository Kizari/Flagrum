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
            var root = "C:\\Testing\\Archiver\\noctis_custom";
            var shaderMetadata = "C:\\Testing\\Archiver\\shaders.json";
            var outputPath = "C:\\Testing\\Archiver\\d090b917-d422-41ed-a641-0047de5fea48.ffxvbinmod";

            var packer = new Packer(root);
            packer.AddFile("data://$mod/temp.ebex");
            AddFilesRecursively(packer, root);

            var shaders = JsonConvert.DeserializeObject<List<ShaderData>>(File.ReadAllText(shaderMetadata));
            foreach (var shader in shaders)
            {
                packer.AddFile(shader.Path);
            }

            packer.WriteToFile(outputPath);
        }

        private static void AddFilesRecursively(Packer packer, string dir)
        {
            foreach (var directory in Directory.EnumerateDirectories(dir))
            {
                AddFilesRecursively(packer, directory);
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

                packer.AddFile(file);
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
