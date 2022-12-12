using System.Collections.Generic;
using Flagrum.Core.Gfxbin.Gmdl;

namespace Flagrum.Core.Archive;

public class UriExtensionMap
{
    private readonly Dictionary<string, string[]> _map = new()
    {
        {"aiia.ref", new[] {"aiia.xml"}},
        {"btex", new[] {"tif", "dds", "exr", "tga", "png"}},
        {"ccb", new[] {"ccf"}},
        {"exml", new[] {"ebex", "prefab"}},
        {"folgbin", new[] {"folg"}},
        {"gmdl.gfxbin", new[] {"gmdl"}},
        {"gmtl.gfxbin", new[] {"gmtl"}},
        {"nav_ref", new[] {"nav_context"}},
        {"nav_world_map", new[] {"nav_world"}},
        {"swfb", new[] {"swf"}},
        {"tcophysx", new[] {"tco"}},
        {"tmsbin", new[] {"tms"}},
        {"tpdbin", new[] {"tpd"}},
        {"win.mab", new[] {"max"}},
        {"win.sab", new[] {"sax"}},
        {"wth2b", new[] {"wth2"}}
    };

    public string[] this[string uriExtension] =>
        _map.TryGetValue(uriExtension, out var extension) ? extension : new[] {uriExtension};
}

public class RelativeExtensionMap
{
    private readonly Dictionary<string, string> _map = new()
    {
        {"aiia.xml", "aiia.ref"},
        {"tif", "btex"},
        {"dds", "btex"},
        {"exr", "btex"},
        {"tga", "btex"},
        {"png", "btex"},
        {"ccf", "ccb"},
        {"ebex", "exml"},
        {"prefab", "exml"},
        {"folg", "folgbin"},
        {"gmdl", "gmdl.gfxbin"},
        {"gmtl", "gmtl.gfxbin"},
        {"nav_context", "nav_ref"},
        {"nav_world", "nav_world_map"},
        {"swf", "swfb"},
        {"tco", "tcophysx"},
        {"tms", "tmsbin"},
        {"tpd", "tpdbin"},
        {"max", "win.mab"},
        {"sax", "win.sab"},
        {"wth2", "wth2b"}
    };

    public string this[string uriExtension]
    {
        get
        {
            uriExtension = uriExtension
                .Replace("high.", "")
                .Replace("low.", "")
                .Replace("color.", "")
                .Replace("mask.", "")
                .Replace("5.", "")
                .Replace("aiia_erudition.", "")
                .Replace("tif.", "")
                .Replace("tif_$h.", "")
                .Replace("dds.", "");

            return _map.TryGetValue(uriExtension, out var extension) ? extension : uriExtension;
        }
    }
}

public static class ArchiveHelper
{
    public static string[] RelativeExtensions => new[]
    {
        "2dcol",
        "acd",
        "aiia",
        "aiia.dbg",
        "aiia.ref",
        "amdl",
        "ani",
        "anmgph",
        "apx",
        "autoext",
        "bcfg",
        "blos",
        "bmdl",
        "bnm",
        "bnmwnd",
        "bod",
        "btex",
        "ccb",
        "clsn",
        "clsx",
        "cmdl",
        "dat",
        "db3",
        "dml",
        "dx11.fntbin",
        "dyn",
        "elx",
        "exml",
        "folgbin",
        "gmdl.gfxbin",
        "gmtl.gfxbin",
        "gpubin",
        "ies",
        "ikts",
        "irr",
        "kab",
        "kdi",
        "lik",
        "listb",
        "lnkani",
        "lsd",
        "mid",
        "nav",
        "nav_cell_ref",
        "nav_ref",
        "nav_waypoint",
        "nav_world_map",
        "nav_world_splitter",
        "pka",
        "pngbin",
        "prt",
        "prtd",
        "prtx",
        "ps.sb",
        "rag",
        "sbmdl",
        "ssd",
        "style",
        "swfb",
        "tcd",
        "tcm",
        "tcophysx",
        "tmsbin",
        "tnav",
        "tpd",
        "tpdbin",
        "tspack",
        "txt",
        "uam",
        "umbra",
        "ups",
        "vegy",
        "vfol",
        "vfuncs",
        "vfx",
        "vhlist",
        "vlink",
        "vs.sb",
        "win.config",
        "win.mab",
        "win.sab",
        "win32.bin",
        "win32.bins",
        "win32.msgbin",
        "wth2b",
        "xml"
    };
    
    public static string[] UriExtensions => new[]
    {
        "2dcol",
        "acd",
        "aiia", 
        "amdl", 
        "ani",
        "anmgph", 
        "apx", 
        "autoext",
        "bcfg",
        "blos",
        "bmdl", 
        "bnm", 
        "bnmwnd",
        "bod",
        "btex",
        "ccf", 
        "clsn", 
        "clsx",
        "cmdl",
        "dat", 
        "db3",
        "dbg",
        "dds",
        "dml",
        "dyn",
        "ebex", 
        "elx",
        "exr", 
        "fntbin",
        "folg", 
        "gmdl", 
        "gmtl", 
        "gpubin",
        "ies", 
        "ikts", 
        "irr", 
        "kab",
        "kdi",
        "lik", 
        "listb", 
        "lnkani", 
        "lsd",
        "max", 
        "mid", 
        "nav",
        "nav_cell_ref",
        "nav_context", 
        "nav_waypoint",
        "nav_world",
        "nav_world_splitter",
        "pka", 
        "png",
        "pngbin", 
        "prefab", 
        "prt", 
        "prtd", 
        "prtx", 
        "ps.sb",
        "rag",
        "sax",
        "sbmdl", 
        "ssd", 
        "style",
        "swf",
        "tcd",
        "tcm", 
        "tco", 
        "tga", 
        "tif",
        "tms", 
        "tnav", 
        "tpd", 
        "tspack",
        "txt", 
        "uam", 
        "umbra", 
        "ups", 
        "vegy", 
        "vfol", 
        "vfuncs", 
        "vfx", 
        "vhlist",
        "vlink",
        "vs.sb",
        "win.config",
        "win32.bin",
        "win32.bins",
        "win32.msgbin",
        "wth2",
        "xml"
    };
}