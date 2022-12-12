using System;
using System.Reflection;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Persistence.Entities.ModManager;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace Flagrum.Web.Persistence;

public class IndexCount
{
    public string name { get; set; }
    public int seq { get; set; }
}

public class FlagrumDbContext : DbContext
{
    private string _databasePath =
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Flagrum\flagrum.db";

    public FlagrumDbContext() { }

    public FlagrumDbContext(SettingsService settings)
    {
        Settings = settings;
    }

    public FlagrumDbContext(SettingsService settings, string pathOverride)
    {
        Settings = settings;
        _databasePath = pathOverride;
    }

    public SettingsService Settings { get; }

    public DbSet<Ps4VertexLayoutTypeMap> Ps4VertexLayoutTypeMaps { get; set; }
    public DbSet<Ps4ArchiveAsset> Ps4ArchiveAssets { get; set; }
    public DbSet<Ps4AssetUri> Ps4AssetUris { get; set; }
    public DbSet<Ps4ArchiveLocation> Ps4ArchiveLocations { get; set; }
    public DbSet<FestivalMaterialDependency> FestivalMaterialDependencies { get; set; }
    public DbSet<FestivalModelDependency> FestivalModelDependencies { get; set; }
    public DbSet<FestivalSubdependency> FestivalSubdependencies { get; set; }
    public DbSet<FestivalDependency> FestivalDependencies { get; set; }
    public DbSet<FestivalDependencyFestivalDependency> FestivalDependencyFestivalDependency { get; set; }
    public DbSet<FestivalDependencyFestivalSubdependency> FestivalDependencyFestivalSubdependency { get; set; }
    public DbSet<FestivalSubdependencyFestivalModelDependency> FestivalSubdependencyFestivalModelDependency { get; set; }
    public DbSet<FestivalModelDependencyFestivalMaterialDependency> FestivalModelDependencyFestivalMaterialDependency { get; set; }
    public DbSet<EarcModBackup> EarcModBackups { get; set; }
    public DbSet<EarcMod> EarcMods { get; set; }
    public DbSet<EarcModEarc> EarcModEarcs { get; set; }
    public DbSet<EarcModFile> EarcModReplacements { get; set; }
    public DbSet<EarcModLooseFile> EarcModLooseFile { get; set; }
    public DbSet<AssetExplorerNode> AssetExplorerNodes { get; set; }
    public DbSet<ArchiveLocation> ArchiveLocations { get; set; }
    public DbSet<AssetUri> AssetUris { get; set; }
    public DbSet<StatePair> StatePairs { get; set; }

    public DbSet<ModelReplacementPreset> ModelReplacementPresets { get; set; }
    public DbSet<ModelReplacementPath> ModelReplacementPaths { get; set; }
    public DbSet<ModelReplacementFavourite> ModelReplacementFavourites { get; set; }
    public DbSet<IndexCount> IndexCounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //_databasePath = @"C:\Users\Kieran\AppData\Local\Flagrum-PS4\flagrum.db";
        optionsBuilder.UseSqlite($"Data Source={_databasePath};", options => { options.CommandTimeout(180); });
        Batteries.Init();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetAssembly(typeof(FlagrumDbContext))
            ?? throw new InvalidOperationException("Assembly cannot be null"));

        modelBuilder.Entity<IndexCount>().HasNoKey().ToTable((string)null);
    }

    public void ClearTable(string name)
    {
        Database.ExecuteSqlRaw($"DELETE FROM {name};");
        Database.ExecuteSqlRaw($"DELETE FROM SQLITE_SEQUENCE WHERE name='{name}';");
    }
}