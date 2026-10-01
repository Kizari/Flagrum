using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Flagrum.Application.Features.EbonySnapsManager.Crypto
{
    internal class Encrypt
    {
        public static byte[] BeginEncryption(byte[] inDecData)
        {
            // Get crypto related variables 
            // and the data to encrypt
            var cryptoVars = new CryptoVariables();
            byte[] dataToEncrypt = new byte[] { };

            using (var inFileReader = new BinaryReader(new MemoryStream(inDecData)))
            {
                var cryptoOffset = inFileReader.BaseStream.Length - 53;

                inFileReader.BaseStream.Position = cryptoOffset;
                inFileReader.AssignByteValuesInClass(cryptoVars);

                inFileReader.BaseStream.Position = 0;
                dataToEncrypt = new byte[(int)cryptoOffset];
                dataToEncrypt = inFileReader.ReadBytes((int)cryptoOffset);
            }

            // Encrypt the data with AES algorithm
            var ivValList = new List<byte>();
            ivValList.AddRange(BitConverter.GetBytes(cryptoVars.IV1));
            ivValList.AddRange(BitConverter.GetBytes(cryptoVars.IV2));

            var encryptedData = new byte[dataToEncrypt.Length];

            using (var aes = Aes.Create())
            {
                aes.Key = CryptoVariables.ConstantKey;
                aes.IV = ivValList.ToArray();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                using (var encryptor = aes.CreateEncryptor())
                {
                    encryptedData = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
                }
            }

            // Shuffle the encrypted data
            ShuffleData(cryptoVars, encryptedData);

            // Tweak the encrypted data
            var tweakBytesList = new List<byte>();
            tweakBytesList.AddRange(BitConverter.GetBytes(cryptoVars.Tweak1));
            tweakBytesList.AddRange(BitConverter.GetBytes(cryptoVars.Tweak2));

            for (int i = 0; i < encryptedData.Length; i++)
            {
                encryptedData[i] = (byte)(encryptedData[i] + tweakBytesList[i & 0xF]);
            }

            // Create the final encrypted file
            var outEncData = new byte[] { };

            using (var outEncDataStream = new MemoryStream())
            {
                using (var outEncDataWriter = new BinaryWriter(outEncDataStream))
                {
                    outEncDataWriter.Write(encryptedData);

                    outEncDataWriter.Write(cryptoVars.IV1);
                    outEncDataWriter.Write(cryptoVars.IV2);
                    outEncDataWriter.Write(cryptoVars.Tweak1);
                    outEncDataWriter.Write(cryptoVars.Tweak2);
                    outEncDataWriter.Write(cryptoVars.Seed);
                    outEncDataWriter.Write(cryptoVars.NullPaddingA);
                    outEncDataWriter.Write(cryptoVars.NullPaddingB);
                    outEncDataWriter.Write(cryptoVars.End2Value);

                    outEncDataWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                    outEncData = outEncDataStream.ToArray();
                }
            }

            return outEncData;
        }


        private static void ShuffleData(CryptoVariables cryptoVars, byte[] encryptedData)
        {
            var bufferSize = encryptedData.Length >> 4;
            var keyStack = SharedCryptoFunctions.GenerateKeyStack(cryptoVars, bufferSize);

            for (var shuffleIterator = bufferSize - 1; shuffleIterator > 0; shuffleIterator--)
            {
                var destination = new List<ulong>();
                var source = new List<ulong>();

                int destStart = 16 * shuffleIterator;
                destination.Add(BitConverter.ToUInt64(encryptedData, destStart));
                destination.Add(BitConverter.ToUInt64(encryptedData, destStart + 8));

                int sourceStart = 16 * keyStack[shuffleIterator];
                source.Add(BitConverter.ToUInt64(encryptedData, sourceStart));
                source.Add(BitConverter.ToUInt64(encryptedData, sourceStart + 8));

                ulong oldDestinationVal;
                ulong oldSourceVal;

                oldDestinationVal = destination[0];
                oldSourceVal = source[0];
                destination[0] = oldSourceVal;
                source[0] = oldDestinationVal;

                oldDestinationVal = destination[1];
                oldSourceVal = source[1];
                destination[1] = oldSourceVal;
                source[1] = oldDestinationVal;

                BitConverter.GetBytes(destination[0]).CopyTo(encryptedData, destStart);
                BitConverter.GetBytes(destination[1]).CopyTo(encryptedData, destStart + 8);
                BitConverter.GetBytes(source[0]).CopyTo(encryptedData, sourceStart);
                BitConverter.GetBytes(source[1]).CopyTo(encryptedData, sourceStart + 8);
            }
        }
    }
}