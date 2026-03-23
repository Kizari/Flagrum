using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flagrum.Abstractions;
using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Host;

/// <summary>
/// Callback that is invoked when <see cref="BlazorWebView" /> sends a message back to Flagrum.
/// </summary>
public delegate void WebMessageReceivedCallback(string message);

/// <summary>
/// Native components that host the .NET/Blazor parts of the program.
/// </summary>
[RegisterSingleton<ApplicationHost>]
[RegisterSingleton<IApplication>(Factory = nameof(Factory))]
public sealed partial class ApplicationHost : IApplication
{
    private readonly IntPtr _instance = ApplicationHost_Create();

    private WebMessageReceivedCallback? _onWebMessageReceived;

    /// <inheritdoc />
    public Version Version => typeof(Program).Assembly.GetName().Version!;

    /// <inheritdoc />
    public string? AssociatedFile { get; set; }

    /// <inheritdoc />
    public void Dispose()
    {
        ApplicationHost_Destroy(_instance);
    }

    /// <inheritdoc />
    public void Restart()
    {
        // Determine path to the Flagrum executable
        var executablePath = Path.Combine(Directory.GetCurrentDirectory(), "Flagrum");
        if (!File.Exists(executablePath))
        {
            executablePath += ".exe";
        }

        // Restart the application
        ApplicationHost_Restart(_instance, 0, executablePath);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetClipboardText(string text) => ApplicationHost_SetClipboardText(_instance, text);

    /// <inheritdoc />
    public string? OpenFile(
        string filter = IApplication.AllFilesFilter,
        string? initialDirectory = null,
        string caption = "Open File")
    {
        var pResult = Marshal.AllocHGlobal(4096);
        ApplicationHost_OpenFile(_instance, caption, initialDirectory, filter, pResult);
        var result = Marshal.PtrToStringUTF8(pResult);
        Marshal.FreeHGlobal(pResult);
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    /// <inheritdoc />
    public string? SaveFile(
        string filter = IApplication.AllFilesFilter,
        string? initialDirectory = null,
        string caption = "Save File")
    {
        var pResult = Marshal.AllocHGlobal(4096);
        ApplicationHost_SaveFile(_instance, caption, initialDirectory, filter, pResult);
        var result = Marshal.PtrToStringUTF8(pResult);
        Marshal.FreeHGlobal(pResult);
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    /// <inheritdoc />
    public string? OpenDirectory(string? initialDirectory = null, string caption = "Select Folder")
    {
        var pResult = Marshal.AllocHGlobal(4096);
        ApplicationHost_OpenDirectory(_instance, caption, initialDirectory, pResult);
        var result = Marshal.PtrToStringUTF8(pResult);
        Marshal.FreeHGlobal(pResult);
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    /// <inheritdoc />
    public void RefreshPatreonButton()
    {
        // TODO: Implement this
    }

    public static IApplication Factory(IServiceProvider provider) => provider.GetRequiredService<ApplicationHost>();

    /// <summary>
    /// Runs the application indefinitely until <see cref="Exit" /> is called, or the main window is closed.
    /// </summary>
    /// <returns>Exit code.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Run() => ApplicationHost_Run(_instance);

    /// <summary>
    /// Closes active windows and exits the event loop, ending the program.
    /// </summary>
    /// <param name="exitCode">Exit code to return to <see cref="Run" />.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Exit(int exitCode) => ApplicationHost_Exit(_instance, exitCode);

    /// <summary>
    /// Invokes an action on the UI thread.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <remarks>
    /// <paramref name="action" /> is automatically wrapped in a try/catch block to ensure that any
    /// exceptions that occur during operation do not cross the native boundary.
    /// Since the exception can't be rethrown, it's simply logged instead.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke(Action action) => ApplicationHost_Invoke(_instance, action);

    /// <summary>
    /// Creates and shows the splash screen.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OpenSplash() => ApplicationHost_OpenSplash(_instance);

    /// <summary>
    /// Closes and destroys the splash screen.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CloseSplash() => ApplicationHost_CloseSplash(_instance);

    /// <summary>
    /// Updates the text above the splash screen's loading bar.
    /// </summary>
    /// <param name="text">Text to display.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetSplashText(string text) => ApplicationHost_SetSplashText(_instance, text);

    /// <summary>
    /// Creates and shows the main application window.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OpenMainWindow() => ApplicationHost_OpenMainWindow(_instance);

    /// <summary>
    /// Closes and destroys the main application window.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CloseMainWindow() => ApplicationHost_CloseMainWindow(_instance);

    /// <summary>
    /// Sets the callback that will handle web messages sent to the host application
    /// by the embedded web view.
    /// </summary>
    /// <param name="handler">Web message handler.</param>
    public void SetWebMessageHandler(WebMessageReceivedCallback handler)
    {
        _onWebMessageReceived = handler; // Prevent GC
        ApplicationHost_SetWebMessageHandler(_instance, _onWebMessageReceived);
    }

    /// <summary>
    /// Navigates the main window's embedded web view to a different page.
    /// </summary>
    /// <param name="url">URL to navigate to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void NavigateWebView(string url) => ApplicationHost_NavigateWebView(_instance, url);

    /// <summary>
    /// Executes a JavaScript snippet in the main window's embedded web view.
    /// </summary>
    /// <param name="script">JS code to execute.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RunJavaScript(string script) => ApplicationHost_RunJavaScript(_instance, script);

    /// <summary>
    /// Shows a native message box dialog.
    /// </summary>
    /// <param name="title">Message box title.</param>
    /// <param name="message">Message box body text.</param>
    /// <param name="type">Message box type.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ShowMessageBox(string title, string message, MessageBoxType type) =>
        ApplicationHost_ShowMessageBox(_instance, title, message, type);
}