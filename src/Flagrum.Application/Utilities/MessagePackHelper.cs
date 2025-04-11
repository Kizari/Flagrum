using System.IO;
using System.Threading.Tasks;
using MessagePack;
using ZstdSharp;

namespace Flagrum.Application.Utilities;

public static class MessagePackHelper
{
    public static async Task SerializeCompressedAsync<TObject>(string path, TObject data)
    {
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        await using var compressionStream = new CompressionStream(stream);
        await MessagePackSerializer.SerializeAsync(compressionStream, data);
    }

    public static async Task<TObject> DeserializeCompressedAsync<TObject>(string path)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        await using var decompressionStream = new DecompressionStream(stream);
        return await MessagePackSerializer.DeserializeAsync<TObject>(decompressionStream);
    }
}