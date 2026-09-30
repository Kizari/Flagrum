using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace Flagrum.Host.Shell;

/// <summary>
/// A simple brushed metal look in Flagrum's dark warm gray.
/// </summary>
public sealed class BrushedMetalBackground : Control
{
    private readonly Color _baseColor;
    private WriteableBitmap? _bitmap;
    private PixelSize _size;

    public BrushedMetalBackground()
    {
        // Cache the base background color
        var baseColor = Color.FromRgb(28, 25, 23).ToHsl();
        _baseColor = new HslColor(1.0, baseColor.H, baseColor.S * 1.3, baseColor.L * 1.3)
            .ToRgb();
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        // Compute size based on control bounds
        var size = new PixelSize(
            Math.Max(1, (int)Bounds.Width),
            Math.Max(1, (int)Bounds.Height));

        // Create the brushed texture if needed
        if (_bitmap is null || _size != size)
        {
            _bitmap?.Dispose();
            _bitmap = CreateTexture(size);
            _size = size;
        }

        // Draw the brushed texture
        context.DrawImage(
            _bitmap,
            new Rect(0, 0, _bitmap.PixelSize.Width, _bitmap.PixelSize.Height),
            Bounds);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        // Clean up the generated texture when the control is no longer needed
        _bitmap?.Dispose();
        _bitmap = null;
        _size = default;
    }

    /// <summary>
    /// Generates the brushed metal texture.
    /// </summary>
    /// <param name="size">Dimensions of the generated texture.</param>
    private WriteableBitmap CreateTexture(PixelSize size)
    {
        // Create a bitmap to hold the texture
        var bitmap = new WriteableBitmap(
            size,
            new Vector(96, 96),
            PixelFormat.Rgba8888,
            AlphaFormat.Opaque);

        using var buffer = bitmap.Lock();

        // Generate grayscale noise texture
        var random = new Random();
        var pixels = new byte[size.Width * size.Height];
        random.NextBytes(pixels);

        // Apply horizontal blur to the noise texture
        for (var pass = 0; pass < 3; pass++)
        {
            for (var y = 0; y < size.Height; y++)
            {
                for (var x = 0; x < size.Width; x++)
                {
                    const int radius = 20;

                    var sum = 0;
                    var count = 0;

                    for (var k = -radius; k <= radius; k++)
                    {
                        var ix = x + k;

                        if (ix >= 0 && ix < size.Width)
                        {
                            sum += pixels[y * size.Width + ix];
                            count++;
                        }
                    }

                    pixels[y * size.Width + x] = (byte)(sum / count);
                }
            }
        }

        // Apply the brushed texture to the base color
        unsafe
        {
            var destination = (byte*)buffer.Address;

            for (var y = 0; y < size.Height; y++)
            {
                for (var x = 0; x < size.Width; x++)
                {
                    // Compute RGB value for the current pixel
                    var factor = pixels[y * size.Width + x] / 255.0;
                    var multiplier = 1.0 / 3.0 + factor;
                    var red = Math.Clamp(_baseColor.R * multiplier, 0, 255);
                    var green = Math.Clamp(_baseColor.G * multiplier, 0, 255);
                    var blue = Math.Clamp(_baseColor.B * multiplier, 0, 255);

                    // Write the pixel to the bitmap memory
                    var offset = y * buffer.RowBytes + x * 4;
                    destination[offset + 0] = (byte)red;
                    destination[offset + 1] = (byte)green;
                    destination[offset + 2] = (byte)blue;
                    destination[offset + 3] = 255;
                }
            }
        }

        return bitmap;
    }
}