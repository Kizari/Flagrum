using Flagrum.Abstractions.ModManager.Project;

namespace Flagrum.Application.Persistence.Entities.ModManager;

public class EarcModLooseFile
{
    public int Id { get; set; }

    public int EarcModId { get; set; }
    public EarcMod EarcMod { get; set; }

    public string RelativePath { get; set; }
    public string FilePath { get; set; }
    public ModChangeType Type { get; set; }

    public long FileLastModified { get; set; }
}