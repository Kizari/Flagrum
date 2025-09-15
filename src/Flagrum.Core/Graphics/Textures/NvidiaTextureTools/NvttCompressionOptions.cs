using System;

namespace Flagrum.Core.Graphics.Textures.NvidiaTextureTools;

/// <summary>
/// Wrapper for NVIDIA Texture Tools' (nvtt) CompressionOptions class.
/// </summary>
public sealed class NvttCompressionOptions : IDisposable
{
    public readonly IntPtr Handle = Nvtt.CreateCompressionOptions();

    public void Dispose()
    {
        Nvtt.DestroyCompressionOptions(Handle);
    }

    public void SetFormat(NvttFormat format) => Nvtt.SetCompressionOptionsFormat(Handle, format);
}