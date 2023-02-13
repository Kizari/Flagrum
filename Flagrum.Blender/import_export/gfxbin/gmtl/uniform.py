from dataclasses import dataclass


@dataclass
class Uniform:
    name_offset: int
    shader_gen_name_offset: int
    name_hash: int
    shader_gen_name_hash: int
    offset: int
    size: int
    buffer_index: int
    type: int
    flags: int

    def __init__(self, reader):
        self.name_offset = reader.read()
        self.shader_gen_name_offset = reader.read()
        self.name_hash = reader.read()
        self.shader_gen_name_hash = reader.read()
        self.offset = reader.read()
        self.size = reader.read()
        self.buffer_index = reader.read()
        self.type = reader.read()
        self.flags = reader.read()
