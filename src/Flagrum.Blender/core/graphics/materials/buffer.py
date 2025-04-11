from dataclasses import dataclass

from ...serialization.messagepack.writer import MessagePackWriter


@dataclass
class Buffer:
    name_offset: int
    shader_gen_name_offset: int
    name_hash: int
    shader_gen_name_hash: int
    offset: int
    size: int
    uniform_index: int
    type: int
    flags: int

    name: str
    shader_gen_name: str

    values: list[float]

    def __init__(self, reader):
        self.values = []
        self.name_offset = reader.read()
        self.shader_gen_name_offset = reader.read()
        self.name_hash = reader.read()
        self.shader_gen_name_hash = reader.read()
        self.offset = reader.read()
        self.size = reader.read()
        self.uniform_index = reader.read()
        self.type = reader.read()
        self.flags = reader.read()

    def write(self, writer: MessagePackWriter):
        writer.write(self.name_offset)
        writer.write(self.shader_gen_name_offset)
        writer.write(self.name_hash)
        writer.write(self.shader_gen_name_hash)
        writer.write(self.offset)
        writer.write(self.size)
        writer.write(self.uniform_index)
        writer.write(self.type)
        writer.write(self.flags)
