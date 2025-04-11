// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This file is an altered version of System.Windows.Media.Color from WPF
// https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/Color.cs

namespace Flagrum.Core.Scripting.Ebex.Data;

public struct Color
{
    private struct
        MILColorF // this structure is the "milrendertypes.h" structure and should be identical for performance
    {
        public float a, r, g, b;

        public override int GetHashCode()
        {
            return a.GetHashCode() ^ r.GetHashCode() ^ g.GetHashCode() ^ b.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }

    private MILColorF scRgbColor;

    private struct MILColor
    {
        public byte a, r, g, b;
    }

    private MILColor sRgbColor;

    private float[] nativeColorValue;

    private bool isFromScRgb;

    private const string c_scRgbFormat = "R";

    /// <summary>
    /// A
    /// </summary>
    public byte A
    {
        get => sRgbColor.a;
        set
        {
            scRgbColor.a = value / 255.0f;
            sRgbColor.a = value;
        }
    }

    /// <value>
    /// The Red channel as a byte whose range is [0..255].
    /// the value is not allowed to be out of range
    /// </value>
    /// <summary>
    /// R
    /// </summary>
    public byte R
    {
        get => sRgbColor.r;
        set
        {
            scRgbColor.r = sRgbToScRgb(value);
            sRgbColor.r = value;
        }
    }

    /// <value>
    /// The Green channel as a byte whose range is [0..255].
    /// the value is not allowed to be out of range
    /// </value>
    /// <summary>
    /// G
    /// </summary>
    public byte G
    {
        get => sRgbColor.g;
        set
        {
            scRgbColor.g = sRgbToScRgb(value);
            sRgbColor.g = value;
        }
    }

    /// <value>
    /// The Blue channel as a byte whose range is [0..255].
    /// the value is not allowed to be out of range
    /// </value>
    /// <summary>
    /// B
    /// </summary>
    public byte B
    {
        get => sRgbColor.b;
        set
        {
            scRgbColor.b = sRgbToScRgb(value);
            sRgbColor.b = value;
        }
    }

    /// <value>
    /// The Alpha channel as a float whose range is [0..1].
    /// the value is allowed to be out of range
    /// </value>
    /// <summary>
    /// ScA
    /// </summary>
    public float ScA
    {
        get => scRgbColor.a;
        set
        {
            scRgbColor.a = value;
            if (value < 0.0f)
            {
                sRgbColor.a = 0;
            }
            else if (value > 1.0f)
            {
                sRgbColor.a = 255;
            }
            else
            {
                sRgbColor.a = (byte)(value * 255f);
            }
        }
    }

    /// <value>
    /// The Red channel as a float whose range is [0..1].
    /// the value is allowed to be out of range
    /// </value>
    /// <summary>
    /// ScR
    /// </summary>
    public float ScR
    {
        get => scRgbColor.r;
        // throw new ArgumentException(SR.Format(SR.Color_ColorContextNotsRgb_or_ScRgb, null));
        set
        {
            scRgbColor.r = value;
            sRgbColor.r = ScRgbTosRgb(value);
        }
    }

    /// <value>
    /// The Green channel as a float whose range is [0..1].
    /// the value is allowed to be out of range
    /// </value>
    /// <summary>
    /// ScG
    /// </summary>
    public float ScG
    {
        get => scRgbColor.g;
        // throw new ArgumentException(SR.Format(SR.Color_ColorContextNotsRgb_or_ScRgb, null));
        set
        {
            scRgbColor.g = value;
            sRgbColor.g = ScRgbTosRgb(value);
        }
    }

    /// <value>
    /// The Blue channel as a float whose range is [0..1].
    /// the value is allowed to be out of range
    /// </value>
    /// <summary>
    /// ScB
    /// </summary>
    public float ScB
    {
        get => scRgbColor.b;
        // throw new ArgumentException(SR.Format(SR.Color_ColorContextNotsRgb_or_ScRgb, null));
        set
        {
            scRgbColor.b = value;
            sRgbColor.b = ScRgbTosRgb(value);
        }
    }

    /// <summary>
    /// private helper function to set context values from a color value with a set context and ScRgb values
    /// </summary>
    private static float sRgbToScRgb(byte bval)
    {
        var val = bval / 255.0f;

        if (!(val > 0.0)) // Handles NaN case too. (Though, NaN isn't actually
            // possible in this case.)
        {
            return 0.0f;
        }

        if (val <= 0.04045)
        {
            return val / 12.92f;
        }

        if (val < 1.0f)
        {
            return (float)System.Math.Pow((val + 0.055) / 1.055, 2.4);
        }

        return 1.0f;
    }

    /// <summary>
    /// private helper function to set context values from a color value with a set context and ScRgb values
    /// </summary>
    private static byte ScRgbTosRgb(float val)
    {
        if (!(val > 0.0)) // Handles NaN case too
        {
            return 0;
        }

        if (val <= 0.0031308)
        {
            return (byte)(255.0f * val * 12.92f + 0.5f);
        }

        if (val < 1.0)
        {
            return (byte)(255.0f * (1.055f * (float)System.Math.Pow(val, 1.0 / 2.4) - 0.055f) + 0.5f);
        }

        return 255;
    }

    /// <summary>
    /// IsEqual operator - Compares two colors for exact equality.  Note that float values can acquire error
    /// when operated upon, such that an exact comparison between two values which are logically
    /// equal may fail. see cref="AreClose".
    /// </summary>
    public static bool operator ==(Color color1, Color color2)
    {
        if (color1.scRgbColor.r != color2.scRgbColor.r)
        {
            return false;
        }

        if (color1.scRgbColor.g != color2.scRgbColor.g)
        {
            return false;
        }

        if (color1.scRgbColor.b != color2.scRgbColor.b)
        {
            return false;
        }

        if (color1.scRgbColor.a != color2.scRgbColor.a)
        {
            return false;
        }

        return true;
    }

    public static bool operator !=(Color color1, Color color2)
    {
        return !(color1 == color2);
    }
}