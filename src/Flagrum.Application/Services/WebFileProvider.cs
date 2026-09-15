using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Flagrum.Application.Services;

/// <summary>
/// Custom file provider that maps asset paths to specific directories.
/// </summary>
/// <param name="webAssetFileProvider">Provides files within the wwwroot folder.</param>
/// <param name="userAssetFileProvider">Provides files within the assets folder in user data.</param>
/// <remarks>
/// User content is mapped separately to the user data directory, as it is not possible to write
/// to wwwroot when it is readonly, such as with a Linux AppImage.
/// </remarks>
public class WebFileProvider(
    IFileProvider webAssetFileProvider,
    IFileProvider userAssetFileProvider) : IFileProvider
{
    /// <inheritdoc />
    public IFileInfo GetFileInfo(string subpath)
    {
        // Route user-data directories to the user asset file provider
        if (subpath.StartsWith("images/", StringComparison.OrdinalIgnoreCase)
            || subpath.StartsWith("EarcMods/", StringComparison.OrdinalIgnoreCase))
        {
            return userAssetFileProvider.GetFileInfo(subpath);
        }

        // Route all other requests to the static asset file provider
        return webAssetFileProvider.GetFileInfo(subpath);
    }

    /// <inheritdoc />
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        // Route user-data directories to the user asset file provider
        if (subpath.Equals("images", StringComparison.OrdinalIgnoreCase)
            || subpath.StartsWith("images/", StringComparison.OrdinalIgnoreCase)
            || subpath.Equals("EarcMods", StringComparison.OrdinalIgnoreCase)
            || subpath.StartsWith("EarcMods/", StringComparison.OrdinalIgnoreCase))
        {
            return userAssetFileProvider.GetDirectoryContents(subpath);
        }

        // Route all other requests to the static asset file provider
        return webAssetFileProvider.GetDirectoryContents(subpath);
    }

    /// <inheritdoc />
    public IChangeToken Watch(string filter)
    {
        // Route user-data directories to the user asset file provider
        if (filter.StartsWith("images/", StringComparison.OrdinalIgnoreCase)
            || filter.StartsWith("EarcMods/", StringComparison.OrdinalIgnoreCase))
        {
            return userAssetFileProvider.Watch(filter);
        }

        // Route all other requests to the static asset file provider
        return webAssetFileProvider.Watch(filter);
    }
}