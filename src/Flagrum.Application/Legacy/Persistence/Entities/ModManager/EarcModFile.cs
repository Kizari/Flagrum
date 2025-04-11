using Flagrum.Abstractions.Archive;
using Flagrum.Application.Features.ModManager.Mod;

namespace Flagrum.Application.Persistence.Entities.ModManager;

public class EarcModFile
{
    public int Id { get; set; }

    public int EarcModEarcId { get; set; }
    public EarcModEarc EarcModEarc { get; set; }

    public string Uri { get; set; }
    public string ReplacementFilePath { get; set; }
    public LegacyModBuildInstruction Type { get; set; }
    public long FileLastModified { get; set; }

    /// <summary>
    /// Only applies to Add type
    /// </summary>
    public EbonyArchiveFileFlags Flags { get; set; }
}