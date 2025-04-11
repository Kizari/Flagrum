from dataclasses import dataclass

from .gmdlvertexelementformat import ElementFormat


@dataclass
class GmdlVertexElement:
    offset: int
    semantic: str
    format: ElementFormat

    def __init__(self, reader):
        self.offset = reader.read()
        self.semantic = reader.read()
        self.format = ElementFormat(reader.read())
