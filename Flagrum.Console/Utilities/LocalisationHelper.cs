using System.IO;
using System.Text;
using Flagrum.Core.Utilities;

namespace Flagrum.Console.Utilities;

public static class LocalisationHelper
{
    public static void GenerateEmptyResXFilesForLanguage(string languageCode)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<root>");
        builder.AppendLine("    <resheader name=\"resmimetype\">");
        builder.AppendLine("        <value>text/microsoft-resx</value>");
        builder.AppendLine("    </resheader>");
        builder.AppendLine("    <resheader name=\"version\">");
        builder.AppendLine("        <value>1.3</value>");
        builder.AppendLine("    </resheader>");
        builder.AppendLine("    <resheader name=\"reader\">");
        builder.AppendLine(
            "        <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>");
        builder.AppendLine("    </resheader>");
        builder.AppendLine("    <resheader name=\"writer\">");
        builder.AppendLine(
            "        <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>");
        builder.AppendLine("    </resheader>");
        builder.AppendLine("</root>");
        var content = builder.ToString();

        foreach (var file in Directory.GetFiles(@"C:\Code\Flagrum", "*.ja-JP.resx", SearchOption.AllDirectories))
        {
            var path = file.Replace(".ja-JP.", $".{languageCode}.");
            File.WriteAllText(path, content);
        }
    }

    public static void RenameLocalisationFiles(string languageCodeToReplace, string languageCode)
    {
        foreach (var file in Directory.GetFiles(@"C:\Code\Flagrum", $"*.{languageCodeToReplace}.resx", SearchOption.AllDirectories))
        {
            var path = file.Replace($".{languageCodeToReplace}.", $".{languageCode}.");
            File.Move(file, path);
        }
    }

    public static void CopyLocalisationFilesToDirectory(params string[] languageCodes)
    {
        foreach (var code in languageCodes)
        {
            foreach (var file in Directory.GetFiles(@"C:\Code\Flagrum", $"*.{code}.resx", SearchOption.AllDirectories))
            {
                var path = file.Replace(@"C:\Code\Flagrum\", @"C:\Modding\Localisation\");
                IOHelper.EnsureDirectoriesExistForFilePath(path);
                File.Copy(file, path);
            }
        }
    }
    
    public static void DeleteLocalisationFiles(params string[] languageCodes)
    {
        foreach (var code in languageCodes)
        {
            foreach (var file in Directory.GetFiles(@"C:\Code\Flagrum", $"*.{code}.resx", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
        }
    }
}