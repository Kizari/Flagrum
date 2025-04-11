using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Core.Persistence;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Generators;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Application.Services;
using MemoryPack;
using Microsoft.Extensions.DependencyInjection;
using ZstdSharp;

namespace Flagrum.Migrations;

[MemoryPackable]
[InjectableDependency(ServiceLifetime.Singleton)]
public partial class MigrationService
{
    private static string FilePath => Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "migrations.fms");

    [MemoryPackInclude] public HashSet<Guid> Completed { get; set; } = [];
    
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

    public void Delete()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}