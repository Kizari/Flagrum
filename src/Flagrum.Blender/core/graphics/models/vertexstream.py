from dataclasses import dataclass

from .vertexelement import VertexElement
from ...serialization.messagepack.writer import MessagePackWriter


@dataclass(init=False)
class VertexStream:
    slot: int
    type: int
    stride: int
    offset: int
    elements: list[VertexElement]

    def read(self, reader):
        self.slot = reader.read()
        self.type = reader.read()
        self.stride = reader.read()
        self.offset = reader.read()

        element_count = reader.read()
        self.elements = []
        for i in range(element_count):
            element = VertexElement()
            element.read(reader)
            self.elements.append(element)

    def write(self, writer: MessagePackWriter):
        writer.write(self.slot)
        writer.write(self.type)
        writer.write(self.stride)
        writer.write(self.offset)
        writer.write(self.elements)
