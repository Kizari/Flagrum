namespace Flagrum.Web.Persistence.Entities.ModManager;

public class EarcFileData
{
    public bool IsCompressed { get; set; }
    public bool IsEncrypted { get; set; }
    public byte[] Data { get; set; }
}