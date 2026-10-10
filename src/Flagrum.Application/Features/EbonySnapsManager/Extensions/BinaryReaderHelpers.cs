using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

/// <summary>
/// Provides endianness for BinaryReader methods.
/// </summary>
public static class BinaryReaderHelpers
{
    /// <summary>
    /// Reads a 2-byte Signed integer from the current stream 
    /// and advances the position of the stream by two bytes.
    /// </summary>
    /// <returns>
    /// A 2-byte Signed integer read from this stream.
    /// </returns>
    /// <param name="isBigEndian">Indicates whether the bytes are read in Big Endian.</param>
    public static short ReadBytesInt16(this BinaryReader reader, bool isBigEndian)
    {
        var readValueBuffer = reader.ReadBytes(2);
        ReverseBuffer(isBigEndian, readValueBuffer);

        return BitConverter.ToInt16(readValueBuffer, 0);
    }


    /// <summary>
    /// Reads a 2-byte Unsigned integer from the current stream 
    /// and advances the position of the stream by two bytes.
    /// </summary>
    /// <returns>
    /// A 2-byte Unsigned integer read from this stream.
    /// </returns>
    /// <param name="isBigEndian">Indicates whether the bytes are read in Big Endian.</param>
    public static ushort ReadBytesUInt16(this BinaryReader reader, bool isBigEndian)
    {
        var readValueBuffer = reader.ReadBytes(2);
        ReverseBuffer(isBigEndian, readValueBuffer);

        return BitConverter.ToUInt16(readValueBuffer, 0);
    }


    /// <summary>
    /// Reads a 4-byte Signed integer from the current stream 
    /// and advances the position of the stream by four bytes.
    /// </summary>
    /// <returns>
    /// A 4-byte Signed integer read from this stream.
    /// </returns>
    /// <param name="isBigEndian">Indicates whether the bytes are read in Big Endian.</param>
    public static int ReadBytesInt32(this BinaryReader reader, bool isBigEndian)
    {
        var readValueBuffer = reader.ReadBytes(4);
        ReverseBuffer(isBigEndian, readValueBuffer);

        return BitConverter.ToInt32(readValueBuffer, 0);
    }


    /// <summary>
    /// Reads a 4-byte Unsigned integer from the current stream 
    /// and advances the position of the stream by four bytes.
    /// </summary>
    /// <returns>
    /// A 4-byte Unsigned integer read from this stream.
    /// </returns>
    /// <param name="isBigEndian">Indicates whether the bytes are read in Big Endian.</param>
    public static uint ReadBytesUInt32(this BinaryReader reader, bool isBigEndian)
    {
        var readValueBuffer = reader.ReadBytes(4);
        ReverseBuffer(isBigEndian, readValueBuffer);

        return BitConverter.ToUInt32(readValueBuffer, 0);
    }


    /// <summary>
    /// Reads a 8-byte Signed integer from the current stream 
    /// and advances the position of the stream by eight bytes.
    /// </summary>
    /// <returns>
    /// A 8-byte Signed integer read from this stream.
    /// </returns>
    /// <param name="isBigEndian">Indicates whether the bytes are read in Big Endian.</param>
    public static long ReadBytesInt64(this BinaryReader reader, bool isBigEndian)
    {
        var readValueBuffer = reader.ReadBytes(8);
        ReverseBuffer(isBigEndian, readValueBuffer);

        return BitConverter.ToInt64(readValueBuffer, 0);
    }


    /// <summary>
    /// Reads a 8-byte Unsigned integer from the current stream and advances the position 
    /// of the stream by eight bytes.
    /// </summary>
    /// <returns>
    /// A 8-byte Unsigned integer read from this stream.
    /// </returns>
    /// <param name="isBigEndian">Indicates whether the bytes are read in Big Endian.</param>
    public static ulong ReadBytesUInt64(this BinaryReader reader, bool isBigEndian)
    {
        var readValueBuffer = reader.ReadBytes(8);
        ReverseBuffer(isBigEndian, readValueBuffer);

        return BitConverter.ToUInt64(readValueBuffer, 0);
    }


    /// <summary>
    /// Reads a 4-byte floating point value from the current stream and advances the
    /// current position of the stream by four bytes.
    /// </summary>
    /// <returns>
    /// A 4-byte floating point value read from the current stream.
    /// </returns>
    /// <param name="isBigEndian">Indicates whether the bytes are read in Big Endian.</param>
    public static float ReadBytesFloat(this BinaryReader reader, bool isBigEndian)
    {
        var readValueBuffer = reader.ReadBytes(4);
        ReverseBuffer(isBigEndian, readValueBuffer);

        return BitConverter.ToSingle(readValueBuffer, 0);
    }


    /// <summary>
    /// Reads an 8-byte floating point value from the current stream and advances the
    /// current position of the stream by eight bytes.
    /// </summary>
    /// <returns>
    /// An 8-byte floating point value read from the current stream.
    /// </returns>
    /// <param name="isBigEndian">Indicates whether the bytes are read in Big Endian.</param>
    public static double ReadBytesDouble(this BinaryReader reader, bool isBigEndian)
    {
        var readValueBuffer = reader.ReadBytes(8);
        ReverseBuffer(isBigEndian, readValueBuffer);

        return BitConverter.ToDouble(readValueBuffer, 0);
    }


    /// <summary>
    /// Reads the specified number of bytes from the current stream and builds a string. then it advances the
    /// current position of the stream by the number of bytes read.
    /// </summary>
    /// <returns>
    /// A string built from the bytes read from the current stream. encoding of the string will be UTF8.
    /// </returns>
    /// <param name="readCount">The number of bytes to read.</param>
    /// <param name="shouldReverse">Indicates whether the bytes should be reversed.</param>
    public static string ReadBytesString(this BinaryReader reader, int readCount, bool shouldReverse)
    {
        var readValueBuffer = reader.ReadBytes(readCount);
        ReverseBuffer(shouldReverse, readValueBuffer);

        return Encoding.UTF8.GetString(readValueBuffer).Replace("\0", "");
    }


    /// <summary>
    /// Reads bytes until a null byte is encountered and builds a string 
    /// with the bytes read from the current stream. 
    /// then it advances the current position of the stream by the number of bytes read.
    /// </summary>
    /// <returns>
    /// A string built from the bytes read from the current stream. encoding of the string will be 
    /// similar to the encoding used in the BinaryReader.
    /// </returns>
    public static string ReadStringTillNull(this BinaryReader reader)
    {
        var sb = new StringBuilder();
        char chars;
        while ((chars = reader.ReadChar()) != default)
        {
            sb.Append(chars);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Reads bytes until a null byte is encountered and builds a list of the bytes 
    /// read from the current stream. then it advances the current position of the stream 
    /// by the number of bytes read.
    /// </summary>
    /// <returns>
    /// A list of bytes read from the current stream.
    /// </returns>
    public static List<byte> ReadBytesTillNull(this BinaryReader reader)
    {
        var byteList = new List<byte>();
        byte currentValue;
        while ((currentValue = reader.ReadByte()) != default)
        {
            byteList.Add(currentValue);
        }

        return byteList;
    }


    private static void ReverseBuffer(bool isBigEndian, byte[] readValueBuffer)
    {
        if (isBigEndian)
        {
            Array.Reverse(readValueBuffer);
        }
    }


    /// <summary>
    /// Reads byte values to each non-static public variables, declared in a class. 
    /// the amount of bytes read, value derivation, and assignment will all depend 
    /// on the variable type.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// To ensure that the bytes are read in BigEndian for a variable, specify a '_BE' suffix at the end of 
    /// the variable name.
    /// </para>
    /// </remarks>
    /// 
    /// <param name="classVar">The name of the class containing the variables.</param>
    public static void AssignByteValuesInClass(this BinaryReader reader, object classVar)
    {
        var classFields = classVar.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in classFields)
        {
            if (!field.IsStatic)
            {
                switch (field.FieldType.Name)
                {
                    case "SByte":
                        field.SetValue(classVar, reader.ReadSByte());
                        break;

                    case "Byte":
                        field.SetValue(classVar, reader.ReadByte());
                        break;

                    case "Int32":
                        field.SetValue(classVar, field.Name.EndsWith("_BE") ?
                            reader.ReadBytesInt32(true) : reader.ReadBytesInt32(false));
                        break;

                    case "UInt32":
                        field.SetValue(classVar, field.Name.EndsWith("_BE") ?
                            reader.ReadBytesUInt32(true) : reader.ReadBytesUInt32(false));
                        break;

                    case "UInt64":
                        field.SetValue(classVar, field.Name.EndsWith("_BE") ?
                            reader.ReadBytesUInt64(true) : reader.ReadBytesUInt64(false));
                        break;

                    case "Int64":
                        field.SetValue(classVar, field.Name.EndsWith("_BE") ?
                            reader.ReadBytesInt64(true) : reader.ReadBytesInt64(false));
                        break;

                    case "String":
                        field.SetValue(classVar, reader.ReadStringTillNull());
                        break;

                    case "Float":
                        field.SetValue(classVar, field.Name.EndsWith("_BE") ?
                            reader.ReadBytesFloat(true) : reader.ReadBytesFloat(false));
                        break;

                    case "Double":
                        field.SetValue(classVar, field.Name.EndsWith("_BE") ?
                            reader.ReadBytesDouble(true) : reader.ReadBytesDouble(false));
                        break;
                }
            }
        }
    }
}