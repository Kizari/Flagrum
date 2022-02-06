using System;

namespace Flagrum.Core.Ebex.Xmb2
{
    public static class XmlUtility
    {
        /// <summary>
        /// Parse a string into a boolean value.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>The parsed boolean.</returns>
        public static bool ToBool(string text)
        {
            if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(text, "false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var intValue = Int32.Parse(text);
            return intValue != 0;
        }
    }
}
