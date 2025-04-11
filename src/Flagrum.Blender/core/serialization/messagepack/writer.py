import struct
from typing import Any, IO

from .format import Format
from ...mathematics.matrix3x4 import Matrix3x4
from ...mathematics.vector3 import Vector3


class MessagePackWriter:
    stream: IO

    def __init__(self, stream: IO):
        self.stream = stream

    def write(self, value: Any):
        stream = self.stream
        if value is None:
            # file.write(struct.pack("<B", Format.Nil))
            raise Exception("You probably weren't intending to serialize a null?")
        elif isinstance(value, bool):
            if value:
                stream.write(struct.pack("<B", Format.BooleanTrue))
            else:
                stream.write(struct.pack("<B", Format.BooleanFalse))
        elif isinstance(value, int):
            if value >= 0:
                if 0 <= value <= 0x7F:
                    stream.write(struct.pack("<B", value))
                elif 0 <= value <= 255:
                    stream.write(struct.pack("<B", Format.Uint8))
                    stream.write(struct.pack("<B", value))
                elif 0 <= value <= 65535:
                    stream.write(struct.pack("<B", Format.Uint16))
                    stream.write(struct.pack("<H", value))
                elif 0 <= value <= 4294967295:
                    stream.write(struct.pack("<B", Format.Uint32))
                    stream.write(struct.pack("<I", value))
                elif 0 <= value <= 18446744073709551615:
                    stream.write(struct.pack("<B", Format.Uint64))
                    stream.write(struct.pack("<Q", value))
                else:
                    raise Exception("How did you even get an int bigger than 64-bit?")
            else:
                if 0 > value >= -0x1F:
                    stream.write(struct.pack("<B", Format.NegativeFixIntStart + (value * -1)))
                elif -128 <= value <= 127:
                    stream.write(struct.pack("<B", Format.Int8))
                    stream.write(struct.pack("<b", value))
                elif -32768 <= value <= 32767:
                    stream.write(struct.pack("<B", Format.Int16))
                    stream.write(struct.pack("<h", value))
                elif -2147483648 <= value <= 2147483647:
                    stream.write(struct.pack("<B", Format.Int32))
                    stream.write(struct.pack("<i", value))
                elif -9223372036854775808 <= value <= 9223372036854775807:
                    stream.write(struct.pack("<B", Format.Int64))
                    stream.write(struct.pack("<q", value))

        elif isinstance(value, float):
            if -3.40282347E+38 <= value <= 3.40282347E+38:
                stream.write(struct.pack("<B", Format.Float32))
                stream.write(struct.pack("<f", value))
            else:
                stream.write(struct.pack("<B", Format.Float64))
                stream.write(struct.pack("<d", value))
        elif isinstance(value, str):
            if len(value) == 0:
                stream.write(struct.pack("<B", Format.FixStrStart))
                return

            length = len(value) + 1
            if length <= 0x1F:
                stream.write(struct.pack("<B", Format.FixStrStart + length))
            elif length <= 255:
                stream.write(struct.pack("<B", Format.Str8))
                stream.write(struct.pack("<B", length))
            elif length <= 65535:
                stream.write(struct.pack("<B", Format.Str16))
                stream.write(struct.pack("<H", length))
            elif length <= 4294967295:
                stream.write(struct.pack("<B", Format.Str32))
                stream.write(struct.pack("<I", length))

            # for char in value:
            #     file.write(struct.pack("<c", char))
            # file.write(struct.pack("<B", 0x00))

            stream.write(struct.pack("<" + str(length - 1) + "s", value.encode('utf-8')))
            stream.write(struct.pack("<B", 0x00))
        elif isinstance(value, bytes):
            length = len(value)
            if length <= 255:
                stream.write(struct.pack("<B", Format.Bin8))
                stream.write(struct.pack("<B", length))
            elif length <= 65535:
                stream.write(struct.pack("<B", Format.Bin16))
                stream.write(struct.pack("<H", length))
            elif length <= 4294967295:
                stream.write(struct.pack("<B", Format.Bin32))
                stream.write(struct.pack("<I", length))

            stream.write(value)
        elif isinstance(value, dict):
            length = len(value)
            if length <= 0xF:
                stream.write(struct.pack("<B", Format.FixMapStart + length))
            elif length <= 65535:
                stream.write(struct.pack("<B", Format.Map16))
                stream.write(struct.pack("<H", length))
            elif length <= 4294967295:
                stream.write(struct.pack("<B", Format.Map32))
                stream.write(struct.pack("<I", length))
            for key in value:
                self.write(key)
                self.write(value[key])
        elif isinstance(value, list):
            length = len(value)
            if length <= 0xF:
                stream.write(struct.pack("<B", Format.FixArrayStart + length))
            elif length <= 65535:
                stream.write(struct.pack("<B", Format.Array16))
                stream.write(struct.pack("<H", length))
            elif length <= 4294967295:
                stream.write(struct.pack("<B", Format.Array32))
                stream.write(struct.pack("<I", length))

            is_first = True
            for item in value:
                if item is None or isinstance(item, bool) or isinstance(item, int) or isinstance(item, float) \
                        or isinstance(item, str) or isinstance(item, bytes) or isinstance(item, dict) \
                        or isinstance(item, list):
                    self.write(item)
                else:
                    item.is_first = is_first
                    item.write(self)
                is_first = False

    def write_vector(self, vector: Vector3):
        self.write(vector.x)
        self.write(vector.y)
        self.write(vector.z)

    def write_matrix(self, matrix: Matrix3x4):
        self.write_vector(matrix.rows[0])
        self.write_vector(matrix.rows[1])
        self.write_vector(matrix.rows[2])
        self.write_vector(matrix.rows[3])
