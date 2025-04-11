from dataclasses import dataclass

from ...serialization.messagepack.reader import MessagePackReader
from ...serialization.messagepack.writer import MessagePackWriter


@dataclass(init=False)
class GameModelMeshPart:
    parts_id: int
    start_index: int
    index_count: int

    def read(self, reader: MessagePackReader):
        self.parts_id = reader.read()
        self.start_index = reader.read()
        self.index_count = reader.read()

    def writer(self, writer: MessagePackWriter):
        writer.write(self.parts_id)
        writer.write(self.start_index)
        writer.write(self.index_count)
