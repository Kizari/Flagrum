using System;
using System.Collections.Generic;

namespace Flagrum.Core.Scripting.Ebex.Data;

[Serializable]
public struct ItemPath
{
    private string fullPath;

    public ItemPath(string s)
    {
        fullPath = s;
    }

    public string FullPath
    {
        get => fullPath;
        set => fullPath = value;
    }

    public override string ToString()
    {
        return fullPath ?? "";
    }

    public string Name
    {
        get
        {
            if (fullPath != null)
            {
                var num = fullPath.LastIndexOf('.');
                if (num >= 0)
                {
                    return fullPath.Substring(num + 1);
                }
            }

            return fullPath;
        }
    }

    public List<string> Names
    {
        get
        {
            var stringList = new List<string>();
            if (fullPath != null)
            {
                if (fullPath.IndexOf('.') >= 0)
                {
                    var strArray = fullPath.Split('.');
                    stringList.AddRange(strArray);
                }
                else
                {
                    stringList.Add(fullPath);
                }
            }

            return stringList;
        }
    }

    public bool Exists => !string.IsNullOrEmpty(fullPath);

    public bool Empty => string.IsNullOrEmpty(fullPath);

    public ItemPath Combine(string s)
    {
        return Combine(new ItemPath(s));
    }

    public ItemPath Combine(ItemPath s)
    {
        var s1 = fullPath;
        if (!string.IsNullOrEmpty(s.fullPath))
        {
            s1 = !string.IsNullOrEmpty(fullPath) ? fullPath + "." + s.fullPath : s.fullPath;
        }

        return new ItemPath(s1);
    }

    public static ItemPath operator +(ItemPath p, string s)
    {
        return p.Combine(s);
    }

    public static ItemPath operator +(ItemPath p1, ItemPath p2)
    {
        return p1.Combine(p2);
    }

    public ItemPath GetRelativePath(ItemPath basePath)
    {
        if (fullPath == basePath.fullPath)
        {
            return new ItemPath();
        }

        return fullPath.StartsWith(basePath.fullPath + ".")
            ? new ItemPath(fullPath.Substring(basePath.fullPath.Length + 1))
            : this;
    }

    public ItemPath GetParent()
    {
        if (fullPath != null)
        {
            var length = fullPath.LastIndexOf('.');
            if (length > 0)
            {
                return new ItemPath(fullPath.Substring(0, length));
            }
        }

        return new ItemPath();
    }

    public ItemPath Parent
    {
        get => GetParent();
        set => this = value + Name;
    }

    public string PopFront()
    {
        if (fullPath == null)
        {
            return null;
        }

        var length = fullPath.IndexOf('.');
        string str;
        if (length >= 0)
        {
            str = fullPath.Substring(0, length);
            fullPath = fullPath.Substring(length + 1);
        }
        else
        {
            str = fullPath;
            fullPath = null;
        }

        return str;
    }

    public string GetFront()
    {
        if (fullPath == null)
        {
            return null;
        }

        var length = fullPath.IndexOf('.');
        return length < 0 ? fullPath : fullPath.Substring(0, length);
    }

    public static bool operator ==(ItemPath p1, ItemPath p2)
    {
        return p1.fullPath == p2.fullPath;
    }

    public static bool operator !=(ItemPath p1, ItemPath p2)
    {
        return p1.fullPath != p2.fullPath;
    }

    public override bool Equals(object obj)
    {
        if (obj is ItemPath itemPath)
        {
            return this == itemPath;
        }

        return fullPath != null ? fullPath.Equals(obj) : base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return fullPath != null ? fullPath.GetHashCode() : base.GetHashCode();
    }
}