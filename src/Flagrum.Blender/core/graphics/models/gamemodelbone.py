from dataclasses import dataclass

from ...serialization.messagepack.writer import MessagePackWriter


@dataclass(init=False)
class GameModelBone:
    name: str
    lod_index: int
    unique_index: int

    def read(self, reader):
        self.name = reader.read()
        self.lod_index = reader.read()

        if reader.data_version >= 20220707:
            self.unique_index = reader.read()
        else:
            self.unique_index = self.lod_index >> 16

    def write(self, writer: MessagePackWriter):
        writer.write(self.name)
        writer.write(self.lod_index)
