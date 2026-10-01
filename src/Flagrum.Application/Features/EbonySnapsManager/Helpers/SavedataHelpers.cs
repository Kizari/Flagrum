using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Flagrum.Application.Features.EbonySnapsManager.Helpers
{
    internal class SavedataHelpers
    {
        private static readonly byte[] SnapEntriesContainerStructId = new byte[] { 0x32, 0x91, 0x76, 0xEE, 0x72, 0xFF, 0xD3, 0xEB };
        private static readonly byte[] SnapEntryStructId = new byte[] { 0xE1, 0xC1, 0x1F, 0xEE, 0x93, 0xDF, 0xB2, 0xB6 };

        public static long LocateOffset(byte[] saveData)
        {
            long locatedOffset = 0;

            long readAmount;
            long readSkipAmount;

            using (var reader = new BinaryReader(new MemoryStream(saveData)))
            {
                reader.BaseStream.Position = 12;
                readAmount = reader.ReadUInt32();

                reader.BaseStream.Position = 36;
                readSkipAmount = reader.ReadUInt32();

                reader.BaseStream.Position += readSkipAmount + 104;

                if (reader.ReadUInt32() == 0)
                {
                    reader.BaseStream.Position += 37;
                    readSkipAmount = reader.ReadUInt32() + 8;
                }
                else
                {
                    readSkipAmount = 49;
                }

                reader.BaseStream.Position += readSkipAmount;

                var pos = reader.BaseStream.Position;
                readAmount -= pos;
                var currentBytes = new byte[8];

                for (int i = 0; i < readAmount; i++)
                {
                    reader.BaseStream.Position = pos + i;
                    currentBytes = reader.ReadBytes(8);

                    if (currentBytes.SequenceEqual(SnapEntriesContainerStructId))
                    {
                        locatedOffset = pos + i;
                        break;
                    }
                }
            }

            return locatedOffset;
        }


        private static byte[] HeaderData = new byte[12];
        private static byte[] DataTillSnapStruct { get; set; }

        public static void InitialDataOperations(byte[] decSaveData, long locatedStructOffset)
        {
            Array.Copy(decSaveData, HeaderData, HeaderData.Length);

            DataTillSnapStruct = new byte[locatedStructOffset - 16];
            Array.ConstrainedCopy(decSaveData, 16, DataTillSnapStruct, 0, DataTillSnapStruct.Length);
        }


        private static byte[] StructId = new byte[8];
        public static uint SnapId { get; set; }
        private static uint AttributeFieldsCount { get; set; }
        private static byte[] AttributeFieldsData { get; set; }
        private static ulong SnapTime { get; set; }
        private static byte[] RemainingData = new byte[59];
        private static ulong NewSnapTime { get; set; }

        public static void ReadSnapEntryDataInSave(BinaryReader saveDataReader, bool isAddingSnap)
        {
            StructId = saveDataReader.ReadBytes(8);

            if (!StructId.SequenceEqual(SnapEntryStructId))
            {
                throw new Exception();
            }

            SnapId = saveDataReader.ReadUInt32();
            AttributeFieldsCount = saveDataReader.ReadUInt32();
            AttributeFieldsData = saveDataReader.ReadBytes((int)AttributeFieldsCount * 4);
            SnapTime = saveDataReader.ReadUInt64();
            RemainingData = saveDataReader.ReadBytes(59);

            if (isAddingSnap)
            {
                NewSnapTime = SnapTime + 4;
            }
        }


        private static uint UpdatedSnapEntriesDataSize { get; set; }
        private static uint UpdatedSnapCount { get; set; }
        private static int DictIndex { get; set; }

        public static void PackSnapEntryDataToDict(Dictionary<int, byte[]> snapEntriesDataDict)
        {
            var currentSnapEntryData = new List<byte>();

            currentSnapEntryData.AddRange(SnapEntryStructId);
            currentSnapEntryData.AddRange(BitConverter.GetBytes(SnapId));
            currentSnapEntryData.AddRange(BitConverter.GetBytes(AttributeFieldsCount));
            currentSnapEntryData.AddRange(AttributeFieldsData);
            currentSnapEntryData.AddRange(BitConverter.GetBytes(SnapTime));
            currentSnapEntryData.AddRange(RemainingData);

            UpdatedSnapEntriesDataSize += (uint)currentSnapEntryData.Count();
            UpdatedSnapCount++;

            snapEntriesDataDict.Add(DictIndex, currentSnapEntryData.ToArray());
            DictIndex++;
        }


        private static byte[] DataTillFooterOffset { get; set; }
        private static byte[] FooterData { get; set; }
        private static byte[] EncFooterData = new byte[53];

        public static void FooterOperations(BinaryReader saveDataReader, int footerOffset)
        {
            var currentPos = (int)saveDataReader.BaseStream.Position;
            DataTillFooterOffset = saveDataReader.ReadBytes(footerOffset - currentPos);

            currentPos = (int)saveDataReader.BaseStream.Position;
            FooterData = saveDataReader.ReadBytes((int)((saveDataReader.BaseStream.Length - 53) - currentPos));
            EncFooterData = saveDataReader.ReadBytes(53);
        }


        public static void AddNewSnapRecordDataToDict(int snapsToAdd, uint newSnapId, Dictionary<int, byte[]> snapEntriesDataDict)
        {
            for (int i = 0; i < snapsToAdd; i++)
            {
                var newSnapEntryData = new List<byte>();

                newSnapEntryData.AddRange(SnapEntryStructId);
                newSnapEntryData.AddRange(BitConverter.GetBytes(newSnapId));
                newSnapEntryData.AddRange(BitConverter.GetBytes((uint)8));
                newSnapEntryData.AddRange(BitConverter.GetBytes((uint)16929630));
                newSnapEntryData.AddRange(BitConverter.GetBytes((uint)16929637));
                newSnapEntryData.AddRange(BitConverter.GetBytes((uint)16929638));
                newSnapEntryData.AddRange(BitConverter.GetBytes((uint)17001451));
                newSnapEntryData.AddRange(BitConverter.GetBytes((uint)17001463));
                newSnapEntryData.AddRange(BitConverter.GetBytes((uint)17017688));
                newSnapEntryData.AddRange(BitConverter.GetBytes((uint)17061971));
                newSnapEntryData.AddRange(BitConverter.GetBytes((uint)17075085));
                newSnapEntryData.AddRange(BitConverter.GetBytes(NewSnapTime));
                newSnapEntryData.AddRange(BitConverter.GetBytes((ulong)5332261958806667265));
                newSnapEntryData.AddRange(BitConverter.GetBytes((ulong)2305843010301418620));
                newSnapEntryData.AddRange(BitConverter.GetBytes((ulong)8214565721402917683));
                newSnapEntryData.AddRange(BitConverter.GetBytes((ulong)1087028557));
                newSnapEntryData.AddRange(BitConverter.GetBytes((ulong)1072693248));
                newSnapEntryData.AddRange(BitConverter.GetBytes((ulong)50331648));
                newSnapEntryData.AddRange(BitConverter.GetBytes(ulong.MinValue));
                newSnapEntryData.AddRange(BitConverter.GetBytes(ushort.MinValue));
                newSnapEntryData.Add(0);

                snapEntriesDataDict.Add(DictIndex, newSnapEntryData.ToArray());

                newSnapId++;
                NewSnapTime += 4;

                UpdatedSnapEntriesDataSize += 115;
                UpdatedSnapCount++;
                DictIndex++;
            }
        }


        public static byte[] BuildUpdatedSaveData(Dictionary<int, byte[]> snapEntriesDataDict)
        {
            var updatedSaveData = new byte[] { };

            using (var updatedSaveDataStream = new MemoryStream())
            {
                using (var updatedSaveDataWriter = new BinaryWriter(updatedSaveDataStream))
                {
                    updatedSaveDataWriter.Write(HeaderData);
                    updatedSaveDataWriter.Write((uint)(16 + DataTillSnapStruct.Length + 12 + UpdatedSnapEntriesDataSize + DataTillFooterOffset.Length));
                    updatedSaveDataWriter.Write(DataTillSnapStruct);
                    updatedSaveDataWriter.Write(SnapEntriesContainerStructId);
                    updatedSaveDataWriter.Write(UpdatedSnapCount);

                    foreach (var snapEntryData in snapEntriesDataDict.Values)
                    {
                        updatedSaveDataWriter.Write(snapEntryData);
                    }

                    updatedSaveDataWriter.Write(DataTillFooterOffset);
                    updatedSaveDataWriter.Write(FooterData);

                    var currentPos = updatedSaveDataWriter.BaseStream.Position;
                    var padValue = 16;

                    if (currentPos % padValue != 0)
                    {
                        var remainder = currentPos % padValue;
                        var increaseByteAmount = padValue - remainder;

                        var newSize = currentPos + increaseByteAmount;
                        var padNulls = newSize - currentPos;

                        for (int p = 0; p < padNulls; p++)
                        {
                            updatedSaveDataWriter.BaseStream.WriteByte(0);
                        }
                    }

                    updatedSaveDataWriter.Write(EncFooterData);

                    updatedSaveDataStream.Seek(0, SeekOrigin.Begin);
                    updatedSaveData = updatedSaveDataStream.ToArray();
                }
            }

            return updatedSaveData;
        }


        public static void ResetVariables()
        {
            Array.Clear(HeaderData, 0, HeaderData.Length);
            DataTillSnapStruct = null;

            Array.Clear(StructId, 0, StructId.Length);
            SnapId = 0;
            AttributeFieldsCount = 0;
            AttributeFieldsData = null;
            SnapTime = 0;
            Array.Clear(RemainingData, 0, RemainingData.Length);
            NewSnapTime = 0;

            UpdatedSnapEntriesDataSize = 0;
            UpdatedSnapCount = 0;
            DictIndex = 0;

            DataTillFooterOffset = null;
            FooterData = null;
            Array.Clear(EncFooterData, 0, EncFooterData.Length);
        }
    }
}