using System;
using System.IO;
using Flagrum.Abstractions.Archive;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Archive;

/// <summary>
/// Represents an archive entry or loose file belonging to the game.
/// </summary>
public interface IGameFile
{
    /// <summary>
    /// The path to the file on disk (or the containing archive if not a loose file).
    /// </summary>
    string Path { get; }

    /// <summary>
    /// The URI that identifies this file.
    /// </summary>
    string Uri { get; }

    /// <summary>
    /// The ID of the file type of this file.
    /// </summary>
    uint TypeId { get; }

    /// <summary>
    /// The state of the file within an archive (if packed).
    /// </summary>
    /// <remarks>Will always be <see cref="EbonyArchiveFileFlags.None" /> for loose files.</remarks>
    EbonyArchiveFileFlags Flags { get; }

    /// <summary>
    /// Gets the decompressed, unencrypted data belonging to this file.
    /// </summary>
    /// <returns>A buffer containing the usable file data.</returns>
    byte[] GetReadableData();
}

/// <summary>
/// Represents a game file on disk that is <b>not</b> packed into an archive.
/// </summary>
public class LooseGameFile : IGameFile
{
    /// <summary>
    /// Creates a new <see cref="LooseGameFile" /> from the given file path.
    /// </summary>
    /// <param name="path">The absolute path to the file on disk.</param>
    public LooseGameFile(string path)
    {
        Path = path;
        var uri = path.Replace('\\', '/');
        var index = uri.IndexOf("datas/", StringComparison.OrdinalIgnoreCase);
        Uri = index == -1 ? "EXTERNAL" : uri[index..].Replace("datas/", "data://");
    }

    /// <inheritdoc />
    public string Path { get; }

    /// <inheritdoc />
    public string Uri { get; }

    /// <inheritdoc />
    public uint TypeId => (uint)(Cryptography.Hash64(Uri.TrimEnd('@')) & 0xFFFFF);

    /// <inheritdoc />
    public EbonyArchiveFileFlags Flags => EbonyArchiveFileFlags.None;

    /// <inheritdoc />
    public byte[] GetReadableData() => File.ReadAllBytes(Path);
}