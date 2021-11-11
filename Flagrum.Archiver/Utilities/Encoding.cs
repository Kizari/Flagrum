using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Flagrum.Archiver.Utilities
{
    public class Encoding
    {
        private const ulong HashSeed = 1469598103934665603;
        private const ulong HashPrime = 1099511628211;

        private AesManaged _aes;

        public Encoding()
        {
            _aes = new AesManaged();
            _aes.Key = new byte[16] { 156, 108, 93, 65, 21, 82, 63, 23, 90, 211, 248, 183, 117, 88, 30, 207 };
        }

        public byte[] Encrypt(byte[] data)
        {
            var size = data.Length;
            var difference = (size + 15 & -16) - size;
            var inputSize = size + difference;
            var encryptedSize = inputSize + 33;     // 16 for IV, 16 empty, 1 for flag

            var inputData = new byte[inputSize];
            var encryptedData = new byte[encryptedSize];

            _aes.GenerateIV();
            var encryptor = _aes.CreateEncryptor();

            Buffer.BlockCopy(data, 0, inputData, 0, size);

            encryptor.TransformBlock(inputData, 0, inputSize, encryptedData, 0);
            Buffer.BlockCopy(_aes.IV, 0, encryptedData, inputSize, _aes.IV.Length);
            Array.Clear(encryptedData, inputSize + _aes.IV.Length, 16);
            encryptedData[inputSize + 32] = 1;  // 16 for IV, 16 empty

            return encryptedData;
        }

        public ulong Hash(string value)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            return bytes.Aggregate(HashSeed, ((current, b) => ((current ^ b) * HashPrime)));
        }

        public ulong Base64Hash(byte[] bytes)
        {
            var provider = new SHA256CryptoServiceProvider();
            var hash = provider.ComputeHash(bytes);
            var long1 = BitConverter.ToUInt64(hash, 0);
            var long2 = BitConverter.ToUInt64(hash, 8);
            var long3 = BitConverter.ToUInt64(hash, 24);
            return long1 ^ long2 ^ long3;
        }

        public ulong MergeHashes(ulong hash1, ulong hash2) => hash1 * 1099511628211 ^ hash2;
    }
}
