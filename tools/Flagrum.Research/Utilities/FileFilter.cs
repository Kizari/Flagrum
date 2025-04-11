using System.Collections.Generic;
using System.Linq;

namespace Flagrum.Research.Utilities;

/// <summary>
/// Flags that represents a list of Luminous file types.
/// </summary>
/// <remarks>
/// There are too many types to fit into a bitfield, so this faked flags type is used instead.
/// </remarks>
public class TypeFlags
{
    private readonly IEnumerable<uint> _types;

    /// <summary>
    /// Creates a new <see cref="TypeFlags"/> with the given flag.
    /// </summary>
    private TypeFlags(uint typeId)
    {
        _types = [typeId];
    }

    /// <summary>
    /// Creates a new <see cref="TypeFlags"/> with the given flags.
    /// </summary>
    /// <param name="types"></param>
    private TypeFlags(IEnumerable<uint> types)
    {
        _types = types.Distinct();
    }

    /// <summary>
    /// Creates a new <see cref="TypeFlags"/> that contains the types from both source objects.
    /// </summary>
    public static TypeFlags operator |(TypeFlags self, TypeFlags other)
    {
        return new TypeFlags(self._types.Union(other._types));
    }
    
    /// <inheritdoc cref="HasFlag(TypeFlags)"/>
    public bool HasFlag(uint flag)
    {
        return _types.Contains(flag);
    }

    /// <summary>
    /// Checks if the given flag exists in these flags.
    /// </summary>
    /// <param name="flag">The flag to check for.</param>
    /// <returns><c>true</c> if the flag is present.</returns>
    public bool HasFlag(TypeFlags flag)
    {
        return flag._types.All(t => _types.Contains(t));
    }
    
    public static TypeFlags HEB256 => new(0x1D37Du);
    public static TypeFlags HEB512 => new(0x608D4u);
    public static TypeFlags HEB1024 => new(0x66DBDu);
    public static TypeFlags HEB2048 => new(0x193F0u);
    public static TypeFlags HEB4096 => new(0xDE1ADu);
    public static TypeFlags CCB5 => new(0xC1010u);
    public static TypeFlags COL2D => new(0xA96B1u);
    public static TypeFlags ACD => new(0xCBC81u);
    public static TypeFlags AIG => new(0x99DCEu);
    public static TypeFlags AIIA => new(0x676E7u);
    public static TypeFlags AIIA_ERUDITION_XML => new(0xECC50u);
    public static TypeFlags AIIA_DBG => new(0xC18DEu);
    public static TypeFlags AIIA_REF => new(0xFD0B4u);
    public static TypeFlags ALIST => new(0x7A4B2u);
    public static TypeFlags AMDL => new(0xA5AB9u);
    public static TypeFlags ANI => new(0x4F423u);
    public static TypeFlags ANMGPH => new(0x56E70u);
    public static TypeFlags APB => new(0xDD5A0u);
    public static TypeFlags APX => new(0xE01CEu);
    public static TypeFlags ATLAS => new(0xA68CAu);
    public static TypeFlags AUTOEXT => new(0x5C91Bu);
    public static TypeFlags BCFG => new(0xCC79Fu);
    public static TypeFlags BIN => new(0xA31A0u);
    public static TypeFlags BINS => new(0x2A989u);
    public static TypeFlags BK2 => new(0x8D0D9u);
    public static TypeFlags BLAST => new(0x38FF3u);
    public static TypeFlags BLASTC => new(0xCF1B0u);
    public static TypeFlags BLOS => new(0x3B24Fu);
    public static TypeFlags BMDL => new(0x474C2u);
    public static TypeFlags BNM => new(0x28B3Eu);
    public static TypeFlags BNMWND => new(0x8F081u);
    public static TypeFlags BOD => new(0xEC2B0u);
    public static TypeFlags BTEX => new(0x259C8u);
    public static TypeFlags BTEXHEADER => new(0xA806Du);
    public static TypeFlags CCB => new(0xEB241u);
    public static TypeFlags CCF => new(0xEB90Du);
    public static TypeFlags CH_SB => new(0x9ACD3u);
    public static TypeFlags CLSN => new(0xA8C41u);
    public static TypeFlags CLSX => new(0xA7477u);
    public static TypeFlags CMDL => new(0x7B3BFu);
    public static TypeFlags COLOR_BTEX => new(0xFA8B7u);
    public static TypeFlags COLOR_DDS_BCFG => new(0xB925u);
    public static TypeFlags CONFIG => new(0x32601u);
    public static TypeFlags CUSTOMDATA => new(0x3D6D8u);
    public static TypeFlags DAT => new(0x38AC8u);
    public static TypeFlags DATA => new(0x49D2Bu);
    public static TypeFlags DB3 => new(0xD1642u);
    public static TypeFlags DBG => new(0xD6106u);
    public static TypeFlags DDS => new(0xB1960u);
    public static TypeFlags DDS_BCFG => new(0x80D26u);
    public static TypeFlags DEARC => new(0x80CF0u);
    public static TypeFlags DML => new(0xFD5BCu);
    public static TypeFlags DUMMY_TXT => new(0x393A3u);
    public static TypeFlags DX11_FNTBIN => new(0xE3850u);
    public static TypeFlags DYN => new(0x9A5EEu);
    public static TypeFlags EARC => new(0x2DCA6u);
    public static TypeFlags EARCREF => new(0x3CF07u);
    public static TypeFlags EBEX => new(0xDC0F5u);
    public static TypeFlags ELX => new(0xFB046u);
    public static TypeFlags EMEM => new(0x9276Fu);
    public static TypeFlags EMPATH => new(0xF072Eu);
    public static TypeFlags ENTTRAY => new(0xAA358u);
    public static TypeFlags EREP => new(0x2E1FDu);
    public static TypeFlags EXML => new(0xE5A3Fu);
    public static TypeFlags EXR => new(0x5FB44u);
    public static TypeFlags FILE_LIST => new(0x16D72u);
    public static TypeFlags FNTBIN => new(0xE8C2Eu);
    public static TypeFlags FOLG => new(0x190C9u);
    public static TypeFlags FOLGBIN => new(0xB0B32u);
    public static TypeFlags GMDL => new(0xEF4DBu);
    public static TypeFlags GMDL_HAIR => new(0xD1078u);
    public static TypeFlags GMDL_GFXBIN => new(0xA2343u);
    public static TypeFlags GMTL => new(0x12F0Bu);
    public static TypeFlags GMTL_GFXBIN => new(0xF2DB3u);
    public static TypeFlags GMTLA => new(0x3911Eu);
    public static TypeFlags GPUBIN => new(0xC5E90u);
    public static TypeFlags HEB => new(0xDD95Au);
    public static TypeFlags HEPHYSX => new(0x3108Cu);
    public static TypeFlags HIGH_FOLGBIN => new(0x6E602u);
    public static TypeFlags HTPK => new(0xC2CC2u);
    public static TypeFlags ID2S => new(0x586A5u);
    public static TypeFlags IES => new(0x70F36u);
    public static TypeFlags IKTS => new(0xEC2F6u);
    public static TypeFlags IRR => new(0xBC182u);
    public static TypeFlags JSON => new(0x44D11u);
    public static TypeFlags KAB => new(0x9473Bu);
    public static TypeFlags KDI => new(0x196A7u);
    public static TypeFlags LAYER_INFO => new(0x36D11u);
    public static TypeFlags LAYER_PCD => new(0x64FA4u);
    public static TypeFlags LIK => new(0xD9035u);
    public static TypeFlags LIPMAP => new(0x677CAu);
    public static TypeFlags LIST => new(0x571EBu);
    public static TypeFlags LISTB => new(0xFEBCBu);
    public static TypeFlags LNKANI => new(0x37D88u);
    public static TypeFlags LOW_TMSBIN => new(0xEF836u);
    public static TypeFlags LSD => new(0xB064Au);
    public static TypeFlags MASK_BTEX => new(0xC830u);
    public static TypeFlags MASK_DDS_BCFG => new(0x41EEu);
    public static TypeFlags MAX => new(0x7A08Bu);
    public static TypeFlags MGPUBIN => new(0xCBFD9u);
    public static TypeFlags MID => new(0x10867u);
    public static TypeFlags MSGBIN => new(0xADB8Du);
    public static TypeFlags N3D2P_RAW => new(0xE959Fu);
    public static TypeFlags NAV => new(0xB9C3Cu);
    public static TypeFlags NAV_CELL_REF => new(0xDB3C3u);
    public static TypeFlags NAV_CONNECTIVITY => new(0xC0A4Eu);
    public static TypeFlags NAV_CONTEXT => new(0x9A9A0u);
    public static TypeFlags NAV_DEBUG_SINGLE => new(0x6DB15u);
    public static TypeFlags NAV_EDGELINKS => new(0x6F5C9u);
    public static TypeFlags NAV_REF => new(0xE03AEu);
    public static TypeFlags NAV_SMP => new(0x921Bu);
    public static TypeFlags NAV_WAYPOINT => new(0xCD7CEu);
    public static TypeFlags NAV_WORLD => new(0x2ECBFu);
    public static TypeFlags NAV_WORLD_DEPS => new(0x4E016u);
    public static TypeFlags NAV_WORLD_MAP => new(0x50B76u);
    public static TypeFlags NAV_WORLD_SPLITTER => new(0x2C7CBu);
    public static TypeFlags NAVALIST => new(0x85D6Fu);
    public static TypeFlags NAVCELLLIST => new(0x216F8u);
    public static TypeFlags PARAMBIN => new(0x719C9u);
    public static TypeFlags PKA => new(0x7B665u);
    public static TypeFlags PKR => new(0x7CC7Cu);
    public static TypeFlags PMDL => new(0x49B7Cu);
    public static TypeFlags PNG => new(0x61F66u);
    public static TypeFlags PNGBIN => new(0x7C643u);
    public static TypeFlags PREFAB => new(0x63A1Du);
    public static TypeFlags PRT => new(0x5AFu);
    public static TypeFlags PRTD => new(0x9D7F1u);
    public static TypeFlags PRTX => new(0x9EC55u);
    public static TypeFlags PS_SB => new(0xE9D31u);
    public static TypeFlags PSOCACHE => new(0xB8247u);
    public static TypeFlags R_BTEX => new(0xE101Cu);
    public static TypeFlags RAG => new(0x4A6D3u);
    public static TypeFlags RAGDOLL => new(0x6473Eu);
    public static TypeFlags RAYGMTL => new(0x928D1u);
    public static TypeFlags RES_INFO => new(0x76A58u);
    public static TypeFlags SAPB => new(0xDAB71u);
    public static TypeFlags SAX => new(0x31C1u);
    public static TypeFlags SAX_SPACE => new(0x4C153u);
    public static TypeFlags SB => new(0x33E76u);
    public static TypeFlags SBD => new(0x27896u);
    public static TypeFlags SBMDL => new(0xBEBC9u);
    public static TypeFlags SSD => new(0x736C7u);
    public static TypeFlags STYLE => new(0xE1934u);
    public static TypeFlags SWF => new(0x325F9u);
    public static TypeFlags SWFB => new(0x8E661u);
    public static TypeFlags TCD => new(0x13D72u);
    public static TypeFlags TCM => new(0x14CBDu);
    public static TypeFlags TCO => new(0x14957u);
    public static TypeFlags TCOPHYSX => new(0xFD979u);
    public static TypeFlags TGA => new(0x53EF5u);
    public static TypeFlags TIF => new(0x21976u);
    public static TypeFlags TIFH_BTEX => new(0xAF7B8u);
    public static TypeFlags TIF_BTEX => new(0x5591Bu);
    public static TypeFlags TMS => new(0x78A01u);
    public static TypeFlags TMSBIN => new(0xE877Au);
    public static TypeFlags TNAV => new(0x4E142u);
    public static TypeFlags TPD => new(0x1A21u);
    public static TypeFlags TPDBIN => new(0xF5E1Au);
    public static TypeFlags TSPACK => new(0x7F1BBu);
    public static TypeFlags TXT => new(0x6AB79u);
    public static TypeFlags UAM => new(0x1F9BCu);
    public static TypeFlags UIFN => new(0x1AF61u);
    public static TypeFlags UIP => new(0x89393u);
    public static TypeFlags UMBRA => new(0xF6084u);
    public static TypeFlags UPS => new(0xD34BFu);
    public static TypeFlags VEGY => new(0xD2546u);
    public static TypeFlags VFOL => new(0x91A20u);
    public static TypeFlags VFUNCS => new(0x98B7Au);
    public static TypeFlags VFX => new(0x44689u);
    public static TypeFlags VFXLIST => new(0xE23B5u);
    public static TypeFlags VHLIST => new(0x2761u);
    public static TypeFlags VLINK => new(0x81523u);
    public static TypeFlags VS_SB => new(0x3DFu);
    public static TypeFlags WIN_CONFIG => new(0xC7BC5u);
    public static TypeFlags WIN_MAB => new(0xCBF7Du);
    public static TypeFlags WIN_SAB => new(0xDF19Fu);
    public static TypeFlags WIN32_BIN => new(0x6B501u);
    public static TypeFlags WIN32_BINS => new(0x650B6u);
    public static TypeFlags WIN32_MSGBIN => new(0xA8B22u);
    public static TypeFlags WLD_PRC => new(0x58D48u);
    public static TypeFlags WLOD => new(0x983EBu);
    public static TypeFlags WLODN => new(0xA7AFFu);
    public static TypeFlags WPCM => new(0x883B8u);
    public static TypeFlags WPCP => new(0x899CFu);
    public static TypeFlags WPCPBIN => new(0x1A5FCu);
    public static TypeFlags WPED => new(0x658F5u);
    public static TypeFlags WPVD => new(0x7CDD6u);
    public static TypeFlags WTH2 => new(0xF05A6u);
    public static TypeFlags WTH2B => new(0x6CC0Cu);
    public static TypeFlags XML => new(0x74EB8u);

    public static TypeFlags None => new([]);
    public static TypeFlags Audio => SAX | MAX | WIN_SAB | WIN_MAB;
    
    public static TypeFlags Mesh => GMDL | GMDL_HAIR | GMDL_GFXBIN | GPUBIN;
    public static TypeFlags Material => GMTL | GMTL_GFXBIN | GMTLA | RAYGMTL | MGPUBIN;
    public static TypeFlags Shader => PS_SB | CH_SB | VS_SB;

    public static TypeFlags Texture => HEB | HEB256 | HEB512 | HEB1024 | HEB2048 | HEB4096
                                       | PNG | PNGBIN | DDS | EXR | TIF | BTEX | TIF_BTEX | TIFH_BTEX | DDS_BCFG
                                       | MASK_DDS_BCFG | COLOR_DDS_BCFG | R_BTEX | BTEXHEADER | MASK_BTEX;

    public static TypeFlags Model => Mesh | Material | Shader | Texture;
    public static TypeFlags Script => EBEX | EXML | PREFAB;

    /// <summary>
    /// A preset blacklist that excludes textures, audio, and video.
    /// </summary>
    public static TypeFlags BlacklistRelaxed => Texture | Audio | BK2;
    
    /// <summary>
    /// A preset blacklist that excludes textures, audio, video, and models.
    /// </summary>
    public static TypeFlags BlacklistStrict => BlacklistRelaxed | Model;
    
    /// <summary>
    /// A preset blacklist that excludes textures, audio, video, models, and scripts/prefabs.
    /// </summary>
    public static TypeFlags BlacklistDefault => BlacklistStrict | Script;

    private static Dictionary<uint, string> _nameTable { get; set; } = new()
    {
        {0x66DBDu, "1024.heb"},
        {0x193F0u, "2048.heb"},
        {0x1D37Du, "256.heb"},
        {0xA96B1u, "2dcol"},
        {0xDE1ADu, "4096.heb"},
        {0xC1010u, "5.ccb"},
        {0x608D4u, "512.heb"},
        {0xCBC81u, "acd"},
        {0x99DCEu, "aig"},
        {0x676E7u, "aiia"},
        {0xECC50u, "aiia_erudition.xml"},
        {0xC18DEu, "aiia.dbg"},
        {0xFD0B4u, "aiia.ref"},
        {0x7A4B2u, "alist"},
        {0xA5AB9u, "amdl"},
        {0x4F423u, "ani"},
        {0x56E70u, "anmgph"},
        {0xDD5A0u, "apb"},
        {0xE01CEu, "apx"},
        {0xA68CAu, "atlas"},
        {0x5C91Bu, "autoext"},
        {0xCC79Fu, "bcfg"},
        {0xA31A0u, "bin"},
        {0x2A989u, "bins"},
        {0x38FF3u, "blast"},
        {0xCF1B0u, "blastc"},
        {0x3B24Fu, "blos"},
        {0x474C2u, "bmdl"},
        {0x28B3Eu, "bnm"},
        {0x8F081u, "bnmwnd"},
        {0xEC2B0u, "bod"},
        {0x259C8u, "btex"},
        {0xA806Du, "btexheader"},
        {0xEB241u, "ccb"},
        {0xEB90Du, "ccf"},
        {0x9ACD3u, "ch.sb"},
        {0xA8C41u, "clsn"},
        {0xA7477u, "clsx"},
        {0x7B3BFu, "cmdl"},
        {0xFA8B7u, "color.btex"},
        {0xB925u, "color.dds.bcfg"},
        {0x32601u, "config"},
        {0x3D6D8u, "customdata"},
        {0x38AC8u, "dat"},
        {0x49D2Bu, "data"},
        {0xD1642u, "db3"},
        {0xD6106u, "dbg"},
        {0xB1960u, "dds"},
        {0x80D26u, "dds.bcfg"},
        {0x80CF0u, "dearc"},
        {0xFD5BCu, "dml"},
        {0x393A3u, "dummy.txt"},
        {0xE3850u, "dx11.fntbin"},
        {0x9A5EEu, "dyn"},
        {0x2DCA6u, "earc"},
        {0x3CF07u, "earcref"},
        {0xDC0F5u, "ebex"},
        {0xFB046u, "elx"},
        {0x9276Fu, "emem"},
        {0xF072Eu, "empath"},
        {0xAA358u, "enttray"},
        {0x2E1FDu, "erep"},
        {0xE5A3Fu, "exml"},
        {0x5FB44u, "exr"},
        {0x16D72u, "file_list"},
        {0xE8C2Eu, "fntbin"},
        {0x190C9u, "folg"},
        {0xB0B32u, "folgbin"},
        {0xEF4DBu, "gmdl"},
        {0xD1078u, "gmdl_hair"},
        {0xA2343u, "gmdl.gfxbin"},
        {0x12F0Bu, "gmtl"},
        {0xF2DB3u, "gmtl.gfxbin"},
        {0x3911Eu, "gmtla"},
        {0xC5E90u, "gpubin"},
        {0xDD95Au, "heb"},
        {0x3108Cu, "hephysx"},
        {0x6E602u, "high.folgbin"},
        {0xC2CC2u, "htpk"},
        {0x586A5u, "id2s"},
        {0x70F36u, "ies"},
        {0xEC2F6u, "ikts"},
        {0xBC182u, "irr"},
        {0x44D11u, "json"},
        {0x9473Bu, "kab"},
        {0x196A7u, "kdi"},
        {0x36D11u, "layer_info"},
        {0x64FA4u, "layer_pcd"},
        {0xD9035u, "lik"},
        {0x677CAu, "lipmap"},
        {0x571EBu, "list"},
        {0xFEBCBu, "listb"},
        {0x37D88u, "lnkani"},
        {0xEF836u, "low.tmsbin"},
        {0xB064Au, "lsd"},
        {0xC830u, "mask.btex"},
        {0x41EEu, "mask.dds.bcfg"},
        {0x7A08Bu, "max"},
        {0xCBFD9u, "mgpubin"},
        {0x10867u, "mid"},
        {0xADB8Du, "msgbin"},
        {0xE959Fu, "n3d2p_raw"},
        {0xB9C3Cu, "nav"},
        {0xDB3C3u, "nav_cell_ref"},
        {0xC0A4Eu, "nav_connectivity"},
        {0x9A9A0u, "nav_context"},
        {0x6DB15u, "nav_debug_single"},
        {0x6F5C9u, "nav_edgelinks"},
        {0xE03AEu, "nav_ref"},
        {0x921Bu, "nav_smp"},
        {0xCD7CEu, "nav_waypoint"},
        {0x2ECBFu, "nav_world"},
        {0x4E016u, "nav_world_deps"},
        {0x50B76u, "nav_world_map"},
        {0x2C7CBu, "nav_world_splitter"},
        {0x85D6Fu, "navalist"},
        {0x216F8u, "navcelllist"},
        {0x719C9u, "parambin"},
        {0x7B665u, "pka"},
        {0x7CC7Cu, "pkr"},
        {0x49B7Cu, "pmdl"},
        {0x61F66u, "png"},
        {0x7C643u, "pngbin"},
        {0x63A1Du, "prefab"},
        {0x5AFu, "prt"},
        {0x9D7F1u, "prtd"},
        {0x9EC55u, "prtx"},
        {0xE9D31u, "ps.sb"},
        {0xB8247u, "psocache"},
        {0xE101Cu, "r.btex"},
        {0x4A6D3u, "rag"},
        {0x6473Eu, "ragdoll"},
        {0x928D1u, "raygmtl"},
        {0x76A58u, "res_info"},
        {0xDAB71u, "sapb"},
        {0x31C1u, "sax"},
        {0x4C153u, "sax(space)"},
        {0x33E76u, "sb"},
        {0x27896u, "sbd"},
        {0xBEBC9u, "sbmdl"},
        {0x736C7u, "ssd"},
        {0xE1934u, "style"},
        {0x325F9u, "swf"},
        {0x8E661u, "swfb"},
        {0x13D72u, "tcd"},
        {0x14CBDu, "tcm"},
        {0x14957u, "tco"},
        {0xFD979u, "tcophysx"},
        {0x53EF5u, "tga"},
        {0x21976u, "tif"},
        {0xAF7B8u, "tif_$h.btex"},
        {0x5591Bu, "tif.btex"},
        {0x78A01u, "tms"},
        {0xE877Au, "tmsbin"},
        {0x4E142u, "tnav"},
        {0x1A21u, "tpd"},
        {0xF5E1Au, "tpdbin"},
        {0x7F1BBu, "tspack"},
        {0x6AB79u, "txt"},
        {0x1F9BCu, "uam"},
        {0x1AF61u, "uifn"},
        {0x89393u, "uip"},
        {0xF6084u, "umbra"},
        {0xD34BFu, "ups"},
        {0xD2546u, "vegy"},
        {0x91A20u, "vfol"},
        {0x98B7Au, "vfuncs"},
        {0x44689u, "vfx"},
        {0xE23B5u, "vfxlist"},
        {0x2761u, "vhlist"},
        {0x81523u, "vlink"},
        {0x3DFu, "vs.sb"},
        {0xC7BC5u, "win.config"},
        {0xCBF7Du, "win.mab"},
        {0xDF19Fu, "win.sab"},
        {0x6B501u, "win32.bin"},
        {0x650B6u, "win32.bins"},
        {0xA8B22u, "win32.msgbin"},
        {0x58D48u, "wld_prc"},
        {0x983EBu, "wlod"},
        {0xA7AFFu, "wlodn"},
        {0x883B8u, "wpcm"},
        {0x899CFu, "wpcp"},
        {0x1A5FCu, "wpcpbin"},
        {0x658F5u, "wped"},
        {0x7CDD6u, "wpvd"},
        {0xF05A6u, "wth2"},
        {0x6CC0Cu, "wth2b"},
        {0x74EB8u, "xml"}
    };
}

