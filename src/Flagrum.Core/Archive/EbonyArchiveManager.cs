using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Archive;

public class EbonyArchiveManager : IDisposable
{
    private readonly Dictionary<ulong, EbonyArchive> _archives = new();

    public EbonyArchive Open(string filePath)
    {
        EbonyArchive archive;
        var hash = Cryptography.Hash64(filePath);

        lock (_archives)
        {
            if (_archives.TryGetValue(hash, out archive))
            {
                return archive;
            }

            archive = new EbonyArchive(filePath);
            _archives[hash] = archive;
        }

        return archive;
    }
        
    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    public void Dispose()
    {
        foreach (var (_, archive) in _archives)
        {
            archive?.Dispose();
        }
    }
}