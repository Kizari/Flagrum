using System;
using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures;

/// <summary>
/// P/Invoke implementation of <c>nvtt_wrapper.h</c> from NVIDIA Texture Tools.
/// </summary>
public static partial class Nvtt
{
    private const string Library = "libnvtt.so.30205";

    [LibraryImport(Library, EntryPoint = "nvttSurfaceWidth")]
    internal static partial int SurfaceWidth(IntPtr surface);
    
    [LibraryImport(Library, EntryPoint = "nvttSurfaceHeight")]
    internal static partial int SurfaceHeight(IntPtr surface);
    
    [LibraryImport(Library, EntryPoint = "nvttSurfaceData")]
    internal static partial IntPtr SurfaceData(IntPtr surface);
    
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
        [MarshalAs(UnmanagedType.Bool)]bool forcenormal);

    [LibraryImport(Library, EntryPoint = "nvttSurfaceSetSaveImage")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SurfaceSetSaveImage(
        IntPtr surfaceSet, 
        [MarshalAs(UnmanagedType.LPStr)] string fileName, 
        int faceId, 
        int mipId);
}