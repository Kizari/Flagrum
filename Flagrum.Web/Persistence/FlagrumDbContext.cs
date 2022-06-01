using System;
using System.Linq;
using System.Reflection;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Web.Persistence;

public class FlagrumDbContext : DbContext
{
    private readonly string _databasePath =
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

    public DbSet<FestivalFinalDependency> FestivalFinalDependencies { get; set; }
    public DbSet<FestivalAllDependency> FestivalAllDependencies { get; set; }
    public DbSet<FestivalMaterialDependency> FestivalMaterialDependencies { get; set; }
    public DbSet<FestivalModelDependency> FestivalModelDependencies { get; set; }
    public DbSet<FestivalSubdependency> FestivalSubdependencies { get; set; }
    public DbSet<FestivalDependency> FestivalDependencies { get; set; }
    public DbSet<EarcModBackup> EarcModBackups { get; set; }
    public DbSet<EarcMod> EarcMods { get; set; }
    public DbSet<EarcModEarc> EarcModEarcs { get; set; }
    public DbSet<EarcModReplacement> EarcModReplacements { get; set; }
    public DbSet<AssetExplorerNode> AssetExplorerNodes { get; set; }
    public DbSet<ArchiveLocation> ArchiveLocations { get; set; }
    public DbSet<AssetUri> AssetUris { get; set; }
    public DbSet<StatePair> StatePairs { get; set; }

    public DbSet<ModelReplacementPreset> ModelReplacementPresets { get; set; }
    public DbSet<ModelReplacementPath> ModelReplacementPaths { get; set; }
    public DbSet<ModelReplacementFavourite> ModelReplacementFavourites { get; set; }

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
    }

    public void ClearTable(string name)
    {
        Database.ExecuteSqlRaw($"DELETE FROM {name};");
        Database.ExecuteSqlRaw($"DELETE FROM SQLITE_SEQUENCE WHERE name='{name}';");
    }
}