using System;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;

namespace Flagrum.Core.Graphics.Textures.Shared.PixelFormats;

public struct R32F : IPixel<R32F>
{
    public float R;

    public PixelOperations<R32F> CreatePixelOperations() => throw new NotImplementedException();

    public void FromScaledVector4(Vector4 vector)
    {
        throw new NotImplementedException();
    }

    public Vector4 ToScaledVector4() => throw new NotImplementedException();

    public void FromVector4(Vector4 vector)
    {
        throw new NotImplementedException();
    }

    public Vector4 ToVector4() => throw new NotImplementedException();

    public void FromArgb32(Argb32 source)
    {
        throw new NotImplementedException();
    }

    public void FromBgra5551(Bgra5551 source)
    {
        throw new NotImplementedException();
    }

    public void FromBgr24(Bgr24 source)
    {
        throw new NotImplementedException();
    }

    public void FromBgra32(Bgra32 source)
    {
        throw new NotImplementedException();
    }

    public void FromAbgr32(Abgr32 source)
    {
        throw new NotImplementedException();
    }

    public void FromL8(L8 source)
    {
        throw new NotImplementedException();
    }

    public void FromL16(L16 source)
    {
        throw new NotImplementedException();
    }

    public void FromLa16(La16 source)
    {
        throw new NotImplementedException();
    }

    public void FromLa32(La32 source)
    {
        throw new NotImplementedException();
    }

    public void FromRgb24(Rgb24 source)
    {
        throw new NotImplementedException();
    }

    public void FromRgba32(Rgba32 source)
    {
        throw new NotImplementedException();
    }

    public void ToRgba32(ref Rgba32 dest)
    {
        throw new NotImplementedException();
    }

    public void FromRgb48(Rgb48 source)
    {
        throw new NotImplementedException();
    }

    public void FromRgba64(Rgba64 source)
    {
        throw new NotImplementedException();
    }

    public bool Equals(R32F other) => throw new NotImplementedException();
}