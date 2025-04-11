from dataclasses import dataclass

from .vector3 import Vector3
from ..serialization.messagepack.reader import MessagePackReader
from ..serialization.messagepack.writer import MessagePackWriter


@dataclass(init=False)
class OrientedBoundingBox:
    center: Vector3
    x_half_extent: Vector3
    y_half_extent: Vector3
    z_half_extent: Vector3

    def read(self, reader: MessagePackReader):
        self.center = reader.read_vector()
        self.x_half_extent = reader.read_vector()
        self.y_half_extent = reader.read_vector()
        self.z_half_extent = reader.read_vector()

    def write(self, writer: MessagePackWriter):
        writer.write_vector(self.center)
        writer.write_vector(self.x_half_extent)
        writer.write_vector(self.y_half_extent)
        writer.write_vector(self.z_half_extent)
