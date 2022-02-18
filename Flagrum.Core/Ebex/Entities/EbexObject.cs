using Flagrum.Core.Ebex.Types;

namespace Flagrum.Core.Ebex.Entities;

public class EbexObject : EbexElement
{
    public EbexObject(EbexElement parent, EbexType type) : base(parent, type) { }
}