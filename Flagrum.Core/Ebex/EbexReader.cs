using System;
using System.Collections.Generic;
using System.IO;
using Flagrum.Core.Ebex.Entities;
using Flagrum.Core.Ebex.Types;
using Flagrum.Core.Ebex.Xmb2;

namespace Flagrum.Core.Ebex;

public class EbexReader : IDisposable
{
    private readonly List<EbexElement> _elements = new();
    private readonly Xmb2Element _rootElement;
    private readonly Stream _stream;

    private readonly Dictionary<string, EbexType> _types;

    public EbexReader(Stream stream)
    {
        _stream = stream;
        _rootElement = Xmb2Document.GetRootElement(_stream);
        _types = new ModuleReader().Read();
    }

    public void Dispose()
    {
        _stream.Close();
        _stream.Dispose();
    }

    public void Read()
    {
        var objectsContainer = _rootElement.GetElementByName("objects");
        var objects = objectsContainer.GetElements("object");

        foreach (var objectElement in objects)
        {
            _elements.Add(ReadObject(objectElement));
        }
    }

    private EbexElement ReadObject(Xmb2Element element)
    {
        var csType = (string)element.GetAttributeByName("csharp_type").Value;
        var type = csType ?? (string)element.GetAttributeByName("type").Value;
        var ownerIndex = (int)element.GetAttributeByName("ownerIndex").Value;
        var ownerPath = (string)element.GetAttributeByName("ownerPath").Value;

        var parent = GetElementByIndexAndPath(ownerIndex, ownerPath);
        return BuildEbexComponent(parent, type) as EbexElement;
    }

    private IEbexComponent BuildEbexComponent(EbexElement parent, string type)
    {
        // Check if primitive type
        var primitiveType = PrimitiveTypeHelper.FromString(type);
        if (primitiveType != PrimitiveType.None)
        {
            return new EbexValue(primitiveType, null);
        }

        // Attempt to instantiate component from type
        if (_types.TryGetValue(type, out var ebexType))
        {
            var result = ebexType.InstantiateElement(parent);
            if (result != null)
            {
                return result;
            }

            if (ebexType is EbexEnum @enum)
            {
                return new EbexValue(@enum, null);
            }

            if (ebexType is EbexTypedef)
            {
                type = ebexType.Name;
            }

            if (type != null)
            {
                primitiveType = PrimitiveTypeHelper.FromCompositeString(type);
                if (primitiveType != PrimitiveType.None)
                {
                    return new EbexValue(primitiveType, null);
                }
            }

            return ebexType.BuildElement(parent);
        }

        return new EbexValue();
    }

    private EbexElement GetElementByIndexAndPath(int index, string path)
    {
        if (index > 0 && index < _elements.Count)
        {
            var ebexPath = new EbexPath(path);
            var element = _elements[index];
            return element != null && !ebexPath.IsNullOrEmpty
                ? element.GetChild(ebexPath)
                : element;
        }

        return null;
    }
}