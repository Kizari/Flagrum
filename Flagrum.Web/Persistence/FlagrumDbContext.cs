using System;
using System.Reflection;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Web.Persistence;

public class FlagrumDbContext : DbContext
{
    public FlagrumDbContext() { }

    public FlagrumDbContext(SettingsService settings)
    {
        Settings = settings;
    }

    public SettingsService Settings { get; }

    public DbSet<AssetExplorerNode> AssetExplorerNodes { get; set; }
    public DbSet<ArchiveLocation> ArchiveLocations { get; set; }
    public DbSet<AssetUri> AssetUris { get; set; }
    public DbSet<StatePair> StatePairs { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var databasePath =
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Flagrum\flagrum.db";

        optionsBuilder.UseSqlite($"Data Source={databasePath};", options => { options.CommandTimeout(90); });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetAssembly(typeof(FlagrumDbContext))
            ?? throw new InvalidOperationException("Assembly cannot be null"));
    }
}