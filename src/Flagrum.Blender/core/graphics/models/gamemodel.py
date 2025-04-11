import struct
from dataclasses import dataclass
from typing import IO

from .gamemodelbone import GameModelBone
from .gamemodelmeshobject import GameModelMeshObject
from .gamemodelnode import GameModelNode
from .gamemodelpart import GameModelPart
from .vertexelementformat import VertexElementFormat
from ..graphicsbinary import GraphicsBinary
from ...mathematics.axisalignedboundingbox import AxisAlignedBoundingBox
from ...serialization.messagepack.writer import MessagePackWriter
from ...serialization.streamhelper import StreamHelper


@dataclass
class GameModel(GraphicsBinary):
    aabb: AxisAlignedBoundingBox
    instance_name_format: str
    shader_class_format: str
    shader_sampler_description_format: list[int]
    shader_parameter_list_format: list[int]
    child_class_format: list[int]
    bones: list[GameModelBone]
    nodes: list[GameModelNode]
    unknown: float
    gpubin_count: int
    gpubin_hashes: list[int]
    mesh_objects: list[GameModelMeshObject]
    unknown2: bool
    name: str
    parts: list[GameModelPart]
    unknown3: float
    unknown4: float
    unknown5: float
    unknown6: float
    unknown7: float
    gpubin_size_count: int
    gpubin_sizes: list[int]
    has_psd_path: bool
    psd_path_hash: int

    def __init__(self):
        GraphicsBinary.__init__(self)
        self.instance_name_format = ""
        self.shader_class_format = ""
        self.shader_sampler_description_format = []
        self.shader_parameter_list_format = []
        self.child_class_format = []
        self.bones = []
        self.nodes = []
        self.unknown = 0.0
        self.gpubin_count = 1
        self.gpubin_hashes = [0]
        self.mesh_objects = []
        self.unknown2 = True
        self.parts = []

    def read(self, reader):
        GraphicsBinary.read(self, reader)

        self.aabb = AxisAlignedBoundingBox()
        self.aabb.read(reader)

        if self.version < 20220707:
            self.instance_name_format = reader.read()
            self.shader_class_format = reader.read()
            # Skip storing these since they're always empty and MessagePackReader is handling arrays poorly right now
            reader.read()  # self.shader_sampler_description_format
            reader.read()  # self.shader_parameter_list_format
            reader.read()  # self.child_class_format

        bone_count = reader.read()
        self.bones = []
        for i in range(bone_count):
            bone = GameModelBone()
            bone.read(reader)
            self.bones.append(bone)

        node_count = reader.read()
        self.nodes = []
        for i in range(node_count):
            node = GameModelNode()
            node.read(reader, i == 0)
            self.nodes.append(node)

        if self.version >= 20220707:
            self.unknown = reader.read()
            self.gpubin_count = reader.read()
        else:
            self.gpubin_count = 1

        if reader.data_version >= 20141113:
            self.gpubin_hashes = []
            for i in range(self.gpubin_count):
                self.gpubin_hashes.append(reader.read())

        mesh_object_count = reader.read()
        self.mesh_objects = []
        for i in range(mesh_object_count):
            mesh_object = GameModelMeshObject()
            mesh_object.read(reader, i == 0)
            self.mesh_objects.append(mesh_object)

        if reader.data_version >= 20140623:
            if len(self.gpubin_hashes) > 0:
                self.unknown2 = reader.read()
            self.name = reader.read()

        if len(self.gpubin_hashes) > 0 and 20140722 < reader.data_version < 20140929:
            self.has_psd_path = reader.read()
            self.psd_path_hash = reader.read()

        if reader.data_version >= 20140815:
            parts_count = reader.read()
            self.parts = []
            for i in range(parts_count):
                part = GameModelPart()
                part.read(reader)
                self.parts.append(part)

        if len(self.gpubin_hashes) > 0 and self.version >= 20220707:
            self.unknown3 = reader.read()
            self.unknown4 = reader.read()
            self.unknown5 = reader.read()
            self.unknown6 = reader.read()
            self.unknown7 = reader.read()

            self.gpubin_size_count = reader.read()
            self.gpubin_sizes = []
            for i in range(self.gpubin_size_count):
                self.gpubin_sizes.append(reader.read())

    def write(self, writer: MessagePackWriter):
        GraphicsBinary.write(self, writer)

        self.aabb.write(writer)

        writer.write(self.instance_name_format)
        writer.write(self.shader_class_format)
        writer.write(self.shader_sampler_description_format)
        writer.write(self.shader_parameter_list_format)
        writer.write(self.child_class_format)

        writer.write(self.bones)
        writer.write(self.nodes)
        writer.write(self.gpubin_hashes[0])
        writer.write(self.mesh_objects)
        writer.write(self.unknown2)
        writer.write(self.name)
        writer.write(self.parts)

    def write_gpubin(self, stream: IO):
        for mesh_object in self.mesh_objects:
            for mesh in mesh_object.meshes:
                mesh.face_index_offset = stream.tell()
                mesh.vertex_count = len(mesh.semantics["POSITION0"])
                mesh.face_index_count = len(mesh.face_indices)

                if mesh.vertex_count > 65535:
                    mesh.face_index_type = 1
                    mesh.face_index_size = mesh.face_index_count * 4
                else:
                    mesh.face_index_type = 0
                    mesh.face_index_size = mesh.face_index_count * 2

                if mesh.face_index_type == 0:
                    for i in range(mesh.face_index_count):
                        stream.write(struct.pack("<H", mesh.face_indices[i]))
                else:
                    for i in range(mesh.face_index_count):
                        stream.write(struct.pack("<I", mesh.face_indices[i]))

                StreamHelper.write_align(stream, 128)
                mesh.vertex_buffer_offset = stream.tell()

                for vertex_stream in mesh.vertex_streams:
                    vertex_stream.offset = stream.tell() - mesh.vertex_buffer_offset

                    for i in range(mesh.vertex_count):
                        for element in vertex_stream.elements:
                            elements = mesh.semantics[element.semantic][i]
                            if element.format == VertexElementFormat.XYZ32_Float:
                                stream.write(struct.pack("<f", elements[0]))
                                stream.write(struct.pack("<f", elements[1]))
                                stream.write(struct.pack("<f", elements[2]))
                            elif element.format == VertexElementFormat.XY16_SintN:
                                stream.write(struct.pack("<h", int(elements[0] * 32767)))
                                stream.write(struct.pack("<h", int(elements[1] * 32767)))
                            elif element.format == VertexElementFormat.XY16_UintN:
                                stream.write(struct.pack("<H", int(elements[0] * 65535)))
                                stream.write(struct.pack("<H", int(elements[1] * 65535)))
                            elif element.format == VertexElementFormat.XY16_Float:
                                stream.write(struct.pack("<e", elements[0]))
                                stream.write(struct.pack("<e", elements[1]))
                            elif element.format == VertexElementFormat.XY32_Float:
                                stream.write(struct.pack("<f", elements[0]))
                                stream.write(struct.pack("<f", elements[1]))
                            elif element.format == VertexElementFormat.XYZW8_UintN \
                                    or element.format == VertexElementFormat.XYZW8_Uint:
                                stream.write(struct.pack("<B", int(elements[0] * 255)))
                                stream.write(struct.pack("<B", int(elements[1] * 255)))
                                stream.write(struct.pack("<B", int(elements[2] * 255)))
                                stream.write(struct.pack("<B", int(elements[3] * 255)))
                            elif element.format == VertexElementFormat.XYZW8_SintN \
                                    or element.format == VertexElementFormat.XYZW8_Sint:
                                stream.write(struct.pack("<b", int(elements[0] * 127)))
                                stream.write(struct.pack("<b", int(elements[1] * 127)))
                                stream.write(struct.pack("<b", int(elements[2] * 127)))
                                stream.write(struct.pack("<b", int(elements[3] * 127)))
                            elif element.format == VertexElementFormat.XYZW16_Float:
                                stream.write(struct.pack("<e", elements[0]))
                                stream.write(struct.pack("<e", elements[1]))
                                stream.write(struct.pack("<e", elements[2]))
                                stream.write(struct.pack("<e", elements[3]))
                            elif element.format == VertexElementFormat.XYZW16_Uint:
                                stream.write(struct.pack("<H", elements[0]))
                                stream.write(struct.pack("<H", elements[1]))
                                stream.write(struct.pack("<H", elements[2]))
                                stream.write(struct.pack("<H", elements[3]))
                            elif element.format == VertexElementFormat.XYZW32_Uint:
                                stream.write(struct.pack("<I", elements[0]))
                                stream.write(struct.pack("<I", elements[1]))
                                stream.write(struct.pack("<I", elements[2]))
                                stream.write(struct.pack("<I", elements[3]))
                            else:
                                raise Exception(f"Vertex element format {element.format} not currently supported")

                mesh.vertex_buffer_size = stream.tell() - mesh.vertex_buffer_offset
                StreamHelper.write_align(stream, 128)
