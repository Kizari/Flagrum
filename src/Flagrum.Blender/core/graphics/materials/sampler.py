from dataclasses import dataclass

from .colour import Colour
from ...serialization.messagepack.writer import MessagePackWriter


@dataclass
class Sampler:
    name_offset: int
    shader_gen_name_offset: int
    unknown: int
    mag_filter: int
    min_filter: int
    mip_filter: int
    wrap_S: int
    wrap_T: int
    wrap_R: int
    mipmap_lod_bias: float
    max_anisotropy: int
    unknown2: int
    unknown3: int
    unknown4: int
    border_colour: Colour
    min_lod: int
    max_lod: int
    flags: int

    name: str
    shader_gen_name: str

    def __init__(self, reader):
        self.name_offset = reader.read()
        self.shader_gen_name_offset = reader.read()
        self.unknown = reader.read()
        self.mag_filter = reader.read()
        self.min_filter = reader.read()
        self.mip_filter = reader.read()
        self.wrap_S = reader.read()
        self.wrap_T = reader.read()
        self.wrap_R = reader.read()
        self.mipmap_lod_bias = reader.read()
        self.max_anisotropy = reader.read()
        self.unknown2 = reader.read()
        self.unknown3 = reader.read()
        self.unknown4 = reader.read()
        self.border_colour = Colour(reader)
        self.min_lod = reader.read()
        self.max_lod = reader.read()
        self.flags = reader.read()

    def write(self, writer: MessagePackWriter):
        writer.write(self.name_offset)
        writer.write(self.shader_gen_name_offset)
        writer.write(self.unknown)

        writer.write(self.mag_filter)
        writer.write(self.min_filter)
        writer.write(self.mip_filter)
        writer.write(self.wrap_S)
        writer.write(self.wrap_T)
        writer.write(self.wrap_R)

        writer.write(self.mipmap_lod_bias)
        writer.write(self.max_anisotropy)

        writer.write(self.unknown2)
        writer.write(self.unknown3)
        writer.write(self.unknown4)

        self.border_colour.write(writer)

        # min_lod = Float16.compress(self.min_lod)
        # max_lod = Float16.compress(self.max_lod)
        writer.write(self.min_lod)
        writer.write(self.max_lod)

        writer.write(self.flags)
