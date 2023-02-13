using System.Collections.Generic;
using Flagrum.Core.Utilities.Types;

namespace Flagrum.Core.Archive;

public static class UriHelper
{
    private static readonly RelativeExtensionMap _relativeExtensionMap = new();

    public static string GetTrueExtensionFromFileName(string fileName)
    {
        var tokens = fileName.Split('.');
        var extension = string.Join('.', tokens[^(tokens.Length > 2 ? 2 : 1)..]).Trim();
        return _relativeExtensionMap[extension];
    }

    public static string ReplaceFileNameExtensionWithTrueExtension(string fileName)
    {
        var tokens = fileName.Split('.');
        var extension = string.Join('.', tokens[^(tokens.Length > 2 ? 2 : 1)..]).Trim();
        var trueExtension = _relativeExtensionMap[extension];
        return fileName.Replace(extension, trueExtension);
    }

    public static string RemoveExtensionFromFileName(string fileName)
    {
        var tokens = fileName.Split('.');
        var extension = string.Join('.', tokens[^(tokens.Length > 2 ? 2 : 1)..]).Trim();
        return fileName.Replace("." + extension, "");
    }
}

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
        {"alist@", "earc"},
        {"tif", "btex"},
        {"dds", "btex"},
        {"exr", "btex"},
        {"tga", "btex"},
        {"png", "btex"},
        {"ccf", "ccb"},
        {"ebex", "exml"},
        {"ebex@", "earc"},
        {"htpk", "earc"},
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
        {"wth2", "wth2b"},
        {"wpcp", "wpcpbin"},
        {"navalist@", "earc"},
        {"vfxlist@", "earc"},
        {"pka@", "earc"},
        {"anmgph@", "earc"},
        {"aig", "exml"}
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
                .Replace("dds.", "")
                .Replace("1024.", "")
                .Replace("2048.", "")
                .Replace("4096.", "")
                .Replace("512.", "")
                .Replace("256.", "")
                .Replace("r.", "");

            return _map.TryGetValue(uriExtension, out var extension) ? extension : uriExtension;
        }
    }
}

public static class ArchiveHelper
{
    public static string[] RelativeExtensions(LuminousGame type)
    {
        if (type == LuminousGame.FFXV)
        {
            return new[]
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
        }

        return new[]
        {
            "acd",
            "amdl",
            "ani",
            "anmgph",
            "apb",
            "atlas",
            "bin",
            "blast",
            "blastc",
            "bnm",
            "btex",
            "ccb",
            "ch.sb",
            "clsn",
            "cmdl",
            "customdata",
            "data",
            "earc",
            "elx",
            "empath",
            "erep",
            "exml",
            "file_list",
            "folgbin",
            "gmdl_hair",
            "gmdl.gfxbin",
            "gmtl.gfxbin",
            "gpubin",
            "heb",
            "hephysx",
            "id2s",
            "json",
            "kdi",
            "layer_info",
            "layer_pcd",
            "lipmap",
            "list",
            "lsd",
            "n3d2p_raw",
            "nav",
            "nav_cell_ref",
            "nav_connectivity",
            "nav_debug_single",
            "nav_edgelinks",
            "nav_smp",
            "nav_world_deps",
            "nav_world_map",
            "navalist",
            "navcelllist",
            "parambin",
            "pka",
            "pkr",
            "pmdl",
            "ps.sb",
            "psocache",
            "ragdoll",
            "res_info",
            "sapb",
            "sbd",
            "tpdbin",
            "tspack",
            "txt",
            "uifn",
            "uip",
            "vegy",
            "vfx",
            "vfxlist",
            "vs.sb",
            "win.config",
            "win.sab",
            "wld_prc",
            "wlod",
            "wlodn",
            "wpcm",
            "wpcpbin",
            "wped",
            "wpvd"
        };
    }
}