from dataclasses import dataclass


@dataclass
class GmdlModelPart:
    name: str
    id: int
    unknown: str
    flags: bool

    def __init__(self, reader):
        self.name = reader.read()
        self.id = reader.read()
        self.unknown = reader.read()
        self.flags = reader.read()
