using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Flagrum.Core.Graphics.Textures.DirectX.PixelFormats;

public static class CompressedBlock
{
    public const int CompressedBlockWidth = 4;
    public const int CompressedBlockHeight = 4;

    /// <summary>
    /// Types of block-compressed pixel formats.
    /// </summary>
    public static HashSet<Type> CompressedBlockTypes =>
    [
        typeof(BC1Block),
        typeof(BC3Block),
        typeof(BC4Block),
        typeof(BC5Block),
        typeof(BC6HBlock),
        typeof(BC7Block)
    ];

    /// <summary>
    /// Calculates the total number of blocks in a single row of an image.
    /// </summary>
    /// <param name="width">Width of the image, in pixels.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetBlocksPerRow<TPixel>(int width) where TPixel : unmanaged
    {
        if (CompressedBlockTypes.Contains(typeof(TPixel)))
        {
            return (width + CompressedBlockWidth - 1) / CompressedBlockWidth;
        }

        return width;
    }

    /// <summary>
    /// Calculates the total number of blocks in a single column of an image.
    /// </summary>
    /// <param name="height">Height of the image, in pixels.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetBlocksPerColumn<TPixel>(int height) where TPixel : unmanaged
    {
        if (CompressedBlockTypes.Contains(typeof(TPixel)))
        {
            return (height + CompressedBlockHeight - 1) / CompressedBlockHeight;
        }

        return height;
    }

    /// <summary>
    /// Calculates the total number of bytes in a single row of an image.
    /// </summary>
    /// <param name="width">Width of the image, in pixels.</param>
    /// <typeparam name="TPixel">Type of the pixel or compressed block the image is encoded with.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetRowPitch<TPixel>(int width) where TPixel : unmanaged
    {
        if (CompressedBlockTypes.Contains(typeof(TPixel)))
        {
            return Math.Max(1, (width + 3) / CompressedBlockWidth) * Unsafe.SizeOf<TPixel>();
        }

        return width * Unsafe.SizeOf<TPixel>();
    }

    public static int GetSizeBytes<TPixel>(int width, int height) where TPixel : unmanaged
    {
        if (CompressedBlockTypes.Contains(typeof(TPixel)))
        {
            var blocksWide = (width + CompressedBlockWidth - 1) / CompressedBlockWidth;
            var blocksHigh = (height + CompressedBlockHeight - 1) / CompressedBlockHeight;
            return blocksWide * blocksHigh * Unsafe.SizeOf<TPixel>();
        }

        return width * height * Unsafe.SizeOf<TPixel>();
    }
}