using System;
using Flagrum.Application.Persistence.Configuration.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Application.Persistence.Configuration;

public class ConfigurationDbContext : DbContext
{
    public DbSet<ConfigurationEntity> ConfigurationEntities { get; set; }
    public DbSet<ProfileEntity> ProfileEntities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource =
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Flagrum\config.fcg",
            Password = "c8cfc6c1-d95e-4a08-b486-76427cf3ca5f"
        };

        optionsBuilder.UseSqlite(builder.ToString(), options => { options.CommandTimeout(180); });
    }
}