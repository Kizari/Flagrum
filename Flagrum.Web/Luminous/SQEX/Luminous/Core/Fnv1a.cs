namespace Luminous.Core
{
    public static class Fnv1a
    {
        /// <summary>
        /// Compute the FNV-1a hash of a string.
        /// </summary>
        /// <param name="str">The string to hash.</param>
        /// <param name="offsetBasis">The base hash value.</param>
        /// <returns>The hash of the string.</returns>
        public static uint Fnv1a32(string str, uint offsetBasis = 2166136261)
        {
            var result = offsetBasis;
            foreach(var character in str)
            {
                result = 16777619 * (result ^ character);
            }

            return result;
        }
    }
}
