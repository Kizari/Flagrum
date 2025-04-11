using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Flagrum.Abstractions;
using Flagrum.Abstractions.ModManager;

namespace Flagrum.Application.Features.Shared;

/// <inheritdoc />
public class FreeService(IProfileService profile) : IPremiumService
{
    /// <summary>
    /// Known hashes for executables that are compatible with the injected DLL.
    /// </summary>
    private static readonly Dictionary<string, GameExecutableType> GameExecutableHashes = new()
    {
        // Final Steam release
        {"78d29c1cbce8b94bf1e0fa2b67053a24521a8c49b763d487c1da29f63dca0c8c", GameExecutableType.Release},
        // Final Steam release with the old Ansel patch modification applied
        {"0c72602fbd3b246de44e0488badc76b722cdfd99478e2edfc0c54d3eee0179e6", GameExecutableType.Release}
    };

    /// <summary>
    /// Free service does not have any premium features, so this will always return <c>null</c>.
    /// </summary>
    public Type GetComponentType(PremiumComponentType component) => null;

    /// <inheritdoc />
    public bool IsClientWhitelisted => false;

    /// <inheritdoc />
    public GameExecutableType GetGameExecutableType()
    {
        var hash = profile.Current.GameExeHash;

        // Ensure the exe hash for this profile is up-to-date
        if (hash == null
            || File.GetLastWriteTimeUtc(profile.Current.GamePath).Ticks != profile.Current.GameExeHashTime)
        {
            using var stream = new FileStream(profile.Current.GamePath, FileMode.Open, FileAccess.Read);
            hash = string.Join("", SHA256.HashData(stream).Select(b => b.ToString("x2")));
            profile.Current.GameExeHash = hash;
            profile.Current.GameExeHashTime = File.GetLastWriteTimeUtc(profile.Current.GamePath).Ticks;
        }

        return GameExecutableHashes.TryGetValue(hash, out var type) ? type : GameExecutableType.Unknown;
    }
}