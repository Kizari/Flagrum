using System;

namespace Flagrum.Core.Graphics.Textures.NvidiaTextureTools;

/// <summary>
/// Wrapper around NVIDIA Texture Tools' (nvtt) Context class.
/// </summary>
public sealed class NvttContext : IDisposable
{
    public readonly IntPtr Handle = Nvtt.CreateContext();

    public void Dispose()
    {
        Nvtt.DestroyContext(Handle);
    }

    public bool OutputHeader(
        NvttSurface surface,
        int mipmapCount,
        NvttCompressionOptions compressionOptions,
        NvttOutputOptions outputOptions) => Nvtt.ContextOutputHeader(
        Handle,
        surface.Handle,
        mipmapCount,
        compressionOptions.Handle,
        outputOptions.Handle);

    public bool Compress(
        NvttSurface surface,
        int face,
        int mipmap,
        NvttCompressionOptions compressionOptions,
        NvttOutputOptions outputOptions) => Nvtt.ContextCompress(
        Handle,
        surface.Handle,
        face,
        mipmap,
        compressionOptions.Handle,
        outputOptions.Handle);
}