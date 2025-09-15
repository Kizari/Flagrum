using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Flagrum.Application.Utilities;

/// <summary>
/// Extension methods for ImageSharp.
/// </summary>
public static class ImageSharpExtensions
{
    /// <summary>
    /// Creates a copy of this image that fits within the desired dimensions,
    /// padding with black pixels if aspect ratio is inconsistent.
    /// </summary>
    /// <param name="image">Image to resize.</param>
    /// <param name="desiredWidth">Width of the output image.</param>
    /// <param name="desiredHeight">Height of the output image.</param>
    /// <returns>Resized copy of the image, or the original image if already the desired size.</returns>
    /// <remarks>
    /// <b>WARNING:</b> Disposes the original image after resizing, unless the dimensions
    /// were already equal to the desired dimensions.
    /// </remarks>
    public static Image ResizeFit(this Image image, int desiredWidth, int desiredHeight)
    {
        if (image.Width == desiredWidth && image.Height == desiredHeight)
        {
            return image;
        }

        // Calculate the scaling ratio to fit within target dimensions
        var ratio = Math.Min((float)desiredWidth / image.Width, (float)desiredHeight / image.Height);
        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);

        // Resize the image while preserving aspect ratio
        image.Mutate(i => i.Resize(newWidth, newHeight));

        // Create a new canvas with the target dimensions and black background
        var canvas = new Image<Rgba32>(desiredWidth, desiredHeight, Color.Black);

        // Place the resized image centered in the canvas
        var x = (desiredWidth - newWidth) / 2;
        var y = (desiredHeight - newHeight) / 2; // ReSharper disable once AccessToDisposedClosure
        canvas.Mutate(c => c.DrawImage(image, new Point(x, y), 1f));
        image.Dispose();
        return canvas;
    }

    /// <summary>
    /// Creates a copy of this image that fills the desired dimensions,
    /// cropping overflow if aspect ratio is inconsistent.
    /// </summary>
    /// <param name="image">Image to resize.</param>
    /// <param name="desiredWidth">Width of the output image.</param>
    /// <param name="desiredHeight">Height of the output image.</param>
    /// <returns>Resized copy of the image, or the original image if already the desired size.</returns>
    /// <remarks>
    /// <b>WARNING:</b> Disposes the original image after resizing, unless the dimensions
    /// were already equal to the desired dimensions.
    /// </remarks>
    public static Image ResizeFill(this Image image, int desiredWidth, int desiredHeight)
    {
        if (image.Width == desiredWidth && image.Height == desiredHeight)
        {
            return image;
        }

        // Resize to cover the entire target area
        var ratio = Math.Max((float)desiredWidth / image.Width, (float)desiredHeight / image.Height);
        var scaledWidth = (int)(image.Width * ratio);
        var scaledHeight = (int)(image.Height * ratio);
        image.Mutate(i => i.Resize(scaledWidth, scaledHeight));

        // Calculate crop origin to center the image
        var cropX = (scaledWidth - desiredWidth) / 2;
        var cropY = (scaledHeight - desiredHeight) / 2;

        // Crop to target dimensions
        var cropped = image.CloneAs<Rgba32>();
        cropped.Mutate(c => c.Crop(new Rectangle(cropX, cropY, desiredWidth, desiredHeight)));
        image.Dispose();
        return cropped;
    }

    /// <summary>
    /// Encodes the image as JPEG, with an optional size limit.
    /// </summary>
    /// <param name="image">Image to encode as a JPEG.</param>
    /// <param name="sizeLimit">Maximum size the file can be, in bytes.</param>
    /// <returns>Buffer containing the JPEG file.</returns>
    /// <exception cref="ArgumentException">Thrown if size limit is unrealistically small.</exception>
    public static byte[] EncodeJpeg(this Image image, int sizeLimit)
    {
        // Sanity check
        if (sizeLimit < 1)
        {
            throw new ArgumentException("Size limit must be > 0");
        }

        // Continually reduce quality and write again until the output is within the configured size limit
        var quality = 100;
        while (quality > 10)
        {
            using var limitedStream = new MemoryStream();
            image.Save(limitedStream, new JpegEncoder {Quality = quality});

            if (limitedStream.Length <= sizeLimit)
            {
                return limitedStream.ToArray();
            }

            quality -= 5;
        }

        // Still too large at 10% quality, consumer to adjust their expectations
        throw new ArgumentException("Could not compress image to fit within the specified size limit.",
            nameof(sizeLimit));
    }
}