namespace Flagrum.Gfxbin.Gmdl.Constructs;

public class OrientedBB
{
    public OrientedBB(Vector3 center, Vector3 xHalfExtent, Vector3 yHalfExtent, Vector3 zHalfExtent)
    {
        Center = center;
        XHalfExtent = xHalfExtent;
        YHalfExtent = yHalfExtent;
        ZHalfExtent = zHalfExtent;
    }

    public Vector3 Center { get; }
    public Vector3 XHalfExtent { get; }
    public Vector3 YHalfExtent { get; }
    public Vector3 ZHalfExtent { get; }
}