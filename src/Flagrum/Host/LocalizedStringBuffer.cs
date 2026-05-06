using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Extensions.Localization;

namespace Flagrum.Host;

/// <summary>
/// Pins a dictionary of localized strings for use in native code.
/// </summary>
public sealed class LocalizedStringBuffer : IDisposable
{
    private readonly MemoryOwner<byte> _buffer;
    private MemoryHandle _pin;

    /// <summary>
    /// Creates a new <see cref="LocalizedStringBuffer"/> from a list of <see cref="LocalizedString"/>s.
    /// </summary>
    public LocalizedStringBuffer(IEnumerable<LocalizedString> strings)
    {
        var binaryStrings = strings
            .Select(s => (Encoding.UTF8.GetBytes(s.Name), Encoding.UTF8.GetBytes(s.Value)))
            .ToArray();
        
        Count = binaryStrings.Length;
        
        // Allocate string buffer
        var size = binaryStrings.Sum(s => s.Item1.Length + s.Item2.Length + 2); // 2 = null terminators
        _buffer = MemoryOwner<byte>.Allocate(size, AllocationMode.Clear);
        
        // Pin memory for safe native access
        _pin = _buffer.Memory.Pin();
        
        // Populate strings
        var span = _buffer.Span;
        var cursor = 0;

        foreach (var (key, value) in binaryStrings)
        {
            key.AsSpan().CopyTo(span[cursor..]);
            cursor += key.Length + 1;
            value.AsSpan().CopyTo(span[cursor..]);
            cursor += value.Length + 1;
        }
    }

    /// <summary>
    /// Number of strings in the buffer.
    /// </summary>
    public int Count { get; }
    
    /// <summary>
    /// Pointer to the pinned memory that contains the localized strings.
    /// </summary>
    /// <remarks>
    /// Stored as null-terminated UTF8 strings, in the pattern <c>key, value, key, value...</c>
    /// </remarks>
    public unsafe IntPtr Pointer => new(_pin.Pointer);

    /// <inheritdoc />
    public void Dispose()
    {
        _pin.Dispose();
        _buffer.Dispose();
    }
}