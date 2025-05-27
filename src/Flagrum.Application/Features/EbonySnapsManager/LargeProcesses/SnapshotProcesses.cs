using Flagrum.Application.Features.EbonySnapsManager.Crypto;
using Flagrum.Application.Features.EbonySnapsManager.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Flagrum.Application.Features.EbonySnapsManager.LargeProcesses
{
    internal class SnapshotProcesses
    {
        public static void AddSnapsInLink(string snapshotlinkFile, ref uint newSnapId, int snaplinksToAdd)
        {
            var decLinkData = Decrypt.BeginDecryption(File.ReadAllBytes(snapshotlinkFile));
            SnapshotHelpers.ResetVariables();

            SnapshotHelpers.InitialDataOperations(decLinkData);

            var snaplinksDataDict = new Dictionary<int, byte[]>();
            var nextSnapId = uint.MinValue;

            using (var snapshotlinkReader = new BinaryReader(new MemoryStream(decLinkData)))
            {
                snapshotlinkReader.BaseStream.Position = 12;
                var footerOffset = (int)snapshotlinkReader.ReadUInt32();

                snapshotlinkReader.BaseStream.Position = 32;
                var snapCount = snapshotlinkReader.ReadUInt32();

                for (int i = 0; i < snapCount; i++)
                {
                    SnapshotHelpers.ReadSnaplinkDataInLink(snapshotlinkReader);
                    SnapshotHelpers.PackSnaplinkDataToDict(snaplinksDataDict);
                }

                nextSnapId = snapshotlinkReader.ReadUInt32();
                newSnapId = nextSnapId;
                snapshotlinkReader.BaseStream.Position += 8;

                SnapshotHelpers.FooterOperations(snapshotlinkReader);
            }

            SnapshotHelpers.AddNewSnaplinksDataToDict(snaplinksToAdd, ref nextSnapId, snaplinksDataDict);

            var updatedSnapshotlinkData = SnapshotHelpers.BuildUpdatedlinksData(snaplinksDataDict, nextSnapId);
            SnapshotHelpers.ResetVariables();

            var outSnapshotlinkData = Encrypt.BeginEncryption(updatedSnapshotlinkData);
            File.Delete(snapshotlinkFile);
            File.WriteAllBytes(snapshotlinkFile, outSnapshotlinkData);
        }


        public static void RemoveBlankSnapsInlink(string snapshotlinkFile, string snapshotDir)
        {
            var decLinkData = Decrypt.BeginDecryption(File.ReadAllBytes(snapshotlinkFile));
            SnapshotHelpers.ResetVariables();

            SnapshotHelpers.InitialDataOperations(decLinkData);

            var snaplinksDataDict = new Dictionary<int, byte[]>();
            var nextSnapId = uint.MinValue;

            using (var snapshotlinkReader = new BinaryReader(new MemoryStream(decLinkData)))
            {
                snapshotlinkReader.BaseStream.Position = 12;
                var footerOffset = (int)snapshotlinkReader.ReadUInt32();

                snapshotlinkReader.BaseStream.Position = 32;
                var snapCount = snapshotlinkReader.ReadUInt32();

                for (int i = 0; i < snapCount; i++)
                {
                    SnapshotHelpers.ReadSnaplinkDataInLink(snapshotlinkReader);

                    if (File.Exists(Path.Combine(snapshotDir, $"{Convert.ToString(SnapshotHelpers.SnapId).PadLeft(8, '0')}.ss")))
                    {
                        SnapshotHelpers.PackSnaplinkDataToDict(snaplinksDataDict);
                    }
                }

                nextSnapId = snapshotlinkReader.ReadUInt32();
                snapshotlinkReader.BaseStream.Position += 8;

                SnapshotHelpers.FooterOperations(snapshotlinkReader);
            }

            var updatedSnapshotlinkData = SnapshotHelpers.BuildUpdatedlinksData(snaplinksDataDict, nextSnapId);
            SnapshotHelpers.ResetVariables();

            var outSnapshotlinkData = Encrypt.BeginEncryption(updatedSnapshotlinkData);
            File.Delete(snapshotlinkFile);
            File.WriteAllBytes(snapshotlinkFile, outSnapshotlinkData);
        }
    }
}