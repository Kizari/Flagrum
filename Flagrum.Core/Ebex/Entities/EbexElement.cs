using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Ebex.Types;

namespace Flagrum.Core.Ebex.Entities;

public class EbexElement : IEbexComponent, IEnumerable<EbexElement>
{
    public EbexElement(EbexElement parent, EbexType type)
    {
        Parent = parent;
    }

    public EbexElement Parent { get; }
    public EbexType Type { get; }
    public List<EbexElement> Children { get; } = new();

    public string Name { get; set; }
    public IEbexComponent Value { get; set; }

    public EbexElement this[string name] => Children.FirstOrDefault(c => c.Name == name);

    public IEnumerator<EbexElement> GetEnumerator()
    {
        return Children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Children.GetEnumerator();
    }

    public EbexElement GetChild(EbexPath path)
    {
        var name = path.Pop();
        var element = string.IsNullOrEmpty(name) ? this : this[name];

        if (element != null && !path.IsNullOrEmpty)
        {
            element = element.GetChild(path);
        }

        return element;
    }
}