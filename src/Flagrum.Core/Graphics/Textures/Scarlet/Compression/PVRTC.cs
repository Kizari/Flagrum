using System;
using System.Diagnostics;
using Scarlet.IO;

namespace Scarlet.Drawing.Compression;
/* Ported from PowerVR Graphics Native SDK, Copyright (c) Imagination Technologies Ltd.
 * https://github.com/powervr-graphics/Native_SDK
 * https://github.com/powervr-graphics/Native_SDK/blob/4.0/Framework/PVRAssets/Texture/PVRTDecompress.cpp
 */

/* Original copyright notice for PVRTDecompress.cpp: */
/*!*********************************************************************************************************************
\file         PVRAssets\Texture\PVRTDecompress.cpp
\author       PowerVR by Imagination, Developer Technology Team
\copyright    Copyright (c) Imagination Technologies Limited.
\brief         Implementation of the Texture Decompression functions.
***********************************************************************************************************************/
/* PowerVR SDK license: */
/* -----------------------------------------------
 * POWERVR SDK SOFTWARE END USER LICENSE AGREEMENT
 * -----------------------------------------------
 * The MIT License (MIT)
 * Copyright (c) Imagination Technologies Ltd.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

internal static class PVRTC
{
    /* Shim function */
    public static byte[] Decompress(EndianBinaryReader reader, int width, int height, PixelDataFormat inputFormat,
        long readLength)
    {
        /* Call ported decompression functions */
        var pixelData = new byte[readLength * 8];
        PVRTDecompressPVRTC(reader.ReadBytes((int)readLength),
            (inputFormat & PixelDataFormat.MaskSpecial) == PixelDataFormat.SpecialFormatPVRT2_Vita ? 1 : 0, width,
            height, ref pixelData);
        return pixelData;
    }

    private static Pixel32 getColorA(uint u32ColorData)
    {
        Pixel32 color;

        if ((u32ColorData & 0x8000) != 0)
        {
            color.red = (byte)((u32ColorData & 0x7c00) >> 10);
            color.green = (byte)((u32ColorData & 0x3e0) >> 5);
            color.blue = (byte)((u32ColorData & 0x1e) | ((u32ColorData & 0x1e) >> 4));
            color.alpha = 0xf;
        }
        else
        {
            color.red = (byte)(((u32ColorData & 0xf00) >> 7) | ((u32ColorData & 0xf00) >> 11));
            color.green = (byte)(((u32ColorData & 0xf0) >> 3) | ((u32ColorData & 0xf0) >> 7));
            color.blue = (byte)(((u32ColorData & 0xe) << 1) | ((u32ColorData & 0xe) >> 2));
            color.alpha = (byte)((u32ColorData & 0x7000) >> 11);
        }

        return color;
    }

    private static Pixel32 getColorB(uint u32ColorData)
    {
        Pixel32 color;

        if ((u32ColorData & 0x80000000) != 0)
        {
            color.red = (byte)((u32ColorData & 0x7c000000) >> 26);
            color.green = (byte)((u32ColorData & 0x3e00000) >> 21);
            color.blue = (byte)((u32ColorData & 0x1f0000) >> 16);
            color.alpha = 0xf;
        }
        else
        {
            color.red = (byte)(((u32ColorData & 0xf000000) >> 23) | ((u32ColorData & 0xf000000) >> 27));
            color.green = (byte)(((u32ColorData & 0xf00000) >> 19) | ((u32ColorData & 0xf00000) >> 23));
            color.blue = (byte)(((u32ColorData & 0xf0000) >> 15) | ((u32ColorData & 0xf0000) >> 19));
            color.alpha = (byte)((u32ColorData & 0x70000000) >> 27);
        }

        return color;
    }

    private static void interpolateColors(Pixel32 P, Pixel32 Q, Pixel32 R, Pixel32 S, ref Pixel128S[] pPixel,
        byte ui8Bpp)
    {
        uint ui32WordWidth = 4;
        uint ui32WordHeight = 4;
        if (ui8Bpp == 2)
        {
            ui32WordWidth = 8;
        }

        var hP = new Pixel128S {red = P.red, green = P.green, blue = P.blue, alpha = P.alpha};
        var hQ = new Pixel128S {red = Q.red, green = Q.green, blue = Q.blue, alpha = Q.alpha};
        var hR = new Pixel128S {red = R.red, green = R.green, blue = R.blue, alpha = R.alpha};
        var hS = new Pixel128S {red = S.red, green = S.green, blue = S.blue, alpha = S.alpha};

        var QminusP = new Pixel128S
            {red = hQ.red - hP.red, green = hQ.green - hP.green, blue = hQ.blue - hP.blue, alpha = hQ.alpha - hP.alpha};
        var SminusR = new Pixel128S
            {red = hS.red - hR.red, green = hS.green - hR.green, blue = hS.blue - hR.blue, alpha = hS.alpha - hR.alpha};

        hP.red *= (int)ui32WordWidth;
        hP.green *= (int)ui32WordWidth;
        hP.blue *= (int)ui32WordWidth;
        hP.alpha *= (int)ui32WordWidth;
        hR.red *= (int)ui32WordWidth;
        hR.green *= (int)ui32WordWidth;
        hR.blue *= (int)ui32WordWidth;
        hR.alpha *= (int)ui32WordWidth;

        if (ui8Bpp == 2)
        {
            for (uint x = 0; x < ui32WordWidth; x++)
            {
                var result = new Pixel128S
                    {red = 4 * hP.red, green = 4 * hP.green, blue = 4 * hP.blue, alpha = 4 * hP.alpha};
                var dY = new Pixel128S
                {
                    red = hR.red - hP.red, green = hR.green - hP.green, blue = hR.blue - hP.blue,
                    alpha = hR.alpha - hP.alpha
                };

                for (uint y = 0; y < ui32WordHeight; y++)
                {
                    pPixel[y * ui32WordWidth + x].red = (result.red >> 7) + (result.red >> 2);
                    pPixel[y * ui32WordWidth + x].green = (result.green >> 7) + (result.green >> 2);
                    pPixel[y * ui32WordWidth + x].blue = (result.blue >> 7) + (result.blue >> 2);
                    pPixel[y * ui32WordWidth + x].alpha = (result.alpha >> 5) + (result.alpha >> 1);

                    result.red += dY.red;
                    result.green += dY.green;
                    result.blue += dY.blue;
                    result.alpha += dY.alpha;
                }

                hP.red += QminusP.red;
                hP.green += QminusP.green;
                hP.blue += QminusP.blue;
                hP.alpha += QminusP.alpha;

                hR.red += SminusR.red;
                hR.green += SminusR.green;
                hR.blue += SminusR.blue;
                hR.alpha += SminusR.alpha;
            }
        }
        else
        {
            for (uint y = 0; y < ui32WordHeight; y++)
            {
                var result = new Pixel128S
                    {red = 4 * hP.red, green = 4 * hP.green, blue = 4 * hP.blue, alpha = 4 * hP.alpha};
                var dY = new Pixel128S
                {
                    red = hR.red - hP.red, green = hR.green - hP.green, blue = hR.blue - hP.blue,
                    alpha = hR.alpha - hP.alpha
                };

                for (uint x = 0; x < ui32WordWidth; x++)
                {
                    pPixel[y * ui32WordWidth + x].red = (result.red >> 6) + (result.red >> 1);
                    pPixel[y * ui32WordWidth + x].green = (result.green >> 6) + (result.green >> 1);
                    pPixel[y * ui32WordWidth + x].blue = (result.blue >> 6) + (result.blue >> 1);
                    pPixel[y * ui32WordWidth + x].alpha = (result.alpha >> 4) + result.alpha;

                    result.red += dY.red;
                    result.green += dY.green;
                    result.blue += dY.blue;
                    result.alpha += dY.alpha;
                }

                hP.red += QminusP.red;
                hP.green += QminusP.green;
                hP.blue += QminusP.blue;
                hP.alpha += QminusP.alpha;

                hR.red += SminusR.red;
                hR.green += SminusR.green;
                hR.blue += SminusR.blue;
                hR.alpha += SminusR.alpha;
            }
        }
    }

    private static void unpackModulations(PVRTCWord word, int offsetX, int offsetY, int[][] i32ModulationValues,
        int[][] i32ModulationModes, byte ui8Bpp)
    {
        var WordModMode = word.u32ColorData & 0x1;
        var ModulationBits = word.u32ModulationData;

        if (ui8Bpp == 2)
        {
            if (WordModMode != 0)
            {
                if ((ModulationBits & 0x1) != 0)
                {
                    if ((ModulationBits & (0x1 << 20)) != 0)
                    {
                        WordModMode = 3;
                    }
                    else
                    {
                        WordModMode = 2;
                    }

                    if ((ModulationBits & (0x1 << 21)) != 0)
                    {
                        ModulationBits |= 0x1 << 20;
                    }
                    else
                    {
                        ModulationBits &= ~((uint)0x1 << 20);
                    }
                }

                if ((ModulationBits & 0x2) != 0)
                {
                    ModulationBits |= 0x1;
                }
                else
                {
                    ModulationBits &= ~(uint)0x1;
                }

                for (var y = 0; y < 4; y++)
                {
                    for (var x = 0; x < 8; x++)
                    {
                        i32ModulationModes[x + offsetX][y + offsetY] = (int)WordModMode;

                        if (((x ^ y) & 1) == 0)
                        {
                            i32ModulationValues[x + offsetX][y + offsetY] = (int)(ModulationBits & 3);
                            ModulationBits >>= 2;
                        }
                    }
                }
            }
            else
            {
                for (var y = 0; y < 4; y++)
                {
                    for (var x = 0; x < 8; x++)
                    {
                        i32ModulationModes[x + offsetX][y + offsetY] = (int)WordModMode;

                        if ((ModulationBits & 1) != 0)
                        {
                            i32ModulationValues[x + offsetX][y + offsetY] = 0x3;
                        }
                        else
                        {
                            i32ModulationValues[x + offsetX][y + offsetY] = 0x0;
                        }

                        ModulationBits >>= 1;
                    }
                }
            }
        }
        else
        {
            if (WordModMode != 0)
            {
                for (var y = 0; y < 4; y++)
                {
                    for (var x = 0; x < 4; x++)
                    {
                        i32ModulationValues[y + offsetY][x + offsetX] = (int)(ModulationBits & 3);
                        //if (i32ModulationValues==0) {}; don't need to check 0, 0 = 0/8.
                        if (i32ModulationValues[y + offsetY][x + offsetX] == 1)
                        {
                            i32ModulationValues[y + offsetY][x + offsetX] = 4;
                        }
                        else if (i32ModulationValues[y + offsetY][x + offsetX] == 2)
                        {
                            i32ModulationValues[y + offsetY][x + offsetX] = 14;
                        }
                        else if (i32ModulationValues[y + offsetY][x + offsetX] == 3)
                        {
                            i32ModulationValues[y + offsetY][x + offsetX] = 8;
                        }

                        ModulationBits >>= 2;
                    }
                }
            }
            else
            {
                for (var y = 0; y < 4; y++)
                {
                    for (var x = 0; x < 4; x++)
                    {
                        i32ModulationValues[y + offsetY][x + offsetX] = (int)(ModulationBits & 3);
                        i32ModulationValues[y + offsetY][x + offsetX] *= 3;
                        if (i32ModulationValues[y + offsetY][x + offsetX] > 3)
                        {
                            i32ModulationValues[y + offsetY][x + offsetX] -= 1;
                        }

                        ModulationBits >>= 2;
                    }
                }
            }
        }
    }

    private static int getModulationValues(int[][] i32ModulationValues, int[][] i32ModulationModes, uint xPos,
        uint yPos, byte ui8Bpp)
    {
        if (ui8Bpp == 2)
        {
            int[] RepVals0 = {0, 3, 5, 8};

            if (i32ModulationModes[xPos][yPos] == 0)
            {
                return RepVals0[i32ModulationValues[xPos][yPos]];
            }

            if (((xPos ^ yPos) & 1) == 0)
            {
                return RepVals0[i32ModulationValues[xPos][yPos]];
            }

            if (i32ModulationModes[xPos][yPos] == 1)
            {
                return (RepVals0[i32ModulationValues[xPos][yPos - 1]] +
                        RepVals0[i32ModulationValues[xPos][yPos + 1]] +
                        RepVals0[i32ModulationValues[xPos - 1][yPos]] +
                        RepVals0[i32ModulationValues[xPos + 1][yPos]] + 2) / 4;
            }

            if (i32ModulationModes[xPos][yPos] == 2)
            {
                return (RepVals0[i32ModulationValues[xPos - 1][yPos]] +
                        RepVals0[i32ModulationValues[xPos + 1][yPos]] + 1) / 2;
            }

            return (RepVals0[i32ModulationValues[xPos][yPos - 1]] +
                    RepVals0[i32ModulationValues[xPos][yPos + 1]] + 1) / 2;
        }

        if (ui8Bpp == 4)
        {
            return i32ModulationValues[xPos][yPos];
        }

        return 0;
    }

    private static void pvrtcGetDecompressedPixels(PVRTCWord P, PVRTCWord Q, PVRTCWord R, PVRTCWord S,
        ref Pixel32[] pColorData, byte ui8Bpp)
    {
        var i32ModulationValues = new int[16][];
        for (var i = 0; i < i32ModulationValues.Length; i++)
        {
            i32ModulationValues[i] = new int[8];
        }

        var i32ModulationModes = new int[16][];
        for (var i = 0; i < i32ModulationModes.Length; i++)
        {
            i32ModulationModes[i] = new int[8];
        }

        var upscaledColorA = new Pixel128S[32];
        var upscaledColorB = new Pixel128S[32];

        uint ui32WordWidth = 4;
        uint ui32WordHeight = 4;
        if (ui8Bpp == 2)
        {
            ui32WordWidth = 8;
        }

        unpackModulations(P, 0, 0, i32ModulationValues, i32ModulationModes, ui8Bpp);
        unpackModulations(Q, (int)ui32WordWidth, 0, i32ModulationValues, i32ModulationModes, ui8Bpp);
        unpackModulations(R, 0, (int)ui32WordHeight, i32ModulationValues, i32ModulationModes, ui8Bpp);
        unpackModulations(S, (int)ui32WordWidth, (int)ui32WordHeight, i32ModulationValues, i32ModulationModes, ui8Bpp);

        interpolateColors(getColorA(P.u32ColorData), getColorA(Q.u32ColorData), getColorA(R.u32ColorData),
            getColorA(S.u32ColorData), ref upscaledColorA, ui8Bpp);
        interpolateColors(getColorB(P.u32ColorData), getColorB(Q.u32ColorData), getColorB(R.u32ColorData),
            getColorB(S.u32ColorData), ref upscaledColorB, ui8Bpp);

        for (uint y = 0; y < ui32WordHeight; y++)
        {
            for (uint x = 0; x < ui32WordWidth; x++)
            {
                var mod = getModulationValues(i32ModulationValues, i32ModulationModes, x + ui32WordWidth / 2,
                    y + ui32WordHeight / 2, ui8Bpp);
                var punchthroughAlpha = false;
                if (mod > 10)
                {
                    punchthroughAlpha = true;
                    mod -= 10;
                }

                Pixel128S result;
                result.red = (upscaledColorA[y * ui32WordWidth + x].red * (8 - mod) +
                              upscaledColorB[y * ui32WordWidth + x].red * mod) / 8;
                result.green = (upscaledColorA[y * ui32WordWidth + x].green * (8 - mod) +
                                upscaledColorB[y * ui32WordWidth + x].green * mod) / 8;
                result.blue = (upscaledColorA[y * ui32WordWidth + x].blue * (8 - mod) +
                               upscaledColorB[y * ui32WordWidth + x].blue * mod) / 8;

                if (punchthroughAlpha)
                {
                    result.alpha = 0;
                }
                else
                {
                    result.alpha = (upscaledColorA[y * ui32WordWidth + x].alpha * (8 - mod) +
                                    upscaledColorB[y * ui32WordWidth + x].alpha * mod) / 8;
                }

                if (ui8Bpp == 2)
                {
                    pColorData[y * ui32WordWidth + x].red = (byte)result.red;
                    pColorData[y * ui32WordWidth + x].green = (byte)result.green;
                    pColorData[y * ui32WordWidth + x].blue = (byte)result.blue;
                    pColorData[y * ui32WordWidth + x].alpha = (byte)result.alpha;
                }
                else if (ui8Bpp == 4)
                {
                    pColorData[y + x * ui32WordHeight].red = (byte)result.red;
                    pColorData[y + x * ui32WordHeight].green = (byte)result.green;
                    pColorData[y + x * ui32WordHeight].blue = (byte)result.blue;
                    pColorData[y + x * ui32WordHeight].alpha = (byte)result.alpha;
                }
            }
        }
    }

    private static uint wrapWordIndex(uint numWords, int word)
    {
        return (uint)((word + numWords) % numWords);
    }

    private static bool isPowerOf2(uint input)
    {
        uint minus1;

        if (input == 0)
        {
            return false;
        }

        minus1 = input - 1;
        return (input | minus1) == (input ^ minus1);
    }

    private static uint TwiddleUV(uint XSize, uint YSize, uint XPos, uint YPos)
    {
        var MinDimension = XSize;
        var MaxValue = YPos;
        uint Twiddled = 0;
        uint SrcBitPos = 1;
        uint DstBitPos = 1;
        var ShiftCount = 0;

        Debug.Assert(YPos < YSize);
        Debug.Assert(XPos < XSize);
        Debug.Assert(isPowerOf2(YSize));
        Debug.Assert(isPowerOf2(XSize));

        if (YSize < XSize)
        {
            MinDimension = YSize;
            MaxValue = XPos;
        }

        while (SrcBitPos < MinDimension)
        {
            if ((YPos & SrcBitPos) != 0)
            {
                Twiddled |= DstBitPos;
            }

            if ((XPos & SrcBitPos) != 0)
            {
                Twiddled |= DstBitPos << 1;
            }

            SrcBitPos <<= 1;
            DstBitPos <<= 2;
            ShiftCount += 1;
        }

        MaxValue >>= ShiftCount;
        Twiddled |= MaxValue << (2 * ShiftCount);

        return Twiddled;
    }

    private static void mapDecompressedData(ref Pixel32[] pOutput, int width, Pixel32[] pWord, PVRTCWordIndices words,
        byte ui8Bpp)
    {
        uint ui32WordWidth = 4;
        uint ui32WordHeight = 4;
        if (ui8Bpp == 2)
        {
            ui32WordWidth = 8;
        }

        for (uint y = 0; y < ui32WordHeight / 2; y++)
        {
            for (uint x = 0; x < ui32WordWidth / 2; x++)
            {
                pOutput[(words.P[1] * ui32WordHeight + y + ui32WordHeight / 2)
                    * width + words.P[0] * ui32WordWidth + x + ui32WordWidth / 2] = pWord[y * ui32WordWidth + x];

                pOutput[(words.Q[1] * ui32WordHeight + y + ui32WordHeight / 2)
                    * width + words.Q[0] * ui32WordWidth + x] = pWord[y * ui32WordWidth + x + ui32WordWidth / 2];

                pOutput[(words.R[1] * ui32WordHeight + y)
                        * width + words.R[0] * ui32WordWidth + x + ui32WordWidth / 2] =
                    pWord[(y + ui32WordHeight / 2) * ui32WordWidth + x];

                pOutput[(words.S[1] * ui32WordHeight + y)
                        * width + words.S[0] * ui32WordWidth + x] =
                    pWord[(y + ui32WordHeight / 2) * ui32WordWidth + x + ui32WordWidth / 2];
            }
        }
    }

    private static int pvrtcDecompress(byte[] pCompressedData, ref Pixel32[] pDecompressedData, uint ui32Width,
        uint ui32Height, byte ui8Bpp)
    {
        uint ui32WordWidth = 4;
        uint ui32WordHeight = 4;
        if (ui8Bpp == 2)
        {
            ui32WordWidth = 8;
        }

        var pWordMembers = new uint[pCompressedData.Length / 4];
        for (var i = 0; i < pCompressedData.Length; i += 4)
        {
            pWordMembers[i / 4] = BitConverter.ToUInt32(pCompressedData, i);
        }

        var i32NumXWords = (int)(ui32Width / ui32WordWidth);
        var i32NumYWords = (int)(ui32Height / ui32WordHeight);

        PVRTCWordIndices indices;
        var pPixels = new Pixel32[ui32WordWidth * ui32WordHeight];

        for (var wordY = -1; wordY < i32NumYWords - 1; wordY++)
        {
            for (var wordX = -1; wordX < i32NumXWords - 1; wordX++)
            {
                indices = new PVRTCWordIndices(
                    (int)wrapWordIndex((uint)i32NumXWords, wordX),
                    (int)wrapWordIndex((uint)i32NumYWords, wordY),
                    (int)wrapWordIndex((uint)i32NumXWords, wordX + 1),
                    (int)wrapWordIndex((uint)i32NumYWords, wordY),
                    (int)wrapWordIndex((uint)i32NumXWords, wordX),
                    (int)wrapWordIndex((uint)i32NumYWords, wordY + 1),
                    (int)wrapWordIndex((uint)i32NumXWords, wordX + 1),
                    (int)wrapWordIndex((uint)i32NumYWords, wordY + 1));

                var WordOffsets = new uint[4]
                {
                    TwiddleUV((uint)i32NumXWords, (uint)i32NumYWords, (uint)indices.P[0], (uint)indices.P[1]) * 2,
                    TwiddleUV((uint)i32NumXWords, (uint)i32NumYWords, (uint)indices.Q[0], (uint)indices.Q[1]) * 2,
                    TwiddleUV((uint)i32NumXWords, (uint)i32NumYWords, (uint)indices.R[0], (uint)indices.R[1]) * 2,
                    TwiddleUV((uint)i32NumXWords, (uint)i32NumYWords, (uint)indices.S[0], (uint)indices.S[1]) * 2
                };

                PVRTCWord P, Q, R, S;
                P.u32ColorData = pWordMembers[WordOffsets[0] + 1];
                P.u32ModulationData = pWordMembers[WordOffsets[0]];
                Q.u32ColorData = pWordMembers[WordOffsets[1] + 1];
                Q.u32ModulationData = pWordMembers[WordOffsets[1]];
                R.u32ColorData = pWordMembers[WordOffsets[2] + 1];
                R.u32ModulationData = pWordMembers[WordOffsets[2]];
                S.u32ColorData = pWordMembers[WordOffsets[3] + 1];
                S.u32ModulationData = pWordMembers[WordOffsets[3]];

                pvrtcGetDecompressedPixels(P, Q, R, S, ref pPixels, ui8Bpp);
                mapDecompressedData(ref pDecompressedData, (int)ui32Width, pPixels, indices, ui8Bpp);
            }
        }

        return (int)(ui32Width * ui32Height / (ui32WordWidth / 2));
    }

    private static int PVRTDecompressPVRTC(byte[] pCompressedData, int Do2bitMode, int XDim, int YDim,
        ref byte[] pResultImage)
    {
        Pixel32[] pDecompressedData;

        var XTrueDim = Math.Max(XDim, Do2bitMode == 1 ? 16 : 8);
        var YTrueDim = Math.Max(YDim, 8);

        if (XTrueDim != XDim || YTrueDim != YDim)
        {
            pDecompressedData = new Pixel32[XTrueDim * YTrueDim];
        }
        else
        {
            pDecompressedData = new Pixel32[XDim * YDim];
        }

        var retval = pvrtcDecompress(pCompressedData, ref pDecompressedData, (uint)XTrueDim, (uint)YTrueDim,
            (byte)(Do2bitMode == 1 ? 2 : 4));

        for (var x = 0; x < XDim; ++x)
        {
            for (var y = 0; y < YDim; ++y)
            {
                pResultImage[(x + y * XDim) * 4 + 2] = pDecompressedData[x + y * XTrueDim].red;
                pResultImage[(x + y * XDim) * 4 + 1] = pDecompressedData[x + y * XTrueDim].green;
                pResultImage[(x + y * XDim) * 4 + 0] = pDecompressedData[x + y * XTrueDim].blue;
                pResultImage[(x + y * XDim) * 4 + 3] = pDecompressedData[x + y * XTrueDim].alpha;
            }
        }

        return retval;
    }

    /* All following from PVR SDK */

    private struct Pixel32
    {
        public byte red, green, blue, alpha;
    }

    private struct Pixel128S
    {
        public int red, green, blue, alpha;
    }

    private struct PVRTCWord
    {
        public uint u32ModulationData;
        public uint u32ColorData;
    }

    private struct PVRTCWordIndices
    {
        public readonly int[] P;
        public readonly int[] Q;
        public readonly int[] R;
        public readonly int[] S;

        public PVRTCWordIndices(int p0, int p1, int q0, int q1, int r0, int r1, int s0, int s1)
        {
            P = new int[2];
            P[0] = p0;
            P[1] = p1;
            Q = new int[2];
            Q[0] = q0;
            Q[1] = q1;
            R = new int[2];
            R[0] = r0;
            R[1] = r1;
            S = new int[2];
            S[0] = s0;
            S[1] = s1;
        }
    }
}