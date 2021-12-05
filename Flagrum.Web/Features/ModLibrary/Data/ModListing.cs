namespace Flagrum.Web.Features.ModLibrary.Data;

public class ModListing
{
    public string DisplayName { get; set; }
    public string FilePath { get; set; }
    public bool IsEnabled { get; set; }
    public string Uuid { get; set; }
    public string PreviewBase64 { get; set; }
}