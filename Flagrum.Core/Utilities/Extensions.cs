using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using Microsoft.Extensions.Logging;

namespace Flagrum.Core.Utilities;

public static class Extensions
{
    public static float[,] ToArray(this Matrix4x4 matrix)
    {
        return new[,]
        {
            {matrix.M11, matrix.M12, matrix.M13, matrix.M14},
            {matrix.M21, matrix.M22, matrix.M23, matrix.M24},
            {matrix.M31, matrix.M32, matrix.M33, matrix.M34},
            {matrix.M41, matrix.M42, matrix.M43, matrix.M44}
        };
    }

    public static byte[] ToArray(this ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public static DateTime LogTimeSince(this ILogger logger, string message, DateTime startTime)
    {
        logger.LogInformation("{Message} after {Milliseconds} milliseconds", message,
            (DateTime.Now - startTime).TotalMilliseconds);
        return DateTime.Now;
    }

    public static void Align(this Stream stream, long blockSize)
    {
        var offset = stream.Position;
        var size = blockSize + blockSize * (offset / blockSize) - offset;
        if (size > 0 && size < blockSize)
        {
            stream.Seek(size, SeekOrigin.Current);
        }
    }

    public static string ReadNullTerminatedString(this BinaryReader reader)
    {
        var result = new List<char>();
        char next;

        while ((next = reader.ReadChar()) != 0x0)
        {
            result.Add(next);
        }

        return new string(result.ToArray());
    }

    public static void WriteNullTerminatedString(this BinaryWriter writer, string value)
    {
        foreach (var character in value)
        {
            writer.Write(character);
        }

        writer.Write((byte)0x00);
    }
}