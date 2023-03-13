import struct
from dataclasses import dataclass

from .buffer import Buffer
from .sampler import Sampler
from .shaderbinary import ShaderBinary
from .shaderprogram import ShaderProgram
from .texture import Texture
from .uniform import Uniform
from ..gfxbinheader import GfxbinHeader


@dataclass
class Gmtl:
    header: GfxbinHeader
    name_offset: int
    unknown: int
    uniform_count: int
    buffer_count: int
    texture_count: int
    sampler_count: int
    total_uniform_count: int
    total_buffer_count: int
    total_texture_count: int
    total_sampler_count: int
    shader_binary_count: int
    shader_program_count: int
    gpubin_size: int
    stringbin_size: int
    name_hash: int
    unknown_hash: int
    blend_type: int
    blend_factor: float
    render_state_bits: int
    skin_vs_max_bone_count: int
    brdf_type: int
    htpk_uri_offset: int
    uniforms: list[Uniform]
    buffers: list[Buffer]
    textures: list[Texture]
    samplers: list[Sampler]
    shader_binaries: list[ShaderBinary]
    shader_programs: list[ShaderProgram]
    gpubin: bytes
    stringbin: bytes

    name: str
    htpk_uri: str

    def __init__(self, reader):
        self.header = GfxbinHeader(reader)
        self.name_offset = reader.read()

        if self.header.version >= 20220707:
            self.unknown = reader.read()

        self.uniform_count = reader.read()
        self.buffer_count = reader.read()
        self.texture_count = reader.read()
        self.sampler_count = reader.read()
        self.total_uniform_count = reader.read()
        self.total_buffer_count = reader.read()
        self.total_texture_count = reader.read()
        self.total_sampler_count = reader.read()
        self.shader_binary_count = reader.read()
        self.shader_program_count = reader.read()
        self.gpubin_size = reader.read()
        self.stringbin_size = reader.read()
        self.name_hash = reader.read()

        if self.header.version >= 20220707:
            self.unknown_hash = reader.read()

        self.blend_type = reader.read()
        self.blend_factor = reader.read()
        self.render_state_bits = reader.read()
        self.skin_vs_max_bone_count = reader.read()
        self.brdf_type = reader.read()
        self.htpk_uri_offset = reader.read()

        self.uniforms = []
        for i in range(self.total_uniform_count):
            self.uniforms.append(Uniform(reader))

        self.buffers = []
        for i in range(self.total_buffer_count):
            self.buffers.append(Buffer(reader))

        self.textures = []
        for i in range(self.total_texture_count):
            self.textures.append(Texture(reader))

        self.samplers = []
        for i in range(self.total_sampler_count):
            self.samplers.append(Sampler(reader))

        self.shader_binaries = []
        for i in range(self.shader_binary_count):
            self.shader_binaries.append(ShaderBinary(reader))

        self.shader_programs = []
        for i in range(self.shader_program_count):
            self.shader_programs.append(ShaderProgram(reader))

        self.gpubin = reader.read()
        self.stringbin = reader.read()

        self.name = self._unpack_string(self.name_offset)

        for i in range(len(self.buffers)):
            self.buffers[i].shader_gen_name = self._unpack_string(self.buffers[i].shader_gen_name_offset)
            self.buffers[i].values = self._unpack_values(self.buffers[i].offset, self.buffers[i].size)

        for i in range(len(self.textures)):
            self.textures[i].name = self._unpack_string(self.textures[i].name_offset)
            self.textures[i].shader_gen_name = self._unpack_string(self.textures[i].shader_gen_name_offset)
            self.textures[i].uri = self._unpack_string(self.textures[i].uri_offset)

    def get_buffer(self, shader_gen_name: str):
        for i in range(len(self.buffers)):
            if self.buffers[i].shader_gen_name == shader_gen_name:
                return self.buffers[i]
        return None

    def _unpack_values(self, offset: int, size: int):
        result = []
        for i in range(int(size / 4)):
            result.append(struct.unpack_from("<f", self.gpubin, offset)[0])

        return result

    def _unpack_string(self, offset: int):
        result = ""
        while True:
            next_char = struct.unpack_from("<c", self.stringbin, offset)[0]
            if next_char == b"\x00":
                break
            else:
                result += next_char.decode("utf-8")
                offset += 1

        return result
