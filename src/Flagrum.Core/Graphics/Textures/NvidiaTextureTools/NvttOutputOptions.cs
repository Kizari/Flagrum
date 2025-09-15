using System;

namespace Flagrum.Core.Graphics.Textures.NvidiaTextureTools;

/// <summary>
/// Wrapper around NVIDIA Texture Tools' (nvtt) OutputOptions class.
/// </summary>
public sealed class NvttOutputOptions : IDisposable
{
    public readonly IntPtr Handle = Nvtt.CreateOutputOptions();

    private Nvtt.BeginImageHandler? _beginImageHandler;
    private Nvtt.EndImageHandler? _endImageHandler;
    private Nvtt.OutputHandler? _outputHandler;

    public void Dispose()
    {
        Nvtt.DestroyOutputOptions(Handle);
    }

    public void SetOutputHandler(
        Nvtt.BeginImageHandler beginImageHandler,
        Nvtt.OutputHandler outputHandler,
        Nvtt.EndImageHandler endImageHandler)
    {
        // Store handlers so they aren't garbage collected before this class is finished with
        _beginImageHandler = beginImageHandler;
        _outputHandler = outputHandler;
        _endImageHandler = endImageHandler;

        // Pass delegates to native code
        Nvtt.SetOutputOptionsOutputHandler(Handle,
            _beginImageHandler,
            _outputHandler,
            _endImageHandler);
    }
}