using Flagrum.Archiver.Models;
using Flagrum.Archiver.Utilities;
using Flagrum.Core.Services.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Flagrum.Archiver
{
    public class Archive
    {
        public const uint PointerSize = 8;
        public const uint BlockSize = 512;
        public const ulong FileHeaderHash = 14695981039346656037;

        private readonly ArchiveHeader _header = new();
        private List<ArchiveFile> _files = new();

        private readonly Logger _logger = new ConsoleLogger();
        private readonly Encoding _encoding = new();

        private string _root;

        public Archive(string root) => _root = root;

        public void AddFile(string path)
        {
            _files.Add(new ArchiveFile(_root, path));
        }

        public Stream Pack()
        {
            var archiveStream = new MemoryStream();
            _logger.LogInformation("Packing archive...");

            _files = _files
                .OrderBy(f => f.TypeHash)
                .ThenBy(f => f.UriHash)
                .ToList();

            _header.UriListOffset = (uint)(ArchiveHeader.Size + Serialization.GetPadding((uint)_files.Count * ArchiveHeader.ListingItemSize, PointerSize));

            var endOfUriList = SerializeUriList(out Stream uriListStream);
            archiveStream.Seek(_header.UriListOffset, SeekOrigin.Begin);
            uriListStream.CopyTo(archiveStream);

            _header.PathListOffset = (uint)(Serialization.GetPadding((uint)(_header.UriListOffset + endOfUriList), PointerSize));

            var endOfPathList = SerializePathList(out Stream pathListStream);
            archiveStream.Seek(_header.PathListOffset, SeekOrigin.Begin);
            pathListStream.CopyTo(archiveStream);

            _header.DataOffset = (uint)Serialization.GetPadding(_header.PathListOffset + (uint)endOfPathList, BlockSize);

            archiveStream.Seek(_header.DataOffset, SeekOrigin.Begin);
            var dataStream = SerializeData(archiveStream);
            //archiveStream.Seek(_header.DataOffset, SeekOrigin.Begin);
            //dataStream.CopyTo(archiveStream);

            var headerStream = SerializeHeader();
            archiveStream.Seek(0, SeekOrigin.Begin);
            headerStream.CopyTo(archiveStream);

            var fileHeaderStream = SerializeFileHeaders();
            archiveStream.Seek(ArchiveHeader.Size, SeekOrigin.Begin);
            Console.WriteLine(fileHeaderStream.Length + " / " + _files.Count * ArchiveHeader.ListingItemSize);
            fileHeaderStream.CopyTo(archiveStream);

            archiveStream.Seek(0, SeekOrigin.Begin);
            return archiveStream;
        }

        private Stream SerializeHeader()
        {
            _logger.LogInformation("Serializing Archive Header");

            var stream = new MemoryStream();

            stream.Write(_header.Tag);
            stream.Write(BitConverter.GetBytes(_header.Version));
            stream.Write(BitConverter.GetBytes((uint)_files.Count));
            stream.Write(BitConverter.GetBytes(BlockSize));
            stream.Write(BitConverter.GetBytes(ArchiveHeader.Size));
            stream.Write(BitConverter.GetBytes(_header.UriListOffset));
            stream.Write(BitConverter.GetBytes(_header.PathListOffset));
            stream.Write(BitConverter.GetBytes(_header.DataOffset));
            stream.Write(BitConverter.GetBytes((uint)0));                   // Flags are always zero
            stream.Write(BitConverter.GetBytes(_header.ChunkSize));

            // Archive hash must be zero before the whole header is hashed
            stream.Write(BitConverter.GetBytes((ulong)0));

            // Constant padding
            stream.Write(new byte[16]);

            _header.Hash = _encoding.Base64Hash(stream.ToArray());

            stream = new MemoryStream();

            stream.Write(_header.Tag);
            stream.Write(BitConverter.GetBytes(_header.Version));
            stream.Write(BitConverter.GetBytes((uint)_files.Count));
            stream.Write(BitConverter.GetBytes(BlockSize));
            stream.Write(BitConverter.GetBytes(ArchiveHeader.Size));
            stream.Write(BitConverter.GetBytes(_header.UriListOffset));
            stream.Write(BitConverter.GetBytes(_header.PathListOffset));
            stream.Write(BitConverter.GetBytes(_header.DataOffset));
            stream.Write(BitConverter.GetBytes((uint)0));
            stream.Write(BitConverter.GetBytes(_header.ChunkSize));
            stream.Write(BitConverter.GetBytes(_header.Hash));

            // Constant padding
            stream.Write(new byte[16]);

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private Stream SerializeFileHeaders()
        {
            _logger.LogInformation("Serializing File Headers");

            var stream = new MemoryStream();
            var hash = FileHeaderHash ^ _header.Hash;

            foreach (var file in _files)
            {
                var size = file.Size;
                var processedSize = file.ProcessedSize;
                var dataOffset = file.DataOffset;
                ushort key = 0;

                if (!file.Flags.HasFlag(ArchiveFlag.MaskProtected))
                {
                    key = file.Key;
                    hash = _encoding.MergeHashes(hash, file.UriAndTypeHash);
                    size ^= (uint)(hash >> 32);
                    processedSize ^= (uint)hash;
                    hash = _encoding.MergeHashes(hash, ~file.UriAndTypeHash);
                    dataOffset ^= hash;
                }

                stream.Write(BitConverter.GetBytes(file.UriAndTypeHash));
                stream.Write(BitConverter.GetBytes(size));
                stream.Write(BitConverter.GetBytes(processedSize));
                stream.Write(BitConverter.GetBytes((uint)file.Flags));
                stream.Write(BitConverter.GetBytes(file.UriOffset));
                stream.Write(BitConverter.GetBytes(dataOffset));
                stream.Write(BitConverter.GetBytes(file.RelativePathOffset));
                stream.WriteByte(ArchiveFile.LocalizationType);
                stream.WriteByte(ArchiveFile.Locale);
                stream.Write(BitConverter.GetBytes(key));
            }

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private int SerializeUriList(out Stream uriListStream)
        {
            _logger.LogInformation("Serializing File URIs");

            uriListStream = new MemoryStream();
            var currentUriOffset = 0;

            foreach (var file in _files)
            {
                var size = Serialization.EncodeString(file.Uri, out byte[] bytes);
                file.UriOffset = _header.UriListOffset + (uint)currentUriOffset;
                uriListStream.Seek(currentUriOffset, SeekOrigin.Begin);
                uriListStream.Write(bytes, 0, size);
                currentUriOffset += (int)Serialization.GetPadding((uint)size, PointerSize);
            }

            uriListStream.Seek(0, SeekOrigin.Begin);
            return currentUriOffset;
        }

        private int SerializePathList(out Stream pathListStream)
        {
            _logger.LogInformation("Serializing File Paths");

            pathListStream = new MemoryStream();
            var currentPathOffset = 0;

            foreach (var file in _files)
            {
                var size = Serialization.EncodeString(file.RelativePath, out byte[] bytes);
                file.RelativePathOffset = _header.PathListOffset + (uint)currentPathOffset;
                pathListStream.Seek(currentPathOffset, SeekOrigin.Begin);
                pathListStream.Write(bytes, 0, size);
                currentPathOffset += (int)Serialization.GetPadding((uint)size, PointerSize);
            }

            pathListStream.Seek(0, SeekOrigin.Begin);
            return currentPathOffset;
        }

        private Stream SerializeData(Stream stream)
        {
            _logger.LogInformation("Serializing File Data");

            //var stream = new MemoryStream();
            var currentDataOffset = 0L;
            var rng = new Random((int)_header.Hash);

            foreach (var file in _files)
            {
                file.DataOffset = _header.DataOffset + (uint)currentDataOffset;

                if (file.Uri.Contains("temp"))
                {
                    bool x = true;
                }

                var fileData = file.GetFileData(_encoding);
                stream.Write(fileData, 0, fileData.Length);

                var finalSize = Serialization.GetPadding(file.ProcessedSize, BlockSize);
                var paddingSize = finalSize - file.ProcessedSize;
                var padding = new byte[paddingSize];
                rng.NextBytes(padding);
                stream.Write(padding, 0, (int)paddingSize);

                currentDataOffset += (long)finalSize;
            }

            //stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}
