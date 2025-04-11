from dataclasses import dataclass

from ...mathematics.matrix3x4 import Matrix3x4
from ...mathematics.vector3 import Vector3
from ...serialization.messagepack.reader import MessagePackReader
from ...serialization.messagepack.writer import MessagePackWriter


@dataclass(init=False)
class GameModelNode:
    matrix: Matrix3x4
    unknown: float
    name: str
    unknown2: int
    unknown3: int
    unknown4: int

    def read(self, reader: MessagePackReader, is_first: bool):
        self.matrix = reader.read_matrix()

        if is_first:
            self.unknown = 0.0
        elif reader.data_version >= 20220707:
            self.unknown = reader.read()

        self.name = reader.read()

        if reader.data_version >= 20220707:
            self.unknown2 = reader.read()
            self.unknown3 = reader.read()
            self.unknown4 = reader.read()

    def write(self, writer: MessagePackWriter):
        writer.write_matrix(self.matrix)
        writer.write(self.name)

    @staticmethod
    def create_default(name: str):
        node = GameModelNode()
        node.name = name
        node.matrix = Matrix3x4()
        vector = Vector3()
        vector.x = 0.0
        vector.y = 0.0
        vector.z = 0.0
        node.matrix.rows.append(vector)
        node.matrix.rows.append(vector)
        node.matrix.rows.append(vector)
        node.matrix.rows.append(vector)
        return node
