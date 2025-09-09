using System;
using System.Runtime.InteropServices;

namespace Flagrum.Platform.Linux.Interop;

internal static class Gtk
{
    private const string LibraryName = "libgtk-3.so.0";
    
    [DllImport(LibraryName, EntryPoint = "gtk_window_new")]
    internal static extern IntPtr GtkWindowNew(int type);
    
    [DllImport(LibraryName, EntryPoint = "gtk_widget_destroy")]
    internal static extern void GtkWidgetDestroy(IntPtr widget);
    
    [DllImport(LibraryName, EntryPoint = "gtk_container_add")]
    internal static extern void GtkContainerAdd(IntPtr container, IntPtr widget);
    
    [DllImport(LibraryName, EntryPoint = "gtk_widget_get_window")]
    internal static extern IntPtr GtkWidgetGetWindow(IntPtr gtkWidget);
    
    [DllImport(LibraryName, EntryPoint = "gtk_widget_realize")]
    internal static extern void GtkWidgetRealize(IntPtr widget);
    
    [DllImport(LibraryName, EntryPoint = "gtk_widget_get_display")]
    internal static extern IntPtr GtkWidgetGetDisplay(IntPtr widget);

    [DllImport(LibraryName, EntryPoint = "gtk_widget_show_all")]
    internal static extern void GtkWidgetShowAll(IntPtr widget);
}