from dataclasses import dataclass


@dataclass
class Texture:
    resource_file_hash: int
    name_offset: int
    shader_gen_name_offset: int
    unknown: int
    uri_offset: int
    name_hash: int
    shader_gen_name_hash: int
    unknown2: int
    uri_hash: int
    flags: int
    unknown3: int

    name: str
    shader_gen_name: str
    uri: str

    def __init__(self, reader):
        self.resource_file_hash = reader.read()
        self.name_offset = reader.read()
        self.shader_gen_name_offset = reader.read()
        self.unknown = reader.read()
        self.uri_offset = reader.read()
        self.name_hash = reader.read()
        self.shader_gen_name_hash = reader.read()
        self.unknown2 = reader.read()
        self.uri_hash = reader.read()
        self.flags = reader.read()
        self.unknown3 = reader.read()
