namespace Flagrum.Abstractions.Archive;

/// <summary>
/// Represents an archive of the <c>.earc</c> format.
/// </summary>
public interface IEbonyArchive : IDisposable
{
    /// <summary>
    /// Adds a new file to the archive.
    /// </summary>
    /// <param name="uri">The URI that will represent this file.</param>
    /// <param name="flags">Flags that apply to this file.</param>
    /// <param name="data">The file data.</param>
    void AddFile(string uri, EbonyArchiveFileFlags flags, byte[] data);

    void AddProcessedFile(
        string uri,
        EbonyArchiveFileFlags flags,
        byte[] data,
        uint originalSize,
        ushort key,
        string relativePath);

    /// <summary>
    /// Whether a file exists in this archive.
    /// </summary>
    /// <param name="uri">The URI that represents the file to check for.</param>
    bool HasFile(string uri);

    /// <summary>
    /// Removes a file from the archive.
    /// </summary>
    /// <param name="uri">The URI that represents the file to remove.</param>
    void RemoveFile(string uri);
}