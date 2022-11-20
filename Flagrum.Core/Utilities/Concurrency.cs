using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Flagrum.Core.Utilities;

public class ConcurrentCollection<T> : ICollection<T>
{
    private readonly ConcurrentDictionary<T, bool> _dictionary = new();

    public ConcurrentCollection() { }

    public ConcurrentCollection(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            _dictionary.TryAdd(item, true);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _dictionary.Select(kvp => kvp.Key).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        _dictionary.TryAdd(item, true);
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public bool Contains(T item)
    {
        return item != null && _dictionary.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        var items = _dictionary.Select(kvp => kvp.Key).ToArray();
        Array.Copy(items, 0, array, arrayIndex, items.Length);
    }

    public bool Remove(T item)
    {
        return _dictionary.Remove(item, out _);
    }

    public int Count => _dictionary.Count;
    public bool IsReadOnly => false;
}