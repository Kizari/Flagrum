using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Flagrum.Abstractions;
using Flagrum.Abstractions.Utilities;

namespace Flagrum.Core.Archive;

// TODO: This whole file is a total mess, refactor all of this

/// <inheritdoc />
public class UriHelper : IUriHelper
{
    private readonly RelativeExtensionMap _relativeExtensionMap = new();

    private readonly Dictionary<string, string> _uriToPathExtensionMap = new()
    {
        {"1024.heb", "1024.heb"},
        {"256.heb", "256.heb"},
        {"2dcol", "2dcol"},
        {"5.ccf", "5.ccb"},
        {"512.heb", "512.heb"},
        {"acd", "acd"},
        {"aiia", "aiia"},
        {"aiia_erudition.xml", "aiia_erudition.xml"},
        {"aiia.dbg", "aiia.dbg"},
        {"aiia.xml", "aiia.ref"},
        {"amdl", "amdl"},
        {"ani", "ani"},
        {"anmgph", "anmgph"},
        {"apx", "apx"},
        {"autoext", "autoext"},
        {"bk2", "bk2"},
        {"blos", "blos"},
        {"bmdl", "bmdl"},
        {"bnm", "bnm"},
        {"bnmwnd", "bnmwnd"},
        {"bod", "bod"},
        {"btex", "btex"},
        {"dds", "btex"},
        {"exr", "btex"},
        {"png", "btex"},
        {"png.copy", "png"},
        {"tga", "btex"},
        {"tif", "btex"},
        {"ccf", "ccb"},
        {"clsn", "clsn"},
        {"clsx", "clsx"},
        {"cmdl", "cmdl"},
        {"color.dds", "color.btex"},
        {"color.dds.bcfg", "color.dds.bcfg"},
        {"dat", "dat"},
        {"db3", "db3"},
        {"dds.bcfg", "dds.bcfg"},
        {"dds.ebex", "dds.earc"},
        {"dds.stpk", "dds.earc"},
        {"dml", "dml"},
        {"dx11.fntbin", "dx11.fntbin"},
        {"dx11.vfxbin", "dx11.vfxbin"},
        {"dyn", "dyn"},
        {"elbin", "elbin" },
        {"ebex@", "earc"},
        {"ebex@.earcref", "earc"},
        {"htpk", "earc"},
        {"prefab@", "earc"},
        {"prefab@.earcref", "earc"},
        {"elx", "elx"},
        {"ebex", "exml"},
        {"prefab", "exml"},
        {"folg", "folgbin"},
        {"gmdl", "gmdl.gfxbin"},
        {"gmtl", "gmtl.gfxbin"},
        {"gpubin", "gpubin"},
        {"heb", "heb"},
        {"hephysx", "hephysx"},
        {"high.folg", "high.folgbin"},
        {"ies", "ies"},
        {"ikts", "ikts"},
        {"irr", "irr"},
        {"kab", "kab"},
        {"kdi", "kdi"},
        {"lik", "lik"},
        {"listb", "listb"},
        {"lnkani", "lnkani"},
        {"low.tms", "low.tmsbin"},
        {"lsd", "lsd"},
        {"mask.dds", "mask.btex"},
        {"mask.dds.bcfg", "mask.dds.bcfg"},
        {"mid", "mid"},
        {"nav", "nav"},
        {"nav_ref", "nav_ref"},
        {"nav_cell_ref", "nav_cell_ref"},
        {"nav_context", "nav_ref"},
        {"nav_waypoint", "nav_waypoint"},
        {"nav_world", "nav_world_map"},
        {"nav_world_splitter", "nav_world_splitter"},
        {"orb.config", "orb.config"},
        {"orb.fntbin", "orb.fntbin"},
        {"pka", "pka"},
        {"pngbin", "pngbin"},
        {"prt", "prt"},
        {"prtd", "prtd"},
        {"prtx", "prtx"},
        {"ps.sb", "ps.sb"},
        {"rag", "rag"},
        {"sax ", "sax"},
        {"sbmdl", "sbmdl"},
        {"style", "style"},
        {"ssd", "ssd"},
        {"swf", "swfb"},
        {"tcd", "tcd"},
        {"tcm", "tcm"},
        {"tco", "tcophysx"},
        {"tif_$h.tif", "tif_$h.btex"},
        {"tif.tif", "tif.btex"},
        {"tms", "tmsbin"},
        {"tnav", "tnav"},
        {"tpd", "tpdbin"},
        {"tmpdbin", "tmpdbin"},
        {"tspack", "tspack"},
        {"txt", "txt"},
        {"uam", "uam"},
        {"umbra", "umbra"},
        {"ups", "ups"},
        {"vegy", "vegy"},
        {"vfol", "vfol"},
        {"vfuncs", "vfuncs"},
        {"vfx", "vfx"},
        {"vhlist", "vhlist"},
        {"vlink", "vlink"},
        {"vs.sb", "vs.sb"},
        {"win.config", "win.config"},
        {"max", "win.mab"},
        {"sax", "win.sab"},
        {"win32.bin", "win32.bin"},
        {"win32.bins", "win32.bins"},
        {"win32.msgbin", "win32.msgbin"},
        {"wth2", "wth2b"},
        {"wthcl", "wthcl"},
        {"wthex", "wthex"},
        {"wthpe", "wthpe"},
        {"wthsky", "wthsky"},
        {"xml", "xml"}
    };

    public static UriHelper Instance { get; } = new();

    public string ReplaceFileNameExtensionWithTrueExtension(string path)
    {
        var tokens = path.Split('.');
        var extension = string.Join('.', tokens[^(tokens.Length > 2 ? 2 : 1)..]).Trim();
        var trueExtension = _relativeExtensionMap.GetWithKeptFakeExtension(extension);
        return path.Replace('.' + extension, '.' + trueExtension);
    }

    public string GetTrueExtensionFromFileName(string fileName)
    {
        var tokens = fileName.Replace("..", ".").Split('.');
        var extension = string.Join('.', tokens[^(tokens.Length > 2 ? 2 : 1)..]).Trim();
        return _relativeExtensionMap[extension];
    }

    /// <summary>
    /// Infers the original file extension of an asset based on its URI.
    /// </summary>
    /// <param name="uri">The URI of the asset to evaluate.</param>
    /// <returns>The true file extension.</returns>
    public string InferExtensionFromUri(string uri)
    {
        var extension = uri[(uri.IndexOf('.') + 1)..];
        return _uriToPathExtensionMap.GetValueOrDefault(extension, extension);
    }
}

public partial class RelativeExtensionMap
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
        {"png.copy", "png"},
        {"ccf", "ccb"},
        {"ebex", "exml"},
        {"ebex@", "earc"},
        {"ebex@.earcref", "earc"},
        {"htpk", "earc"},
        {"prefab", "exml"},
        {"prefab@", "earc"},
        {"prefab@.earcref", "earc"},
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
        {"wthcl", "wthclb"},
        {"wthex", "wthexb"},
        {"wthpe", "wthpeb"},
        {"wthsky", "wthskyb"},
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
            uriExtension = FakeExtensionRegex().Replace(uriExtension, "");
            return _map.GetValueOrDefault(uriExtension, uriExtension);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetValueOrDefault(string key, string defaultValue) => _map.GetValueOrDefault(key, defaultValue);

    public string GetWithKeptFakeExtension(string uriExtension)
    {
        var matches = FakeExtensionRegex().Matches(uriExtension);
        uriExtension = FakeExtensionRegex().Replace(uriExtension, "");
        var extension = _map.GetValueOrDefault(uriExtension, uriExtension);
        return string.Join('.', matches.Select(m => m.Value[..^1]).Union([extension]));
    }

    [GeneratedRegex(
        @"high\.|low\.|color\.|mask\.|5\.|aiia_erudition\.|tif\.|tif_\$h\.|dds\.|1024\.|2048\.|4096\.|512\.|256\.|r\.|dummy\.")]
    private static partial Regex FakeExtensionRegex();
}

public static class ArchiveHelper
{
    public static string[] RelativeExtensions(LuminousGame type)
    {
        if (type == LuminousGame.FFXV)
        {
            return
            [
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
                "bk2",
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
                "dx11.vfxbin",
                "dyn",
                "earc",
                "elbin",
                "elx",
                "exml",
                "folgbin",
                "gmdl.gfxbin",
                "gmtl.gfxbin",
                "gpubin",
                "heb",
                "hephysx",
                "ies",
                "ikts",
                "irr",
                "kab",
                "kdi",
                "lik",
                "listb",
                "lnkani",
                "lsd",
                "mab",
                "mid",
                "nav",
                "nav_cell_ref",
                "nav_ref",
                "nav_waypoint",
                "nav_world_map",
                "nav_world_splitter",
                "orb.config",
                "orb.fntbin",
                "orb.mab",
                "orb.sab",
                "pfp",
                "pka",
                "png",
                "pngbin",
                "prt",
                "prtd",
                "prtx",
                "ps.sb",
                "rag",
                "sab",
                "sax",
                "sbmdl",
                "ssd",
                "stpk",
                "style",
                "swfb",
                "tcd",
                "tcm",
                "tcophysx",
                "tmpdbin",
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
                "wthcl",
                "wthex",
                "wthpe",
                "wthsky",
                "xml"
            ];
        }

        return new[]
        {
            "acd",
            "aig",
            "amdl",
            "ani",
            "anmgph",
            "apb",
            "atlas",
            "bin",
            "bk2",
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
            "emem",
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
            "prc_tex",
            "prmsk",
            "prtex",
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
            "win.mab",
            "win.sab",
            "wld_prc",
            "wlod",
            "wlodn",
            "wpcd",
            "wpcm",
            "wpcpbin",
            "wped",
            "wpvd"
        };
    }
}
