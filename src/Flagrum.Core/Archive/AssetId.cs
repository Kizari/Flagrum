using System;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Archive;

/// <summary>
/// Identifier for a game asset.
/// </summary>
/// <remarks>
/// The identifier is computed by a hashing algorithm.
/// </remarks>
public readonly record struct AssetId : IComparable<AssetId>
{
    /// <summary>
    /// Instantiates an <see cref="AssetId" /> from an existing hash.
    /// </summary>
    /// <param name="value">The asset ID this instance will represent.</param>
    public AssetId(ulong value) => Value = value;

    /// <summary>
    /// Computes an <see cref="AssetId" /> from a 32-bit type hash and 64-bit URI hash.
    /// </summary>
    /// <param name="typeId">The ID of the file type of this asset.</param>
    /// <param name="uriHash">The 64-bit FNV1A hash of the URI that represents this asset.</param>
    public AssetId(uint typeId, ulong uriHash) => Value = ((ulong)typeId << 44) | (uriHash & 0xFFFFFFFFFFF);

    /// <summary>
    /// Computes an <see cref="AssetId" /> from an asset URI.
    /// </summary>
    /// <param name="uri">The URI of the asset this ID represents.</param>
    public AssetId(string uri)
    {
        var extension = UriHelper.Instance.InferExtensionFromUri(uri);
        var uriHash = Cryptography.Hash64(uri);
        var typeHash = Cryptography.Hash64(extension);
        Value = (typeHash << 44) | (uriHash & 0xFFFFFFFFFFF);
    }

    /// <summary>
    /// Computes an <see cref="AssetId" /> from an asset URI, forcing the given type hash to be used instead of
    /// the one inferred from the URI if they aren't the same.
    /// </summary>
    /// <param name="uri">The URI of the asset this ID represents.</param>
    /// <param name="typeHash">The 32-bit FNV1A hash of the file extension pertaining to this asset.</param>
    public AssetId(string uri, uint typeHash)
    {
        var uriHash = Cryptography.Hash64(uri);
        Value = ((ulong)typeHash << 44) | (uriHash & 0xFFFFFFFFFFF);
    }

    /// <summary>
    /// Represents a zero value.
    /// </summary>
    public static AssetId Zero { get; } = new(0);

    /// <summary>
    /// The backing value for this type.
    /// </summary>
    public ulong Value { get; }

    /// <summary>
    /// <c>true</c> if this ID is zero, otherwise <c>false</c>.
    /// </summary>
    public bool IsNull => Value == 0;

    /// <summary>
    /// The ID of the type of asset this identifier represents.
    /// </summary>
    public TypeId TypeId => (TypeId)(Value >> 44);

    /// <summary>
    /// The 64-bit FNV1A hash of the URI that represents this asset.
    /// </summary>
    public ulong UriHash => Value & 0xFFFFFFFFFFF;

    /// <inheritdoc />
    public override string ToString() => UriHash.ToString("x11") + $".{TypeId}";

    /// <inheritdoc />
    public int CompareTo(AssetId other) => Value.CompareTo(other.Value);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)" />
    public bool Equals(AssetId? other) => other?.Value == Value;

    public static implicit operator AssetId(ulong value) => new(value);
    public static implicit operator AssetId(TypeId value) => new((ulong)value.Value << 44);
    public static implicit operator ulong(AssetId id) => id.Value;
}