using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Project;
using Injectio.Attributes;

namespace Flagrum.Application.Features.ModManager.Project;

/// <inheritdoc />
[RegisterSingleton<IFlagrumProjectArchiveFactory>]
public class FlagrumProjectArchiveFactory : IFlagrumProjectArchiveFactory
{
    /// <inheritdoc />
    public IFlagrumProjectArchive Create(ModChangeType type, string relativePath, EbonyArchiveFlags flags) =>
        new FlagrumProjectArchive
        {
            Type = type,
            RelativePath = relativePath,
            Flags = flags
        };
}