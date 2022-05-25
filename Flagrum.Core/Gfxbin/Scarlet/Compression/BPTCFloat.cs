using System;
using Scarlet.IO;

namespace Scarlet.Drawing.Compression;
/* Ported from detex by Harm Hanemaaijer
 * https://github.com/hglm/detex
 * Based on revision https://github.com/hglm/detex/tree/cab11584d9be140602a66fd9a88ef0b99f08a97a */

/* detex license (ISC):
 * --------------------
 * 
 * Copyright (c) 2015 Harm Hanemaaijer <fgenfb@yahoo.com>
 * 
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

// TODO: little conversion issues here and there, fixme!

internal static class BPTCFloat
{
    /* All following ported/adapted from detex */

    private static readonly sbyte[] map_mode_table =
    {
        0, 1, 2, 10, -1, -1, 3, 11, -1, -1, 4, 12, -1, -1, 5, 13,
        -1, -1, 6, -1, -1, -1, 7, -1, -1, -1, 8, -1, -1, -1, 9, -1
    };

    private static readonly byte[] bptc_float_EPB = {10, 7, 11, 11, 11, 9, 8, 8, 8, 6, 10, 11, 12, 16};

    /* Shim functions */
    public static byte[] Decompress(EndianBinaryReader reader, int width, int height, PixelDataFormat inputFormat,
        long readLength)
    {
        var outPixels = new byte[readLength * 8];

        var pixelOrderingFunc = ImageBinary.GetPixelOrderingFunction(inputFormat & PixelDataFormat.MaskPixelOrdering);

        for (var y = 0; y < height; y += 4)
        {
            for (var x = 0; x < width; x += 4)
            {
                var decompressedBlock = new ushort[4 * 4 * 4];
                if ((inputFormat & PixelDataFormat.MaskSpecial) == PixelDataFormat.SpecialFormatBPTC_Float)
                {
                    DecompressBlockBPTCFloatShared(reader, false, ref decompressedBlock);
                }
                else if ((inputFormat & PixelDataFormat.MaskSpecial) == PixelDataFormat.SpecialFormatBPTC_SignedFloat)
                {
                    DecompressBlockBPTCFloatShared(reader, true, ref decompressedBlock);
                }
                else
                {
                    throw new Exception("Trying to decode BPTC Float/SignedFloat with format set to non-BPTC");
                }

                int rx, ry;
                pixelOrderingFunc(x / 4, y / 4, width / 4, height / 4, inputFormat, out rx, out ry);
                rx *= 4;
                ry *= 4;

                for (var py = 0; py < 4; py++)
                {
                    for (var px = 0; px < 4; px++)
                    {
                        var ix = rx + px;
                        var iy = ry + py;

                        if (ix >= width || iy >= height)
                        {
                            continue;
                        }

                        for (var c = 0; c < 4; c++)
                        {
                            var value = Float16toFloat32(decompressedBlock[(py * 4 + px) * 4 + c]);
                            outPixels[(iy * width + ix) * 4 + c] = (byte)(value * 255);
                        }
                    }
                }
            }
        }

        return outPixels;
    }

    // http://codereview.stackexchange.com/q/45007
    private static float Float16toFloat32(ushort hbits)
    {
        uint mant = (ushort)(hbits & 0x03FF);
        uint exp = (ushort)(hbits & 0x7C00);

        if (exp == 0x7C00)
        {
            exp = 0x3FC00;
        }
        else if (exp != 0)
        {
            exp += 0x1C000;
            if (mant == 0 && exp > 0x1C400)
            {
                return BitConverter.ToSingle(
                    BitConverter.GetBytes((((uint)hbits & 0x8000) << 16) | (exp << 13) | 0x3FF), 0);
            }
        }
        else if (mant != 0)
        {
            exp = 0x1C400;
            do
            {
                mant <<= 1;
                exp -= 0x400;
            } while ((mant & 0x400) == 0);

            mant &= 0x3FF;
        }

        return BitConverter.ToSingle(BitConverter.GetBytes((((uint)hbits & 0x8000) << 16) | ((exp | mant) << 13)), 0);
    }

    private static int ExtractMode(detexBlock128 block)
    {
        var mode = (int)BPTC.detexBlock128ExtractBits(block, 2);
        if (mode < 2)
        {
            return mode;
        }

        return map_mode_table[mode | BPTC.detexBlock128ExtractBits(block, 3) << 2];
    }

    private static int GetPartitionIndex(int nu_subsets, uint partition_set_id, int i)
    {
        if (nu_subsets == 1)
        {
            return 0;
        }

        // nu_subset == 2
        return BPTC.detex_bptc_table_P2[partition_set_id * 16 + i];
    }

    private static int GetAnchorIndex(uint partition_set_id, int partition, int nu_subsets)
    {
        if (partition == 0)
        {
            return 0;
        }

        // nu_subsets = 2, partition = 1.
        return BPTC.detex_bptc_table_anchor_index_second_subset[partition_set_id];
    }

    private static uint Unquantize(uint x, int mode)
    {
        int unq;
        if (mode == 13)
        {
            unq = (int)x;
        }
        else if (x == 0)
        {
            unq = 0;
        }
        else if (x == (1 << bptc_float_EPB[mode]) - 1)
        {
            unq = 0xFFFF;
        }
        else
        {
            unq = (((int)x << 15) + 0x4000) >> (bptc_float_EPB[mode] - 1);
        }

        return (uint)unq;
    }

    private static int UnquantizeSigned(int x, int mode)
    {
        var s = 0;
        int unq;
        if (bptc_float_EPB[mode] >= 16)
        {
            unq = x;
        }
        else
        {
            if (x < 0)
            {
                s = 1;
                x = -x;
            }

            if (x == 0)
            {
                unq = 0;
            }
            else if (x >= (1 << (bptc_float_EPB[mode] - 1)) - 1)
            {
                unq = 0x7FFF;
            }
            else
            {
                unq = ((x << 15) + 0x4000) >> (bptc_float_EPB[mode] - 1);
            }

            if (s != 0)
            {
                unq = -unq;
            }
        }

        return unq;
    }

    private static uint SignExtend(uint value, int source_nu_bits, int target_nu_bits)
    {
        var sign_bit = (uint)(value & (1 << (source_nu_bits - 1)));
        if (sign_bit == 0)
        {
            return value;
        }

        var sign_extend_bits = (uint)(0xFFFFFFFF ^ ((1 << source_nu_bits) - 1));
        sign_extend_bits &= (uint)(((ulong)1 << target_nu_bits) - 1);
        return value | sign_extend_bits;
    }

    private static int InterpolateFloat(uint e0, uint e1, short index, byte indexprecision)
    {
        return (int)(((64 - BPTC.detex_bptc_table_aWeights[indexprecision - 2][index]) * e0 +
                      BPTC.detex_bptc_table_aWeights[indexprecision - 2][index] * e1 + 32) >> 6);
    }

    private static void DecompressBlockBPTCFloatShared(EndianBinaryReader reader, bool signed_flag,
        ref ushort[] pixel_buffer)
    {
        detexBlock128 block = new detexBlock128();
        block.data0 = reader.ReadUInt64();
        block.data1 = reader.ReadUInt64();
        block.index = 0;

        var mode = ExtractMode(block);
        if (mode == -1)
        {
            return;
        }

        uint[] r = new uint[4], g = new uint[4], b = new uint[4];
        uint partition_set_id = 0;
        int delta_bits_r = 0, delta_bits_g = 0, delta_bits_b = 0;

        ulong data0 = block.data0;
        ulong data1 = block.data1;

        switch (mode)
        {
            case 0:
                // m[1:0],g2[4],b2[4],b3[4],r0[9:0],g0[9:0],b0[9:0],r1[4:0],g3[4],g2[3:0],
                // g1[4:0],b3[0],g3[3:0],b1[4:0],b3[1],b2[3:0],r2[4:0],b3[2],r3[4:0],b3[3]
                g[2] = BPTC.detexGetBits64(data0, 2, 2) << 4;
                b[2] = BPTC.detexGetBits64(data0, 3, 3) << 4;
                b[3] = BPTC.detexGetBits64(data0, 4, 4) << 4;
                r[0] = BPTC.detexGetBits64(data0, 5, 14);
                g[0] = BPTC.detexGetBits64(data0, 15, 24);
                b[0] = BPTC.detexGetBits64(data0, 25, 34);
                r[1] = BPTC.detexGetBits64(data0, 35, 39);
                g[3] = BPTC.detexGetBits64(data0, 40, 40) << 4;
                g[2] |= BPTC.detexGetBits64(data0, 41, 44);
                g[1] = BPTC.detexGetBits64(data0, 45, 49);
                b[3] |= BPTC.detexGetBits64(data0, 50, 50);
                g[3] |= BPTC.detexGetBits64(data0, 51, 54);
                b[1] = BPTC.detexGetBits64(data0, 55, 59);
                b[3] |= BPTC.detexGetBits64(data0, 60, 60) << 1;
                b[2] |= BPTC.detexGetBits64(data0, 61, 63);
                b[2] |= BPTC.detexGetBits64(data1, 0, 0) << 3;
                r[2] = BPTC.detexGetBits64(data1, 1, 5);
                b[3] |= BPTC.detexGetBits64(data1, 6, 6) << 2;
                r[3] = BPTC.detexGetBits64(data1, 7, 11);
                b[3] |= BPTC.detexGetBits64(data1, 12, 12) << 3;
                partition_set_id = BPTC.detexGetBits64(data1, 13, 17);
                block.index = 64 + 18;
                delta_bits_r = delta_bits_g = delta_bits_b = 5;
                break;
            case 1:
                // m[1:0],g2[5],g3[4],g3[5],r0[6:0],b3[0],b3[1],b2[4],g0[6:0],b2[5],b3[2],
                // g2[4],b0[6:0],b3[3],b3[5],b3[4],r1[5:0],g2[3:0],g1[5:0],g3[3:0],b1[5:0],
                // b2[3:0],r2[5:0],r3[5:0]
                g[2] = BPTC.detexGetBits64(data0, 2, 2) << 5;
                g[3] = BPTC.detexGetBits64(data0, 3, 3) << 4;
                g[3] |= BPTC.detexGetBits64(data0, 4, 4) << 5;
                r[0] = BPTC.detexGetBits64(data0, 5, 11);
                b[3] = BPTC.detexGetBits64(data0, 12, 12);
                b[3] |= BPTC.detexGetBits64(data0, 13, 13) << 1;
                b[2] = BPTC.detexGetBits64(data0, 14, 14) << 4;
                g[0] = BPTC.detexGetBits64(data0, 15, 21);
                b[2] |= BPTC.detexGetBits64(data0, 22, 22) << 5;
                b[3] |= BPTC.detexGetBits64(data0, 23, 23) << 2;
                g[2] |= BPTC.detexGetBits64(data0, 24, 24) << 4;
                b[0] = BPTC.detexGetBits64(data0, 25, 31);
                b[3] |= BPTC.detexGetBits64(data0, 32, 32) << 3;
                b[3] |= BPTC.detexGetBits64(data0, 33, 33) << 5;
                b[3] |= BPTC.detexGetBits64(data0, 34, 34) << 4;
                r[1] = BPTC.detexGetBits64(data0, 35, 40);
                g[2] |= BPTC.detexGetBits64(data0, 41, 44);
                g[1] = BPTC.detexGetBits64(data0, 45, 50);
                g[3] |= BPTC.detexGetBits64(data0, 51, 54);
                b[1] = BPTC.detexGetBits64(data0, 55, 60);
                b[2] |= BPTC.detexGetBits64(data0, 61, 63);
                b[2] |= BPTC.detexGetBits64(data1, 0, 0) << 3;
                r[2] = BPTC.detexGetBits64(data1, 1, 6);
                r[3] = BPTC.detexGetBits64(data1, 7, 12);
                partition_set_id = BPTC.detexGetBits64(data1, 13, 17);
                block.index = 64 + 18;
                delta_bits_r = delta_bits_g = delta_bits_b = 6;
                break;
            case 2:
                // m[4:0],r0[9:0],g0[9:0],b0[9:0],r1[4:0],r0[10],g2[3:0],g1[3:0],g0[10],
                // b3[0],g3[3:0],b1[3:0],b0[10],b3[1],b2[3:0],r2[4:0],b3[2],r3[4:0],b3[3]
                r[0] = BPTC.detexGetBits64(data0, 5, 14);
                g[0] = BPTC.detexGetBits64(data0, 15, 24);
                b[0] = BPTC.detexGetBits64(data0, 25, 34);
                r[1] = BPTC.detexGetBits64(data0, 35, 39);
                r[0] |= BPTC.detexGetBits64(data0, 40, 40) << 10;
                g[2] = BPTC.detexGetBits64(data0, 41, 44);
                g[1] = BPTC.detexGetBits64(data0, 45, 48);
                g[0] |= BPTC.detexGetBits64(data0, 49, 49) << 10;
                b[3] = BPTC.detexGetBits64(data0, 50, 50);
                g[3] = BPTC.detexGetBits64(data0, 51, 54);
                b[1] = BPTC.detexGetBits64(data0, 55, 58);
                b[0] |= BPTC.detexGetBits64(data0, 59, 59) << 10;
                b[3] |= BPTC.detexGetBits64(data0, 60, 60) << 1;
                b[2] = BPTC.detexGetBits64(data0, 61, 63);
                b[2] |= BPTC.detexGetBits64(data1, 0, 0) << 3;
                r[2] = BPTC.detexGetBits64(data1, 1, 5);
                b[3] |= BPTC.detexGetBits64(data1, 6, 6) << 2;
                r[3] = BPTC.detexGetBits64(data1, 7, 11);
                b[3] |= BPTC.detexGetBits64(data1, 12, 12) << 3;
                partition_set_id = BPTC.detexGetBits64(data1, 13, 17);
                block.index = 64 + 18;
                delta_bits_r = 5;
                delta_bits_g = delta_bits_b = 4;
                break;
            case 3: // Original mode 6.
                // m[4:0],r0[9:0],g0[9:0],b0[9:0],r1[3:0],r0[10],g3[4],g2[3:0],g1[4:0],
                // g0[10],g3[3:0],b1[3:0],b0[10],b3[1],b2[3:0],r2[3:0],b3[0],b3[2],r3[3:0],
                // g2[4],b3[3]
                r[0] = BPTC.detexGetBits64(data0, 5, 14);
                g[0] = BPTC.detexGetBits64(data0, 15, 24);
                b[0] = BPTC.detexGetBits64(data0, 25, 34);
                r[1] = BPTC.detexGetBits64(data0, 35, 38);
                r[0] |= BPTC.detexGetBits64(data0, 39, 39) << 10;
                g[3] = BPTC.detexGetBits64(data0, 40, 40) << 4;
                g[2] = BPTC.detexGetBits64(data0, 41, 44);
                g[1] = BPTC.detexGetBits64(data0, 45, 49);
                g[0] |= BPTC.detexGetBits64(data0, 50, 50) << 10;
                g[3] |= BPTC.detexGetBits64(data0, 51, 54);
                b[1] = BPTC.detexGetBits64(data0, 55, 58);
                b[0] |= BPTC.detexGetBits64(data0, 59, 59) << 10;
                b[3] = BPTC.detexGetBits64(data0, 60, 60) << 1;
                b[2] = BPTC.detexGetBits64(data0, 61, 63);
                b[2] |= BPTC.detexGetBits64(data1, 0, 0) << 3;
                r[2] = BPTC.detexGetBits64(data1, 1, 4);
                b[3] |= BPTC.detexGetBits64(data1, 5, 5);
                b[3] |= BPTC.detexGetBits64(data1, 6, 6) << 2;
                r[3] = BPTC.detexGetBits64(data1, 7, 10);
                g[2] |= BPTC.detexGetBits64(data1, 11, 11) << 4;
                b[3] |= BPTC.detexGetBits64(data1, 12, 12) << 3;
                partition_set_id = BPTC.detexGetBits64(data1, 13, 17);
                block.index = 64 + 18;
                delta_bits_r = delta_bits_b = 4;
                delta_bits_g = 5;
                break;
            case 4: // Original mode 10.
                // m[4:0],r0[9:0],g0[9:0],b0[9:0],r1[3:0],r0[10],b2[4],g2[3:0],g1[3:0],
                // g0[10],b3[0],g3[3:0],b1[4:0],b0[10],b2[3:0],r2[3:0],b3[1],b3[2],r3[3:0],
                // b3[4],b3[3]
                r[0] = BPTC.detexGetBits64(data0, 5, 14);
                g[0] = BPTC.detexGetBits64(data0, 15, 24);
                b[0] = BPTC.detexGetBits64(data0, 25, 34);
                r[1] = BPTC.detexGetBits64(data0, 35, 38);
                r[0] |= BPTC.detexGetBits64(data0, 39, 39) << 10;
                b[2] = BPTC.detexGetBits64(data0, 40, 40) << 4;
                g[2] = BPTC.detexGetBits64(data0, 41, 44);
                g[1] = BPTC.detexGetBits64(data0, 45, 48);
                g[0] |= BPTC.detexGetBits64(data0, 49, 49) << 10;
                b[3] = BPTC.detexGetBits64(data0, 50, 50);
                g[3] = BPTC.detexGetBits64(data0, 51, 54);
                b[1] = BPTC.detexGetBits64(data0, 55, 59);
                b[0] |= BPTC.detexGetBits64(data0, 60, 60) << 10;
                b[2] |= BPTC.detexGetBits64(data0, 61, 63);
                b[2] |= BPTC.detexGetBits64(data1, 0, 0) << 3;
                r[2] = BPTC.detexGetBits64(data1, 1, 4);
                b[3] |= BPTC.detexGetBits64(data1, 5, 5) << 1;
                b[3] |= BPTC.detexGetBits64(data1, 6, 6) << 2;
                r[3] = BPTC.detexGetBits64(data1, 7, 10);
                b[3] |= BPTC.detexGetBits64(data1, 11, 11) << 4;
                b[3] |= BPTC.detexGetBits64(data1, 12, 12) << 3;
                partition_set_id = BPTC.detexGetBits64(data1, 13, 17);
                block.index = 64 + 18;
                delta_bits_r = delta_bits_g = 4;
                delta_bits_b = 5;
                break;
            case 5: // Original mode 14
                // m[4:0],r0[8:0],b2[4],g0[8:0],g2[4],b0[8:0],b3[4],r1[4:0],g3[4],g2[3:0],
                // g1[4:0],b3[0],g3[3:0],b1[4:0],b3[1],b2[3:0],r2[4:0],b3[2],r3[4:0],b3[3]
                r[0] = BPTC.detexGetBits64(data0, 5, 13);
                b[2] = BPTC.detexGetBits64(data0, 14, 14) << 4;
                g[0] = BPTC.detexGetBits64(data0, 15, 23);
                g[2] = BPTC.detexGetBits64(data0, 24, 24) << 4;
                b[0] = BPTC.detexGetBits64(data0, 25, 33);
                b[3] = BPTC.detexGetBits64(data0, 34, 34) << 4;
                r[1] = BPTC.detexGetBits64(data0, 35, 39);
                g[3] = BPTC.detexGetBits64(data0, 40, 40) << 4;
                g[2] |= BPTC.detexGetBits64(data0, 41, 44);
                g[1] = BPTC.detexGetBits64(data0, 45, 49);
                b[3] |= BPTC.detexGetBits64(data0, 50, 50);
                g[3] |= BPTC.detexGetBits64(data0, 51, 54);
                b[1] = BPTC.detexGetBits64(data0, 55, 59);
                b[3] |= BPTC.detexGetBits64(data0, 60, 60) << 1;
                b[2] |= BPTC.detexGetBits64(data0, 61, 63);
                b[2] |= BPTC.detexGetBits64(data1, 0, 0) << 3;
                r[2] = BPTC.detexGetBits64(data1, 1, 5);
                b[3] |= BPTC.detexGetBits64(data1, 6, 6) << 2;
                r[3] = BPTC.detexGetBits64(data1, 7, 11);
                b[3] |= BPTC.detexGetBits64(data1, 12, 12) << 3;
                partition_set_id = BPTC.detexGetBits64(data1, 13, 17);
                block.index = 64 + 18;
                delta_bits_r = delta_bits_g = delta_bits_b = 5;
                break;
            case 6: // Original mode 18
                // m[4:0],r0[7:0],g3[4],b2[4],g0[7:0],b3[2],g2[4],b0[7:0],b3[3],b3[4],
                // r1[5:0],g2[3:0],g1[4:0],b3[0],g3[3:0],b1[4:0],b3[1],b2[3:0],r2[5:0],r3[5:0]
                r[0] = BPTC.detexGetBits64(data0, 5, 12);
                g[3] = BPTC.detexGetBits64(data0, 13, 13) << 4;
                b[2] = BPTC.detexGetBits64(data0, 14, 14) << 4;
                g[0] = BPTC.detexGetBits64(data0, 15, 22);
                b[3] = BPTC.detexGetBits64(data0, 23, 23) << 2;
                g[2] = BPTC.detexGetBits64(data0, 24, 24) << 4;
                b[0] = BPTC.detexGetBits64(data0, 25, 32);
                b[3] |= BPTC.detexGetBits64(data0, 33, 33) << 3;
                b[3] |= BPTC.detexGetBits64(data0, 34, 34) << 4;
                r[1] = BPTC.detexGetBits64(data0, 35, 40);
                g[2] |= BPTC.detexGetBits64(data0, 41, 44);
                g[1] = BPTC.detexGetBits64(data0, 45, 49);
                b[3] |= BPTC.detexGetBits64(data0, 50, 50);
                g[3] |= BPTC.detexGetBits64(data0, 51, 54);
                b[1] = BPTC.detexGetBits64(data0, 55, 59);
                b[3] |= BPTC.detexGetBits64(data0, 60, 60) << 1;
                b[2] |= BPTC.detexGetBits64(data0, 61, 63);
                b[2] |= BPTC.detexGetBits64(data1, 0, 0) << 3;
                r[2] = BPTC.detexGetBits64(data1, 1, 6);
                r[3] = BPTC.detexGetBits64(data1, 7, 12);
                partition_set_id = BPTC.detexGetBits64(data1, 13, 17);
                block.index = 64 + 18;
                delta_bits_r = 6;
                delta_bits_g = delta_bits_b = 5;
                break;
            case 7: // Original mode 22
                // m[4:0],r0[7:0],b3[0],b2[4],g0[7:0],g2[5],g2[4],b0[7:0],g3[5],b3[4],
                // r1[4:0],g3[4],g2[3:0],g1[5:0],g3[3:0],b1[4:0],b3[1],b2[3:0],r2[4:0],
                // b3[2],r3[4:0],b3[3]
                r[0] = BPTC.detexGetBits64(data0, 5, 12);
                b[3] = BPTC.detexGetBits64(data0, 13, 13);
                b[2] = BPTC.detexGetBits64(data0, 14, 14) << 4;
                g[0] = BPTC.detexGetBits64(data0, 15, 22);
                g[2] = BPTC.detexGetBits64(data0, 23, 23) << 5;
                g[2] |= BPTC.detexGetBits64(data0, 24, 24) << 4;
                b[0] = BPTC.detexGetBits64(data0, 25, 32);
                g[3] = BPTC.detexGetBits64(data0, 33, 33) << 5;
                b[3] |= BPTC.detexGetBits64(data0, 34, 34) << 4;
                r[1] = BPTC.detexGetBits64(data0, 35, 39);
                g[3] |= BPTC.detexGetBits64(data0, 40, 40) << 4;
                g[2] |= BPTC.detexGetBits64(data0, 41, 44);
                g[1] = BPTC.detexGetBits64(data0, 45, 50);
                g[3] |= BPTC.detexGetBits64(data0, 51, 54);
                b[1] = BPTC.detexGetBits64(data0, 55, 59);
                b[3] |= BPTC.detexGetBits64(data0, 60, 60) << 1;
                b[2] |= BPTC.detexGetBits64(data0, 61, 63);
                b[2] |= BPTC.detexGetBits64(data1, 0, 0) << 3;
                r[2] = BPTC.detexGetBits64(data1, 1, 5);
                b[3] |= BPTC.detexGetBits64(data1, 6, 6) << 2;
                r[3] = BPTC.detexGetBits64(data1, 7, 11);
                b[3] |= BPTC.detexGetBits64(data1, 12, 12) << 3;
                partition_set_id = BPTC.detexGetBits64(data1, 13, 17);
                block.index = 64 + 18;
                delta_bits_r = delta_bits_b = 5;
                delta_bits_g = 6;
                break;
            case 8: // Original mode 26
                // m[4:0],r0[7:0],b3[1],b2[4],g0[7:0],b2[5],g2[4],b0[7:0],b3[5],b3[4],
                // r1[4:0],g3[4],g2[3:0],g1[4:0],b3[0],g3[3:0],b1[5:0],b2[3:0],r2[4:0],
                // b3[2],r3[4:0],b3[3]
                r[0] = BPTC.detexGetBits64(data0, 5, 12);
                b[3] = BPTC.detexGetBits64(data0, 13, 13) << 1;
                b[2] = BPTC.detexGetBits64(data0, 14, 14) << 4;
                g[0] = BPTC.detexGetBits64(data0, 15, 22);
                b[2] |= BPTC.detexGetBits64(data0, 23, 23) << 5;
                g[2] = BPTC.detexGetBits64(data0, 24, 24) << 4;
                b[0] = BPTC.detexGetBits64(data0, 25, 32);
                b[3] |= BPTC.detexGetBits64(data0, 33, 33) << 5;
                b[3] |= BPTC.detexGetBits64(data0, 34, 34) << 4;
                r[1] = BPTC.detexGetBits64(data0, 35, 39);
                g[3] = BPTC.detexGetBits64(data0, 40, 40) << 4;
                g[2] |= BPTC.detexGetBits64(data0, 41, 44);
                g[1] = BPTC.detexGetBits64(data0, 45, 49);
                b[3] |= BPTC.detexGetBits64(data0, 50, 50);
                g[3] |= BPTC.detexGetBits64(data0, 51, 54);
                b[1] = BPTC.detexGetBits64(data0, 55, 60);
                b[2] |= BPTC.detexGetBits64(data0, 61, 63);
                b[2] |= BPTC.detexGetBits64(data1, 0, 0) << 3;
                r[2] = BPTC.detexGetBits64(data1, 1, 5);
                b[3] |= BPTC.detexGetBits64(data1, 6, 6) << 2;
                r[3] = BPTC.detexGetBits64(data1, 7, 11);
                b[3] |= BPTC.detexGetBits64(data1, 12, 12) << 3;
                partition_set_id = BPTC.detexGetBits64(data1, 13, 17);
                block.index = 64 + 18;
                delta_bits_r = delta_bits_g = 5;
                delta_bits_b = 6;
                break;
            case 9: // Original mode 30
                // m[4:0],r0[5:0],g3[4],b3[0],b3[1],b2[4],g0[5:0],g2[5],b2[5],b3[2],
                // g2[4],b0[5:0],g3[5],b3[3],b3[5],b3[4],r1[5:0],g2[3:0],g1[5:0],g3[3:0],
                // b1[5:0],b2[3:0],r2[5:0],r3[5:0]
                r[0] = BPTC.detexGetBits64(data0, 5, 10);
                g[3] = BPTC.detexGetBits64(data0, 11, 11) << 4;
                b[3] = BPTC.detexGetBits64(data0, 12, 13);
                b[2] = BPTC.detexGetBits64(data0, 14, 14) << 4;
                g[0] = BPTC.detexGetBits64(data0, 15, 20);
                g[2] = BPTC.detexGetBits64(data0, 21, 21) << 5;
                b[2] |= BPTC.detexGetBits64(data0, 22, 22) << 5;
                b[3] |= BPTC.detexGetBits64(data0, 23, 23) << 2;
                g[2] |= BPTC.detexGetBits64(data0, 24, 24) << 4;
                b[0] = BPTC.detexGetBits64(data0, 25, 30);
                g[3] |= BPTC.detexGetBits64(data0, 31, 31) << 5;
                b[3] |= BPTC.detexGetBits64(data0, 32, 32) << 3;
                b[3] |= BPTC.detexGetBits64(data0, 33, 33) << 5;
                b[3] |= BPTC.detexGetBits64(data0, 34, 34) << 4;
                r[1] = BPTC.detexGetBits64(data0, 35, 40);
                g[2] |= BPTC.detexGetBits64(data0, 41, 44);
                g[1] = BPTC.detexGetBits64(data0, 45, 50);
                g[3] |= BPTC.detexGetBits64(data0, 51, 54);
                b[1] = BPTC.detexGetBits64(data0, 55, 60);
                b[2] |= BPTC.detexGetBits64(data0, 61, 63);
                b[2] |= BPTC.detexGetBits64(data1, 0, 0) << 3;
                r[2] = BPTC.detexGetBits64(data1, 1, 6);
                r[3] = BPTC.detexGetBits64(data1, 7, 12);
                partition_set_id = BPTC.detexGetBits64(data1, 13, 17);
                block.index = 64 + 18;
                //		delta_bits_r = delta_bits_g = delta_bits_b = 6;
                break;
            case 10: // Original mode 3
                // m[4:0],r0[9:0],g0[9:0],b0[9:0],r1[9:0],g1[9:0],b1[9:0]
                r[0] = BPTC.detexGetBits64(data0, 5, 14);
                g[0] = BPTC.detexGetBits64(data0, 15, 24);
                b[0] = BPTC.detexGetBits64(data0, 25, 34);
                r[1] = BPTC.detexGetBits64(data0, 35, 44);
                g[1] = BPTC.detexGetBits64(data0, 45, 54);
                b[1] = BPTC.detexGetBits64(data0, 55, 63);
                b[1] |= BPTC.detexGetBits64(data1, 0, 0) << 9;
                partition_set_id = 0;
                block.index = 65;
                //		delta_bits_r = delta_bits_g = delta_bits_b = 10;
                break;
            case 11: // Original mode 7
                // m[4:0],r0[9:0],g0[9:0],b0[9:0],r1[8:0],r0[10],g1[8:0],g0[10],b1[8:0],b0[10]
                r[0] = BPTC.detexGetBits64(data0, 5, 14);
                g[0] = BPTC.detexGetBits64(data0, 15, 24);
                b[0] = BPTC.detexGetBits64(data0, 25, 34);
                r[1] = BPTC.detexGetBits64(data0, 35, 43);
                r[0] |= BPTC.detexGetBits64(data0, 44, 44) << 10;
                g[1] = BPTC.detexGetBits64(data0, 45, 53);
                g[0] |= BPTC.detexGetBits64(data0, 54, 54) << 10;
                b[1] = BPTC.detexGetBits64(data0, 55, 63);
                b[0] |= BPTC.detexGetBits64(data1, 0, 0) << 10;
                partition_set_id = 0;
                block.index = 65;
                delta_bits_r = delta_bits_g = delta_bits_b = 9;
                break;
            case 12: // Original mode 11
                // m[4:0],r0[9:0],g0[9:0],b0[9:0],r1[7:0],r0[10:11],g1[7:0],g0[10:11],
                // b1[7:0],b0[10:11]
                r[0] = BPTC.detexGetBits64(data0, 5, 14);
                g[0] = BPTC.detexGetBits64(data0, 15, 24);
                b[0] = BPTC.detexGetBits64(data0, 25, 34);
                r[1] = BPTC.detexGetBits64(data0, 35, 42);
                r[0] |= BPTC.detexGetBits64Reversed(data0, 44, 43) << 10; // Reversed.
                g[1] = BPTC.detexGetBits64(data0, 45, 52);
                g[0] |= BPTC.detexGetBits64Reversed(data0, 54, 53) << 10; // Reversed.
                b[1] = BPTC.detexGetBits64(data0, 55, 62);
                b[0] |= BPTC.detexGetBits64(data0, 63, 63) << 11; // MSB
                b[0] |= BPTC.detexGetBits64(data1, 0, 0) << 10;   // LSB
                partition_set_id = 0;
                block.index = 65;
                delta_bits_r = delta_bits_g = delta_bits_b = 8;
                break;
            case 13: // Original mode 15
                // m[4:0],r0[9:0],g0[9:0],b0[9:0],r1[3:0],r0[10:15],g1[3:0],g0[10:15],
                // b1[3:0],b0[10:15]
                r[0] = BPTC.detexGetBits64(data0, 5, 14);
                g[0] = BPTC.detexGetBits64(data0, 15, 24);
                b[0] = BPTC.detexGetBits64(data0, 25, 34);
                r[1] = BPTC.detexGetBits64(data0, 35, 38);
                r[0] |= BPTC.detexGetBits64Reversed(data0, 44, 39) << 10; // Reversed.
                g[1] = BPTC.detexGetBits64(data0, 45, 48);
                g[0] |= BPTC.detexGetBits64Reversed(data0, 54, 49) << 10; // Reversed.
                b[1] = BPTC.detexGetBits64(data0, 55, 58);
                b[0] |= BPTC.detexGetBits64Reversed(data0, 63, 59) << 11; // Reversed.
                b[0] |= BPTC.detexGetBits64(data1, 0, 0) << 10;
                partition_set_id = 0;
                block.index = 65;
                delta_bits_r = delta_bits_g = delta_bits_b = 4;
                break;
        }

        var nu_subsets = mode >= 10 ? 1 : 2;

        if (signed_flag)
        {
            r[0] = SignExtend(r[0], bptc_float_EPB[mode], 32);
            g[0] = SignExtend(g[0], bptc_float_EPB[mode], 32);
            b[0] = SignExtend(b[0], bptc_float_EPB[mode], 32);
        }

        if (mode != 9 && mode != 10)
        {
            // Transformed endpoints.
            for (var i = 1; i < nu_subsets * 2; i++)
            {
                r[i] = SignExtend(r[i], delta_bits_r, 32);
                r[i] = (r[0] + r[i]) & (((uint)1 << bptc_float_EPB[mode]) - 1);
                g[i] = SignExtend(g[i], delta_bits_g, 32);
                g[i] = (g[0] + g[i]) & (((uint)1 << bptc_float_EPB[mode]) - 1);
                b[i] = SignExtend(b[i], delta_bits_b, 32);
                b[i] = (b[0] + b[i]) & (((uint)1 << bptc_float_EPB[mode]) - 1);
                if (signed_flag)
                {
                    r[i] = SignExtend(r[i], bptc_float_EPB[mode], 32);
                    g[i] = SignExtend(g[i], bptc_float_EPB[mode], 32);
                    b[i] = SignExtend(b[i], bptc_float_EPB[mode], 32);
                }
            }
        }
        else // Mode 9 or 10, no transformed endpoints.
        {
            if (signed_flag)
            {
                for (var i = 1; i < nu_subsets * 2; i++)
                {
                    r[i] = SignExtend(r[i], bptc_float_EPB[mode], 32);
                    g[i] = SignExtend(g[i], bptc_float_EPB[mode], 32);
                    b[i] = SignExtend(b[i], bptc_float_EPB[mode], 32);
                }
            }
        }

        // Unquantize endpoints.
        if (signed_flag)
        {
            for (var i = 0; i < 2 * nu_subsets; i++)
            {
                r[i] = (uint)UnquantizeSigned((int)r[i], mode);
                g[i] = (uint)UnquantizeSigned((int)g[i], mode);
                b[i] = (uint)UnquantizeSigned((int)b[i], mode);
            }
        }
        else
        {
            for (var i = 0; i < 2 * nu_subsets; i++)
            {
                r[i] = Unquantize(r[i], mode);
                g[i] = Unquantize(g[i], mode);
                b[i] = Unquantize(b[i], mode);
            }
        }

        var subset_index = new byte[16];
        for (var i = 0; i < 16; i++)
        {
            // subset_index[i] is a number from 0 to 1, depending on the number of subsets.
            subset_index[i] = (byte)GetPartitionIndex(nu_subsets, partition_set_id, i);
        }

        var anchor_index = new byte[4]; // Only need max. 2 elements
        for (var i = 0; i < nu_subsets; i++)
        {
            anchor_index[i] = (byte)GetAnchorIndex(partition_set_id, i, nu_subsets);
        }

        var color_index = new byte[16];
        // Extract index bits.
        var color_index_bit_count = 3;
        if ((block.data0 & 3) == 3) // This defines original modes 3, 7, 11, 15
        {
            color_index_bit_count = 4;
        }

        // Because the index bits are all in the second 64-bit word, there is no need to use
        // block_extract_bits().
        data1 >>= block.index - 64;
        var mask1 = (byte)((1 << color_index_bit_count) - 1);
        var mask2 = (byte)((1 << (color_index_bit_count - 1)) - 1);
        for (var i = 0; i < 16; i++)
        {
            if (i == anchor_index[subset_index[i]])
            {
                // Highest bit is zero.
                color_index[i] = (byte)(data1 & mask2);
                data1 >>= color_index_bit_count - 1;
            }
            else
            {
                color_index[i] = (byte)(data1 & mask1);
                data1 >>= color_index_bit_count;
            }
        }

        for (var i = 0; i < 16; i++)
        {
            uint endpoint_start_r, endpoint_start_g, endpoint_start_b;
            uint endpoint_end_r, endpoint_end_g, endpoint_end_b;

            endpoint_start_r = r[2 * subset_index[i]];
            endpoint_end_r = r[2 * subset_index[i] + 1];
            endpoint_start_g = g[2 * subset_index[i]];
            endpoint_end_g = g[2 * subset_index[i] + 1];
            endpoint_start_b = b[2 * subset_index[i]];
            endpoint_end_b = b[2 * subset_index[i] + 1];

            int r16, g16, b16;

            if (signed_flag)
            {
                r16 = InterpolateFloat(endpoint_start_r, endpoint_end_r, color_index[i], (byte)color_index_bit_count);
                if (r16 < 0)

                {
                    r16 = -((-r16 * 31) >> 5);
                }
                else
                {
                    r16 = (r16 * 31) >> 5;
                }

                var s = 0;
                if (r16 < 0)
                {
                    s = 0x8000;
                    r16 = -r16;
                }

                r16 |= s;
                g16 = InterpolateFloat(endpoint_start_g, endpoint_end_g, color_index[i], (byte)color_index_bit_count);
                if (g16 < 0)

                {
                    g16 = -((-g16 * 31) >> 5);
                }
                else
                {
                    g16 = (g16 * 31) >> 5;
                }

                s = 0;
                if (g16 < 0)
                {
                    s = 0x8000;
                    g16 = -g16;
                }

                g16 |= s;
                b16 = InterpolateFloat(endpoint_start_b, endpoint_end_b, color_index[i], (byte)color_index_bit_count);
                if (b16 < 0)

                {
                    b16 = -((-b16 * 31) >> 5);
                }
                else
                {
                    b16 = (b16 * 31) >> 5;
                }

                s = 0;
                if (b16 < 0)
                {
                    s = 0x8000;
                    b16 = -b16;
                }

                b16 |= s;
            }
            else
            {
                r16 = InterpolateFloat(endpoint_start_r, endpoint_end_r, color_index[i], (byte)color_index_bit_count) *
                    31 / 64;
                g16 = InterpolateFloat(endpoint_start_g, endpoint_end_g, color_index[i], (byte)color_index_bit_count) *
                    31 / 64;
                b16 = InterpolateFloat(endpoint_start_b, endpoint_end_b, color_index[i], (byte)color_index_bit_count) *
                    31 / 64;
            }

            pixel_buffer[i * 4 + 0] = (ushort)b16;
            pixel_buffer[i * 4 + 1] = (ushort)g16;
            pixel_buffer[i * 4 + 2] = (ushort)r16;
            pixel_buffer[i * 4 + 3] = 0x3C00;
        }
    }
}