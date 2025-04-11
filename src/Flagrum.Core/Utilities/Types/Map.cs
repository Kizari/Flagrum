using System.Collections.Generic;

namespace Flagrum.Core.Utilities.Types;

public class Map<T1, T2>
{
    private readonly Dictionary<T1, T2> _forward = new();
    private readonly Dictionary<T2, T1> _reverse = new();

    public T1 this[T2 index]
    {
        get => _reverse[index];
        set
        {
            _forward[value] = index;
            _reverse[index] = value;
        }
    }

    public T2 this[T1 index]
    {
        get => _forward[index];
        set
        {
            _forward[index] = value;
            _reverse[value] = index;
        }
    }

    public bool TryGetValue(T1 key, out T2 value) => _forward.TryGetValue(key, out value);
    public bool TryGetValue(T2 key, out T1 value) => _reverse.TryGetValue(key, out value);

    public void Clear()
    {
        _forward.Clear();
        _reverse.Clear();
    }
}