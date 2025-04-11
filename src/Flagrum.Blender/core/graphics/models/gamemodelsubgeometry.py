from dataclasses import dataclass

from ...mathematics.axisalignedboundingbox import AxisAlignedBoundingBox
from ...mathematics.vector3 import Vector3
from ...serialization.messagepack.reader import MessagePackReader
from ...serialization.messagepack.writer import MessagePackWriter


@dataclass
class GameModelSubgeometry:
    aabb: AxisAlignedBoundingBox
    start_index: int
    primitive_count: int
    cluster_index_bitflag: int
    draw_order: int

    def __init__(self):
        aabb = AxisAlignedBoundingBox()
        aabb.start = Vector3()
        aabb.start.x = 0.0
        aabb.start.y = 0.0
        aabb.start.z = 0.0
        aabb.end = Vector3()
        aabb.end.x = 0.0
        aabb.end.y = 0.0
        aabb.end.z = 0.0
        self.aabb = aabb
        self.start_index = 0
        self.primitive_count = 0
        self.cluster_index_bitflag = 0
        self.draw_order = 0

    def read(self, reader: MessagePackReader):
        self.aabb = AxisAlignedBoundingBox()
        self.aabb.read(reader)
        self.start_index = reader.read()
        self.primitive_count = reader.read()
        self.cluster_index_bitflag = reader.read()
        self.draw_order = reader.read()

    def write(self, writer: MessagePackWriter):
        self.aabb.write(writer)
        writer.write(self.start_index)
        writer.write(self.primitive_count)
        writer.write(self.cluster_index_bitflag)
        writer.write(self.draw_order)
