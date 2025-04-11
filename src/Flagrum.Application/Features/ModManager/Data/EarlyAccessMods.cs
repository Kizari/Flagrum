using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Flagrum.Application.Features.ModManager.Data;

public enum OfficialMod
{
    MoogleChocoboCarnivalEarlyAccess,
    MoogleChocoboCarnival
}

public static class EarlyAccessMods
{
    public static Dictionary<Guid, OfficialModData> GuidMap { get; } = new()
    {
        {
            new Guid(OfficialMods.MoogleChocoboCarnivalEarlyAccess),
            new OfficialModData
            {
                Id = OfficialMod.MoogleChocoboCarnivalEarlyAccess,
                Guid = new Guid(OfficialMods.MoogleChocoboCarnivalEarlyAccess),
                Key = new byte[]
                {
                    163, 9, 118, 76, 6, 154, 50, 226, 156, 22, 122, 222, 64, 108, 169, 60, 63, 172, 175, 173, 132, 173,
                    38, 72, 29, 246, 112, 131, 123, 231, 155, 154
                },
                IV = new byte[] {129, 40, 132, 16, 247, 66, 177, 82, 89, 72, 158, 62, 221, 98, 128, 34},
                Checksum = new byte[]
                {
                    104, 253, 0, 103, 134, 175, 87, 115, 31, 177, 106, 156, 155, 128, 64, 84, 193, 59, 127, 117, 39, 70,
                    72, 137, 149, 144, 3, 18, 24, 55, 202, 134
                }
            }
        }
    };

    public static byte[] CalculateChecksum(string fmodFilePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = new FileStream(fmodFilePath, FileMode.Open, FileAccess.Read);
        return sha256.ComputeHash(stream);
    }
}

public static class OfficialMods
{
    public const string MoogleChocoboCarnivalEarlyAccess = "2873c355-c68b-45c2-8b98-eb69dc13d678";
    public const string MoogleChocoboCarnival = "c822c5a5-5b58-4994-a6a8-e66ebe3fb602";

    public static List<Guid> Guids = new()
    {
        new Guid(MoogleChocoboCarnivalEarlyAccess),
        new Guid(MoogleChocoboCarnival)
    };
}

public class OfficialModData
{
    public OfficialMod Id { get; set; }
    public Guid Guid { get; set; }
    public byte[] Key { get; set; }
    public byte[] IV { get; set; }
    public byte[] Checksum { get; set; }
}