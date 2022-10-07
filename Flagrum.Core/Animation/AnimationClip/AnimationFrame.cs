using System;

namespace Flagrum.Core.Animation.AnimationClip;

public class AnimationFrame
{
    public byte[] NumKeys { get; set; }
    public byte[] Unknowns { get; set; }
    public byte[] CurrentKeyTimeBytes { get; set; }
    public AnimationPackedKey[] PackedKeys { get; set; }
}

public class AnimationPackedKey
{
    public ushort[] Values { get; set; }

    public Quaternion Unpack()
    {
        var x = (Values[0] & 0x7FFF) * 0.000061037019 - 1.0;
        var y = (Values[1] & 0x7FFF) * 0.000061037019 - 1.0;
        var z = (Values[2] & 0x7FFF) * 0.000061037019 - 1.0;
        var w = Math.Sqrt(1.0 - (x * x + y * y + z * z));

        // var v11 = (uint)(Values[0] >> 15) + Values[1] + 1;
        // var v15 = v11 + (uint)(Values[2] >> 15) + 1;
        // var v16 = (x * x) + (y * y);
        // var v18 = ((v15 & 1) + 3) & 0x11 | (uint)((1 - (Values[0] >> 15)) << (1 - Values[1]));
        // var v19 = ((((v18 + 1) | (uint)((Values[2] & 0x7FFF) >> 12)) >> 2)
        //            + (((v18 + 1) | (uint)((Values[2] & 0x7FFF) >> 12)) >> 2)) - 1.0;

        //var w = Math.Sqrt(1.0 - ((z * z) + v16));

        //var w = v19 * (Math.Sqrt(1.0 - ((z * z) + v16)) * v19);
        var a = w;
        return new Quaternion();
        /*
          ushort1FirstTransform = ushort1 & 0x7FFF;
          ushort1SecondTransform = ushort1 >> 15;
          ushort2FirstTransform = ushort2 & 0x7FFF;
          ushort2 >>= 15;
          v11 = (unsigned int)ushort1SecondTransform + ushort2 + 1;
          ushort3FirstTransform = ushort3 & 0x7FFF;
          v15 = (unsigned int)v11 + (ushort3 >> 15) + 1;
          v16 = (float)(x * x) + (float)(y * y);
          v18 = ((v15 & 1) + 3) & 0x11 | ((1 - (_DWORD)ushort1SecondTransform) << (1 - ushort2));
          v19 = ((((v18 + 1) | (ushort3FirstTransform >> 12)) >> 2)
                      + (((v18 + 1) | (ushort3FirstTransform >> 12)) >> 2))
              - 1.0;
          result->m_value.m_vec128.m128_f32[v18] = fsqrt(1.0 - (float)((float)(z * z) + v16)) * v19;
          pUnpackedQuaternion = result;
          result->m_value.m_vec128.m128_f32[3] = v19 * result->m_value.m_vec128.m128_f32[3];
          return pUnpackedQuaternion;
         */
    }
}