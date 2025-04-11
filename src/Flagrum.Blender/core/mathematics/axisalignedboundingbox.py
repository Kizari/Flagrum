from dataclasses import dataclass

from .vector3 import Vector3
from ..serialization.messagepack.reader import MessagePackReader
from ..serialization.messagepack.writer import MessagePackWriter


@dataclass(init=False)
class AxisAlignedBoundingBox:
    start: Vector3
    end: Vector3

    def read(self, reader: MessagePackReader):
        self.start = reader.read_vector()
        self.end = reader.read_vector()

    def write(self, writer: MessagePackWriter):
        writer.write_vector(self.start)
        writer.write_vector(self.end)
