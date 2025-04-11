import io
import struct
from dataclasses import dataclass


@dataclass
class StringBuffer:
    values: dict[int, str]
    offsets: dict[str, int]
    offset: int

    def __init__(self):
        self.offset = 0
        self.values = {}
        self.offsets = {}

    def initialize(self, buffer: bytes):
        while self.offset < len(buffer) - 1:
            next_offset = self.offset
            result = ""
            while True:
                next_char = struct.unpack_from("<c", buffer, self.offset)[0]
                if next_char == b"\x00":
                    self.offset += 1
                    break
                else:
                    result += next_char.decode("utf-8")
                    self.offset += 1
            self.values[next_offset] = result
            self.offsets[result] = next_offset

    def size(self) -> int:
        result = 0
        for key in self.offsets:
            result += len(key) + 1
        return result

    def get(self, offset: int) -> str:
        return self.values[offset]

    def put(self, value: str) -> int:
        if value in self.offsets:
            return self.offsets[value]

        self.offsets[value] = self.offset
        self.values[self.offset] = value

        offset = self.offset
        self.offset += len(value) + 1
        return offset

    def to_bytes(self) -> bytes:
        buffer = io.BytesIO(bytes(0))
        for key in self.offsets:
            buffer.write(struct.pack("<" + str(len(key)) + "s", key.encode('utf-8')))
            buffer.write(struct.pack("<B", 0x00))

        return buffer.getvalue()
