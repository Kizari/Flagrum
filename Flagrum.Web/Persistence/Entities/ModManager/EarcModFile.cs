using Flagrum.Core.Archive;

namespace Flagrum.Web.Persistence.Entities.ModManager;

public class EarcModFile
{
    public int Id { get; set; }

    public int EarcModEarcId { get; set; }
    public EarcModEarc EarcModEarc { get; set; }

    public string Uri { get; set; }
    public string ReplacementFilePath { get; set; }
    public EarcFileChangeType Type { get; set; }
    public long FileLastModified { get; set; }

    /// <summary>
    /// Only applies to Add type
    /// </summary>
    public ArchiveFileFlag Flags { get; set; }
}