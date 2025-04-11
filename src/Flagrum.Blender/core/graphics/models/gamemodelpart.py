from dataclasses import dataclass

from ...serialization.messagepack.reader import MessagePackReader
from ...serialization.messagepack.writer import MessagePackWriter


@dataclass(init=False)
class GameModelPart:
    name: str
    id: int
    unknown: str
    flags: bool

    def read(self, reader: MessagePackReader):
        self.name = reader.read()
        self.id = reader.read()
        self.unknown = reader.read()
        self.flags = reader.read()

    def write(self, writer: MessagePackWriter):
        writer.write(self.name)
        writer.write(self.id)
        writer.write(self.unknown)
        writer.write(self.flags)
