namespace Flagrum.Abstractions.Archive;

/// <summary>
/// Represents a processed mod asset.
/// </summary>
public interface IFlagrumModFragment
{
    /// <summary>
    /// Retrieves original file data from the fragment.
    /// </summary>
    /// <returns>Unencrypted, decompressed file data.</returns>
    /// <remarks>
    /// Any asset conversions that were done on the original source file (such as PNG->BTEX) will
    /// not be reversed as a result of this action. Instead, the raw BTEX file will be returned.
    /// </remarks>
    byte[] GetReadableData();
}