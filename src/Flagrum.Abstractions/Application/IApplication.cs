using Flagrum.Abstractions.Application;

namespace Flagrum.Abstractions;

/// <summary>
/// Represents the Flagrum application.
/// </summary>
public interface IApplication : IDisposable
{
    /// <summary>
    /// Semantic version of the currently running build of Flagrum.
    /// </summary>
    Version Version { get; }
    
    /// <summary>
    /// File that Flagrum was launched to handle.
    /// </summary>
    /// <remarks>
    /// Will be <c>null</c> if Flagrum was not launched via file association,
    /// or if the associated file has already been handled and cleared.
    /// </remarks>
    string? AssociatedFile { get; set; }

    /// <summary>
    /// Invokes an action on the UI thread.
    /// </summary>
    /// <param name="callback">Action to execute on the UI thread.</param>
    void Invoke(Action callback);
    
    /// <summary>
    /// Shows an open file dialog.
    /// </summary>
    /// <param name="filter">Qt-style file type filter string.</param>
    /// <param name="initialDirectory">Directory to show in the dialog when it first appears.</param>
    /// <param name="caption">Dialog title.</param>
    /// <returns><c>null</c> if the user canceled the dialog, otherwise the full path to the file.</returns>
    Task<string?> OpenFileAsync(
        FileDialogFileType[] filter, 
        string? initialDirectory = null, 
        string caption = "Open File");

    /// <summary>
    /// Shows a save file dialog.
    /// </summary>
    /// <param name="filter">Qt-style file type filter string.</param>
    /// <param name="defaultFileName">Optional default file name to populate in the save dialog.</param>
    /// <param name="caption">Dialog title.</param>
    /// <returns><c>null</c> if the user canceled the dialog, otherwise the full path to the file.</returns>
    Task<string?> SaveFileAsync(
        FileDialogFileType[] filter,
        string? defaultFileName = null,
        string caption = "Save File");

    /// <summary>
    /// Shows a directory selection dialog.
    /// </summary>
    /// <param name="caption">Dialog title.</param>
    /// <returns><c>null</c> if the user canceled the dialog, otherwise the full path to the directory.</returns>
    Task<string?> OpenDirectoryAsync(string caption = "Select Folder");
    
    /// <summary>
    /// Gracefully shuts down this instance of the Flagrum process, and begins a new one.
    /// </summary>
    void Restart();

    /// <summary>
    /// Copies the given text into the system's clipboard.
    /// </summary>
    /// <param name="text">Text to copy.</param>
    Task SetClipboardTextAsync(string text);
    
    /// <summary>
    /// Updates the state of the Patreon button in the application title bar.
    /// </summary>
    /// <remarks>Used to show/hide the button based on user preferences.</remarks>
    void RefreshPatreonButton();

    /// <summary>
    /// Sets the caption on the splash screen.
    /// </summary>
    /// <param name="text">Caption to set.</param>
    void SetSplashText(string text);
}