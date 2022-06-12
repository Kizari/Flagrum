using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Flagrum.Core.Utilities;

public class ConcurrentCollection<T> : ICollection<T>
{
    private readonly ConcurrentBag<T> _bag = new();

    public IEnumerator<T> GetEnumerator()
    {
        return _bag.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        _bag.Add(item);
    }

    public void Clear()
    {
        _bag.Clear();
    }

    public bool Contains(T item)
    {
        return _bag.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _bag.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException("ConcurrentBag does not have Remove method!");
    }

    public int Count => _bag.Count;
    public bool IsReadOnly => false;
}