using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Flagrum.Core.Utilities
{
    public static class Cryptography
    {
        private const ulong HashSeed = 1469598103934665603;
        private const ulong HashPrime = 1099511628211;

        private static readonly byte[] _aesKey = new byte[16]
            {156, 108, 93, 65, 21, 82, 63, 23, 90, 211, 248, 183, 117, 88, 30, 207};

        public static byte[] Encrypt(byte[] data)
        {
            var unencryptedSize = (data.Length + 15) & -16;
            var encryptedSize = unencryptedSize + 33; // 16 for IV, 16 empty, 1 for flag

            var unencryptedData = new byte[unencryptedSize];
            var encryptedData = new byte[encryptedSize];
            Buffer.BlockCopy(data, 0, unencryptedData, 0, data.Length);

            var aes = new AesManaged {Key = _aesKey};
            aes.GenerateIV();
            var encryptor = aes.CreateEncryptor();
            encryptor.TransformBlock(unencryptedData, 0, unencryptedSize, encryptedData, 0);

            Buffer.BlockCopy(aes.IV, 0, encryptedData, unencryptedSize, aes.IV.Length);

            // 1 signifies that the data is encrypted
            encryptedData[unencryptedSize + 32] = 1;

            return encryptedData;
        }

        public static byte[] Decrypt(byte[] data)
        {
            var size = data.Length - 33;

            var iv = new byte[16];
            Array.Copy(data, size, iv, 0, 16);

            var aes = new AesManaged
            {
                Key = _aesKey,
                IV = iv,
                Padding = PaddingMode.None
            };

            var decryptedBuffer = new byte[size];
            var encryptedBuffer = new byte[size];

            Array.Copy(data, 0, encryptedBuffer, 0, size);

            var decryptor = aes.CreateDecryptor();
            decryptor.TransformBlock(encryptedBuffer, 0, size, decryptedBuffer, 0);

            return decryptedBuffer;
        }

        public static ulong Hash(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            return bytes.Aggregate(HashSeed, (current, b) => (current ^ b) * HashPrime);
        }

        public static ulong Base64Hash(byte[] bytes)
        {
            var provider = new SHA256CryptoServiceProvider();
            var hash = provider.ComputeHash(bytes);
            var long1 = BitConverter.ToUInt64(hash, 0);
            var long2 = BitConverter.ToUInt64(hash, 8);
            var long3 = BitConverter.ToUInt64(hash, 24);
            return long1 ^ long2 ^ long3;
        }

        public static ulong MergeHashes(ulong hash1, ulong hash2)
        {
            return (hash1 * 1099511628211) ^ hash2;
        }
    }
}