namespace Flagrum.Archiver.Utilities
{
    public class Serialization
    {
        /// <summary>
        /// Aligns current offset to the given block size
        /// </summary>
        /// <param name="offset">The offset of the end of the data that needs to be aligned</param>
        /// <param name="blockSize">The size to align to</param>
        /// <returns>The offset of the end of the alignment</returns>
        public static ulong GetAlignment(ulong offset, ulong blockSize) => blockSize + blockSize * (offset / blockSize);

        public static int EncodeString(string value, out byte[] bytes)
        {
            var stringBufferSize = value.Length + 1 > 256 ? value.Length + 1 : 256;

            var stringBuffer = new char[stringBufferSize];
            bytes = new byte[stringBufferSize * 4];

            uint i;
            for (i = 0; i < value.Length; ++i)
            {
                stringBuffer[i] = value[(int)i];
            }

            stringBuffer[i] = char.MinValue;

            return System.Text.Encoding.UTF8.GetEncoder().GetBytes(stringBuffer, 0, value.Length, bytes, 0, true);
        }
    }
}
