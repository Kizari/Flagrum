using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Flagrum.Core.Utilities;

public static class Serialization
{
    public static uint Align(uint offset, uint blockSize)
    {
        return blockSize + blockSize * (offset / blockSize) - offset;
    }
    
    /// <summary>
    /// Aligns current offset to the given block size
    /// </summary>
    /// <param name="offset">The offset of the end of the data that needs to be aligned</param>
    /// <param name="blockSize">The size to align to</param>
    /// <returns>The offset of the end of the alignment</returns>
    public static uint GetAlignment(uint offset, uint blockSize)
    {
        return blockSize + blockSize * (offset / blockSize);
    }

    public static ulong GetAlignment(ulong offset, ulong blockSize)
    {
        return blockSize + blockSize * (offset / blockSize);
    }

    public static string ToSafeString(this string input)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.Any(char.IsLetter))
        {
            return Guid.NewGuid().ToString().ToLower();
        }

        while (!char.IsLetter(input[0]) && input.Length > 0)
        {
            input = input[1..];
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            return Guid.NewGuid().ToString().ToLower();
        }

        input = input.Replace('.', '_');
        input = input.Replace('-', '_');
        input = input.Replace(' ', '_');

        var output = "";
        foreach (var character in input)
        {
            if (char.IsLetterOrDigit(character) || character == '_')
            {
                output += character;
            }
        }

        return string.IsNullOrWhiteSpace(input) ? Guid.NewGuid().ToString().ToLower() : output.ToLower();
    }

    public static string ToBase64(this string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    public static string FromBase64(this string input)
    {
        var bytes = Convert.FromBase64String(input);
        return Encoding.UTF8.GetString(bytes);
    }

    public static void Align(this Stream stream, int blockSize)
    {
        var size = Align((uint)stream.Position, (uint)blockSize);
        if (size == blockSize)
        {
            return;
        }
        
        stream.Seek(size, SeekOrigin.Current);
    }

    public static void Align(this BinaryWriter writer, int blockSize, byte paddingByte)
    {
        var size = Align((uint)writer.BaseStream.Position, (uint)blockSize);
        if (size == blockSize)
        {
            return;
        }
        
        for (var i = 0; i < size; i++)
        {
            writer.Write(paddingByte);
        }
    }
    
    public static void Align(this BinaryWriter writer, int blockSize, byte paddingByte)
    {
        var size = Align((uint)writer.BaseStream.Position, (uint)blockSize);
        if (size == blockSize)
        {
            return;
        }
        
        for (var i = 0; i < size; i++)
        {
            writer.Write(paddingByte);
        }
    }
    
    public static void Align(this BinaryReader reader, int blockSize)
    {
        var size = Align((uint)reader.BaseStream.Position, (uint)blockSize);
        if (size == blockSize)
        {
            return;
        }

        reader.BaseStream.Seek(size, SeekOrigin.Current);
    }
    
    public static void CopyTo(this FileStream source, FileStream destination, uint count)
    {
        var bytesRemaining = count;
        while (bytesRemaining > 0)
        {
            var readSize = Math.Min(4096, bytesRemaining);
            var buffer = new byte[readSize];
            _ = source.Read(buffer);

            destination.Write(buffer);
            bytesRemaining -= readSize;
        }
    }
}