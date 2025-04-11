from dataclasses import dataclass

from ...serialization.messagepack.writer import MessagePackWriter


@dataclass
class Uniform:
    name_offset: int
    shader_gen_name_offset: int
    unknown: int
    name_hash: int
    shader_gen_name_hash: int
    unknown2: int
    offset: int
    size: int
    buffer_count: int
    flags: int

    name: str
    shader_gen_name: str
    values: list[float]

    def __init__(self, reader):
        self.values = []
        self.name_offset = reader.read()
        self.shader_gen_name_offset = reader.read()
        self.unknown = reader.read()
        self.name_hash = reader.read()
        self.shader_gen_name_hash = reader.read()
        self.unknown2 = reader.read()
        self.offset = reader.read()
        self.size = reader.read()
        self.buffer_count = reader.read()
        self.flags = reader.read()

    def write(self, writer: MessagePackWriter):
        writer.write(self.name_offset)
        writer.write(self.shader_gen_name_offset)
        writer.write(self.unknown)
        writer.write(self.name_hash)
        writer.write(self.shader_gen_name_hash)
        writer.write(self.unknown2)
        writer.write(self.offset)
        writer.write(self.size)
        writer.write(self.buffer_count)
        writer.write(self.flags)
