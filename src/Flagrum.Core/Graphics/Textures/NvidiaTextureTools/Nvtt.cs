using System;
using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.NvidiaTextureTools;

/// <summary>
/// P/Invoke implementation of <c>nvtt_wrapper.h</c> from NVIDIA Texture Tools.
/// </summary>
public static partial class Nvtt
{
    private const string Library = "libnvtt";

    #region CompressionOptions

    [LibraryImport(Library, EntryPoint = "nvttCreateCompressionOptions")]
    internal static partial IntPtr CreateCompressionOptions();

    [LibraryImport(Library, EntryPoint = "nvttDestroyCompressionOptions")]
    internal static partial void DestroyCompressionOptions(IntPtr compressionOptions);

    [LibraryImport(Library, EntryPoint = "nvttSetCompressionOptionsFormat")]
    internal static partial void SetCompressionOptionsFormat(IntPtr compressionOptions, NvttFormat format);

    #endregion


    #region OutputOptions

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void BeginImageHandler(int size, int width, int height, int depth, int face, int miplevel);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool OutputHandler(IntPtr data, int size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void EndImageHandler();

    [LibraryImport(Library, EntryPoint = "nvttCreateOutputOptions")]
    internal static partial IntPtr CreateOutputOptions();

    [LibraryImport(Library, EntryPoint = "nvttDestroyOutputOptions")]
    internal static partial void DestroyOutputOptions(IntPtr compressionOptions);

    [LibraryImport(Library, EntryPoint = "nvttSetOutputOptionsOutputHandler")]
    internal static partial void SetOutputOptionsOutputHandler(IntPtr outputOptions,
        BeginImageHandler beginImageHandler,
        OutputHandler outputHandler,
        EndImageHandler endImageHandler);

    #endregion


    #region Context

    [LibraryImport(Library, EntryPoint = "nvttCreateContext")]
    internal static partial IntPtr CreateContext();

    [LibraryImport(Library, EntryPoint = "nvttDestroyContext")]
    internal static partial void DestroyContext(IntPtr context);

    [LibraryImport(Library, EntryPoint = "nvttContextOutputHeader")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ContextOutputHeader(
        IntPtr context,
        IntPtr surface,
        int mipmapCount,
        IntPtr compressionOptions,
        IntPtr outputOptions);

    [LibraryImport(Library, EntryPoint = "nvttContextCompress")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ContextCompress(
        IntPtr context,
        IntPtr surface,
        int face,
        int mipmap,
        IntPtr compressionOptions,
        IntPtr outputOptions);

    #endregion


    #region Surface

    [LibraryImport(Library, EntryPoint = "nvttCreateSurface")]
    internal static partial IntPtr CreateSurface();

    [LibraryImport(Library, EntryPoint = "nvttSurfaceWidth")]
    internal static partial int SurfaceWidth(IntPtr surface);

    [LibraryImport(Library, EntryPoint = "nvttSurfaceHeight")]
    internal static partial int SurfaceHeight(IntPtr surface);

    [LibraryImport(Library, EntryPoint = "nvttSurfaceData")]
    internal static partial IntPtr SurfaceData(IntPtr surface);

    [LibraryImport(Library, EntryPoint = "nvttSurfaceSetImageData")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SurfaceSetImageData(
        IntPtr surface,
        NvttInputFormat format,
        int w,
        int h,
        int d,
        IntPtr data,
        [MarshalAs(UnmanagedType.Bool)] bool unsignedToSigned,
        IntPtr timingContext);

    [LibraryImport(Library, EntryPoint = "nvttSurfaceSetImage2D")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SurfaceSetImage2D(
        IntPtr surface,
        NvttFormat format,
        int w,
        int h,
        IntPtr data,
        IntPtr timingContext);

    [LibraryImport(Library, EntryPoint = "nvttDestroySurface")]
    internal static partial void DestroySurface(IntPtr surface);

    #endregion


    #region SurfaceSet

    [LibraryImport(Library, EntryPoint = "nvttCreateSurfaceSet")]
    internal static partial IntPtr CreateSurfaceSet();

    [LibraryImport(Library, EntryPoint = "nvttDestroySurfaceSet")]
    internal static partial void DestroySurfaceSet(IntPtr surfaceSet);

    [LibraryImport(Library, EntryPoint = "nvttSurfaceSetGetSurface")]
    internal static partial IntPtr SurfaceSetGetSurface(
        IntPtr surfaceSet,
        int faceId,
        int mipId,
        [MarshalAs(UnmanagedType.Bool)] bool expectSigned);

    [LibraryImport(Library, EntryPoint = "nvttSurfaceSetLoadDDSFromMemory")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SurfaceSetLoadDDSFromMemory(
        IntPtr surfaceSet,
        IntPtr data,
        ulong sizeInBytes,
        [MarshalAs(UnmanagedType.Bool)] bool forcenormal);

    [LibraryImport(Library, EntryPoint = "nvttSurfaceSetSaveImage")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SurfaceSetSaveImage(
        IntPtr surfaceSet,
        [MarshalAs(UnmanagedType.LPStr)] string fileName,
        int faceId,
        int mipId);

    #endregion
}