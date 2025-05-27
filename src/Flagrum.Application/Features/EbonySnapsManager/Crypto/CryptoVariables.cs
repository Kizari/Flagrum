namespace Flagrum.Application.Features.EbonySnapsManager.Crypto
{
    internal class CryptoVariables
    {
        public static readonly byte[] ConstantKey = 
        { 
            0x54, 0x5f, 0xd4, 0x9b, 0xce, 0x5d, 0xc2, 0x76, 
            0xa5, 0x1a, 0xcb, 0x86, 0x44, 0x79, 0x1d, 0x6d, 
            0xf4, 0xb7, 0xe7, 0xb1, 0x33, 0x0c, 0x3f, 0x1d 
        };

        public ulong IV1;
        public ulong IV2;
        public ulong Tweak1;
        public ulong Tweak2;
        public uint Seed;
        public ulong NullPaddingA;
        public ulong NullPaddingB;
        public byte End2Value;
    }
}