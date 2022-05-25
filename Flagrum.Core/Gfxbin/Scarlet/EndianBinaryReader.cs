using System;
using System.IO;
using System.Text;

namespace Scarlet.IO;

/// <summary>
/// Specifies an endianness
/// </summary>
public enum Endian
{
    /// <summary>
    /// Little endian (i.e. DDCCBBAA)
    /// </summary>
    LittleEndian = 0,

    /// <summary>
    /// Big endian (i.e. AABBCCDD)
    /// </summary>
    BigEndian = 1
}

/// <summary>
/// Read data from stream with data of specified endianness
/// </summary>
public class EndianBinaryReader : BinaryReader
{
    /* TODO: BIGENDIAN check taken from BitConverter source; does this work as intended? */
#if BIGENDIAN
        public const Endian NativeEndianness = Endian.BigEndian;
#else
    public const Endian NativeEndianness = Endian.LittleEndian;
#endif

    /// <summary>
    /// Currently specified endianness
    /// </summary>
    public Endian Endianness { get; set; }

    /// <summary>
    /// Boolean representing if the currently specified endianness equal to the system's native endianness
    /// </summary>
    public bool IsNativeEndianness => NativeEndianness == Endianness;

    /* TODO: doublecheck every non-native read result; slim down reverse functions? */

    public EndianBinaryReader(Stream input) : this(input, Endian.LittleEndian) { }
    public EndianBinaryReader(Stream input, Encoding encoding) : this(input, encoding, Endian.LittleEndian) { }
    public EndianBinaryReader(Stream input, Endian endianness) : this(input, Encoding.UTF8, endianness) { }

    public EndianBinaryReader(Stream input, Encoding encoding, Endian endianness)
        : base(input, encoding)
    {
        Endianness = endianness;
    }

    public override float ReadSingle()
    {
        return ReadSingle(Endianness);
    }

    public float ReadSingle(Endian endianness)
    {
        if (endianness == NativeEndianness)
        {
            return base.ReadSingle();
        }

        return BitConverter.ToSingle(BitConverter.GetBytes(Reverse(base.ReadUInt32())), 0);
    }

    public override double ReadDouble()
    {
        return ReadDouble(Endianness);
    }

    public double ReadDouble(Endian endianness)
    {
        if (endianness == NativeEndianness)
        {
            return base.ReadDouble();
        }

        return BitConverter.ToDouble(BitConverter.GetBytes(Reverse(base.ReadUInt64())), 0);
    }

    public override short ReadInt16()
    {
        return ReadInt16(Endianness);
    }

    public short ReadInt16(Endian endianness)
    {
        if (endianness == NativeEndianness)
        {
            return base.ReadInt16();
        }

        return Reverse(base.ReadInt16());
    }

    public override ushort ReadUInt16()
    {
        return ReadUInt16(Endianness);
    }

    public ushort ReadUInt16(Endian endianness)
    {
        if (endianness == NativeEndianness)
        {
            return base.ReadUInt16();
        }

        return Reverse(base.ReadUInt16());
    }

    public override int ReadInt32()
    {
        return ReadInt32(Endianness);
    }

    public int ReadInt32(Endian endianness)
    {
        if (endianness == NativeEndianness)
        {
            return base.ReadInt32();
        }

        return Reverse(base.ReadInt32());
    }

    public override uint ReadUInt32()
    {
        return ReadUInt32(Endianness);
    }

    public uint ReadUInt32(Endian endianness)
    {
        if (endianness == NativeEndianness)
        {
            return base.ReadUInt32();
        }

        return Reverse(base.ReadUInt32());
    }

    public override long ReadInt64()
    {
        return ReadInt64(Endianness);
    }

    public long ReadInt64(Endian endianness)
    {
        if (endianness == NativeEndianness)
        {
            return base.ReadInt64();
        }

        return Reverse(base.ReadInt64());
    }

    public override ulong ReadUInt64()
    {
        return ReadUInt64(Endianness);
    }

    public ulong ReadUInt64(Endian endianness)
    {
        if (endianness == NativeEndianness)
        {
            return base.ReadUInt64();
        }

        return Reverse(base.ReadUInt64());
    }

    private short Reverse(short value)
    {
        return (short)(
            (((value & 0xFF00) >> 8) << 0) |
            (((value & 0x00FF) >> 0) << 8));
    }

    private ushort Reverse(ushort value)
    {
        return (ushort)(
            (((value & 0xFF00) >> 8) << 0) |
            (((value & 0x00FF) >> 0) << 8));
    }

    private int Reverse(int value)
    {
        return (int)(
            ((((uint)value & 0xFF000000) >> 24) << 0) |
            ((((uint)value & 0x00FF0000) >> 16) << 8) |
            ((((uint)value & 0x0000FF00) >> 8) << 16) |
            ((((uint)value & 0x000000FF) >> 0) << 24));
    }

    private uint Reverse(uint value)
    {
        return (((value & 0xFF000000) >> 24) << 0) |
               (((value & 0x00FF0000) >> 16) << 8) |
               (((value & 0x0000FF00) >> 8) << 16) |
               (((value & 0x000000FF) >> 0) << 24);
    }

    private long Reverse(long value)
    {
        return (long)(
            ((((ulong)value & 0xFF00000000000000UL) >> 56) << 0) |
            ((((ulong)value & 0x00FF000000000000UL) >> 48) << 8) |
            ((((ulong)value & 0x0000FF0000000000UL) >> 40) << 16) |
            ((((ulong)value & 0x000000FF00000000UL) >> 32) << 24) |
            ((((ulong)value & 0x00000000FF000000UL) >> 24) << 32) |
            ((((ulong)value & 0x0000000000FF0000UL) >> 16) << 40) |
            ((((ulong)value & 0x000000000000FF00UL) >> 8) << 48) |
            ((((ulong)value & 0x00000000000000FFUL) >> 0) << 56));
    }

    private ulong Reverse(ulong value)
    {
        return (((value & 0xFF00000000000000UL) >> 56) << 0) |
               (((value & 0x00FF000000000000UL) >> 48) << 8) |
               (((value & 0x0000FF0000000000UL) >> 40) << 16) |
               (((value & 0x000000FF00000000UL) >> 32) << 24) |
               (((value & 0x00000000FF000000UL) >> 24) << 32) |
               (((value & 0x0000000000FF0000UL) >> 16) << 40) |
               (((value & 0x000000000000FF00UL) >> 8) << 48) |
               (((value & 0x00000000000000FFUL) >> 0) << 56);
    }
}