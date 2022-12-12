using System;

namespace Flagrum.Console.Utilities;

public static class Log
{
    public static void WriteLine(string message)
    {
        System.Console.WriteLine(message);
    }

    public static void WriteLine(string message, ConsoleColor color)
    {
        var original = System.Console.ForegroundColor;
        System.Console.ForegroundColor = color;
        System.Console.WriteLine(message);
        System.Console.ForegroundColor = original;
    }
}