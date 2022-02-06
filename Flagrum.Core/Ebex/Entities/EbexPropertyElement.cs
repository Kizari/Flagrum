using Flagrum.Core.Ebex.Types;

namespace Flagrum.Core.Ebex.Entities;

public class EbexPropertyElement : EbexElement
{
    public EbexPropertyElement(EbexElement parent, EbexProperty property)
        : base(parent, property) { }
}