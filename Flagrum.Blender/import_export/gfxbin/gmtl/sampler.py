from dataclasses import dataclass

from .colour import Colour


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
    max_ansio: int
    unknown2: int
    unknown3: int
    unknown4: int
    border_colour: Colour
    min_lod: int
    max_lod: int
    flags: int

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
        self.max_ansio = reader.read()
        self.unknown2 = reader.read()
        self.unknown3 = reader.read()
        self.unknown4 = reader.read()
        self.border_colour = Colour(reader)
        self.min_lod = reader.read()
        self.max_lod = reader.read()
        self.flags = reader.read()
