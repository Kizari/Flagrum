using System.Collections.Generic;
using System.Globalization;

namespace Flagrum.Application.Utilities;

public static class SupportedCultures
{
    public const string English = "en-GB";
    public const string Japanese = "ja-JP";
    public const string ChineseSimplified = "zh-Hans";
    public const string ChineseTraditional = "zh-Hant";
}

public static class CultureHelper
{
    private static List<CultureInfo> SupportedExtraCultures => new()
    {
        new CultureInfo(SupportedCultures.Japanese),
        new CultureInfo(SupportedCultures.ChineseSimplified),
        new CultureInfo(SupportedCultures.ChineseTraditional)
    };

    public static string GetClosestCulture()
    {
        foreach (var culture in SupportedExtraCultures)
        {
            var closestCulture = GetClosestCulture(culture);
            if (closestCulture != null)
            {
                return closestCulture;
            }
        }

        return SupportedCultures.English;
    }

    private static string GetClosestCulture(CultureInfo culture)
    {
        var cultureToMatch = CultureInfo.CurrentCulture;

        do
        {
            if (cultureToMatch.Name == culture.Name)
            {
                return culture.Name;
            }

            cultureToMatch = cultureToMatch.Parent;
        } while (cultureToMatch.Parent.Name != cultureToMatch.Name);

        return null;
    }
}