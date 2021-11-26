using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Utilities;

namespace Flagrum.Archiver.Binmod
{
    public enum Boye
    {
        Noctis,
        Prompto,
        Ignis,
        Gladiolus
    }

    public class BinmodBuilder
    {
        private readonly Packer _packer;

        public BinmodBuilder(string modTitle, string modDirectoryName, string modelName, string uuid, Boye target)
        {
            ModTitle = modTitle;
            ModDirectoryName = modDirectoryName;
            ModelName = modelName;
            Uuid = uuid;
            Target = target;

            _packer = new Packer();

            var exml = EntityPackageBuilder.BuildExml(ModelName, ModDirectoryName, Target);
            _packer.AddInMemoryFile(exml, "data://$mod/temp.exml");

            var modmeta = BuildModmeta();
            _packer.AddInMemoryFile(modmeta, GetDataPath("index.modmeta"));

            var preview = File.ReadAllBytes($"{IOHelper.GetExecutingDirectory()}\\Binmod\\Resources\\preview.png");
            _packer.AddInMemoryFile(preview, GetDataPath("$preview.png.bin"));

            var previewBtex = File.ReadAllBytes($"{IOHelper.GetExecutingDirectory()}\\Binmod\\Resources\\preview.btex");
            _packer.AddInMemoryFile(previewBtex, GetDataPath("$preview.btex"));
        }

        public string ModTitle { get; }
        public string ModDirectoryName { get; }
        public string ModelName { get; }
        public string Uuid { get; }
        public Boye Target { get; }

        public void WriteToFile(string outPath)
        {
            _packer.WriteToFile(outPath);
        }

        public void AddMaterial(string name, byte[] material)
        {
            _packer.AddInMemoryFile(material, GetDataPath($"materials/{name}.gmtl.gfxbin"));
        }

        public void AddModel(string name, byte[] gfxbin, byte[] gpubin)
        {
            _packer.AddInMemoryFile(gfxbin, GetDataPath($"{name}.gmdl.gfxbin"));
            _packer.AddInMemoryFile(gpubin, GetDataPath($"{name}.gpubin"));
        }

        public void AddFile(string uri, byte[] data)
        {
            _packer.AddInMemoryFile(data, uri);
        }

        /// <summary>
        ///     Add a copy of a game asset to the archive
        ///     Asset will be read from the EARC and copied to the archive
        /// </summary>
        public void AddGameAssets(IEnumerable<string> paths)
        {
            var dataDirectory = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Final Fantasy XV\\datas\\";
            var archiveDictionary = new Dictionary<string, List<string>>();

            foreach (var uri in paths)
            {
                var path = uri.Replace("data://", dataDirectory).Replace('/', '\\');
                var fileName = path.Split('\\').Last();
                path = path.Replace(fileName, "autoexternal.earc");

                while (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    var newPath = "";
                    var tokens = path.Split('\\');
                    for (var i = 0; i < tokens.Length - 2; i++)
                    {
                        newPath += tokens[i];

                        if (i != tokens.Length - 3)
                        {
                            newPath += '\\';
                        }
                    }

                    path = newPath + "\\autoexternal.earc";
                }

                if (!archiveDictionary.ContainsKey(path))
                {
                    archiveDictionary.Add(path, new List<string>());
                }

                archiveDictionary[path].Add(uri);
            }

            foreach (var (archivePath, uriList) in archiveDictionary)
            {
                var unpacker = new Unpacker(archivePath);
                var files = unpacker.Unpack();

                foreach (var uri in uriList)
                {
                    var match = files.FirstOrDefault(f => f.Uri == uri);
                    if (match == null)
                    {
                        throw new InvalidOperationException($"URI {uri} must exist in game files!");
                    }

                    _packer.AddInMemoryFile(match.GetData(), uri);
                }
            }
        }

        private string GetDataPath(string relativePath)
        {
            return $"data://mod/{ModDirectoryName}/{relativePath}";
        }

        private byte[] BuildModmeta()
        {
            var builder = new StringBuilder();
            builder.AppendLine("[meta]");
            builder.AppendLine("modtype=cloth");
            builder.AppendLine($"title={ModTitle}");
            builder.AppendLine("desc=");
            builder.AppendLine($"uuid={Uuid}");
            builder.AppendLine($"type={Target.ToString().ToLower()}");
            builder.AppendLine("itemid=0");
            builder.AppendLine("ischecked=False");
            builder.AppendLine("isapplytogame=True");
            builder.AppendLine("itemplace=E_Local");
            builder.AppendLine($"modify_gmdl[0]=mod/{ModDirectoryName}/{ModelName}.fbx");
            builder.AppendLine($"name={ModTitle}");
            builder.AppendLine("help=");
            builder.AppendLine("strength=0");
            builder.AppendLine("vitality=0");
            builder.AppendLine("magic=0");
            builder.AppendLine("spirit=0");
            builder.AppendLine("hp_max=0");
            builder.AppendLine("mp_max=0");
            builder.AppendLine("bullet=0");
            builder.AppendLine("fire=0");
            builder.AppendLine("ice=0");
            builder.AppendLine("thunder=0");
            builder.AppendLine("dark=0");
            return Encoding.ASCII.GetBytes(builder.ToString());
        }
    }
}