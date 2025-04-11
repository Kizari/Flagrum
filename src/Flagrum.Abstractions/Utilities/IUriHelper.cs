namespace Flagrum.Abstractions.Utilities;

/// <summary>
/// Represents a class with helper methods for working with URIs.
/// </summary>
public interface IUriHelper
{
    /// <summary>
    /// Replaces the file extension on a file name with the true file extension associated with the asset's file type.
    /// </summary>
    /// <param name="path">The file name, path, or URI whose extension is to be replaced.</param>
    /// <returns>Same as the input string, but with the file extension replaced.</returns>
    /// <remarks>
    /// URIs in Luminous are generated from the file type of the source file before it has been processed through the
    /// build pipeline. As such, the file extension doesn't always match the actual file type. For example, a URI
    /// that ends with <c>.tif</c> is actually a <c>.btex</c> file. This method will correct the file extension for
    /// a file whose extension comes from the URI.
    /// </remarks>
    string ReplaceFileNameExtensionWithTrueExtension(string path);
}