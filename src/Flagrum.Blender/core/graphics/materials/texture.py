from dataclasses import dataclass

from ...serialization.messagepack.writer import MessagePackWriter


@dataclass
class Texture:
    uri_hash: int
    name_offset: int
    shader_gen_name_offset: int
    unknown: int
    uri_offset: int
    name_hash: int
    shader_gen_name_hash: int
    unknown2: int
    uri_hash32: int
    flags: int
    high_texture_streaming_levels: int

    name: str
    shader_gen_name: str
    uri: str

    def __init__(self, reader):
        self.name = None
        self.shader_gen_name = None
        self.uri = None

        self.uri_hash = reader.read()
        self.name_offset = reader.read()
        self.shader_gen_name_offset = reader.read()
        self.unknown = reader.read()
        self.uri_offset = reader.read()
        self.name_hash = reader.read()
        self.shader_gen_name_hash = reader.read()
        self.unknown2 = reader.read()
        self.uri_hash32 = reader.read()
        self.flags = reader.read()

        if reader.data_version > 20150508:
            self.high_texture_streaming_levels = reader.read()

    def write(self, writer: MessagePackWriter):
        writer.write(self.uri_hash)
        writer.write(self.name_offset)
        writer.write(self.shader_gen_name_offset)
        writer.write(self.unknown)
        writer.write(self.uri_offset)
        writer.write(self.name_hash)
        writer.write(self.shader_gen_name_hash)
        writer.write(self.unknown2)
        writer.write(self.uri_hash32)
        writer.write(self.flags)
        writer.write(self.high_texture_streaming_levels)
