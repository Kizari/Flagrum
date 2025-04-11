using System;
using System.IO;
using System.Reflection;

namespace Flagrum.Core.Scripting.Ebex.Configuration;

public class Project
{
    public const uint PlaceID = 12958433;
    private static string dataRoot = "DATA ROOT NOT SET";
    private static string sdkRoot = "SDK ROOT NOT SET";

    public static string DataRoot
    {
        get => dataRoot;
        set => dataRoot = value;
    }

    public static string SDKRoot
    {
        get
        {
            if (sdkRoot == null)
            {
                sdkRoot = ""; //Luminous.EnvironmentSettings.EnvironmentSettings.GetSDKPath().Replace('\\', '/');
            }

            return sdkRoot;
        }
    }

    public static string ApplicationDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public static string ModulesDirectory => ""; //Luminous.EnvironmentSettings.EnvironmentSettings.GetModulePath();

    public static string Grape2ServerPath => GetDirectoryFromApplication("../GRAPE2/Grape2Server.exe");

    public static string BuildJobDirectory => GetDirectoryFromApplication("../BuildConsole");

    public static string ApplicationConfiguraitonPath => "";

    // var directoryFromApplication = GetDirectoryFromApplication(
    //     Luminous.EnvironmentSettings.EnvironmentSettings.GetDataPath() +
    //     "/config/Jenova/JenovaApplication.xml");
    // return File.Exists(directoryFromApplication)
    //     ? directoryFromApplication
    //     : GetDirectoryFromApplication("../../config/Jenova/JenovaApplication.xml");
    public static string AttributeParserConfigurationPath => Path.Combine(ModulesDirectory, "config.xml");

    public static string GetDataRelativePath(string fullPath)
    {
        var str = fullPath.Replace('\\', '/');
        if (str.StartsWith(DataRoot, StringComparison.CurrentCultureIgnoreCase))
        {
            str = str.Length <= DataRoot.Length ? "" : str.Substring(DataRoot.Length + 1);
        }
        else if (str.StartsWith(SDKRoot, StringComparison.CurrentCultureIgnoreCase))
        {
            str = str.Length <= SDKRoot.Length ? "" : "sdk://" + str.Substring(SDKRoot.Length + 1);
        }

        return str;
    }

    public static string GetDataRelativePath(FileInfo fileInfo)
    {
        return GetDataRelativePath(fileInfo.FullName);
    }

    public static string GetDataFullPath(string relativePath)
    {
        return relativePath.StartsWith("sdk://")
            ? Path.Combine(SDKRoot, relativePath.Substring(6)).Replace('\\', '/')
            : Path.Combine(DataRoot, relativePath).Replace('\\', '/');
    }

    public static bool StartsWithDataRoot(string fullPath)
    {
        var str = fullPath.Replace('\\', '/');
        return str.StartsWith(DataRoot, StringComparison.CurrentCultureIgnoreCase) ||
               str.StartsWith(SDKRoot, StringComparison.CurrentCultureIgnoreCase);
    }

    public static string GetDirectoryFromApplication(string path)
    {
        return Path.GetFullPath(Path.Combine(ApplicationDirectory, path));
    }
}