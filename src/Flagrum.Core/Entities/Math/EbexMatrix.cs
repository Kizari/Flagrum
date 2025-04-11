using System;

namespace Flagrum.Core.Scripting.Ebex.Math;

public struct Matrix
{
    public Vector Col0;
    public Vector Col1;
    public Vector Col2;
    public Vector Col3;

    public static Vector GetEulerAngleRadian(Matrix mat)
    {
        throw new NotImplementedException();
    }

    public Vector TransformCoord(Vector vec)
    {
        throw new NotImplementedException();
    }

    public static Matrix GetRotationZYX(Vector eulerRadians)
    {
        throw new NotImplementedException();
    }

    public void SetCol(int col, Vector value)
    {
        throw new NotImplementedException();
    }

    public static Matrix operator *(Matrix first, Matrix second)
    {
        throw new NotImplementedException();
    }
}