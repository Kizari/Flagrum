using System;
using System.IO;
using Flagrum.Core.Gfxbin.Gmdl.Components;

namespace Flagrum.Core.Gfxbin.Gmdl.Buffering;

public class GpubinPacker
{
    private const uint ChunkSize = 64;

    private readonly MemoryStream _gpubinStream = new();

    public (uint offset, uint size) PackFaceIndices(int[,] faceIndices, IndexType faceIndexType)
    {
        var stream = new MemoryStream();

        Func<int, byte[]> pack = faceIndexType switch
        {
            IndexType.IndexType32 => BitConverter.GetBytes,
            IndexType.IndexType16 => index => BitConverter.GetBytes((ushort)index),
            _ => throw new ArgumentException("Provided face index type not supported.", nameof(faceIndexType))
        };

        for (var i = 0; i < faceIndices.Length / 3; i++)
        {
            stream.Write(pack(faceIndices[i, 0]));
            stream.Write(pack(faceIndices[i, 1]));
            stream.Write(pack(faceIndices[i, 2]));
        }

        var offset = _gpubinStream.Position;
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(_gpubinStream);

        AlignToChunk();
        return ((uint)offset, (uint)stream.Length);
    }

    public uint PackVertexBuffer(Stream vertexBufferStream)
    {
        var offset = _gpubinStream.Position;
        vertexBufferStream.Seek(0, SeekOrigin.Begin);
        vertexBufferStream.CopyTo(_gpubinStream);

        AlignToChunk();
        return (uint)offset;
    }

    private void AlignToChunk()
    {
        var currentPosition = _gpubinStream.Position;
        var alignment = Core.Utilities.Serialization.GetAlignment((uint)currentPosition, ChunkSize);

        // Only write padding if it isn't already the end of the chunk
        if (alignment - currentPosition != ChunkSize)
        {
            _gpubinStream.Seek(alignment, SeekOrigin.Begin);
        }
    }

    public byte[] ToArray()
    {
        return _gpubinStream.ToArray();
    }
}