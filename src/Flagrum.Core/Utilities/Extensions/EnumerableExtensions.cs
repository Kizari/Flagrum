using System;
using System.Collections.Generic;
using System.Linq;

namespace Flagrum.Core.Utilities.Extensions;

public static class EnumerableExtensions
{
    public static void RemoveFirst<TItem>(this ICollection<TItem> items, Func<TItem, bool> predicate)
    {
        var match = items.First(predicate);
        items.Remove(match);
    }

    public static bool TryRemoveFirst<TItem>(this ICollection<TItem> items, Func<TItem, bool> predicate)
    {
        var match = items.FirstOrDefault(predicate);
        if (match != null)
        {
            items.Remove(match);
            return true;
        }

        return false;
    }

    public static void RemoveWhere<TItem>(this ICollection<TItem> items, Func<TItem, bool> predicate)
    {
        var itemsToRemove = items.Where(predicate).ToList();
        foreach (var item in itemsToRemove)
        {
            items.Remove(item);
        }
    }

    public static void AddRange<TItem>(this HashSet<TItem> hashSet, IEnumerable<TItem> items)
    {
        foreach (var item in items)
        {
            hashSet.Add(item);
        }
    }
}