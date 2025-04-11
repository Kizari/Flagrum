namespace Flagrum.Abstractions.Archive;

/// <summary>
/// Represents a factory that creates instances of <see cref="IFlagrumModFragment" />.
/// </summary>
public interface IFlagrumModFragmentFactory
{
    /// <summary>
    /// Creates a new <see cref="IFlagrumModFragment" /> and populates it from an FFG file.
    /// </summary>
    /// <param name="filePath">Path to the <c>.ffg</c> file on disk.</param>
    IFlagrumModFragment Create(string filePath);
}