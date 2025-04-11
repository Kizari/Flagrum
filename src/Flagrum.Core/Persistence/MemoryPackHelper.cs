using System.IO;
using System.Threading.Tasks;
using Flagrum.Core.Utilities.Extensions;
using MemoryPack;
using ZstdSharp;

namespace Flagrum.Core.Persistence;

public static class MemoryPackHelper
{
    public static void SerializeCompressed<T>(string path, T data)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var compressionStream = new CompressionStream(stream);
        MemoryPackSerializer.SerializeAsync(compressionStream, data, MemoryPackSerializerOptions.Utf8)
            .AwaitSynchronous();
    }

    public static async Task SerializeCompressedAsync<T>(string path, T data)
    {
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        await using var compressionStream = new CompressionStream(stream);
        await MemoryPackSerializer.SerializeAsync(compressionStream, data, MemoryPackSerializerOptions.Utf8);
    }

    public static T DeserializeCompressed<T>(string path)
    {
        var decompressor = new Decompressor();
        return MemoryPackSerializer.Deserialize<T>(decompressor.Unwrap(File.ReadAllBytes(path)));
    }

    public static async ValueTask<T?> DeserializeCompressedAsync<T>(string path)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        await using var decompressionStream = new DecompressionStream(stream);
        return await MemoryPackSerializer.DeserializeAsync<T>(decompressionStream, MemoryPackSerializerOptions.Utf8);
    }
}