from dataclasses import dataclass


@dataclass
class Buffer:
    name_offset: int
    shader_gen_name_offset: int
    unknown: int
    name_hash: int
    shader_gen_name_hash: int
    unknown2: int
    offset: int
    size: int
    uniform_count: int
    flags: int

    shader_gen_name: str

    values: list[float]

    def __init__(self, reader):
        self.name_offset = reader.read()
        self.shader_gen_name_offset = reader.read()
        self.unknown = reader.read()
        self.name_hash = reader.read()
        self.shader_gen_name_hash = reader.read()
        self.unknown2 = reader.read()
        self.offset = reader.read()
        self.size = reader.read()
        self.uniform_count = reader.read()
        self.flags = reader.read()
