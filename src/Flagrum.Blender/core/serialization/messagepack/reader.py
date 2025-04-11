import struct
from dataclasses import dataclass

from .format import Format
from ...mathematics.matrix3x4 import Matrix3x4
from ...mathematics.vector3 import Vector3


@dataclass
class MessagePackReader:
    data_version: int

    def __init__(self, file_buffer):
        self.buffer = file_buffer
        self.offset = 0

    def read(self):
        format_type = struct.unpack_from("<B", self.buffer, self.offset)[0]
        self.offset += 1

        if Format.PositiveFixIntStart <= format_type <= Format.PositiveFixIntEnd:
            return format_type & 0x7F
        elif Format.FixMapStart <= format_type <= Format.FixMapEnd:
            return self._read_map(format_type & 0xF)
        elif Format.FixArrayStart <= format_type <= Format.FixArrayEnd:
            return format_type & 0xF
        elif Format.FixStrStart <= format_type <= Format.FixStrEnd:
            size = format_type & 0x1F
            if size > 0:
                result = struct.unpack_from("<" + str(size - 1) + "s", self.buffer, self.offset)[0].decode('utf-8')
                self.offset += size
                return result
            else:
                return ""
        elif format_type == Format.BooleanFalse:
            return False
        elif format_type == Format.BooleanTrue:
            return True
        elif format_type == Format.Bin8:
            size = struct.unpack_from("<B", self.buffer, self.offset)[0]
            self.offset += 1
            result = self.buffer[self.offset:(self.offset + size)]
            self.offset += size
            return result
        elif format_type == Format.Bin16:
            size = struct.unpack_from("<H", self.buffer, self.offset)[0]
            self.offset += 2
            result = self.buffer[self.offset:(self.offset + size)]
            self.offset += size
            return result
        elif format_type == Format.Bin32:
            size = struct.unpack_from("<I", self.buffer, self.offset)[0]
            self.offset += 4
            result = self.buffer[self.offset:(self.offset + size)]
            self.offset += size
            return result
        elif format_type == Format.Float32:
            result = struct.unpack_from("<f", self.buffer, self.offset)[0]
            self.offset += 4
            return result
        elif format_type == Format.Float64:
            result = struct.unpack_from("<d", self.buffer, self.offset)[0]
            self.offset += 8
            return result
        elif format_type == Format.Uint8:
            result = struct.unpack_from("<B", self.buffer, self.offset)[0]
            self.offset += 1
            return result
        elif format_type == Format.Uint16:
            result = struct.unpack_from("<H", self.buffer, self.offset)[0]
            self.offset += 2
            return result
        elif format_type == Format.Uint32:
            result = struct.unpack_from("<I", self.buffer, self.offset)[0]
            self.offset += 4
            return result
        elif format_type == Format.Uint64:
            result = struct.unpack_from("<Q", self.buffer, self.offset)[0]
            self.offset += 8
            return result
        elif format_type == Format.Int8:
            result = struct.unpack_from("<b", self.buffer, self.offset)[0]
            self.offset += 1
            return result
        elif format_type == Format.Int16:
            result = struct.unpack_from("<h", self.buffer, self.offset)[0]
            self.offset += 2
            return result
        elif format_type == Format.Int32:
            result = struct.unpack_from("<i", self.buffer, self.offset)[0]
            self.offset += 4
            return result
        elif format_type == Format.Int64:
            result = struct.unpack_from("<q", self.buffer, self.offset)[0]
            self.offset += 8
            return result
        elif format_type == Format.Str8:
            size = struct.unpack_from("<B", self.buffer, self.offset)[0]
            self.offset += 1
            result = struct.unpack_from("<" + str(size - 1) + "s", self.buffer, self.offset)[0].decode('utf-8')
            self.offset += size
            return result
        elif format_type == Format.Str16:
            size = struct.unpack_from("<H", self.buffer, self.offset)[0]
            self.offset += 2
            result = struct.unpack_from("<" + str(size - 1) + "s", self.buffer, self.offset)[0].decode('utf-8')
            self.offset += size
            return result
        elif format_type == Format.Str32:
            size = struct.unpack_from("<I", self.buffer, self.offset)[0]
            self.offset += 4
            result = struct.unpack_from("<" + str(size - 1) + "s", self.buffer, self.offset)[0].decode('utf-8')
            self.offset += size
            return result
        elif format_type == Format.Array16:
            size = struct.unpack_from("<H", self.buffer, self.offset)[0]
            self.offset += 2
            return size
        elif format_type == Format.Array32:
            size = struct.unpack_from("<I", self.buffer, self.offset)[0]
            self.offset += 4
            return size
        elif format_type == Format.Map16:
            size = struct.unpack_from("<H", self.buffer, self.offset)[0]
            self.offset += 2
            return self._read_map(size)
        elif format_type == Format.Map32:
            size = struct.unpack_from("<I", self.buffer, self.offset)[0]
            self.offset += 4
            return self._read_map(size)
        elif Format.NegativeFixIntStart <= format_type <= Format.NegativeFixIntEnd:
            return -(format_type & 0x1F)

    def read_vector(self) -> Vector3:
        vector = Vector3()
        vector.x = self.read()
        vector.y = self.read()
        vector.z = self.read()
        return vector

    def read_matrix(self) -> Matrix3x4:
        matrix = Matrix3x4()
        matrix.rows.append(self.read_vector())
        matrix.rows.append(self.read_vector())
        matrix.rows.append(self.read_vector())
        matrix.rows.append(self.read_vector())
        return matrix

    def _read_map(self, size):
        result = {}

        for a in range(size):
            uri_hash = self.read()
            uri = self.read()
            result[uri_hash] = uri

        return result
