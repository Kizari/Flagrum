using System.Collections.Generic;

namespace Flagrum.Core.Utilities;

/// <summary>
/// Two-way dictionary that returns a default value if a key is accessed that doesn't exist
/// </summary>
public class FallbackMap<T1, T2>
{
    private readonly T1 _default1;
    private readonly T2 _default2;
    private readonly Dictionary<T1, T2> _forward = new();
    private readonly Dictionary<T2, T1> _reverse = new();

    public FallbackMap(T1 default1, T2 default2)
    {
        _default1 = default1;
        _default2 = default2;
    }

    public T1 this[T2 index]
    {
        get => _reverse.TryGetValue(index, out var value) ? value : _default1;
        set => _reverse[index] = value;
    }

    public T2 this[T1 index]
    {
        get => _forward.TryGetValue(index, out var value) ? value : _default2;
        set => _forward[index] = value;
    }

    public void Add(T1 first, T2 second)
    {
        _forward.Add(first, second);
        _reverse.Add(second, first);
    }
}