using System;
using System.IO;

namespace Flagrum.Core.Scripting.Ebex;

public static class EbexUtility
{
    public static string DataDirectory;

    public static string SourcePathToRelativePath(string packageFilePath, string relativePath)
    {
        if (!relativePath.StartsWith('.'))
        {
            return relativePath;
        }

        var uri = new Uri(new Uri(Path.GetDirectoryName(packageFilePath) + '/'), relativePath);
        var absolutePath = uri.AbsolutePath.Replace('\\', '/').Replace("%20", " ");
        return AbsolutePathToRelativePath(absolutePath);
    }

    public static string SourcePathToAbsolutePath(string packageFilePath, string relativePath)
    {
        if (!relativePath.StartsWith('.'))
        {
            return RelativePathToAbsolutePath(relativePath);
        }

        var uri = new Uri(new Uri(Path.GetDirectoryName(packageFilePath) + '/'), relativePath);
        return uri.AbsolutePath.Replace('\\', '/').Replace("%20", " ").ToLower();
    }

    public static string AbsolutePathToRelativePath(string absolutePath)
    {
        absolutePath = absolutePath.Replace('\\', '/');
        if (!absolutePath.StartsWith(DataDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception($"Failed to resolve path {absolutePath} with data root {DataDirectory}");
        }

        return absolutePath.Length <= DataDirectory.Length ? "" : absolutePath[(DataDirectory.Length + 1)..];
    }

    public static string RelativePathToAbsolutePath(string relativePath)
    {
        return Path.Combine(DataDirectory, relativePath).Replace('\\', '/').ToLower();
    }
}