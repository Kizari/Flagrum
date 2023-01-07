namespace Flagrum.Web.Persistence.Entities.ModManager;

public class EarcModLooseFile
{
    public int Id { get; set; }

    public string EarcModId { get; set; }
    public EarcMod EarcMod { get; set; }

    public string RelativePath { get; set; }
    public string FilePath { get; set; }
    public EarcChangeType Type { get; set; }

    public long FileLastModified { get; set; }
}