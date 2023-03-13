from dataclasses import dataclass


@dataclass
class GmdlNode:
    matrix: list[list[float]]
    unknown: float
    name: str
    unknown2: int
    unknown3: int
    unknown4: int

    def __init__(self, reader, is_first: bool, version: int):
        self.matrix = [
            [reader.read(), reader.read(), reader.read()],
            [reader.read(), reader.read(), reader.read()],
            [reader.read(), reader.read(), reader.read()],
            [reader.read(), reader.read(), reader.read()]
        ]

        if is_first:
            self.unknown = 0.0
        elif version >= 20220707:
            self.unknown = reader.read()

        self.name = reader.read()

        if version >= 20220707:
            self.unknown2 = reader.read()
            self.unknown3 = reader.read()
            self.unknown4 = reader.read()
