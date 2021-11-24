using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Flagrum.Archiver.Data;
using Flagrum.Core.Utilities;

namespace Flagrum.Archiver
{
    public class Unpacker
    {
        private const ulong KeyMultiplier = 1103515245;
        private const ulong KeyAdditive = 12345;

        private readonly MemoryStream _stream;
        private ArchiveHeader _header;

        public Unpacker(string archivePath)
        {
            _stream = new MemoryStream(File.ReadAllBytes(archivePath));
        }

        public IEnumerable<ArchiveFile> Unpack()
        {
            _header = ReadHeader();
            return ReadFiles();
        }

        private IEnumerable<ArchiveFile> ReadFiles()
        {
            var hash = ArchiveFile.HeaderHash ^ _header.Hash;

            for (var i = 0; i < _header.FileCount; i++)
            {
                _stream.Seek(_header.FileHeadersOffset + i * ArchiveFile.HeaderSize, SeekOrigin.Begin);

                var file = new ArchiveMemoryFile();
                file.UriAndTypeHash = ReadUint64();
                file.Size = ReadUint32();
                file.ProcessedSize = ReadUint32();
                file.Flags = (ArchiveFileFlag)ReadUint32();
                file.UriOffset = ReadUint32();
                file.DataOffset = ReadUint64();
                file.RelativePathOffset = ReadUint32();
                file.LocalizationType = ReadByte();
                file.Locale = ReadByte();
                file.Key = ReadUint16();

                var subhash = Cryptography.MergeHashes(hash, file.UriAndTypeHash);
                file.Size ^= (uint)(subhash >> 32);
                file.ProcessedSize ^= (uint)subhash;
                hash = Cryptography.MergeHashes(subhash, ~file.UriAndTypeHash);
                file.DataOffset ^= hash;

                _stream.Seek(file.UriOffset, SeekOrigin.Begin);
                file.Uri = ReadString();

                _stream.Seek((long)file.DataOffset, SeekOrigin.Begin);
                var buffer = new byte[file.ProcessedSize];
                _stream.Read(buffer, 0, (int)file.ProcessedSize);

                if (file.Key > 0)
                {
                    var partialKey = file.Key * KeyMultiplier + KeyAdditive;
                    var finalKey = partialKey * KeyMultiplier + KeyAdditive;

                    var firstNumber = BitConverter.ToUInt32(buffer, 0);
                    var secondNumber = BitConverter.ToUInt32(buffer, 4);

                    firstNumber ^= (uint)(finalKey >> 32);
                    secondNumber ^= (uint)finalKey;

                    var firstKey = BitConverter.GetBytes(firstNumber);
                    var secondKey = BitConverter.GetBytes(secondNumber);

                    for (var k = 0; k < 4; k++)
                    {
                        buffer[k] = firstKey[k];
                    }

                    for (var k = 0; k < 4; k++)
                    {
                        buffer[k + 4] = secondKey[k];
                    }
                }

                file.SetData(buffer);

                yield return file;
            }
        }

        private ArchiveHeader ReadHeader()
        {
            var header = new ArchiveHeader();

            var buffer = new byte[4];
            _stream.Read(buffer, 0, 4);
            header.Tag = buffer;

            header.Version = ReadUint32();
            header.FileCount = ReadUint32();
            header.BlockSize = ReadUint32();
            header.FileHeadersOffset = ReadUint32();
            header.UriListOffset = ReadUint32();
            header.PathListOffset = ReadUint32();
            header.DataOffset = ReadUint32();
            header.Flags = ReadUint32();
            header.ChunkSize = ReadUint32();
            header.Hash = ReadUint64();

            return header;
        }

        private byte ReadByte()
        {
            var buffer = new byte[1];
            _stream.Read(buffer, 0, 1);
            return buffer[0];
        }

        private ushort ReadUint16()
        {
            var buffer = new byte[2];
            _stream.Read(buffer, 0, 2);
            return BitConverter.ToUInt16(buffer);
        }

        private uint ReadUint32()
        {
            var buffer = new byte[4];
            _stream.Read(buffer, 0, 4);
            return BitConverter.ToUInt32(buffer);
        }

        private ulong ReadUint64()
        {
            var buffer = new byte[8];
            _stream.Read(buffer, 0, 8);
            return BitConverter.ToUInt64(buffer);
        }

        private string ReadString()
        {
            var builder = new StringBuilder();

            var buffer = new byte[1];
            while (true)
            {
                _stream.Read(buffer, 0, 1);

                if (buffer[0] == byte.MinValue)
                {
                    break;
                }

                builder.Append(Convert.ToChar(buffer[0]));
            }

            return builder.ToString();
        }
    }
}