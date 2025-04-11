using System;
using System.IO;

namespace Flagrum.Core.Utilities;

public static class IOHelper
{
    public static string LocalApplicationData =>
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public static string GetExecutingDirectory()
    {
        return Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
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

    /// <summary>
    /// Compares if two file paths are the same.
    /// Accounts for slash direction, casing, trailing slash, and untrimmed whitespace.
    /// </summary>
    /// <returns>True if paths are the same</returns>
    public static bool ComparePaths(string path1, string path2)
    {
        path1 = path1.Replace('/', '\\').Trim().TrimEnd('\\').ToLower();
        path2 = path2.Replace('/', '\\').Trim().TrimEnd('\\').ToLower();
        return path1 == path2;
    }

    /// <summary>
    /// Checks if a file path begins with another partial file path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="startingSegment">The partial file path to check if the path starts with.</param>
    /// <returns><c>true</c> if the path starts with the segment, otherwise <c>false</c>.</returns>
    /// <remarks>Accounts for slash direction, whitespace, trailing slash, and casing.</remarks>
    public static bool DoesPathStartWith(string path, string startingSegment)
    {
        path = path.Replace('\\', '/').Trim().TrimEnd('/').ToLower();
        startingSegment = startingSegment.Replace('\\', '/').Trim().TrimEnd('/').ToLower();
        return path.StartsWith(startingSegment);
    }

    /// <summary>
    /// Compares if two files are in the same directory based on the paths.
    /// Accounts for slash direction, casing, trailing slash, and untrimmed whitespace.
    /// </summary>
    /// <returns>True if files are in the same directory</returns>
    public static bool AreInSameDirectory(string path1, string path2)
    {
        path1 = Path.GetDirectoryName(path1.Replace('/', '\\').Trim().TrimEnd('\\').ToLower());
        path2 = Path.GetDirectoryName(path2.Replace('/', '\\').Trim().TrimEnd('\\').ToLower());
        return path1 == path2;
    }

    public static void DeleteEmptySubdirectoriesRecursively(string directory)
    {
        if (Directory.Exists(directory))
        {
            foreach (var subdirectory in Directory.EnumerateDirectories(directory))
            {
                DeleteEmptySubdirectoriesRecursively(subdirectory);
                if (Directory.GetFileSystemEntries(subdirectory).Length == 0)
                {
                    Directory.Delete(subdirectory);
                }
            }
        }
    }

    /// <summary>
    /// Deletes every empty folder in a given file path
    /// </summary>
    /// <param name="basePath">The folder to stop deleting at</param>
    /// <param name="fullPath">The full path to delete from</param>
    public static void DeleteEmptyDirectoriesInPath(string basePath, string fullPath)
    {
        basePath = basePath.Replace('/', '\\').Trim().TrimEnd('\\').ToLower();
        fullPath = fullPath.Replace('/', '\\').Trim().TrimEnd('\\').ToLower();

        if (!Directory.Exists(basePath))
        {
            throw new ArgumentException("Base path must be a directory and must exist", nameof(basePath));
        }

        if (!Directory.Exists(fullPath))
        {
            fullPath = Path.GetDirectoryName(fullPath)!;
            if (!Directory.Exists(fullPath))
            {
                // Nothing to delete, carry on
                return;
            }
        }

        if (!fullPath.Contains(basePath))
        {
            throw new ArgumentException("Full path must contain the base path", nameof(fullPath));
        }

        while (fullPath != basePath)
        {
            if (Directory.GetFileSystemEntries(fullPath).Length == 0)
            {
                // Folder was empty, delete it and go up to the next one
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath);
                }

                fullPath = Path.GetDirectoryName(fullPath)!;
            }
            else
            {
                // Folder wasn't empty, nothing more to delete
                return;
            }
        }
    }

    public static void SetFileAttributesNormal(string path)
    {
        if (File.Exists(path))
        {
            File.SetAttributes(path, FileAttributes.Normal);
        }
    }

    public static void DeleteFileIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}