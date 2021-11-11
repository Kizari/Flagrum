using Flagrum.Gfxbin.Materials;
using Newtonsoft.Json;
using System.IO;

namespace Flagrum.Console
{
    public static class GfxbinTests
    {
        public static void ImportThenExportMaterial()
        {
            var input = "C:\\Testing\\Gfxbin\\material.gmtl.gfxbin";
            var output = "C:\\Testing\\Gfxbin\\export.gmtl.gfxbin";

            var reader = new MaterialReader(input);
            var material = reader.Read();

            var writer = new MaterialWriter(material);
            var data = writer.Write();

            File.WriteAllBytes(output, data);
        }

        public static void ReadMaterialToJson()
        {
            var input = "C:\\Testing\\Gfxbin\\material.gmtl.gfxbin";
            var output = "C:\\Testing\\Gfxbin\\material.json";

            var reader = new MaterialReader(input);
            var material = reader.Read();

            File.WriteAllText(output, JsonConvert.SerializeObject(material, Formatting.Indented));
        }
    }
}
