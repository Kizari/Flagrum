using System.Text;
using Flagrum.Core.Archive.Mod;

namespace Flagrum.Application.Services;

public class EntityPackageBuilder
{
    private readonly BinmodTypeHelper _binmodType;

    public EntityPackageBuilder(BinmodTypeHelper binmodType)
    {
        _binmodType = binmodType;
    }

    public byte[] BuildExml(Binmod mod)
    {
        var type = (WorkshopModType)mod.Type;

        if (type == WorkshopModType.StyleEdit)
        {
            var target = (ComradesStyleTarget)mod.Target;
            return target == ComradesStyleTarget.Accessory
                ? BuildSingleModelMultiPreview(mod)
                : BuildMultiModelMultiPreview(mod);
        }

        return _binmodType.GetModelCount(mod.Type, mod.Target) > 1
            ? BuildMultiModelSinglePreview(mod)
            : BuildSingleModelSinglePreview(mod);
    }

    private byte[] BuildSingleModelSinglePreview(Binmod mod)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        builder.AppendLine("<package name=\"FFXVMOD\" copyguard=\"False\">");
        builder.AppendLine("  <objects>");
        builder.AppendLine(
            "    <object objectIndex=\"0\" name=\"MOD Package\" type=\"SQEX.Ebony.Framework.Entity.EntityPackage\" path=\"\" checked=\"True\">");
        builder.AppendLine("      <entities_>");
        builder.AppendLine("        <reference reference=\"True\" object=\"entities_.index\" objectIndex=\"1\" />");
        builder.AppendLine("        <reference reference=\"True\" object=\"entities_.$preview\" objectIndex=\"2\" />");
        builder.AppendLine(
            "        <reference reference=\"True\" object=\"entities_.$preview.png\" objectIndex=\"3\" />");
        builder.AppendLine(
            $"        <reference reference=\"True\" object=\"entities_.{mod.ModelName}\" objectIndex=\"4\" />");
        builder.AppendLine("      </entities_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"1\" name=\"index\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.index\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/index.modmeta</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"2\" name=\"$preview\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.$preview\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/$preview.png</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"3\" name=\"$preview.png\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.$preview.png\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/$preview.png.bin</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            $"    <object objectIndex=\"4\" name=\"{mod.ModelName}\" type=\"Black.Entity.SkeletalModelEntity\" path=\"entities_.{mod.ModelName}\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine(
            $"      <sourcePath_ type=\"string\">mod/{mod.ModDirectoryName}/{mod.ModelName}.{mod.ModelExtension}</sourcePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine("  </objects>");
        builder.AppendLine("</package>");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static byte[] BuildMultiModelSinglePreview(Binmod mod)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        builder.AppendLine("<package name=\"FFXVMOD\" copyguard=\"False\">");
        builder.AppendLine("  <objects>");
        builder.AppendLine(
            "    <object objectIndex=\"0\" name=\"MOD Package\" type=\"SQEX.Ebony.Framework.Entity.EntityPackage\" path=\"\" checked=\"True\">");
        builder.AppendLine("      <entities_>");
        builder.AppendLine("        <reference reference=\"True\" object=\"entities_.index\" objectIndex=\"1\" />");
        builder.AppendLine("        <reference reference=\"True\" object=\"entities_.$preview\" objectIndex=\"2\" />");
        builder.AppendLine(
            "        <reference reference=\"True\" object=\"entities_.$preview.png\" objectIndex=\"3\" />");
        builder.AppendLine(
            $"        <reference reference=\"True\" object=\"entities_.{mod.Model1Name}\" objectIndex=\"4\" />");
        builder.AppendLine(
            $"        <reference reference=\"True\" object=\"entities_.{mod.Model2Name}\" objectIndex=\"5\" />");
        builder.AppendLine("      </entities_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"1\" name=\"index\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.index\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/index.modmeta</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"2\" name=\"$preview\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.$preview\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/$preview.png</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"3\" name=\"$preview.png\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.$preview.png\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/$preview.png.bin</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            $"    <object objectIndex=\"4\" name=\"{mod.Model1Name}\" type=\"Black.Entity.SkeletalModelEntity\" path=\"entities_.{mod.Model1Name}\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine(
            $"      <sourcePath_ type=\"string\">mod/{mod.ModDirectoryName}/{mod.Model1Name}.{mod.ModelExtension}</sourcePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            $"    <object objectIndex=\"5\" name=\"{mod.Model2Name}\" type=\"Black.Entity.SkeletalModelEntity\" path=\"entities_.{mod.Model2Name}\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine(
            $"      <sourcePath_ type=\"string\">mod/{mod.ModDirectoryName}/{mod.Model2Name}.{mod.ModelExtension}</sourcePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine("  </objects>");
        builder.AppendLine("</package>");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static byte[] BuildSingleModelMultiPreview(Binmod mod)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        builder.AppendLine("<package name=\"FFXVMOD\" copyguard=\"False\">");
        builder.AppendLine("  <objects>");
        builder.AppendLine(
            "    <object objectIndex=\"0\" name=\"MOD Package\" type=\"SQEX.Ebony.Framework.Entity.EntityPackage\" path=\"\" checked=\"True\">");
        builder.AppendLine("      <entities_>");
        builder.AppendLine("        <reference reference=\"True\" object=\"entities_.default\" objectIndex=\"1\" />");
        builder.AppendLine("        <reference reference=\"True\" object=\"entities_.index\" objectIndex=\"2\" />");
        builder.AppendLine("        <reference reference=\"True\" object=\"entities_.$preview\" objectIndex=\"3\" />");
        builder.AppendLine(
            "        <reference reference=\"True\" object=\"entities_.$preview.png\" objectIndex=\"4\" />");
        builder.AppendLine(
            $"        <reference reference=\"True\" object=\"entities_.{mod.ModelName}\" objectIndex=\"5\" />");
        builder.AppendLine("      </entities_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"1\" name=\"default\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.default\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/default.png</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"2\" name=\"index\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.index\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/index.modmeta</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"3\" name=\"$preview\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.$preview\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/$preview.png</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"4\" name=\"$preview.png\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.$preview.png\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/$preview.png.bin</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            $"    <object objectIndex=\"5\" name=\"{mod.ModelName}\" type=\"Black.Entity.SkeletalModelEntity\" path=\"entities_.{mod.ModelName}\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine(
            $"      <sourcePath_ type=\"string\">mod/{mod.ModDirectoryName}/{mod.ModelName}.{mod.ModelExtension}</sourcePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine("  </objects>");
        builder.AppendLine("</package>");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static byte[] BuildMultiModelMultiPreview(Binmod mod)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        builder.AppendLine("<package name=\"FFXVMOD\" copyguard=\"False\">");
        builder.AppendLine("  <objects>");
        builder.AppendLine(
            "    <object objectIndex=\"0\" name=\"MOD Package\" type=\"SQEX.Ebony.Framework.Entity.EntityPackage\" path=\"\" checked=\"True\">");
        builder.AppendLine("      <entities_>");
        builder.AppendLine("        <reference reference=\"True\" object=\"entities_.default\" objectIndex=\"1\" />");
        builder.AppendLine("        <reference reference=\"True\" object=\"entities_.index\" objectIndex=\"2\" />");
        builder.AppendLine("        <reference reference=\"True\" object=\"entities_.$preview\" objectIndex=\"3\" />");
        builder.AppendLine(
            "        <reference reference=\"True\" object=\"entities_.$preview.png\" objectIndex=\"4\" />");
        builder.AppendLine(
            $"        <reference reference=\"True\" object=\"entities_.{mod.Model1Name}\" objectIndex=\"5\" />");
        builder.AppendLine(
            $"        <reference reference=\"True\" object=\"entities_.{mod.Model2Name}\" objectIndex=\"6\" />");
        builder.AppendLine("      </entities_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"1\" name=\"default\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.default\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/default.png</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"2\" name=\"index\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.index\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/index.modmeta</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"3\" name=\"$preview\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.$preview\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/$preview.png</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            "    <object objectIndex=\"4\" name=\"$preview.png\" type=\"Black.Entity.Data.UnknownResource\" path=\"entities_.$preview.png\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine($"      <filePath_ type=\"string\">mod/{mod.ModDirectoryName}/$preview.png.bin</filePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            $"    <object objectIndex=\"5\" name=\"{mod.Model1Name}\" type=\"Black.Entity.SkeletalModelEntity\" path=\"entities_.{mod.Model1Name}\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine(
            $"      <sourcePath_ type=\"string\">mod/{mod.ModDirectoryName}/{mod.Model1Name}.{mod.ModelExtension}</sourcePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine(
            $"    <object objectIndex=\"6\" name=\"{mod.Model2Name}\" type=\"Black.Entity.SkeletalModelEntity\" path=\"entities_.{mod.Model2Name}\" owner=\"\" ownerIndex=\"0\" ownerPath=\"entities_\">");
        builder.AppendLine(
            $"      <sourcePath_ type=\"string\">mod/{mod.ModDirectoryName}/{mod.Model2Name}.{mod.ModelExtension}</sourcePath_>");
        builder.AppendLine("    </object>");
        builder.AppendLine("  </objects>");
        builder.AppendLine("</package>");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}