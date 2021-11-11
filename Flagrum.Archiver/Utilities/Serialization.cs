namespace Flagrum.Archiver.Utilities
{
    public class Serialization
    {
        public static ulong GetPadding(ulong size, ulong alignment) => alignment + alignment * (size / alignment);

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
