using System.IO;
using System.Text;

namespace Flagrum.Archiver.Binmod
{
    public static class EntityPackage
    {
        private static readonly byte[] _part1 = new byte[970]
        {
            88, 77, 66, 50, 48, 5, 0, 0, 0, 0, 0, 0, 184, 0, 0, 0, 0, 0, 0, 0, 108, 2, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0,
            112, 2, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 116, 2, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 120, 2, 0, 0, 0, 0, 0, 3, 4, 2,
            0, 0, 124, 2, 0, 0, 4, 0, 0, 0, 8, 2, 0, 0, 116, 2, 0, 0, 1, 0, 0, 5, 17, 1, 0, 0, 128, 2, 0, 0, 0, 0, 0, 1,
            244, 1, 0, 0, 124, 2, 0, 0, 1, 0, 0, 7, 74, 1, 0, 0, 104, 2, 0, 0, 0, 0, 0, 1, 224, 1, 0, 0, 132, 2, 0, 0,
            1, 0, 0, 7, 77, 1, 0, 0, 80, 2, 0, 0, 0, 0, 0, 1, 204, 1, 0, 0, 140, 2, 0, 0, 1, 0, 0, 7, 80, 1, 0, 0, 160,
            2, 0, 0, 0, 0, 0, 1, 184, 1, 0, 0, 156, 2, 0, 0, 1, 0, 0, 7, 176, 1, 0, 0, 176, 2, 0, 0, 5, 0, 0, 0, 184, 1,
            0, 0, 168, 2, 0, 0, 1, 0, 0, 2, 1, 154, 243, 129, 90, 167, 2, 0, 0, 4, 142, 229, 43, 12, 1, 0, 0, 0, 2, 154,
            243, 129, 90, 1, 0, 0, 0, 1, 186, 12, 198, 184, 150, 2, 0, 0, 4, 142, 229, 43, 12, 2, 0, 0, 0, 1, 186, 12,
            198, 184, 148, 2, 0, 0, 4, 142, 229, 43, 12, 3, 0, 0, 0, 1, 186, 12, 198, 184, 149, 2, 0, 0, 4, 142, 229,
            43, 12, 4, 0, 0, 0, 1, 186, 12, 198, 184, 154, 2, 0, 0, 1, 239, 160, 159, 47, 161, 2, 0, 0, 1, 186, 12, 198,
            184, 162, 2, 0, 0, 4, 142, 229, 43, 12, 0, 0, 0, 0, 2, 198, 78, 17, 33, 1, 0, 0, 0, 1, 77, 241, 39, 81, 154,
            2, 0, 0, 1, 54, 77, 135, 132, 187, 2, 0, 0, 1, 230, 189, 57, 141, 124, 2, 0, 0, 1, 0, 0, 0, 0, 180, 2, 0, 0,
            1, 65, 203, 252, 56, 161, 2, 0, 0, 1, 77, 241, 39, 81, 194, 2, 0, 0, 1, 227, 135, 240, 31, 71, 2, 0, 0, 1,
            77, 241, 39, 81, 189, 2, 0, 0, 1, 54, 77, 135, 132, 235, 1, 0, 0, 1, 230, 189, 57, 141, 165, 2, 0, 0, 4,
            176, 208, 50, 178, 0, 0, 0, 0, 1, 212, 76, 103, 245, 97, 2, 0, 0, 1, 0, 0, 0, 0, 178, 2, 0, 0, 1, 54, 77,
            135, 132, 206, 1, 0, 0, 1, 230, 189, 57, 141, 191, 2, 0, 0, 1, 0, 0, 0, 0, 191, 2, 0, 0, 1, 54, 77, 135,
            132, 198, 1, 0, 0, 1, 230, 189, 57, 141, 208, 2, 0, 0, 1, 0, 0, 0, 0, 224, 2, 0, 0, 1, 208, 22, 84, 20, 203,
            2, 0, 0, 1, 77, 241, 39, 81, 240, 2, 0, 0, 1, 54, 77, 135, 132, 176, 1, 0, 0, 1, 230, 189, 57, 141, 216, 2,
            0, 0, 1, 107, 32, 198, 168, 246, 2, 0, 0, 1, 251, 43, 59, 145, 245, 2, 0, 0, 1, 230, 189, 57, 141, 244, 2,
            0, 0, 2, 105, 79, 196, 250, 0, 0, 0, 0, 0, 0, 0, 204, 253, 255, 255, 212, 253, 255, 255, 220, 253, 255, 255,
            228, 253, 255, 255, 236, 253, 255, 255, 0, 254, 255, 255, 20, 254, 255, 255, 40, 254, 255, 255, 60, 254,
            255, 255, 228, 253, 255, 255, 248, 253, 255, 255, 12, 254, 255, 255, 32, 254, 255, 255, 52, 254, 255, 255,
            60, 254, 255, 255, 80, 254, 255, 255, 85, 254, 255, 255, 90, 254, 255, 255, 95, 254, 255, 255, 64, 254, 255,
            255, 96, 254, 255, 255, 74, 254, 255, 255, 97, 254, 255, 255, 48, 254, 255, 255, 98, 254, 255, 255, 58, 254,
            255, 255, 99, 254, 255, 255, 32, 254, 255, 255, 100, 254, 255, 255, 42, 254, 255, 255, 101, 254, 255, 255,
            106, 254, 255, 255, 111, 254, 255, 255, 116, 254, 255, 255, 121, 254, 255, 255, 126, 254, 255, 255, 131,
            254, 255, 255, 136, 254, 255, 255, 150, 254, 255, 255, 155, 254, 255, 255, 79, 254, 255, 255, 241, 253, 255,
            255, 152, 254, 255, 255, 157, 254, 255, 255, 162, 254, 255, 255, 167, 254, 255, 255, 172, 254, 255, 255,
            177, 254, 255, 255, 47, 254, 255, 255, 236, 253, 255, 255, 120, 254, 255, 255, 125, 254, 255, 255, 175, 254,
            255, 255, 180, 254, 255, 255, 140, 254, 255, 255, 145, 254, 255, 255, 15, 254, 255, 255, 222, 253, 255, 255,
            88, 254, 255, 255, 93, 254, 255, 255, 170, 254, 255, 255, 175, 254, 255, 255, 108, 254, 255, 255, 113, 254,
            255, 255, 181, 254, 255, 255, 51, 254, 255, 255, 231, 253, 255, 255, 200, 253, 255, 255, 48, 254, 255, 255,
            170, 254, 255, 255, 175, 254, 255, 255, 180, 254, 255, 255, 68, 254, 255, 255, 73, 254, 255, 255, 177, 254,
            255, 255, 182, 254, 255, 255, 187, 254, 255, 255, 192, 254, 255, 255, 114, 101, 102, 101, 114, 101, 110, 99,
            101, 0, 101, 110, 116, 105, 116, 105, 101, 115, 95, 46, 105, 110, 100, 101, 120, 0, 101, 110, 116, 105, 116,
            105, 101, 115, 95, 46, 36, 112, 114, 101, 118, 105, 101, 119, 0, 101, 110, 116, 105, 116, 105, 101, 115, 95,
            46, 36, 112, 114, 101, 118, 105, 101, 119, 46, 112, 110, 103, 0, 101, 110, 116, 105, 116, 105, 101, 115, 95,
            46
        };

        private static readonly byte[] _part2 = new byte[87]
        {
            0, 101, 110, 116, 105, 116, 105, 101, 115, 95, 0, 111, 98, 106, 101, 99, 116, 0, 77, 79, 68, 32, 80, 97, 99,
            107, 97, 103, 101, 0, 83, 81, 69, 88, 46, 69, 98, 111, 110, 121, 46, 70, 114, 97, 109, 101, 119, 111, 114,
            107, 46, 69, 110, 116, 105, 116, 121, 46, 69, 110, 116, 105, 116, 121, 80, 97, 99, 107, 97, 103, 101, 0, 0,
            102, 105, 108, 101, 80, 97, 116, 104, 95, 0, 109, 111, 100, 47
        };

        private static readonly byte[] _part3 = new byte[66]
        {
            47, 105, 110, 100, 101, 120, 46, 109, 111, 100, 109, 101, 116, 97, 0, 115, 116, 114, 105, 110, 103, 0, 105,
            110, 100, 101, 120, 0, 66, 108, 97, 99, 107, 46, 69, 110, 116, 105, 116, 121, 46, 68, 97, 116, 97, 46, 85,
            110, 107, 110, 111, 119, 110, 82, 101, 115, 111, 117, 114, 99, 101, 0, 109, 111, 100, 47
        };

        private static readonly byte[] _part4 = new byte[27]
        {
            47, 36, 112, 114, 101, 118, 105, 101, 119, 46, 112, 110, 103, 0, 36, 112, 114, 101, 118, 105, 101, 119, 0,
            109, 111, 100, 47
        };

        private static readonly byte[] _part5 = new byte[47]
        {
            47, 36, 112, 114, 101, 118, 105, 101, 119, 46, 112, 110, 103, 46, 98, 105, 110, 0, 36, 112, 114, 101, 118,
            105, 101, 119, 46, 112, 110, 103, 0, 115, 111, 117, 114, 99, 101, 80, 97, 116, 104, 95, 0, 109, 111, 100, 47
        };

        private static readonly byte[] _part6 = new byte[1] {47};
        private static readonly byte[] _part7 = new byte[1] {0};

        private static readonly byte[] _part8 = new byte[58]
        {
            0, 66, 108, 97, 99, 107, 46, 69, 110, 116, 105, 116, 121, 46, 83, 107, 101, 108, 101, 116, 97, 108, 77, 111,
            100, 101, 108, 69, 110, 116, 105, 116, 121, 0, 111, 98, 106, 101, 99, 116, 115, 0, 112, 97, 99, 107, 97,
            103, 101, 0, 70, 70, 88, 86, 77, 79, 68, 0
        };

        public static byte[] BuildExml(string modDirectory, string modelName)
        {
            var modelNameBytes = Encoding.ASCII.GetBytes(modelName);
            var modDirectoryBytes = Encoding.ASCII.GetBytes(modDirectory);
            var modelNameExtensionBytes = Encoding.ASCII.GetBytes($"{modelName}.fbx");

            using var stream = new MemoryStream();
            stream.Write(_part1);
            stream.Write(modelNameBytes);
            stream.Write(_part2);
            stream.Write(modDirectoryBytes);
            stream.Write(_part3);
            stream.Write(modDirectoryBytes);
            stream.Write(_part4);
            stream.Write(modDirectoryBytes);
            stream.Write(_part5);
            stream.Write(modDirectoryBytes);
            stream.Write(_part6);
            stream.Write(modelNameExtensionBytes);
            stream.Write(_part7);
            stream.Write(modelNameBytes);
            stream.Write(_part8);

            return stream.ToArray();
        }

        private static string GetTemplate(string modName, string modelName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            builder.AppendLine("<package name=\"FFXVMOD\" copyguard=\"False\">");
            builder.AppendLine("  <objects>");
            builder.AppendLine(
                "    <object objectIndex=\"0\" name=\"MOD Package\" type=\"SQEX.Ebony.Framework.Entity.EntityPackage\" path=\"\" checked=\"True\">");
            builder.AppendLine("      <entities_>");
            builder.AppendLine("        <reference reference=\"True\" object=\"entities_.index\" objectIndex=\"1\" />");
            builder.AppendLine(
                "        <reference reference=\"True\" object=\"entities_.$preview\" objectIndex=\"2\" />");
            builder.AppendLine(
                "        <reference reference=\"True\" object=\"entities_.$preview.png\" objectIndex=\"3\" />");
            builder.AppendLine(
                $"        <reference reference=\"True\" object=\"entities_.{modelName}\" objectIndex=\"4\" />");
            builder.AppendLine("      </entities_>");
            builder.AppendLine("    </object>");
            builder.AppendLine(
                "    <object objectIndex=\"1\" name=\"index\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.index\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
            builder.AppendLine($"      <filePath_ type=\"string\">mod/{modName}/index.modmeta</filePath_>");
            builder.AppendLine("    </object>");
            builder.AppendLine(
                "    <object objectIndex=\"2\" name=\"$preview\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.$preview\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
            builder.AppendLine($"      <filePath_ type=\"string\">mod/{modName}/$preview.png</filePath_>");
            builder.AppendLine("    </object>");
            builder.AppendLine(
                "    <object objectIndex=\"3\" name=\"$preview.png\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.$preview.png\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
            builder.AppendLine($"      <filePath_ type=\"string\">mod/{modName}/$preview.png.bin</filePath_>");
            builder.AppendLine("    </object>");
            builder.AppendLine(
                $"    <object objectIndex=\"4\" name=\"{modelName}\" type=\"Black.Entity.SkeletalModelEntity\" path=\"entities_.{modelName}\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
            builder.AppendLine($"      <sourcePath_ type=\"string\">mod/{modName}/{modName}.gmdl.gfxbin</sourcePath_>");
            builder.AppendLine("    </object>");
            builder.AppendLine("  </objects>");
            builder.AppendLine("</package>");
            return builder.ToString();
        }
    }
}