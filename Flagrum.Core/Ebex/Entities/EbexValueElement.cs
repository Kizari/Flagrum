using Flagrum.Core.Ebex.Types;

namespace Flagrum.Core.Ebex.Entities;

public class EbexValueElement : EbexElement
{
    public EbexValueElement(EbexElement parent, EbexType type)
        : base(parent, type) { }
}