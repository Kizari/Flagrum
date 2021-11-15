namespace Flagrum.Gfxbin.Gmdl.Data
{
    public enum VertexElementFormat
    {
        Disable = 0x0,
        XYZW32_Float = 0x1,
        XYZW32_UintN = 0x2,
        XYZW32_Uint = 0x3,
        XYZW32_SintN = 0x4,
        XYZW32_Sint = 0x5,
        XYZW16_Float = 0x6,
        XYZW16_UintN = 0x7,
        XYZW16_Uint = 0x8,
        XYZW16_SintN = 0x9,
        XYZW16_Sint = 0xA,
        XYZW8_Float = 0xB,
        XYZW8_UintN = 0xC,
        XYZW8_Uint = 0xD,
        XYZW8_SintN = 0xE,
        XYZW8_Sint = 0xF,
        XYZ32_Float = 0x10,
        XYZ32_UintN = 0x11,
        XYZ32_Uint = 0x12,
        XYZ32_SintN = 0x13,
        XYZ32_Sint = 0x14,
        XY32_Float = 0x15,
        XY32_UintN = 0x16,
        XY32_Uint = 0x17,
        XY32_SintN = 0x18,
        XY32_Sint = 0x19,
        XY16_Float = 0x1A,
        XY16_UintN = 0x1B,
        XY16_Uint = 0x1C,
        XY16_SintN = 0x1D,
        XY16_Sint = 0x1E,
        X32_Float = 0x1F,
        X32_UintN = 0x20,
        X32_Uint = 0x21,
        X32_SintN = 0x22,
        X32_Sint = 0x23,
        X16_Float = 0x24,
        X16_UintN = 0x25,
        X16_Uint = 0x26,
        X16_SintN = 0x27,
        X16_Sint = 0x28,
        X8_Float = 0x29,
        X8_UintN = 0x2A,
        X8_Uint = 0x2B,
        X8_SintN = 0x2C,
        X8_Sint = 0x2D,
        Num = 0x2E
    }

    public class VertexElementDescription
    {
        public const string Position0 = "POSITION0";
        public const string Normal0 = "NORMAL0";
        public const string Binormal0 = "BINORMAL0";
        public const string Binormal1 = "BINORMAL1";
        public const string Normal2Factors = "NORMAL2FACTORS0";
        public const string Normal4Factors = "NORMAL4FACTORS0";
        public const string Tangent0 = "TANGENT0";
        public const string Tangent1 = "TANGENT1";
        public const string Color0 = "COLOR0";
        public const string Color1 = "COLOR1";
        public const string Color2 = "COLOR2";
        public const string Color3 = "COLOR3";
        public const string FogCoord0 = "FOGCOORD0";
        public const string PSize0 = "PSIZE0";
        public const string BlendWeight0 = "BLENDWEIGHT0";
        public const string BlendWeight1 = "BLENDWEIGHT1";
        public const string BlendIndices0 = "BLENDINDICES0";
        public const string BlendIndices1 = "BLENDINDICES1";
        public const string TexCoord0 = "TEXCOORD0";
        public const string TexCoord1 = "TEXCOORD1";
        public const string TexCoord2 = "TEXCOORD2";
        public const string TexCoord3 = "TEXCOORD3";
        public const string TexCoord4 = "TEXCOORD4";
        public const string TexCoord5 = "TEXCOORD5";
        public const string TexCoord6 = "TEXCOORD6";
        public const string TexCoord7 = "TEXCOORD7";

        public uint Offset { get; set; }
        public string Semantic { get; set; }
        public VertexElementFormat Format { get; set; }
    }
}