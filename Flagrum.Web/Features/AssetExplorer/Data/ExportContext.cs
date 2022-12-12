using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Archive;

namespace Flagrum.Web.Features.AssetExplorer.Data;

public enum ExportContextPreset
{
    None,
    Textures,
    Scripts,
    Models
}

public class ExportContext
{
    public bool Recursive { get; set; }
    public Dictionary<string, bool> TextureFormats { get; set; }
    public Dictionary<string, bool> ScriptFormats { get; set; }
    public Dictionary<string, bool> ExportFormats { get; set; }
    
    public ExportContext(ExportContextPreset preset)
    {
        TextureFormats = new Dictionary<string, bool>
        {
            {"png", true},
            {"tga", false},
            {"dds", false},
            {"btex", false}
        };
        
        ScriptFormats = new Dictionary<string, bool>
        {
            {"xml", true},
            {"exml", false}
        };
        
        ExportFormats = preset switch
        {
            ExportContextPreset.Scripts => ArchiveHelper.RelativeExtensions.ToDictionary(e => e,
                e => e is "exml"),
            ExportContextPreset.Models => ArchiveHelper.RelativeExtensions.ToDictionary(e => e,
                e => e is "btex" or "gmdl.gfxbin" or "gpubin" or "gmtl.gfxbin" or "amdl"),
            _ or ExportContextPreset.Textures => ArchiveHelper.RelativeExtensions.ToDictionary(e => e,
                e => e == "btex")
        };
    }
}