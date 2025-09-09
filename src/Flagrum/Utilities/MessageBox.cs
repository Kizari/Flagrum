using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

namespace Flagrum.Utilities;

public static class MessageBox
{
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