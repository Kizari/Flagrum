using System.Collections.Generic;

namespace Flagrum.Application.Features.WorkshopMods.Data.Model;

public class BinmodColorMap
{
    public IList<BinmodColor4> Colors;
}

public class BinmodColor4
{
    public byte A;
    public byte B;
    public byte G;
    public byte R;
}