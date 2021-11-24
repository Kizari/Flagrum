using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Flagrum.Archiver.Binmod
{
    public class BinmodBuilder
    {
        private readonly string _modName;
        private readonly Packer _packer;

        public BinmodBuilder(string name, string uuid)
        {
            _modName = name;
            _packer = new Packer();

            var exml = EntityPackage.BuildExml(name, "main");
            _packer.AddInMemoryFile(exml, $"data://$mod/temp.exml");
            
            var modmeta = Modmeta.Build(name, "", uuid, Modmeta.Boye.Noctis);
            _packer.AddInMemoryFile(modmeta, GetDataPath("index.modmeta"));
            
            var preview = File.ReadAllBytes($"{Directory.GetCurrentDirectory()}\\Binmod\\Resources\\preview.png");
            _packer.AddInMemoryFile(preview, GetDataPath("$preview.png.bin"));
            
            var previewBtex = File.ReadAllBytes($"{Directory.GetCurrentDirectory()}\\Binmod\\Resources\\preview.btex");
            _packer.AddInMemoryFile(previewBtex, GetDataPath("$preview.btex"));
        }

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
            return $"data://mod/{_modName}/{relativePath}";
        }
    }
}