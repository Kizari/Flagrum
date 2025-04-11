from enum import IntEnum


class VertexElementFormat(IntEnum):
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
    X8_Sint = 0x2D

    @staticmethod
    def get_size(element):
        if element.format == VertexElementFormat.XYZ32_Float:
            return 12
        elif element.format == VertexElementFormat.XY16_SintN:
            return 4
        elif element.format == VertexElementFormat.XY16_UintN:
            return 4
        elif element.format == VertexElementFormat.XY16_Float:
            return 4
        elif element.format == VertexElementFormat.XYZW8_UintN:
            return 4
        elif element.format == VertexElementFormat.XYZW8_SintN:
            return 4
        elif element.format == VertexElementFormat.XYZW16_Uint:
            return 8
        elif element.format == VertexElementFormat.XYZW32_Uint:
            return 16
        else:
            print(f"[ERROR] Unsupported element format {str(element.format)} on {element.semantic}")
            return None

    @staticmethod
    def get_count(element):
        if element.format == VertexElementFormat.XYZ32_Float:
            return 3
        elif element.format == VertexElementFormat.XY16_SintN:
            return 2
        elif element.format == VertexElementFormat.XY16_UintN:
            return 2
        elif element.format == VertexElementFormat.XY16_Float:
            return 2
        elif element.format == VertexElementFormat.XY32_Float:
            return 2
        elif element.format == VertexElementFormat.XYZW8_UintN or element.format == VertexElementFormat.XYZW8_Uint:
            return 4
        elif element.format == VertexElementFormat.XYZW8_SintN or element.format == VertexElementFormat.XYZW8_Sint:
            return 4
        elif element.format == VertexElementFormat.XYZW16_Uint:
            return 4
        elif element.format == VertexElementFormat.XYZW32_Uint:
            return 4
        else:
            print(f"[ERROR] Unsupported element format {str(element.format)} on {element.semantic}")
            return None

    @staticmethod
    def get_data_type(element):
        if element.format == VertexElementFormat.XYZ32_Float:
            return "<f"
        elif element.format == VertexElementFormat.XY16_SintN:
            return "<f"
        elif element.format == VertexElementFormat.XY16_UintN:
            return "<f"
        elif element.format == VertexElementFormat.XY16_Float:
            return "<f2"
        elif element.format == VertexElementFormat.XY32_Float:
            return "<f"
        elif element.format == VertexElementFormat.XYZW8_UintN:
            return "<f"
        elif element.format == VertexElementFormat.XYZW8_Uint:
            return "<B"
        elif element.format == VertexElementFormat.XYZW8_SintN:
            return "<f"
        elif element.format == VertexElementFormat.XYZW8_Sint:
            return "<b"
        elif element.format == VertexElementFormat.XYZW16_Uint:
            return "<H"
        elif element.format == VertexElementFormat.XYZW32_Uint:
            return "<I"
        else:
            print(f"[ERROR] Unsupported element format {str(element.format)} on {element.semantic}")
            return None
