using System.IO;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Persistence;

public class Repository
{
    public static void Save<TRepository>(TRepository repository, string path)
    {
        IOHelper.EnsureDirectoriesExistForFilePath(path);
        MemoryPackHelper.SerializeCompressed(path, repository);
    }

    public static TRepository Load<TRepository>(string path) where TRepository : new()
    {
        return File.Exists(path)
            ? MemoryPackHelper.DeserializeCompressed<TRepository>(path)
            : default;
    }
}