from dataclasses import dataclass

from .gmdlvertexelement import GmdlVertexElement


@dataclass
class GmdlVertexStream:
    slot: int
    type: int
    stride: int
    offset: int
    elements: list[GmdlVertexElement]

    def __init__(self, reader):
        self.slot = reader.read()
        self.type = reader.read()
        self.stride = reader.read()
        self.offset = reader.read()

        element_count = reader.read()
        self.elements = []
        for i in range(element_count):
            self.elements.append(GmdlVertexElement(reader))
