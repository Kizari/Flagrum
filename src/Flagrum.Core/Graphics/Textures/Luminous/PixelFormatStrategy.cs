using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flagrum.Core.Graphics.Textures.DirectX.PixelFormats;
using Flagrum.Core.Graphics.Textures.Shared;

namespace Flagrum.Core.Graphics.Textures.Luminous;

/// <inheritdoc />
public class PixelFormatStrategy<TPixel> : IPixelFormatStrategy
    where TPixel : unmanaged
{
    private const int SwizzledTileWidth = 8;
    private const int SwizzledTileHeight = 8;
    private const int SwizzledTileSize = SwizzledTileWidth * SwizzledTileHeight;

    /// <summary>
    /// Block order of swizzled textures.
    /// </summary>
    private static int[] TiledBlockOrder =>
    [
        0, 1, 8, 9, 2, 3, 10, 11,
        16, 17, 24, 25, 18, 19, 26, 27,
        4, 5, 12, 13, 6, 7, 14, 15,
        20, 21, 28, 29, 22, 23, 30, 31,
        32, 33, 40, 41, 34, 35, 42, 43,
        48, 49, 56, 57, 50, 51, 58, 59,
        36, 37, 44, 45, 38, 39, 46, 47,
        52, 53, 60, 61, 54, 55, 62, 63
    ];

    /// <inheritdoc />
    public bool IsCompressed { get; } = CompressedBlock.CompressedBlockTypes.Contains(typeof(TPixel));

    /// <inheritdoc />
    public IEnumerator<TextureSurface> GetEnumerator(TextureSurfaceEnumeratorConfiguration configuration) =>
        new TextureSurfaceEnumerator<TPixel>(configuration);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetRowPitch(int width) => CompressedBlock.GetRowPitch<TPixel>(width);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetSizeBytes(int width, int height) => CompressedBlock.GetSizeBytes<TPixel>(width, height);

    /// <inheritdoc />
    public void DeswizzleSurface(ReadOnlySpan<byte> sourceBuffer, Span<byte> destinationBuffer, int width, int height)
    {
        var source = MemoryMarshal.Cast<byte, TPixel>(sourceBuffer);
        var destination = MemoryMarshal.Cast<byte, TPixel>(destinationBuffer);
        var blockOrder = TiledBlockOrder;

        var blocksWide = CompressedBlock.GetBlocksPerRow<TPixel>(width);
        var blocksHigh = CompressedBlock.GetBlocksPerColumn<TPixel>(height);

        for (var i = 0; i < source.Length; i++)
        {
            // Determine tile origin
            var tileIndex = i / SwizzledTileSize;
            var tilesPerRow = (blocksWide + SwizzledTileWidth - 1) / SwizzledTileWidth;
            var tileX = tileIndex % tilesPerRow;
            var tileY = tileIndex / tilesPerRow;
            var globalX = tileX * SwizzledTileWidth;
            var globalY = tileY * SwizzledTileHeight;

            // Block position relative to the tile
            var localIndex = i % SwizzledTileSize;
            var localX = blockOrder[localIndex] % SwizzledTileWidth;
            var localY = blockOrder[localIndex] / SwizzledTileWidth;

            // Final position
            var finalX = globalX + localX;
            var finalY = globalY + localY;

            // Bounds check
            if (finalX < blocksWide && finalY < blocksHigh)
            {
                var finalIndex = finalY * blocksWide + finalX;
                destination[finalIndex] = source[i];
            }
        }
    }
}