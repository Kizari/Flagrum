﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Flagrum.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Application.Persistence.Entities;

public class StatePair
{
    [Key] public StateKey Key { get; set; }

    public string Value { get; set; }
}

public static class StatePairExtensions
{
    public static string GetString(this FlagrumDbContext context, StateKey key)
    {
        return context.StatePairs.AsNoTracking().FirstOrDefault(p => p.Key == key)?.Value;
    }

    public static int GetInt(this FlagrumDbContext context, StateKey key)
    {
        var value = context.StatePairs.AsNoTracking().FirstOrDefault(p => p.Key == key);
        return value == null ? -1 : Convert.ToInt32(value.Value);
    }

    public static bool GetBool(this FlagrumDbContext context, StateKey key)
    {
        var value = context.StatePairs.AsNoTracking().FirstOrDefault(p => p.Key == key);
        return value?.Value == "True";
    }

    public static TEnum GetEnum<TEnum>(this FlagrumDbContext context, StateKey key) where TEnum : struct
    {
        var value = context.StatePairs.AsNoTracking().FirstOrDefault(p => p.Key == key);
        return value == null ? default : Enum.Parse<TEnum>(value.Value);
    }

    public static void SetString(this FlagrumDbContext context, StateKey key, string value)
    {
        var pair = context.StatePairs.FirstOrDefault(p => p.Key == key);
        if (pair == null)
        {
            pair = new StatePair {Key = key, Value = value};
            context.Add(pair);
        }
        else
        {
            pair.Value = value;
        }

        context.SaveChanges();
        context.ChangeTracker.Clear();
    }

    public static void SetInt(this FlagrumDbContext context, StateKey key, int value)
    {
        SetString(context, key, value.ToString());
    }

    public static void SetBool(this FlagrumDbContext context, StateKey key, bool value)
    {
        SetString(context, key, value.ToString());
    }

    public static void SetEnum<TEnum>(this FlagrumDbContext context, StateKey key, TEnum value)
    {
        SetString(context, key, value.ToString());
    }

    public static void DeleteStateKey(this FlagrumDbContext context, StateKey key)
    {
        var match = context.StatePairs.FirstOrDefault(p => p.Key == key);
        if (match != null)
        {
            context.Remove(match);
        }

        context.SaveChanges();
        context.ChangeTracker.Clear();
    }
}