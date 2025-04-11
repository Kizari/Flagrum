namespace Flagrum.Application.Features.WorkshopMods.Data.Model;

public struct BinmodVector3
{
    public BinmodVector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float X;
    public float Y;
    public float Z;

    public override string ToString()
    {
        return $"[{X}, {Y}, {Z}]";
    }
}