namespace Flagrum.Core.Utilities
{
    public static class Serialization
    {
        /// <summary>
        /// Aligns current offset to the given block size
        /// </summary>
        /// <param name="offset">The offset of the end of the data that needs to be aligned</param>
        /// <param name="blockSize">The size to align to</param>
        /// <returns>The offset of the end of the alignment</returns>
        public static uint GetAlignment(uint offset, uint blockSize) => blockSize + blockSize * (offset / blockSize);
        public static ulong GetAlignment(ulong offset, ulong blockSize) => blockSize + blockSize * (offset / blockSize);
    }
}