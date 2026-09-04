using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

namespace Flagrum.Host.Shell;

/// <summary>
/// Helper for displaying simple message popups.
/// </summary>
public static class MessageBox
{
    /// <summary>
    /// Shows a message box.
    /// </summary>
    /// <param name="caption">Text that goes in the title bar of the message box.</param>
    /// <param name="message">Text that goes in the body of the message box.</param>
    /// <param name="icon">Icon to display in the message box.</param>
    public static Task ShowAsync(string caption, string message, Icon icon)
    {
        return MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentTitle = caption,
            ContentMessage = message,
            MaxWidth = 350,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Topmost = true,
            CloseOnClickAway = false,
            Icon = icon,
            ButtonDefinitions =
            [
                new ButtonDefinition
                {
                    Name = "OK",
                    IsDefault = true
                }
            ]
        }).ShowAsync();
    }
}