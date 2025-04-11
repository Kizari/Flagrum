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

internal static class BPTC
{
    /* All following ported/adapted from detex */

    private static readonly byte[] color_precision_table = {4, 6, 5, 7, 5, 7, 7, 5};
    private static readonly byte[] color_precision_plus_pbit_table = {5, 7, 5, 8, 5, 7, 8, 6};
    private static readonly byte[] alpha_precision_table = {0, 0, 0, 0, 6, 8, 7, 5};
    private static readonly byte[] alpha_precision_plus_pbit_table = {0, 0, 0, 0, 6, 8, 8, 6};
    private static readonly sbyte[] components_in_qword0_table = {2, -1, 1, 1, 3, 3, 3, 2};

    private static readonly byte[] mode_has_p_bits = {1, 1, 0, 1, 0, 0, 1, 1};

    private static readonly byte[] bptc_color_index_bitcount = {3, 3, 2, 2, 2, 2, 4, 2};
    private static readonly byte[] bptc_alpha_index_bitcount = {3, 3, 2, 2, 3, 2, 4, 2};
    private static readonly byte[] bptc_NS = {3, 2, 3, 2, 1, 1, 1, 2};
    private static readonly byte[] PB = {4, 6, 6, 6, 0, 0, 0, 6};
    private static readonly byte[] RB = {0, 0, 0, 0, 2, 2, 0, 0};

    private static readonly byte[] IB = {3, 3, 2, 2, 2, 2, 4, 2};
    private static readonly byte[] IB2 = {0, 0, 0, 0, 3, 2, 0, 0};
    private static readonly byte[] mode_has_partition_bits = {1, 1, 1, 1, 0, 0, 0, 1};

    internal static readonly byte[] detex_bptc_table_P2 =
    {
        0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1,
        0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1,
        0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1,
        0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 1,
        0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1,
        0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1,
        0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1,
        0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1,
        0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1,
        0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1,
        0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1,
        0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1,
        0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0,
        0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0,
        0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0,
        0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1,
        0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0,
        0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0,
        0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0,
        0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0,
        0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0,
        0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
        0, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0,
        0, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0,
        0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1,
        0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1,
        0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0,
        0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0,
        0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0,
        0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0,
        0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1,
        0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1,
        0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0,
        0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0,
        0, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0,
        0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 0, 0,
        0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
        0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1,
        0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1,
        0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0,
        0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0,
        0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0,
        0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0,
        0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1,
        0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1,
        0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0,
        0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 0,
        0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 1,
        0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1,
        0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1,
        0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1,
        0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1,
        0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
        0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0,
        0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1
    };

    internal static readonly byte[] detex_bptc_table_P3 =
    {
        0, 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 1, 2, 2, 2, 2,
        0, 0, 0, 1, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 2, 1,
        0, 0, 0, 0, 2, 0, 0, 1, 2, 2, 1, 1, 2, 2, 1, 1,
        0, 2, 2, 2, 0, 0, 2, 2, 0, 0, 1, 1, 0, 1, 1, 1,
        0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2,
        0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 2, 2, 0, 0, 2, 2,
        0, 0, 2, 2, 0, 0, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1,
        0, 0, 1, 1, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1,
        0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
        0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2,
        0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2,
        0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2,
        0, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2,
        0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 2,
        0, 0, 1, 1, 0, 1, 1, 2, 1, 1, 2, 2, 1, 2, 2, 2,
        0, 0, 1, 1, 2, 0, 0, 1, 2, 2, 0, 0, 2, 2, 2, 0,
        0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 2, 1, 1, 2, 2,
        0, 1, 1, 1, 0, 0, 1, 1, 2, 0, 0, 1, 2, 2, 0, 0,
        0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2,
        0, 0, 2, 2, 0, 0, 2, 2, 0, 0, 2, 2, 1, 1, 1, 1,
        0, 1, 1, 1, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2,
        0, 0, 0, 1, 0, 0, 0, 1, 2, 2, 2, 1, 2, 2, 2, 1,
        0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 2, 2, 0, 1, 2, 2,
        0, 0, 0, 0, 1, 1, 0, 0, 2, 2, 1, 0, 2, 2, 1, 0,
        0, 1, 2, 2, 0, 1, 2, 2, 0, 0, 1, 1, 0, 0, 0, 0,
        0, 0, 1, 2, 0, 0, 1, 2, 1, 1, 2, 2, 2, 2, 2, 2,
        0, 1, 1, 0, 1, 2, 2, 1, 1, 2, 2, 1, 0, 1, 1, 0,
        0, 0, 0, 0, 0, 1, 1, 0, 1, 2, 2, 1, 1, 2, 2, 1,
        0, 0, 2, 2, 1, 1, 0, 2, 1, 1, 0, 2, 0, 0, 2, 2,
        0, 1, 1, 0, 0, 1, 1, 0, 2, 0, 0, 2, 2, 2, 2, 2,
        0, 0, 1, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 0, 1, 1,
        0, 0, 0, 0, 2, 0, 0, 0, 2, 2, 1, 1, 2, 2, 2, 1,
        0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 2, 2, 2,
        0, 2, 2, 2, 0, 0, 2, 2, 0, 0, 1, 2, 0, 0, 1, 1,
        0, 0, 1, 1, 0, 0, 1, 2, 0, 0, 2, 2, 0, 2, 2, 2,
        0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0,
        0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0, 0, 0, 0,
        0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0,
        0, 1, 2, 0, 2, 0, 1, 2, 1, 2, 0, 1, 0, 1, 2, 0,
        0, 0, 1, 1, 2, 2, 0, 0, 1, 1, 2, 2, 0, 0, 1, 1,
        0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0, 0, 0, 0, 1, 1,
        0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2,
        0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 2, 1, 2, 1, 2, 1,
        0, 0, 2, 2, 1, 1, 2, 2, 0, 0, 2, 2, 1, 1, 2, 2,
        0, 0, 2, 2, 0, 0, 1, 1, 0, 0, 2, 2, 0, 0, 1, 1,
        0, 2, 2, 0, 1, 2, 2, 1, 0, 2, 2, 0, 1, 2, 2, 1,
        0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 0, 1, 0, 1,
        0, 0, 0, 0, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1,
        0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2,
        0, 2, 2, 2, 0, 1, 1, 1, 0, 2, 2, 2, 0, 1, 1, 1,
        0, 0, 0, 2, 1, 1, 1, 2, 0, 0, 0, 2, 1, 1, 1, 2,
        0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2,
        0, 2, 2, 2, 0, 1, 1, 1, 0, 1, 1, 1, 0, 2, 2, 2,
        0, 0, 0, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2,
        0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 2, 2,
        0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 1, 2,
        0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 2, 2, 2, 2, 2, 2,
        0, 0, 2, 2, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 2, 2,
        0, 0, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 0, 0, 2, 2,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2,
        0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 1,
        0, 2, 2, 2, 1, 2, 2, 2, 0, 2, 2, 2, 1, 2, 2, 2,
        0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        0, 1, 1, 1, 2, 0, 1, 1, 2, 2, 0, 1, 2, 2, 2, 0
    };

    internal static readonly byte[] detex_bptc_table_anchor_index_second_subset =
    {
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 2, 8, 2, 2, 8, 8, 15,
        2, 8, 2, 2, 8, 8, 2, 2,
        15, 15, 6, 8, 2, 8, 15, 15,
        2, 8, 2, 2, 2, 15, 15, 6,
        6, 2, 6, 8, 15, 15, 2, 2,
        15, 15, 15, 15, 15, 2, 2, 15
    };

    private static readonly byte[] detex_bptc_table_anchor_index_second_subset_of_three =
    {
        3, 3, 15, 15, 8, 3, 15, 15,
        8, 8, 6, 6, 6, 5, 3, 3,
        3, 3, 8, 15, 3, 3, 6, 10,
        5, 8, 8, 6, 8, 5, 15, 15,
        8, 15, 3, 5, 6, 10, 8, 15,
        15, 3, 15, 5, 15, 15, 15, 15,
        3, 15, 5, 5, 5, 8, 5, 10,
        5, 10, 8, 13, 15, 12, 3, 3
    };

    private static readonly byte[] detex_bptc_table_anchor_index_third_subset =
    {
        15, 8, 8, 3, 15, 15, 3, 8,
        15, 15, 15, 15, 15, 15, 15, 8,
        15, 8, 15, 3, 15, 8, 15, 8,
        3, 15, 6, 10, 15, 15, 10, 8,
        15, 3, 15, 10, 10, 8, 9, 10,
        6, 15, 8, 15, 3, 6, 6, 8,
        15, 3, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 3, 15, 15, 8
    };

    internal static readonly ushort[][] detex_bptc_table_aWeights =
    {
        new ushort[] {0, 21, 43, 64},                                              // 2
        new ushort[] {0, 9, 18, 27, 37, 46, 55, 64},                               // 3
        new ushort[] {0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64} // 4
    };

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
                var decompressedBlock = new byte[4 * 4 * 4];
                detexDecompressBlockBPTC(reader, ref decompressedBlock);

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
                            outPixels[(iy * width + ix) * 4 + c] = decompressedBlock[(py * 4 + px) * 4 + c];
                        }
                    }
                }
            }
        }

        return outPixels;
    }

    private static void ExtractEndpoints(int mode, int nu_subsets, detexBlock128 block, byte[] endpoint_array)
    {
        // Optimized version avoiding the use of block_extract_bits().

        int components_in_qword0 = components_in_qword0_table[mode];
        var data = block.data0 >> block.index;
        var precision = color_precision_table[mode];
        var mask = (byte)((1 << precision) - 1);
        var total_bits_per_component = nu_subsets * 2 * precision;

        for (var i = 0; i < components_in_qword0; i++)
        {
            // For each color component.
            for (var j = 0; j < nu_subsets; j++)
            {
                // For each subset.
                for (var k = 0; k < 2; k++)
                {
                    // For each endpoint.
                    endpoint_array[j * 8 + k * 4 + i] = (byte)(data & mask);
                    data >>= precision;
                }
            }
        }

        block.index += components_in_qword0 * total_bits_per_component;

        if (components_in_qword0 < 3)
        {
            // Handle the color component that crosses the boundary between data0 and data1
            data = block.data0 >> block.index;
            data |= block.data1 << (64 - block.index);

            var i = components_in_qword0;

            for (var j = 0; j < nu_subsets; j++)
            {
                // For each subset.
                for (var k = 0; k < 2; k++)
                {
                    // For each endpoint.
                    endpoint_array[j * 8 + k * 4 + i] = (byte)(data & mask);
                    data >>= precision;
                }
            }

            block.index += total_bits_per_component;
        }

        if (components_in_qword0 < 2)
        {
            // Handle the color component that is wholly in data1.
            data = block.data1 >> (block.index - 64);

            var i = 2;

            for (var j = 0; j < nu_subsets; j++)
            {
                // For each subset.
                for (var k = 0; k < 2; k++)
                {
                    // For each endpoint.
                    endpoint_array[j * 8 + k * 4 + i] = (byte)(data & mask);
                    data >>= precision;
                }
            }

            block.index += total_bits_per_component;
        }

        // Alpha component.
        if (alpha_precision_table[mode] > 0)
        {
            // For mode 7, the alpha data is wholly in data1.
            // For modes 4 and 6, the alpha data is wholly in data0.
            // For mode 5, the alpha data is in data0 and data1.

            if (mode == 7)
            {
                data = block.data1 >> (block.index - 64);
            }
            else if (mode == 5)
            {
                data = (block.data0 >> block.index) | ((block.data1 & 0x3) << 14);
            }
            else
            {
                data = block.data0 >> block.index;
            }

            var alpha_precision = alpha_precision_table[mode];
            mask = (byte)((1 << alpha_precision) - 1);

            for (var j = 0; j < nu_subsets; j++)
            {
                for (var k = 0; k < 2; k++)
                {
                    // For each endpoint.
                    endpoint_array[j * 8 + k * 4 + 3] = (byte)(data & mask);
                    data >>= alpha_precision;
                }
            }

            block.index += nu_subsets * 2 * alpha_precision;
        }
    }

    private static void FullyDecodeEndpoints(byte[] endpoint_array, int nu_subsets, int mode, detexBlock128 block)
    {
        if (mode_has_p_bits[mode] != 0)
        {
            // Mode 1 (shared P-bits) handled elsewhere.
            // Extract end-point P-bits. Take advantage of the fact that they don't cross the
            // 64-bit word boundary in any mode.

            uint bits;
            if (block.index < 64)
            {
                bits = (uint)(block.data0 >> block.index);
            }
            else
            {
                bits = (uint)(block.data1 >> (block.index - 64));
            }

            for (var i = 0; i < nu_subsets * 2; i++)
            {
                endpoint_array[i * 4 + 0] <<= 1;
                endpoint_array[i * 4 + 1] <<= 1;
                endpoint_array[i * 4 + 2] <<= 1;
                endpoint_array[i * 4 + 3] <<= 1;
                endpoint_array[i * 4 + 0] |= (byte)(bits & 1);
                endpoint_array[i * 4 + 1] |= (byte)(bits & 1);
                endpoint_array[i * 4 + 2] |= (byte)(bits & 1);
                endpoint_array[i * 4 + 3] |= (byte)(bits & 1);
                bits >>= 1;
            }

            block.index += nu_subsets * 2;
        }

        int color_prec = color_precision_plus_pbit_table[mode];
        int alpha_prec = alpha_precision_plus_pbit_table[mode];

        for (var i = 0; i < nu_subsets * 2; i++)
        {
            // Color_component_precision & alpha_component_precision includes pbit
            // left shift endpoint components so that their MSB lies in bit 7
            endpoint_array[i * 4 + 0] <<= 8 - color_prec;
            endpoint_array[i * 4 + 1] <<= 8 - color_prec;
            endpoint_array[i * 4 + 2] <<= 8 - color_prec;
            endpoint_array[i * 4 + 3] <<= 8 - alpha_prec;

            // Replicate each component's MSB into the LSBs revealed by the left-shift operation above.
            endpoint_array[i * 4 + 0] |= (byte)(endpoint_array[i * 4 + 0] >> color_prec);
            endpoint_array[i * 4 + 1] |= (byte)(endpoint_array[i * 4 + 1] >> color_prec);
            endpoint_array[i * 4 + 2] |= (byte)(endpoint_array[i * 4 + 2] >> color_prec);
            endpoint_array[i * 4 + 3] |= (byte)(endpoint_array[i * 4 + 3] >> alpha_prec);
        }

        if (mode <= 3)
        {
            for (var i = 0; i < nu_subsets * 2; i++)
            {
                endpoint_array[i * 4 + 3] = 0xFF;
            }
        }
    }

    private static byte Interpolate(byte e0, byte e1, byte index, byte indexprecision)
    {
        return (byte)(((64 - detex_bptc_table_aWeights[indexprecision - 2][index]) * e0 +
                       detex_bptc_table_aWeights[indexprecision - 2][index] * e1 + 32) >> 6);
    }

    // Functions to extract parameters. */

    private static int ExtractMode(detexBlock128 block)
    {
        for (var i = 0; i < 8; i++)
        {
            if ((block.data0 & ((ulong)1 << i)) != 0)
            {
                block.index = i + 1;
                return i;
            }
        }

        // Illegal.
        return -1;
    }

    private static int ExtractPartitionSetID(detexBlock128 block, int mode)
    {
        return (int)detexBlock128ExtractBits(block, PB[mode]);
    }

    private static byte GetPartitionIndex(int nu_subsets, int partition_set_id, int i)
    {
        if (nu_subsets == 1)
        {
            return 0;
        }

        if (nu_subsets == 2)
        {
            return detex_bptc_table_P2[partition_set_id * 16 + i];
        }

        return detex_bptc_table_P3[partition_set_id * 16 + i];
    }

    private static int ExtractRotationBits(detexBlock128 block, int mode)
    {
        return (int)detexBlock128ExtractBits(block, RB[mode]);
    }

    private static byte GetAnchorIndex(int partition_set_id, int partition, int nu_subsets)
    {
        if (partition == 0)
        {
            return 0;
        }

        if (nu_subsets == 2)
        {
            return detex_bptc_table_anchor_index_second_subset[partition_set_id];
        }

        if (partition == 1)
        {
            return detex_bptc_table_anchor_index_second_subset_of_three[partition_set_id];
        }

        return detex_bptc_table_anchor_index_third_subset[partition_set_id];
    }

    /// <summary>
    /// Decompress a 128-bit 4x4 pixel texture block compressed using BPTC mode 1
    /// </summary>
    /// <param name="block"></param>
    /// <param name="pixel_buffer"></param>
    private static void DecompressBlockBPTCMode1(detexBlock128 block, ref byte[] pixel_buffer)
    {
        var data0 = block.data0;
        var data1 = block.data1;

        var partition_set_id = (int)detexGetBits64(data0, 2, 7);

        var endpoint = new byte[2 * 2 * 3];                 // 2 subsets.
        endpoint[0] = (byte)detexGetBits64(data0, 8, 13);   // red, subset 0, endpoint 0
        endpoint[3] = (byte)detexGetBits64(data0, 14, 19);  // red, subset 0, endpoint 1
        endpoint[6] = (byte)detexGetBits64(data0, 20, 25);  // red, subset 1, endpoint 0
        endpoint[9] = (byte)detexGetBits64(data0, 26, 31);  // red, subset 1, endpoint 1
        endpoint[1] = (byte)detexGetBits64(data0, 32, 37);  // green, subset 0, endpoint 0
        endpoint[4] = (byte)detexGetBits64(data0, 38, 43);  // green, subset 0, endpoint 1
        endpoint[7] = (byte)detexGetBits64(data0, 44, 49);  // green, subset 1, endpoint 0
        endpoint[10] = (byte)detexGetBits64(data0, 50, 55); // green, subset 1, endpoint 1
        endpoint[2] = (byte)detexGetBits64(data0, 56, 61);  // blue, subset 0, endpoint 0
        endpoint[5] =
            (byte)((byte)detexGetBits64(data0, 62, 63) |
                   (byte)(detexGetBits64(data1, 0, 3) << 2)); // blue, subset 0, endpoint 1
        endpoint[8] = (byte)detexGetBits64(data1, 4, 9);      // blue, subset 1, endpoint 0
        endpoint[11] = (byte)detexGetBits64(data1, 10, 15);   // blue, subset 1, endpoint 1

        // Decode endpoints.
        for (var i = 0; i < 2 * 2; i++)
        {
            //component-wise left-shift
            endpoint[i * 3 + 0] <<= 2;
            endpoint[i * 3 + 1] <<= 2;
            endpoint[i * 3 + 2] <<= 2;
        }

        // P-bit is shared.
        var pbit_zero = (byte)(detexGetBits64(data1, 16, 16) << 1);
        var pbit_one = (byte)(detexGetBits64(data1, 17, 17) << 1);

        // RGB only pbits for mode 1, one for each subset.
        for (var j = 0; j < 3; j++)
        {
            endpoint[0 * 3 + j] |= pbit_zero;
            endpoint[1 * 3 + j] |= pbit_zero;
            endpoint[2 * 3 + j] |= pbit_one;
            endpoint[3 * 3 + j] |= pbit_one;
        }

        for (var i = 0; i < 2 * 2; i++)
        {
            // Replicate each component's MSB into the LSB.
            endpoint[i * 3 + 0] |= (byte)(endpoint[i * 3 + 0] >> 7);
            endpoint[i * 3 + 1] |= (byte)(endpoint[i * 3 + 1] >> 7);
            endpoint[i * 3 + 2] |= (byte)(endpoint[i * 3 + 2] >> 7);
        }

        // subset_index[i] is a number from 0 to 1.
        var subset_index = new byte[16];
        for (var i = 0; i < 16; i++)
        {
            subset_index[i] = detex_bptc_table_P2[partition_set_id * 16 + i];
        }

        var anchor_index = new byte[2];
        anchor_index[0] = 0;
        anchor_index[1] = detex_bptc_table_anchor_index_second_subset[partition_set_id];

        var color_index = new byte[16];

        // Extract primary index bits.
        data1 >>= 18;
        for (var i = 0; i < 16; i++)
        {
            if (i == anchor_index[subset_index[i]])
            {
                // Highest bit is zero.
                color_index[i] = (byte)(data1 & 3); // Get two bits.
                data1 >>= 2;
            }
            else
            {
                color_index[i] = (byte)(data1 & 7); // Get three bits.
                data1 >>= 3;
            }
        }

        for (var i = 0; i < 16; i++)
        {
            var endpoint_start = new byte[3];
            var endpoint_end = new byte[3];

            for (var j = 0; j < 3; j++)
            {
                endpoint_start[j] = endpoint[2 * subset_index[i] * 3 + j];
                endpoint_end[j] = endpoint[(2 * subset_index[i] + 1) * 3 + j];
            }

            pixel_buffer[i * 4 + 0] = Interpolate(endpoint_start[2], endpoint_end[2], color_index[i], 3);
            pixel_buffer[i * 4 + 1] = Interpolate(endpoint_start[1], endpoint_end[1], color_index[i], 3);
            pixel_buffer[i * 4 + 2] = Interpolate(endpoint_start[0], endpoint_end[0], color_index[i], 3);
            pixel_buffer[i * 4 + 3] = 0xFF;
        }
    }

    /// <summary>
    /// Decompress a 128-bit 4x4 pixel texture block compressed using the BPTC (BC7) format
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="pixel_buffer"></param>
    private static void detexDecompressBlockBPTC(EndianBinaryReader reader, ref byte[] pixel_buffer)
    {
        var block = new detexBlock128();
        block.data0 = reader.ReadUInt64();
        block.data1 = reader.ReadUInt64();
        block.index = 0;

        var mode = ExtractMode(block);
        if (mode != -1)
        {
            if (mode == 1)
            {
                DecompressBlockBPTCMode1(block, ref pixel_buffer);
                return;
            }

            var nu_subsets = 1;
            var partition_set_id = 0;

            if (mode_has_partition_bits[mode] != 0)
            {
                nu_subsets = bptc_NS[mode];
                partition_set_id = ExtractPartitionSetID(block, mode);
            }

            var rotation = ExtractRotationBits(block, mode);

            var index_selection_bit = 0;
            if (mode == 4)
            {
                index_selection_bit = (int)detexBlock128ExtractBits(block, 1);
            }

            var alpha_index_bitcount = (byte)(bptc_alpha_index_bitcount[mode] - index_selection_bit);
            var color_index_bitcount = (byte)(bptc_color_index_bitcount[mode] + index_selection_bit);

            var endpoint_array = new byte[3 * 2 * 4]; // Max. 3 subsets.

            ExtractEndpoints(mode, nu_subsets, block, endpoint_array);

            FullyDecodeEndpoints(endpoint_array, nu_subsets, mode, block);

            // subset_index[i] is a number from 0 to 2, or 0 to 1, or 0 depending on the number of subsets.
            var subset_index = new byte[16];
            for (var i = 0; i < 16; i++)
            {
                subset_index[i] = GetPartitionIndex(nu_subsets, partition_set_id, i);
            }

            var anchor_index = new byte[4]; // Only need max. 3 elements.
            for (var i = 0; i < nu_subsets; i++)
            {
                anchor_index[i] = GetAnchorIndex(partition_set_id, i, nu_subsets);
            }

            var color_index = new byte[16];
            var alpha_index = new byte[16];

            // Extract primary index bits.
            ulong data1;
            if (block.index >= 64)
            {
                // Because the index bits are all in the second 64-bit word, there is no need to use
                // block_extract_bits().
                // This implies the mode is not 4.
                data1 = block.data1 >> (block.index - 64);
                var mask1 = (byte)((1 << IB[mode]) - 1);
                var mask2 = (byte)((1 << (IB[mode] - 1)) - 1);

                for (var i = 0; i < 16; i++)
                {
                    if (i == anchor_index[subset_index[i]])
                    {
                        // Highest bit is zero.
                        color_index[i] = (byte)(data1 & mask2);
                        data1 >>= IB[mode] - 1;
                        alpha_index[i] = color_index[i];
                    }
                    else
                    {
                        color_index[i] = (byte)(data1 & mask1);
                        data1 >>= IB[mode];
                        alpha_index[i] = color_index[i];
                    }
                }
            }
            else
            {
                // Implies mode 4.
                // Because the bits cross the 64-bit word boundary, we have to be careful.
                // Block index is 50 at this point.
                var data = block.data0 >> 50;
                data |= block.data1 << 14;

                for (var i = 0; i < 16; i++)
                {
                    if (i == anchor_index[subset_index[i]])
                    {
                        // Highest bit is zero.
                        if (index_selection_bit != 0)
                        {
                            // Implies mode == 4.
                            alpha_index[i] = (byte)(data & 0x1);
                            data >>= 1;
                        }
                        else
                        {
                            color_index[i] = (byte)(data & 0x1);
                            data >>= 1;
                        }
                    }
                    else
                    {
                        if (index_selection_bit != 0)
                        {
                            // Implies mode == 4.
                            alpha_index[i] = (byte)(data & 0x3);
                            data >>= 2;
                        }
                        else
                        {
                            color_index[i] = (byte)(data & 0x3);
                            data >>= 2;
                        }
                    }
                }

                // Block index is 81 at this point.
                data1 = block.data1 >> (81 - 64);
            }

            // Extract secondary index bits.
            if (IB2[mode] > 0)
            {
                var mask1 = (byte)((1 << IB2[mode]) - 1);
                var mask2 = (byte)((1 << (IB2[mode] - 1)) - 1);

                for (var i = 0; i < 16; i++)
                {
                    if (i == anchor_index[subset_index[i]])
                    {
                        // Highest bit is zero.
                        if (index_selection_bit != 0)
                        {
                            color_index[i] = (byte)(data1 & 0x3);
                            data1 >>= 2;
                        }
                        else
                        {
                            alpha_index[i] = (byte)(data1 & mask2);
                            data1 >>= IB2[mode] - 1;
                        }
                    }
                    else
                    {
                        if (index_selection_bit != 0)
                        {
                            color_index[i] = (byte)(data1 & 0x7);
                            data1 >>= 3;
                        }
                        else
                        {
                            alpha_index[i] = (byte)(data1 & mask1);
                            data1 >>= IB2[mode];
                        }
                    }
                }
            }

            for (var i = 0; i < 16; i++)
            {
                var endpoint_start = new byte[4];
                var endpoint_end = new byte[4];

                for (var j = 0; j < 4; j++)
                {
                    endpoint_start[j] = endpoint_array[2 * subset_index[i] * 4 + j];
                    endpoint_end[j] = endpoint_array[(2 * subset_index[i] + 1) * 4 + j];
                }

                var r = Interpolate(endpoint_start[0], endpoint_end[0], color_index[i], color_index_bitcount);
                var g = Interpolate(endpoint_start[1], endpoint_end[1], color_index[i], color_index_bitcount);
                var b = Interpolate(endpoint_start[2], endpoint_end[2], color_index[i], color_index_bitcount);
                var a = Interpolate(endpoint_start[3], endpoint_end[3], alpha_index[i], alpha_index_bitcount);

                if (rotation > 0)
                {
                    byte temp = 0;
                    if (rotation == 1)
                    {
                        temp = r;
                        r = a;
                        a = temp;
                    }
                    else if (rotation == 2)
                    {
                        temp = g;
                        g = a;
                        a = temp;
                    }
                    else // rotation == 3
                    {
                        temp = b;
                        b = a;
                        a = temp;
                    }
                }

                pixel_buffer[i * 4 + 0] = b;
                pixel_buffer[i * 4 + 1] = g;
                pixel_buffer[i * 4 + 2] = r;
                pixel_buffer[i * 4 + 3] = a;
            }
        }
    }

    internal static uint detexGetBits64(ulong data, int bit0, int bit1)
    {
        var mask = ((ulong)(1 << (bit1 + 1 - bit0)) - 1) << bit0;
        return (uint)((data & mask) >> bit0);
    }

    internal static uint detexGetBits64Reversed(ulong data, int bit0, int bit1)
    {
        // Assumes bit0 > bit1.
        // Reverse the bits.
        uint val = 0;
        for (var i = 0; i <= bit0 - bit1; i++)
        {
            var shift_right = bit0 - 2 * i;
            if (shift_right >= 0)
            {
                val |= (uint)((data & ((ulong)1 << (bit0 - i))) >> shift_right);
            }
            else
            {
                val |= (uint)((data & ((ulong)1 << (bit0 - i))) << -shift_right);
            }
        }

        return val;
    }

    internal static uint detexBlock128ExtractBits(detexBlock128 block, int nu_bits)
    {
        uint value = 0;
        for (var i = 0; i < nu_bits; i++)
        {
            if (block.index < 64)
            {
                var shift = block.index - i;
                if (shift < 0)
                {
                    value |= (uint)((block.data0 & ((ulong)1 << block.index)) << -shift);
                }
                else
                {
                    value |= (uint)((block.data0 & ((ulong)1 << block.index)) >> shift);
                }
            }
            else
            {
                var shift = block.index - 64 - i;
                if (shift < 0)
                {
                    value |= (uint)((block.data1 & ((ulong)1 << (block.index - 64))) << -shift);
                }
                else
                {
                    value |= (uint)((block.data1 & ((ulong)1 << (block.index - 64))) >> shift);
                }
            }

            block.index++;
        }

        return value;
    }
}

internal class detexBlock128
{
    public ulong data0 { get; set; }
    public ulong data1 { get; set; }
    public int index { get; set; }
}