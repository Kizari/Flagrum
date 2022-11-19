using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Flagrum.Core.Utilities;

public static class IOHelper
{
    public static string GetExecutingDirectory()
    {
        return Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        //return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        //return System.AppContext.BaseDirectory;
        //return Directory.GetCurrentDirectory();
        //return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //return Environment.CurrentDirectory;
        //return System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        
        // var flagrumDirectory = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Flagrum";
        // return Directory.EnumerateDirectories(flagrumDirectory)
        //     .Where(d => d.Split('\\').Last().StartsWith("app-"))
        //     .OrderByDescending(d => d)
        //     .First();
    }

    public static string GetWebRoot()
    {
        return $"{GetExecutingDirectory()}\\wwwroot";
    }

    public static void EnsureDirectoryExists(string path)
    {
        path = path.Replace('/', '\\');
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