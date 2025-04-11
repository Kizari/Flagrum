using System;
using System.Collections.Generic;
using System.IO;
using Flagrum.Abstractions;
using Flagrum.Core.Persistence;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Application.Utilities;
using MemoryPack;

namespace Flagrum.Application.Features.Settings.Data;

[MemoryPackable]
public partial class Profile : IProfile
{
    private IConfiguration _configuration;
    
    [MemoryPackConstructor]
    private Profile() { }

    public Profile(IConfiguration configuration)
    {
        _configuration = configuration;
    }
        
    [MemoryPackInclude] [ConcurrentProperty] private Guid _id;
    [MemoryPackInclude] [ConcurrentProperty] private LuminousGame _type;
    [MemoryPackInclude] [ConcurrentProperty] private string _name;
    [MemoryPackInclude] [ConcurrentProperty] private string _gamePath;
    [MemoryPackInclude] [ConcurrentProperty] private string _binmodListPath;
    [MemoryPackInclude] private HashSet<Guid> _migrations = [];
    private HashSet<Guid> Migrations => _migrations ??= [];
    public bool HasUpgradedToSteppedMigrations { get; set; }
    [MemoryPackInclude] [ConcurrentProperty] private Version _lastSeenVersion;

    [MemoryPackInclude] [ConcurrentProperty]
    private string _gameExeHash;

    [MemoryPackInclude] [ConcurrentProperty]
    private long _gameExeHashTime;

    // ReSharper disable once InconsistentlySynchronizedField
    public void SetConfiguration(IConfiguration configuration) => _configuration = configuration;

    private void Save()
    {
        lock (_configuration)
        {
            Repository.Save(_configuration, Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "configuration.fcg"));
        }
    }

    public bool HasMigrated(Guid migration) => Migrations.Contains(migration);

    public void SetMigrated(Guid migration)
    {
        Migrations.Add(migration);
        Save();
    }

    public void SetMigratedNoSave(IEnumerable<Guid> migrations) => Migrations.AddRange(migrations);
    
    public sealed class Formatter : MemoryPackFormatter<IProfile>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, 
            scoped ref IProfile value)
        {
            writer.WritePackable((Profile)value);
        }
        
        public override void Deserialize(ref MemoryPackReader reader, scoped ref IProfile value)
        {
            value = reader.ReadPackable<Profile>();
        }
    }
}