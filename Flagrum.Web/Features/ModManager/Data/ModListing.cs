namespace Flagrum.Web.Features.ModManager.Data;

public class ModListing
{
    public string DisplayName { get; set; }
    public string FilePath { get; set; }
    public bool IsEnabled { get; set; }
    public string Uuid { get; set; }
}