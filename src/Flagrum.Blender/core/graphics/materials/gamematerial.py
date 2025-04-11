from dataclasses import dataclass

from .buffer import Buffer
from .sampler import Sampler
from .shaderbinary import ShaderBinary
from .shaderprogram import ShaderProgram
from .texture import Texture
from .uniform import Uniform
from ..graphicsbinary import GraphicsBinary
from ...serialization.messagepack.writer import MessagePackWriter
from ...serialization.stringbuffer import StringBuffer
from ...serialization.valuebuffer import ValueBuffer
from ...utilities.cryptography import Cryptography


@dataclass
class GameMaterial(GraphicsBinary):
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
    value_buffer_size: int
    string_buffer_size: int
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
    value_buffer: ValueBuffer
    string_buffer: StringBuffer

    name: str
    htpk_uri: str

    def __init__(self, reader):
        GraphicsBinary.__init__(self)
        GraphicsBinary.read(self, reader)

        self.name_offset = reader.read()

        if self.version >= 20220707:
            self.unknown = reader.read()
        else:
            self.unknown = 0

        self.buffer_count = reader.read()
        self.uniform_count = reader.read()
        self.texture_count = reader.read()
        self.sampler_count = reader.read()
        self.total_buffer_count = reader.read()
        self.total_uniform_count = reader.read()
        self.total_texture_count = reader.read()
        self.total_sampler_count = reader.read()
        self.shader_binary_count = reader.read()
        self.shader_program_count = reader.read()
        self.value_buffer_size = reader.read()
        self.string_buffer_size = reader.read()
        self.name_hash = reader.read()

        if self.version >= 20220707:
            self.unknown_hash = reader.read()
        else:
            self.unknown_hash = 0

        self.blend_type = reader.read()
        self.blend_factor = reader.read()
        self.render_state_bits = reader.read()
        self.skin_vs_max_bone_count = reader.read()
        self.brdf_type = reader.read()
        self.htpk_uri_offset = reader.read()

        self.buffers = []
        for i in range(self.total_buffer_count):
            self.buffers.append(Buffer(reader))

        self.uniforms = []
        for i in range(self.total_uniform_count):
            self.uniforms.append(Uniform(reader))

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

        self.value_buffer = ValueBuffer()
        self.value_buffer.initialize(reader.read())
        self.string_buffer = StringBuffer()
        self.string_buffer.initialize(reader.read())

        self.name = self.string_buffer.get(self.name_offset)
        self.htpk_uri = self.string_buffer.get(self.htpk_uri_offset)

        for i in range(self.total_buffer_count):
            self.buffers[i].name = self.string_buffer.get(self.buffers[i].name_offset)
            self.buffers[i].shader_gen_name = self.string_buffer.get(self.buffers[i].shader_gen_name_offset)
            if self.buffers[i].uniform_index < self.uniform_count:
                self.buffers[i].values = self.value_buffer.get(self.buffers[i].offset, self.buffers[i].size)

        for i in range(self.total_uniform_count):
            self.uniforms[i].name = self.string_buffer.get(self.uniforms[i].name_offset)
            self.uniforms[i].shader_gen_name = self.string_buffer.get(self.uniforms[i].shader_gen_name_offset)
            if i < self.uniform_count:
                self.uniforms[i].values = self.value_buffer.get(self.uniforms[i].offset, self.uniforms[i].size)

        for i in range(self.total_texture_count):
            self.textures[i].name = self.string_buffer.get(self.textures[i].name_offset)
            self.textures[i].shader_gen_name = self.string_buffer.get(self.textures[i].shader_gen_name_offset)
            self.textures[i].uri = self.string_buffer.get(self.textures[i].uri_offset)

        for i in range(self.total_sampler_count):
            self.samplers[i].name = self.string_buffer.get(self.samplers[i].name_offset)
            self.samplers[i].shader_gen_name = self.string_buffer.get(self.samplers[i].shader_gen_name_offset)

        for i in range(self.shader_binary_count):
            self.shader_binaries[i].uri = self.string_buffer.get(self.shader_binaries[i].uri_offset)

    def write(self, writer: MessagePackWriter):
        self.update_value_buffer()
        self.update_string_buffer()
        self.regenerate_dependency_table()

        GraphicsBinary.write(self, writer)

        writer.write(self.name_offset)
        writer.write(self.buffer_count)
        writer.write(self.uniform_count)
        writer.write(self.texture_count)
        writer.write(self.sampler_count)
        writer.write(self.total_buffer_count)
        writer.write(self.total_uniform_count)
        writer.write(self.total_texture_count)
        writer.write(self.total_sampler_count)
        writer.write(self.shader_binary_count)
        writer.write(self.shader_program_count)
        writer.write(self.value_buffer_size)
        writer.write(self.string_buffer_size)
        writer.write(self.name_hash)
        writer.write(self.blend_type)
        writer.write(self.blend_factor)
        writer.write(self.render_state_bits)
        writer.write(self.skin_vs_max_bone_count)
        writer.write(self.brdf_type)
        writer.write(self.htpk_uri_offset)

        for buffer in self.buffers:
            buffer.write(writer)

        for uniform in self.uniforms:
            uniform.write(writer)

        for texture in self.textures:
            texture.write(writer)

        for sampler in self.samplers:
            sampler.write(writer)

        for shader_binary in self.shader_binaries:
            shader_binary.write(writer)

        for shader_program in self.shader_programs:
            shader_program.write(writer)

        writer.write(self.value_buffer.to_bytes())
        writer.write(self.string_buffer.to_bytes())

    def update_value_buffer(self):
        value_buffer = ValueBuffer()

        for buffer in self.buffers:
            if buffer.uniform_index < self.uniform_count:
                buffer.offset = value_buffer.put(buffer.values)

        for i in range(self.uniform_count):
            uniform = self.uniforms[i]
            uniform.offset = value_buffer.put(uniform.values)

        self.value_buffer = value_buffer
        self.value_buffer_size = value_buffer.size()

    def update_string_buffer(self):
        string_buffer = StringBuffer()

        self.name_offset = string_buffer.put(self.name)

        for buffer in self.buffers:
            buffer.name_offset = string_buffer.put(buffer.name)
            buffer.shader_gen_name_offset = string_buffer.put(buffer.shader_gen_name)

        for uniform in self.uniforms:
            uniform.name_offset = string_buffer.put(uniform.name)
            uniform.shader_gen_name_offset = string_buffer.put(uniform.shader_gen_name)

        for texture in self.textures:
            texture.name_offset = string_buffer.put(texture.name)
            texture.shader_gen_name_offset = string_buffer.put(texture.shader_gen_name)
            texture.uri_offset = string_buffer.put(texture.uri)

        for sampler in self.samplers:
            sampler.name_offset = string_buffer.put(sampler.name)
            sampler.shader_gen_name_offset = string_buffer.put(sampler.shader_gen_name)

        for binary in self.shader_binaries:
            binary.uri_offset = string_buffer.put(binary.uri)

        self.htpk_uri_offset = string_buffer.put(self.htpk_uri)

        self.string_buffer = string_buffer
        self.string_buffer_size = string_buffer.size()

    def regenerate_dependency_table(self):
        dependencies = {}
        self.hashes = []
        uri = self.dependencies["ref"]
        asset_uri = uri[:(uri.rindex("/") + 1)]

        for binary in self.shader_binaries:
            if binary.uri_hash > 0:
                dependencies[str(binary.uri_hash)] = binary.uri
                self.hashes.append(binary.uri_hash)

        for texture in self.textures:
            if texture.uri_hash > 0:
                dependencies[str(texture.uri_hash)] = texture.uri
                self.hashes.append(texture.uri_hash)

        dependencies["asset_uri"] = asset_uri
        dependencies["ref"] = uri

        self.dependencies = dependencies

    def get_buffer(self, shader_gen_name: str):
        for i in range(len(self.buffers)):
            if self.buffers[i].shader_gen_name is not None \
                    and self.buffers[i].shader_gen_name.upper() == shader_gen_name.upper():
                return self.buffers[i]
        return None

    def set_texture_uri(self, texture_slot: str, uri: str):
        for texture in self.textures:
            if texture.shader_gen_name.upper() == texture_slot.upper():
                texture.uri = uri
                texture.uri_hash32 = Cryptography.hash32(uri)
                texture.uri_hash = Cryptography.uri_hash64(uri)
                break

    def set_buffer_value(self, buffer_name: str, value: list[float]):
        for buffer in self.buffers:
            if buffer.shader_gen_name.upper() == buffer_name.upper():
                buffer.values = value
