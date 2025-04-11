using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Core.Persistence;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Application.Persistence.Configuration;
using Flagrum.Application.Persistence.Configuration.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Application.Legacy.Migration;

public static class ConfigurationMigration
{
    public static async Task RunAsync()
    {
        if (!File.Exists(Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "config.fcg")))
        {
            return;
        }

        // Ensure the DB is up to date
        var context = new ConfigurationDbContext();
        await context.Database.MigrateAsync();

        // Create the new configuration
        var clientIdString = context.GetString(ConfigurationKey.ClientId);
        var currentProfileString = context.GetString(ConfigurationKey.CurrentProfile);
        var giftToken = context.GetString(ConfigurationKey.GiftToken);

        var configuration = new Configuration
        {
            ClientId = clientIdString == null ? Guid.Empty : new Guid(context.GetString(ConfigurationKey.ClientId)),
            CurrentProfile = currentProfileString == null
                ? Guid.Empty
                : new Guid(context.GetString(ConfigurationKey.CurrentProfile)),
            LatestVersionNotes = context.GetString(ConfigurationKey.LatestVersionNotes),
            GiftToken = giftToken == null ? Guid.Empty : new Guid(context.GetString(ConfigurationKey.GiftToken)),
            PatreonToken = context.GetString(ConfigurationKey.PatreonToken),
            PatreonRefreshToken = context.GetString(ConfigurationKey.PatreonRefreshToken),
            PatreonTokenExpiry = context.GetDateTime(ConfigurationKey.PatreonTokenExpiry)
        };

        // Create the new profiles
        configuration.Profiles = context.ProfileEntities.Select(p => new Profile(configuration)
        {
            Id = new Guid(p.Id),
            Type = p.Type,
            Name = p.Name,
            GamePath = p.GamePath,
            BinmodListPath = p.BinmodListPath
        }).Cast<IProfile>().ToList();

        // Save the new configuration
        Repository.Save(configuration, Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "configuration.fcg"));
        await context.DisposeAsync();
        SqliteConnection.ClearAllPools();

        // Delete the old configuration
        File.Delete(Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "config.fcg"));
    }
}