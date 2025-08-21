using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Flagrum.Application.Features.EbonySnapsManager.Helpers
{
    internal class SnapshotHelpers
    {
        private static readonly byte[] SnapLinkStructId = new byte[] { 0xA1, 0x1C, 0xFC, 0x58, 0x08, 0x02, 0x47, 0x2E };

        public static byte[] GetImgDataFromSnapshotFile(string ssFile)
        {
            var imgData = new byte[] { };

            if (File.Exists(ssFile))
            {
                using (var ssReader = new BinaryReader(File.Open(ssFile, FileMode.Open, FileAccess.Read)))
                {
                    ssReader.BaseStream.Seek(32, SeekOrigin.Begin);
                    var imgSize = ssReader.ReadUInt32();
                    imgData = new byte[imgSize];
                    imgData = ssReader.ReadBytes((int)imgSize);
                }
            }

            return imgData;
        }


        public static string SaveImgDataToFile(string outImgSavetimeFileName, string imgSaveDir, byte[] imgData)
        {
            var detectedExtn = Path.GetExtension(outImgSavetimeFileName);

            using (var imgStream = new MemoryStream())
            {
                using (var imgReader = new BinaryReader(imgStream))
                {
                    imgStream.Write(imgData, 0, imgData.Length);
                    imgStream.Seek(0, SeekOrigin.Begin);

                    switch (imgReader.ReadUInt16())
                    {
                        case 55551:
                            detectedExtn += ".jpg";
                            break;

                        case 20617:
                            detectedExtn += ".png";
                            break;
                    }
                }
            }

            var outImgFile = Path.Combine(imgSaveDir, Path.GetFileNameWithoutExtension(outImgSavetimeFileName) + detectedExtn);

            if (File.Exists(outImgFile))
            {
                File.Delete(outImgFile);
            }

            File.WriteAllBytes(outImgFile, imgData);

            return outImgFile;
        }


        public static void CreateSnapshotFile(string ssFile, byte[] imgData)
        {
            using (var ssStream = new MemoryStream())
            {
                using (var ssWriter = new BinaryWriter(ssStream))
                {
                    ssWriter.Write(Encoding.UTF8.GetBytes("ebb\0"));
                    ssWriter.Write((uint)4);
                    ssWriter.Write((uint)0);
                    ssWriter.Write((uint)imgData.Length + 36);
                    ssWriter.Write(4279566338);
                    ssWriter.Write((uint)1);
                    ssWriter.Write(0xA4CEBC89AE0F8ED9);

                    ssWriter.Write((uint)imgData.Length);
                    ssWriter.Write(imgData);

                    ssWriter.Write((uint)1);
                    ssWriter.Write((uint)49);
                    ssWriter.Write(Encoding.UTF8.GetBytes("Black.Save.Snapshot.SaveSnapshotImageBinaryStruct"));
                    ssWriter.Write(0xA4CEBC89AE0F8ED9);
                    ssWriter.Write(ulong.MaxValue);
                    ssWriter.Write((ushort)1);
                    ssWriter.Write((uint)7);
                    ssWriter.Write(Encoding.UTF8.GetBytes("binary_"));
                    ssWriter.Write((uint)27);
                    ssWriter.Write(Encoding.UTF8.GetBytes("Luminous.Core.Memory.Buffer"));
                    ssWriter.Write((uint)0);
                    ssWriter.Write((uint)24);
                    ssWriter.Write((uint)0x00160001);
                    ssWriter.Write((byte)0);

                    ssWriter.Seek(0, SeekOrigin.Begin);
                    File.WriteAllBytes(ssFile, ssStream.ToArray());
                }
            }
        }


        private static byte[] HeaderData = new byte[12];
        private static byte[] DataTillLinkStruct = new byte[16];

        public static void InitialDataOperations(byte[] decLinkData)
        {
            Array.Copy(decLinkData, HeaderData, HeaderData.Length);
            Array.ConstrainedCopy(decLinkData, 16, DataTillLinkStruct, 0, DataTillLinkStruct.Length);
        }


        private static byte[] StructId = new byte[8];
        public static uint SnapId { get; set; }
        private static uint FieldsCount { get; set; }
        private static byte[] FieldsData { get; set; }

        public static void ReadSnaplinkDataInLink(BinaryReader snapshotlinkReader)
        {
            StructId = snapshotlinkReader.ReadBytes(8);

            if (!StructId.SequenceEqual(SnapLinkStructId))
            {
                throw new Exception();
            }

            SnapId = snapshotlinkReader.ReadUInt32();
            FieldsCount = snapshotlinkReader.ReadUInt32();
            FieldsData = snapshotlinkReader.ReadBytes((int)FieldsCount * 4);
        }


        private static uint UpdatedSnaplinksDataSize { get; set; }
        private static uint UpdatedSnapCount { get; set; }
        private static int DictIndex { get; set; }

        public static void PackSnaplinkDataToDict(Dictionary<int, byte[]> snaplinksDataDict)
        {
            var currentSnaplinkData = new List<byte>();

            currentSnaplinkData.AddRange(StructId);
            currentSnaplinkData.AddRange(BitConverter.GetBytes(SnapId));
            currentSnaplinkData.AddRange(BitConverter.GetBytes(FieldsCount));
            currentSnaplinkData.AddRange(FieldsData);

            UpdatedSnaplinksDataSize += (uint)currentSnaplinkData.Count();

            snaplinksDataDict.Add(DictIndex, currentSnaplinkData.ToArray());
            UpdatedSnapCount++;
            DictIndex++;
        }


        private static byte[] FooterData { get; set; }
        private static byte[] EncFooterData = new byte[53];

        public static void FooterOperations(BinaryReader snapshotlinkReader)
        {
            var currentPos = (int)snapshotlinkReader.BaseStream.Position;
            FooterData = snapshotlinkReader.ReadBytes((int)((snapshotlinkReader.BaseStream.Length - 53) - currentPos));
            EncFooterData = snapshotlinkReader.ReadBytes(53);
        }


        public static void AddNewSnaplinksDataToDict(int snaplinksToAdd, ref uint nextSnapId, Dictionary<int, byte[]> snaplinksDataDict)
        {
            for (int i = 0; i < snaplinksToAdd; i++)
            {
                var newSnaplinkData = new List<byte>();
                var newSnapId = nextSnapId;
                nextSnapId++;

                newSnaplinkData.AddRange(SnapLinkStructId);
                newSnaplinkData.AddRange(BitConverter.GetBytes(newSnapId));
                newSnaplinkData.AddRange(BitConverter.GetBytes((uint)3));
                newSnaplinkData.AddRange(BitConverter.GetBytes((uint)1));
                newSnaplinkData.AddRange(BitConverter.GetBytes((uint)2));
                newSnaplinkData.AddRange(BitConverter.GetBytes((uint)0));

                snaplinksDataDict.Add(DictIndex, newSnaplinkData.ToArray());

                UpdatedSnaplinksDataSize += (uint)newSnaplinkData.Count();
                UpdatedSnapCount++;
                DictIndex++;
            }
        }


        public static byte[] BuildUpdatedlinksData(Dictionary<int, byte[]> snaplinksDataDict, uint nextSnapId)
        {
            var updatedSnapshotlinkData = new byte[] { };

            using (var updatedSnapshotlinkStream = new MemoryStream())
            {
                using (var updatedSnapshotlinkWriter = new BinaryWriter(updatedSnapshotlinkStream))
                {
                    updatedSnapshotlinkWriter.Write(HeaderData);
                    updatedSnapshotlinkWriter.Write((uint)(16 + DataTillLinkStruct.Length + UpdatedSnaplinksDataSize + 16));
                    updatedSnapshotlinkWriter.Write(DataTillLinkStruct);
                    updatedSnapshotlinkWriter.Write(UpdatedSnapCount);

                    foreach (var snaplinkData in snaplinksDataDict.Values)
                    {
                        updatedSnapshotlinkWriter.Write(snaplinkData);
                    }

                    updatedSnapshotlinkWriter.Write(nextSnapId);
                    updatedSnapshotlinkWriter.Write(ulong.MaxValue);
                    updatedSnapshotlinkWriter.Write(FooterData);

                    var currentPos = updatedSnapshotlinkWriter.BaseStream.Position;
                    var padValue = 16;

                    if (currentPos % padValue != 0)
                    {
                        var remainder = currentPos % padValue;
                        var increaseByteAmount = padValue - remainder;

                        var newSize = currentPos + increaseByteAmount;
                        var padNulls = newSize - currentPos;

                        for (int p = 0; p < padNulls; p++)
                        {
                            updatedSnapshotlinkWriter.BaseStream.WriteByte(0);
                        }
                    }

                    updatedSnapshotlinkWriter.Write(EncFooterData);

                    updatedSnapshotlinkWriter.Seek(0, SeekOrigin.Begin);
                    updatedSnapshotlinkData = updatedSnapshotlinkStream.ToArray();
                }
            }

            return updatedSnapshotlinkData;
        }


        public static void ResetVariables()
        {
            Array.Clear(HeaderData, 0, HeaderData.Length);
            Array.Clear(DataTillLinkStruct, 0, DataTillLinkStruct.Length);

            Array.Clear(StructId, 0, StructId.Length);
            SnapId = 0;
            FieldsCount = 0;
            FieldsData = null;

            UpdatedSnaplinksDataSize = 0;
            UpdatedSnapCount = 0;
            DictIndex = 0;

            FooterData = null;
            Array.Clear(EncFooterData, 0, EncFooterData.Length);
        }
    }
}