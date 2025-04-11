using System;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Archive;

/// <summary>
/// Identifier for a game asset file type.
/// </summary>
/// <remarks>
/// The identifier is computed by a hashing algorithm.
/// </remarks>
public readonly partial record struct TypeId : IComparable<TypeId>
{
    /// <summary>
    /// Computes a <see cref="TypeId" /> from an existing URI hash.
    /// </summary>
    /// <param name="value">The hash of the URI whose type is to be represented by this identifier.</param>
    public TypeId(ulong value) => Value = (uint)(value >> 44);

    /// <summary>
    /// Instantiates a <see cref="TypeId" /> from an existing type hash.
    /// </summary>
    /// <param name="typeId">The ID of the type represented by this identifier.</param>
    public TypeId(uint typeId) => Value = typeId;

    /// <summary>
    /// Computes a <see cref="TypeId" /> from an asset URI.
    /// </summary>
    /// <param name="uri">The URI of the asset whose type is to be represented by this identifier.</param>
    public TypeId(string uri)
    {
        var extension = UriHelper.Instance.InferExtensionFromUri(uri);
        Value = (uint)(Cryptography.Hash64(extension) & 0xFFFFF);
    }

    /// <summary>
    /// Represents a zero value.
    /// </summary>
    public static TypeId Zero { get; } = new(0);

    /// <summary>
    /// The backing value for this type.
    /// </summary>
    public uint Value { get; }

    /// <summary>
    /// <c>true</c> if this ID is zero, otherwise <c>false</c>.
    /// </summary>
    public bool IsNull => Value == 0;

    /// <inheritdoc />
    public override string ToString() => Extensions.TryGetValue(this, out var result)
        ? result
        : Value.ToString("x5");

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)" />
    public bool Equals(TypeId? other) => other?.Value == Value;

    /// <inheritdoc />
    public int CompareTo(TypeId other) => Value.CompareTo(other.Value);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    public static implicit operator TypeId(uint value) => new(value);
    public static implicit operator TypeId(AssetId value) => value.TypeId;
    public static implicit operator uint(TypeId id) => id.Value;
}