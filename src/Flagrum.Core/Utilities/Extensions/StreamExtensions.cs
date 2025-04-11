using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Text;
using Flagrum.Core.Serialization;

namespace Flagrum.Core.Utilities.Extensions;

public static class StreamExtensions
{
    public static void Align(this Stream stream, long blockSize)
    {
        var offset = stream.Position;
        var size = blockSize + blockSize * (offset / blockSize) - offset;
        if (size > 0 && size < blockSize)
        {
            stream.Seek(size, SeekOrigin.Current);
        }
    }

    public static void Align(this BinaryReader reader, int blockSize)
    {
        var size = SerializationHelper.Align((uint)reader.BaseStream.Position, (uint)blockSize);
        if (size == blockSize)
        {
            return;
        }

        reader.BaseStream.Seek(size, SeekOrigin.Current);
    }

    public static void Align(this BinaryWriter writer, int blockSize, byte paddingByte)
    {
        var size = SerializationHelper.Align((uint)writer.BaseStream.Position, (uint)blockSize);
        if (size == blockSize)
        {
            return;
        }

        for (var i = 0; i < size; i++)
        {
            writer.Write(paddingByte);
        }
    }

    public static string ReadNullTerminatedString(this BinaryReader reader, Encoding encoding = null)
    {
        var result = new List<byte>();
        byte next;

        while ((next = reader.ReadByte()) != 0x0)
        {
            result.Add(next);
        }

        return (encoding ?? Encoding.UTF8).GetString(result.ToArray());
    }

    public static void WriteNullTerminatedString(this BinaryWriter writer, string value)
    {
        if (value != null)
        {
            foreach (var character in value)
            {
                writer.Write(character);
            }
        }

        writer.Write((byte)0x00);
    }

    public static void CopyTo(this Stream source, Stream destination, long count)
    {
        var bufferSize = source is FileStream ? 81920 : 4096;
        var bytesRemaining = count;
        while (bytesRemaining > 0)
        {
            var readSize = Math.Min(bufferSize, bytesRemaining);
            var buffer = new byte[readSize];
            _ = source.Read(buffer);

            destination.Write(buffer);
            bytesRemaining -= readSize;
        }
    }

    public static byte[] ToArray(this ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public static Vector3 ReadVector3(this BinaryReader reader)
    {
        return new Vector3(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle()
        );
    }

    public static Vector4 ReadVector4(this BinaryReader reader)
    {
        return new Vector4(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle()
        );
    }

    public static void WriteVector3(this BinaryWriter writer, Vector3 vector)
    {
        writer.Write(vector.X);
        writer.Write(vector.Y);
        writer.Write(vector.Z);
    }

    public static void WriteVector4(this BinaryWriter writer, Vector4 vector)
    {
        writer.Write(vector.X);
        writer.Write(vector.Y);
        writer.Write(vector.Z);
        writer.Write(vector.W);
    }

    public static void WriteParameterTableValue(this BinaryWriter writer, object value)
    {
        switch (value)
        {
            case int i:
                writer.Write(i);
                break;
            case uint u:
                writer.Write(u);
                break;
            case float f:
                writer.Write(f);
                break;
            default:
                throw new ArgumentException("Parameter type unsupported", nameof(value));
        }
    }

    public static byte[] ToNullTerminatedBytes(this string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var final = new byte[bytes.Length + 1];
        Array.Copy(bytes, final, bytes.Length);
        return final;
    }

    public static byte[] ReadNullTerminatedStringAsBytes(this BinaryReader reader)
    {
        var result = new List<byte>();

        while (true)
        {
            var next = reader.ReadByte();
            result.Add(next);

            if (next == 0x0)
            {
                break;
            }
        }

        return result.ToArray();
    }

    public static string ReadString(this BinaryReader reader, int length)
    {
        return Encoding.UTF8.GetString(reader.ReadBytes(length));
    }
}