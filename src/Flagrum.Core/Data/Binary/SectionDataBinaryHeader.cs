using System.Runtime.InteropServices;

namespace Flagrum.Core.Data.Binary;

/// <summary>
/// SEDB header for some binary formats.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SectionDataBinaryHeader
{
    /// <summary>
    /// Expected value of <see cref="Type" />.
    /// </summary>
    public const uint TypeValue = 0x42444553; // "SEDB"

    /// <summary>
    /// Magic value, should be "SEDB" in all cases.
    /// </summary>
    public uint Type;

    /// <summary>
    /// Magic value for the resource contained after this header.
    /// </summary>
    public uint Subtype;

    /// <summary>
    /// Version of the SEDB header.
    /// </summary>
    public uint Version;

    /// <summary>
    /// Endianness used for this file.
    /// </summary>
    public BinaryEndianType EndianType;

    /// <summary>
    /// Not well understood.
    /// </summary>
    public byte AlignmentBits;

    /// <summary>
    /// Byte position of the resource that follows this header.
    /// </summary>
    public ushort Offset;

    /// <summary>
    /// Size of the file, in bytes.
    /// </summary>
    public ulong Size;

    /// <summary>
    /// Not well understood.
    /// </summary>
    public ulong DateTime;

    /// <summary>
    /// Identifier for the resource contained within the file.
    /// </summary>
    public ResourceId ResourceId;
}