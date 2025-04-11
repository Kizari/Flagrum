using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Flagrum.Core.Utilities.Extensions;

public static class StringExtensions
{
    public static string ToBase64(this string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    public static string FromBase64(this string input)
    {
        var bytes = Convert.FromBase64String(input);
        return Encoding.UTF8.GetString(bytes);
    }

    public static string SpacePascalCase(this string pascalCaseString)
    {
        return Regex.Replace(pascalCaseString, @"(?<=[A-Za-z])(?=[A-Z][a-z])|(?<=[a-z0-9])(?=[0-9]?[A-Z])", " ");
    }

    public static string CapitaliseFirstLetter(this string value)
    {
        var result = value.ToUpper()[0];
        return result + value[1..];
    }

    public static string WithLeadingZeroes(this int value, int totalDigits)
    {
        var result = value.ToString();
        while (result.Length < totalDigits)
        {
            result = "0" + result;
        }

        return result;
    }

    /// Ensures that a string does not have any illegal characters (for use in binmods)
    public static string ToSafeString(this string input)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.Any(char.IsLetter))
        {
            return Guid.NewGuid().ToString().ToLower();
        }

        while (!char.IsLetter(input[0]) && input.Length > 0)
        {
            input = input[1..];
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            return Guid.NewGuid().ToString().ToLower();
        }

        input = input.Replace('.', '_');
        input = input.Replace('-', '_');
        input = input.Replace(' ', '_');

        var output = input
            .Where(character => char.IsLetterOrDigit(character) || character == '_')
            .Aggregate("", (current, character) => current + character);

        return string.IsNullOrWhiteSpace(input) ? Guid.NewGuid().ToString().ToLower() : output.ToLower();
    }
}