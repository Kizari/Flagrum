using System.Collections.Generic;

namespace Flagrum.Core.Gfxbin.Gmdl.Constructs;

public class ColorMap
{
    public IList<Color4> Colors;
}

public class Color4
{
    public byte A;
    public byte B;
    public byte G;
    public byte R;
}