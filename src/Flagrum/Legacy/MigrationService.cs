using System;
using System.Collections.Generic;
using System.IO;
using Flagrum.Core.Utilities;
using Injectio.Attributes;
using MemoryPack;
using ZstdSharp;

namespace Flagrum.Legacy;

public class DummyService;

[MemoryPackable]
[RegisterSingleton<MigrationService>]
public partial class MigrationService
{
    [MemoryPackConstructor]
    public MigrationService() { }

    public MigrationService(DummyService dummy)
    {
        if (File.Exists(FilePath))
        {
            var buffer = File.ReadAllBytes(FilePath);
            var decompressor = new Decompressor();
            var self = this;
            MemoryPackSerializer.Deserialize(decompressor.Unwrap(buffer), ref self,
                MemoryPackSerializerOptions.Utf8);
        }
    }

    private static string FilePath => Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "migrations.fms");

    [MemoryPackInclude] public HashSet<Guid> Completed { get; set; } = [];

    public void Delete()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}