using System;
using System.Collections.Generic;

namespace Flagrum.Application.Utilities;

/// <summary>
/// This type of exception will be specifically processed by the global exception event handlers
/// to display a user-friendly error message to the user before the app terminates
/// </summary>
public class UserFriendlyException : Exception
{
    /// <summary>
    /// Contains localized user-friendly error messages
    /// Key: The locale code (e.g. en-GB, ja-JP, zh-Hans)
    /// Value: The message in the given language
    /// </summary>
    private readonly Dictionary<string, string> _localizedMessage;

    /// <param name="localizedMessage">Key: Locale Code, Value: Message—Must have at least English!</param>
    /// <param name="innerException">The exception associated with this user friendly message</param>
    public UserFriendlyException(Dictionary<string, string> localizedMessage, Exception innerException) : base(null,
        innerException)
    {
        _localizedMessage = localizedMessage;
    }

    public string GetMessage(string culture)
    {
        if (_localizedMessage.TryGetValue(culture, out var message))
        {
            return message;
        }

        return _localizedMessage.TryGetValue(SupportedCultures.English, out var english)
            ? english
            : "Oops, the developer forgot to provide a valid translation for this error message.";
    }
}