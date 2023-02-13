from dataclasses import dataclass


@dataclass
class GmdlBone:
    name: str
    lod_index: int
    unique_index: int

    def __init__(self, reader, version: int):
        self.name = reader.read()
        self.lod_index = reader.read()

        if version >= 20220707:
            self.unique_index = reader.read()
        else:
            self.unique_index = self.lod_index >> 16
