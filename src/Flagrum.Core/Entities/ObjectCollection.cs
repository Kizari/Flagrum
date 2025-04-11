using System.Collections.Generic;
using Flagrum.Core.Scripting.Ebex.Data;

namespace Flagrum.Core.Scripting.Ebex;

public class ObjectCollection
{
    private readonly List<Object> _objects = new();

    public Object this[int i]
    {
        get => _objects[i];
        set => _objects[i] = value;
    }
    
    public int Count => _objects.Count;
    
    public void Add(Object @object)
    {
        _objects.Add(@object);
    }

    public DataItem GetObjectByIndexAndPath(int objectIndex, string relativePath = null)
    {
        if (objectIndex < 0 || objectIndex >= _objects.Count)
        {
            return null;
        }

        DataItem child = _objects[objectIndex];

        var path = new ItemPath(relativePath);
        if (child != null && path.Exists)
        {
            child = child.GetChild(path);
        }

        return child;
    }
}