using System;
using System.Data;
using System.IO;
using System.Reflection;
using Flagrum.Abstractions;
using Flagrum.Application.Persistence.Entities;
using Flagrum.Application.Persistence.Entities.ModManager;
using Flagrum.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Application.Persistence;

public class IndexCount
{
    public string name { get; set; }
    public int seq { get; set; }
}

public class FlagrumDbContext : DbContext
{
    private readonly string _databasePath;

    /// <summary>
    /// This should only be used by EF migrations
    /// </summary>
    public FlagrumDbContext()
    {
        _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Flagrum", "Profiles", "1e333018-307a-478d-8d91-d1e234df737f", "flagrum.db");
    }

    public FlagrumDbContext(IProfileService profile)
    {
        Profile = profile;
        _databasePath = profile.DatabasePath;
    }

    public IProfileService Profile { get; }

    public DbSet<EarcModBackup> EarcModBackups { get; set; }
    public DbSet<EarcMod> EarcMods { get; set; }
    public DbSet<EarcModEarc> EarcModEarcs { get; set; }
    public DbSet<EarcModFile> EarcModReplacements { get; set; }
    public DbSet<EarcModLooseFile> EarcModLooseFile { get; set; }

    public DbSet<AssetExplorerNode> AssetExplorerNodes { get; set; }
    public DbSet<ArchiveLocation> ArchiveLocations { get; set; }
    public DbSet<AssetUri> AssetUris { get; set; }
    public DbSet<StatePair> StatePairs { get; set; }

    public DbSet<ModelReplacementPresetEntity> ModelReplacementPresets { get; set; }
    public DbSet<ModelReplacementPathEntity> ModelReplacementPaths { get; set; }
    public DbSet<ModelReplacementFavouriteEntity> ModelReplacementFavourites { get; set; }
    public DbSet<IndexCount> IndexCounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_databasePath};", options => { options.CommandTimeout(180); });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetAssembly(typeof(FlagrumDbContext))
            ?? throw new InvalidOperationException("Assembly cannot be null"));

        modelBuilder.Entity<IndexCount>().HasNoKey().ToTable((string)null);
    }

    public bool DoesTableExist(string tableName)
    {
        var connection = Database.GetDbConnection();
        if (connection.State == ConnectionState.Closed)
        {
            connection.Open();
        }

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        return command.ExecuteScalar() != null;
    }
}