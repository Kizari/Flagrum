using System.Collections.Generic;
using System.Linq;
using Flagrum.Abstractions;
using Flagrum.Core.Archive;

namespace Flagrum.Application.Features.AssetExplorer.Data;

public enum ExportContextPreset
{
    None,
    Textures,
    Scripts,
    Models
}

public class ExportContext
{
    public ExportContext(ExportContextPreset preset, LuminousGame type)
    {
        TextureFormats = new Dictionary<string, bool>
        {
            {"png", true},
            {"tga", false},
            {"dds", false},
            {"btex", false}
        };

        TerrainTextureFormats = new Dictionary<string, bool>
        {
            {"png", false},
            {"tga", true},
            {"dds", false},
            {"heb", false}
        };

        ScriptFormats = new Dictionary<string, bool>
        {
            {"xml", true},
            {"exml", false}
        };

        AnimationPackFormats = new Dictionary<string, bool>
        {
            {"ani", true},
            {"pka", false}
        };

        ExportFormats = preset switch
        {
            ExportContextPreset.Scripts => ArchiveHelper.RelativeExtensions(type).ToDictionary(e => e,
                e => e is "exml"),
            ExportContextPreset.Models => ArchiveHelper.RelativeExtensions(type).ToDictionary(e => e,
                e => e is "btex" or "gmdl.gfxbin" or "gpubin" or "gmtl.gfxbin" or "amdl"),
            _ => ArchiveHelper.RelativeExtensions(type).ToDictionary(e => e,
                e => e == "btex")
        };
    }

    public bool Recursive { get; set; }
    public Dictionary<string, bool> TextureFormats { get; set; }
    public Dictionary<string, bool> TerrainTextureFormats { get; set; }
    public Dictionary<string, bool> ScriptFormats { get; set; }
    public Dictionary<string, bool> ExportFormats { get; set; }
    public Dictionary<string, bool> AnimationPackFormats { get; set; }
}