from dataclasses import dataclass

from .vertexelementformat import VertexElementFormat
from ...serialization.messagepack.reader import MessagePackReader
from ...serialization.messagepack.writer import MessagePackWriter


@dataclass(init=False)
class VertexElement:
    offset: int
    semantic: str
    format: VertexElementFormat

    def read(self, reader: MessagePackReader):
        self.offset = reader.read()
        self.semantic = reader.read()
        self.format = VertexElementFormat(reader.read())

    def write(self, writer: MessagePackWriter):
        writer.write(self.offset)
        writer.write(self.semantic)
        writer.write(self.format)
