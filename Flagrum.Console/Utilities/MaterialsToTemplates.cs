using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Gfxbin.Gmtl;
using Newtonsoft.Json;

namespace Flagrum.Console.Utilities;

public static class MaterialsToTemplates
{
    public static void Run()
    {
        var files = new List<MaterialFile>();
        const string directory = "C:\\Testing\\Gfxbin\\Gmtl\\mod_materials";
        const string outDirectory = "C:\\Testing\\Gfxbin\\Gmtl\\material_templates";

        foreach (var file in Directory.EnumerateFiles(directory))
        {
            files.Add(new MaterialFile
            {
                Id = int.Parse(file.Split('\\', '/').Last().Split('.')[0]),
                Name = file.Split('\\', '/').Last().Split('.')[1],
                Path = file
            });
        }

        foreach (var file in files)
        {
            var reader = new MaterialReader(file.Path);
            var material = reader.Read();
            var json = JsonConvert.SerializeObject(material);
            File.WriteAllText($"{outDirectory}\\{file.Name.ToUpper()}.json", json);
        }
    }

    private class MaterialFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}