using System;
using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.OpenExtendedRange;

/// <summary>
/// P/Invoke wrapper for OpenEXR Core.
/// </summary>
public static partial class OpenExrCore
{
    private const string Library = "OpenEXRCore";

    public const int EXR_ERR_SUCCESS = 0;
    
    public enum PixelType
    {
        UInt32 = 0,
        Float16 = 1,
        Float32 = 2
    }

    public static readonly ContextInitializer DefaultInitializer = new()
    {
        error_handler_fn = IntPtr.Zero,
        memory_alloc_fn = IntPtr.Zero,
        memory_free_fn = IntPtr.Zero,
        user_data = IntPtr.Zero,
        max_image_width = 16384,
        max_image_height = 16384,
        max_tile_width = 256,
        max_tile_height = 256
    };

    [LibraryImport(Library, EntryPoint = "exr_start_read")]
    internal static partial int StartRead(
        out IntPtr context,
        [MarshalAs(UnmanagedType.LPStr)] string filename,
        ref ContextInitializer init);

    [LibraryImport(Library, EntryPoint = "exr_get_data_window")]
    internal static partial int GetDataWindow(IntPtr context, int partIndex, out Box2I dataWindow);

    [LibraryImport(Library, EntryPoint = "exr_read_scanline_channel")]
    internal static partial int ReadScanlineChannel(
        IntPtr context,
        int partIndex,
        [MarshalAs(UnmanagedType.LPStr)] string channelName,
        PixelType pixelType,
        int startY,
        int endY,
        IntPtr buffer,
        int pixelStrideBytes
    );

    [LibraryImport(Library, EntryPoint = "exr_finish")]
    internal static partial int Finish(ref IntPtr context);

    [LibraryImport(Library, EntryPoint = "exr_get_channel_list")]
    internal static partial int GetChannelList(IntPtr context, int partIndex, out AttributeChannelList chlist);

    [StructLayout(LayoutKind.Sequential)]
    public struct Box2I
    {
        public Point2I min;
        public Point2I max;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point2I
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ContextInitializer
    {
        public IntPtr error_handler_fn;
        public IntPtr memory_alloc_fn;
        public IntPtr memory_free_fn;
        public IntPtr user_data;
        public int max_image_width;
        public int max_image_height;
        public int max_tile_width;
        public int max_tile_height;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AttributeChannelListEntry
    {
        public IntPtr name; // UTF-8 string
        public PixelType pixel_type;
        public byte p_linear;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] reserved;

        public int x_sampling;
        public int y_sampling;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AttributeChannelList
    {
        public int num_channels;
        public int num_alloced;
        public IntPtr entries; // pointer to array of attr_chlist_entry_t
    }
}