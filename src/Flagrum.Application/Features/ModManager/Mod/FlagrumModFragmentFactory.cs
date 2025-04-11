using Flagrum.Abstractions.Archive;
using Injectio.Attributes;

namespace Flagrum.Application.Features.ModManager.Mod;

/// <inheritdoc />
[RegisterSingleton<IFlagrumModFragmentFactory>]
public class FlagrumModFragmentFactory : IFlagrumModFragmentFactory
{
    /// <inheritdoc />
    public IFlagrumModFragment Create(string filePath)
    {
        var fragment = new FmodFragment();
        fragment.Read(filePath);
        return fragment;
    }
}