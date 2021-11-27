// Decompiled with JetBrains decompiler
// Type: UnityEngine.Color
// Assembly: UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 735D43B3-A378-4A19-B226-5A41179B0FE2
// Assembly location: C:\Program Files\Unity\Hub\Editor\2019.3.12f1\Editor\Data\Managed\UnityEngine.dll

using System;
using System.Globalization;

namespace UnityEngine
{
    /// <summary>
    ///   <para>Representation of RGBA colors.</para>
    /// </summary>
    public struct Color : IEquatable<Color>
    {
        /// <summary>
        ///   <para>Red component of the color.</para>
        /// </summary>
        public float r;
        /// <summary>
        ///   <para>Green component of the color.</para>
        /// </summary>
        public float g;
        /// <summary>
        ///   <para>Blue component of the color.</para>
        /// </summary>
        public float b;
        /// <summary>
        ///   <para>Alpha component of the color (0 is transparent, 1 is opaque).</para>
        /// </summary>
        public float a;

        /// <summary>
        ///   <para>Constructs a new Color with given r,g,b,a components.</para>
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        /// <param name="a">Alpha component.</param>
        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        /// <summary>
        ///   <para>Constructs a new Color with given r,g,b components and sets a to 1.</para>
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        public Color(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 1f;
        }

        /// <summary>
        ///   <para>Returns a nicely formatted string of this color.</para>
        /// </summary>
        /// <param name="format"></param>
        //public override string ToString() => UnityString.Format("RGBA({0:F3}, {1:F3}, {2:F3}, {3:F3})", (object)this.r, (object)this.g, (object)this.b, (object)this.a);

        /// <summary>
        ///   <para>Returns a nicely formatted string of this color.</para>
        /// </summary>
        /// <param name="format"></param>
        //public string ToString(string format) => UnityString.Format("RGBA({0}, {1}, {2}, {3})", (object)this.r.ToString(format, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat), (object)this.g.ToString(format, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat), (object)this.b.ToString(format, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat), (object)this.a.ToString(format, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat));

        public override int GetHashCode() => ((Vector4)this).GetHashCode();

        public override bool Equals(object other) => other is Color other1 && this.Equals(other1);

        public bool Equals(Color other) => this.r.Equals(other.r) && this.g.Equals(other.g) && this.b.Equals(other.b) && this.a.Equals(other.a);

        public static Color operator +(Color a, Color b) => new Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);

        public static Color operator -(Color a, Color b) => new Color(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);

        public static Color operator *(Color a, Color b) => new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);

        public static Color operator *(Color a, float b) => new Color(a.r * b, a.g * b, a.b * b, a.a * b);

        public static Color operator *(float b, Color a) => new Color(a.r * b, a.g * b, a.b * b, a.a * b);

        public static Color operator /(Color a, float b) => new Color(a.r / b, a.g / b, a.b / b, a.a / b);

        public static bool operator ==(Color lhs, Color rhs) => (Vector4)lhs == (Vector4)rhs;

        public static bool operator !=(Color lhs, Color rhs) => !(lhs == rhs);

        /// <summary>
        ///   <para>Linearly interpolates between colors a and b by t.</para>
        /// </summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <param name="t">Float for combining a and b.</param>
        //public static Color Lerp(Color a, Color b, float t)
        //{
        //    t = Mathf.Clamp01(t);
        //    return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t);
        //}

        /// <summary>
        ///   <para>Linearly interpolates between colors a and b by t.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        public static Color LerpUnclamped(Color a, Color b, float t) => new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t);

        internal Color RGBMultiplied(float multiplier) => new Color(this.r * multiplier, this.g * multiplier, this.b * multiplier, this.a);

        internal Color AlphaMultiplied(float multiplier) => new Color(this.r, this.g, this.b, this.a * multiplier);

        internal Color RGBMultiplied(Color multiplier) => new Color(this.r * multiplier.r, this.g * multiplier.g, this.b * multiplier.b, this.a);

        /// <summary>
        ///   <para>Solid red. RGBA is (1, 0, 0, 1).</para>
        /// </summary>
        public static Color red => new Color(1f, 0.0f, 0.0f, 1f);

        /// <summary>
        ///   <para>Solid green. RGBA is (0, 1, 0, 1).</para>
        /// </summary>
        public static Color green => new Color(0.0f, 1f, 0.0f, 1f);

        /// <summary>
        ///   <para>Solid blue. RGBA is (0, 0, 1, 1).</para>
        /// </summary>
        public static Color blue => new Color(0.0f, 0.0f, 1f, 1f);

        /// <summary>
        ///   <para>Solid white. RGBA is (1, 1, 1, 1).</para>
        /// </summary>
        public static Color white => new Color(1f, 1f, 1f, 1f);

        /// <summary>
        ///   <para>Solid black. RGBA is (0, 0, 0, 1).</para>
        /// </summary>
        public static Color black => new Color(0.0f, 0.0f, 0.0f, 1f);

        /// <summary>
        ///   <para>Yellow. RGBA is (1, 0.92, 0.016, 1), but the color is nice to look at!</para>
        /// </summary>
        public static Color yellow => new Color(1f, 0.9215686f, 0.01568628f, 1f);

        /// <summary>
        ///   <para>Cyan. RGBA is (0, 1, 1, 1).</para>
        /// </summary>
        public static Color cyan => new Color(0.0f, 1f, 1f, 1f);

        /// <summary>
        ///   <para>Magenta. RGBA is (1, 0, 1, 1).</para>
        /// </summary>
        public static Color magenta => new Color(1f, 0.0f, 1f, 1f);

        /// <summary>
        ///   <para>Gray. RGBA is (0.5, 0.5, 0.5, 1).</para>
        /// </summary>
        public static Color gray => new Color(0.5f, 0.5f, 0.5f, 1f);

        /// <summary>
        ///   <para>English spelling for gray. RGBA is the same (0.5, 0.5, 0.5, 1).</para>
        /// </summary>
        public static Color grey => new Color(0.5f, 0.5f, 0.5f, 1f);

        /// <summary>
        ///   <para>Completely transparent. RGBA is (0, 0, 0, 0).</para>
        /// </summary>
        public static Color clear => new Color(0.0f, 0.0f, 0.0f, 0.0f);

        /// <summary>
        ///   <para>The grayscale value of the color. (Read Only)</para>
        /// </summary>
        public float grayscale => (float)(0.29899999499321 * (double)this.r + 0.587000012397766 * (double)this.g + 57.0 / 500.0 * (double)this.b);

        /// <summary>
        ///   <para>A linear value of an sRGB color.</para>
        /// </summary>
        //public Color linear => new Color(Mathf.GammaToLinearSpace(this.r), Mathf.GammaToLinearSpace(this.g), Mathf.GammaToLinearSpace(this.b), this.a);

        /// <summary>
        ///   <para>A version of the color that has had the gamma curve applied.</para>
        /// </summary>
        //public Color gamma => new Color(Mathf.LinearToGammaSpace(this.r), Mathf.LinearToGammaSpace(this.g), Mathf.LinearToGammaSpace(this.b), this.a);

        /// <summary>
        ///   <para>Returns the maximum color component value: Max(r,g,b).</para>
        /// </summary>
        //public float maxColorComponent => Mathf.Max(Mathf.Max(this.r, this.g), this.b);

        public static implicit operator Vector4(Color c) => new Vector4(c.r, c.g, c.b, c.a);

        public static implicit operator Color(Vector4 v) => new Color(v.x, v.y, v.z, v.w);

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.r;
                    case 1:
                        return this.g;
                    case 2:
                        return this.b;
                    case 3:
                        return this.a;
                    default:
                        throw new IndexOutOfRangeException("Invalid Color index(" + (object)index + ")!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.r = value;
                        break;
                    case 1:
                        this.g = value;
                        break;
                    case 2:
                        this.b = value;
                        break;
                    case 3:
                        this.a = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Color index(" + (object)index + ")!");
                }
            }
        }

        public static void RGBToHSV(Color rgbColor, out float H, out float S, out float V)
        {
            if ((double)rgbColor.b > (double)rgbColor.g && (double)rgbColor.b > (double)rgbColor.r)
                Color.RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
            else if ((double)rgbColor.g > (double)rgbColor.r)
                Color.RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
            else
                Color.RGBToHSVHelper(0.0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
        }

        private static void RGBToHSVHelper(
          float offset,
          float dominantcolor,
          float colorone,
          float colortwo,
          out float H,
          out float S,
          out float V)
        {
            V = dominantcolor;
            if ((double)V != 0.0)
            {
                float num1 = (double)colorone <= (double)colortwo ? colorone : colortwo;
                float num2 = V - num1;
                if ((double)num2 != 0.0)
                {
                    S = num2 / V;
                    H = offset + (colorone - colortwo) / num2;
                }
                else
                {
                    S = 0.0f;
                    H = offset + (colorone - colortwo);
                }
                H /= 6f;
                if ((double)H >= 0.0)
                    return;
                ++H;
            }
            else
            {
                S = 0.0f;
                H = 0.0f;
            }
        }

        /// <summary>
        ///   <para>Creates an RGB colour from HSV input.</para>
        /// </summary>
        /// <param name="H">Hue [0..1].</param>
        /// <param name="S">Saturation [0..1].</param>
        /// <param name="V">Brightness value [0..1].</param>
        /// <param name="hdr">Output HDR colours. If true, the returned colour will not be clamped to [0..1].</param>
        /// <returns>
        ///   <para>An opaque colour with HSV matching the input.</para>
        /// </returns>
        //public static Color HSVToRGB(float H, float S, float V) => Color.HSVToRGB(H, S, V, true);

        /// <summary>
        ///   <para>Creates an RGB colour from HSV input.</para>
        /// </summary>
        /// <param name="H">Hue [0..1].</param>
        /// <param name="S">Saturation [0..1].</param>
        /// <param name="V">Brightness value [0..1].</param>
        /// <param name="hdr">Output HDR colours. If true, the returned colour will not be clamped to [0..1].</param>
        /// <returns>
        ///   <para>An opaque colour with HSV matching the input.</para>
        /// </returns>
        //public static Color HSVToRGB(float H, float S, float V, bool hdr)
        //{
        //    Color white = Color.white;
        //    if ((double)S == 0.0)
        //    {
        //        white.r = V;
        //        white.g = V;
        //        white.b = V;
        //    }
        //    else if ((double)V == 0.0)
        //    {
        //        white.r = 0.0f;
        //        white.g = 0.0f;
        //        white.b = 0.0f;
        //    }
        //    else
        //    {
        //        white.r = 0.0f;
        //        white.g = 0.0f;
        //        white.b = 0.0f;
        //        float num1 = S;
        //        float num2 = V;
        //        float f = H * 6f;
        //        int num3 = (int)Mathf.Floor(f);
        //        float num4 = f - (float)num3;
        //        float num5 = num2 * (1f - num1);
        //        float num6 = num2 * (float)(1.0 - (double)num1 * (double)num4);
        //        float num7 = num2 * (float)(1.0 - (double)num1 * (1.0 - (double)num4));
        //        switch (num3)
        //        {
        //            case -1:
        //                white.r = num2;
        //                white.g = num5;
        //                white.b = num6;
        //                break;
        //            case 0:
        //                white.r = num2;
        //                white.g = num7;
        //                white.b = num5;
        //                break;
        //            case 1:
        //                white.r = num6;
        //                white.g = num2;
        //                white.b = num5;
        //                break;
        //            case 2:
        //                white.r = num5;
        //                white.g = num2;
        //                white.b = num7;
        //                break;
        //            case 3:
        //                white.r = num5;
        //                white.g = num6;
        //                white.b = num2;
        //                break;
        //            case 4:
        //                white.r = num7;
        //                white.g = num5;
        //                white.b = num2;
        //                break;
        //            case 5:
        //                white.r = num2;
        //                white.g = num5;
        //                white.b = num6;
        //                break;
        //            case 6:
        //                white.r = num2;
        //                white.g = num7;
        //                white.b = num5;
        //                break;
        //        }
        //        if (!hdr)
        //        {
        //            white.r = Mathf.Clamp(white.r, 0.0f, 1f);
        //            white.g = Mathf.Clamp(white.g, 0.0f, 1f);
        //            white.b = Mathf.Clamp(white.b, 0.0f, 1f);
        //        }
        //    }
        //    return white;
        //}
    }
}
