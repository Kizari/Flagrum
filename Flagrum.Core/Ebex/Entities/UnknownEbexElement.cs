using Flagrum.Core.Ebex.Types;

namespace Flagrum.Core.Ebex.Entities;

public class UnknownEbexElement : EbexElement
{
    public UnknownEbexElement(EbexElement parent, EbexType type)
        : base(parent, type) { }
}