using System.Collections.Generic;

namespace Flagrum.Core.Utilities;

public class Map<T1, T2>
{
    private readonly Dictionary<T1, T2> _forward = new();
    private readonly Dictionary<T2, T1> _reverse = new();

    public void Add(T1 first, T2 second)
    {
        _forward.Add(first, second);
        _reverse.Add(second, first);
    }

    public T1 this[T2 index]
    {
        get => _reverse[index];
        set => _reverse[index] = value;
    }

    public T2 this[T1 index]
    {
        get => _forward[index];
        set => _forward[index] = value;
    }
}