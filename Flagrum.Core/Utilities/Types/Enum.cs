using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flagrum.Core.Utilities.Types;

public abstract class Enum<TValue> : IComparable where TValue : IComparable
{
    protected Enum(TValue value)
    {
        Value = value;
    }

    public TValue Value { get; }

    public int CompareTo(object other)
    {
        return Value.CompareTo(other);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static IEnumerable<TEnum> GetAll<TEnum>() where TEnum : Enum<TValue>
    {
        var fields = typeof(TEnum).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        return fields.Select(f => f.GetValue(null)).Cast<TEnum>();
    }

    public override bool Equals(object obj)
    {
        if (obj is not Enum<TValue> other)
        {
            return false;
        }

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Value.Equals(other.Value);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}