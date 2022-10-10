using System;
using System.IO;

namespace Flagrum.Core.Utilities;

public static class IOHelper
{
    public static string GetExecutingDirectory()
    {
        return Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
    }

    public static string GetWebRoot()
    {
        return $"{GetExecutingDirectory()}\\wwwroot";
    }
    
    public static void EnsureDirectoriesExistForFilePath(string path)
    {
        path = path.Replace('/', '\\');
        path = path[..path.LastIndexOf('\\')];
        var directories = path.Split('\\');
        var currentPath = "";
        foreach (var directory in directories)
        {
            currentPath += directory + '\\';
            if (!Directory.Exists(currentPath))
            {
                Directory.CreateDirectory(currentPath);
            }
        }
    }
}