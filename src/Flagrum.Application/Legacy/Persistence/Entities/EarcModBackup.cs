using System.ComponentModel.DataAnnotations;
using Flagrum.Abstractions.Archive;

namespace Flagrum.Application.Persistence.Entities;

public class EarcModBackup
{
    [Key] public string Uri { get; set; }

    public string RelativePath { get; set; }
    public uint Size { get; set; }
    public EbonyArchiveFileFlags Flags { get; set; }
    public byte LocalizationType { get; set; }
    public byte Locale { get; set; }
    public ushort Key { get; set; }
}