import io
import struct
from dataclasses import dataclass


@dataclass
class ValueBuffer:
    values: list[float]

    def __init__(self):
        self.values = []

    def initialize(self, buffer: bytes):
        offset = 0
        while offset < len(buffer) - 1:
            value = struct.unpack_from("<f", buffer, offset)[0]
            self.values.append(value)
            offset += 4

    def size(self) -> int:
        return len(self.values) * 4

    def get(self, offset: int, size: int) -> list[float]:
        values = []
        for i in range(int(size / 4)):
            values.append(self.values[int(offset / 4) + i])
        return values

    def put(self, values: list[float]) -> int:
        offset = len(self.values)
        for value in values:
            self.values.append(value)
        return offset * 4

    def to_bytes(self) -> bytes:
        buffer = io.BytesIO(bytes(0))
        for value in self.values:
            buffer.write(struct.pack("<f", value))

        return buffer.getvalue()
