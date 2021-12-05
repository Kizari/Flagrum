using System;

namespace Flagrum.Core.Utilities
{
    public static class Serialization
    {
        /// <summary>
        ///     Aligns current offset to the given block size
        /// </summary>
        /// <param name="offset">The offset of the end of the data that needs to be aligned</param>
        /// <param name="blockSize">The size to align to</param>
        /// <returns>The offset of the end of the alignment</returns>
        public static uint GetAlignment(uint offset, uint blockSize)
        {
            return blockSize + blockSize * (offset / blockSize);
        }

        public static ulong GetAlignment(ulong offset, ulong blockSize)
        {
            return blockSize + blockSize * (offset / blockSize);
        }

        public static string ToSafeString(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
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

            var output = "";
            foreach (var character in input)
            {
                if (char.IsLetterOrDigit(character) || character == '_')
                {
                    output += character;
                }
            }
            
            return string.IsNullOrWhiteSpace(input) ? Guid.NewGuid().ToString().ToLower() : output.ToLower();
        }
    }
}