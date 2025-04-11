using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Flagrum.Application.Persistence.Configuration.Entities;

public enum ConfigurationKey
{
    CurrentProfile,
    LatestVersionNotes,
    PatreonToken,
    PatreonRefreshToken,
    PatreonTokenExpiry,
    ClientId,
    GiftToken
}

public class ConfigurationEntity
{
    [Key]
    public ConfigurationKey Key { get; set; }
    
    public string Value { get; set; }
}

public static class ConfigurationExtensions
{
    public static string GetString(this ConfigurationDbContext context, ConfigurationKey key)
    {
        var pair = context.ConfigurationEntities.FirstOrDefault(e => e.Key == key);
        return pair?.Value;
    }
    
    public static DateTime GetDateTime(this ConfigurationDbContext context, ConfigurationKey key)
    {
        var pair = context.ConfigurationEntities.FirstOrDefault(e => e.Key == key);
        return pair != null ? new DateTime(long.Parse(pair.Value)) : default;
    }

    public static void SetString(this ConfigurationDbContext context, ConfigurationKey key, string value)
    {
        var pair = context.ConfigurationEntities.FirstOrDefault(e => e.Key == key);
        if (pair == null)
        {
            context.Add(new ConfigurationEntity {Key = key, Value = value});
        }
        else
        {
            pair.Value = value;
        }

        context.SaveChanges();
    }
    
    public static void SetDateTime(this ConfigurationDbContext context, ConfigurationKey key, DateTime value)
    {
        var pair = context.ConfigurationEntities.FirstOrDefault(e => e.Key == key);
        if (pair == null)
        {
            context.Add(new ConfigurationEntity {Key = key, Value = value.Ticks.ToString()});
        }
        else
        {
            pair.Value = value.Ticks.ToString();
        }

        context.SaveChanges();
    }
}