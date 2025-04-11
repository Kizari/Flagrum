from dataclasses import dataclass

from .gamemodelmeshpart import GameModelMeshPart
from .gamemodelsubgeometry import GameModelSubgeometry
from .vertexelement import VertexElement
from .vertexelementformat import VertexElementFormat
from .vertexstream import VertexStream
from ...mathematics.axisalignedboundingbox import AxisAlignedBoundingBox
from ...mathematics.orientedboundingbox import OrientedBoundingBox
from ...serialization.messagepack.reader import MessagePackReader
from ...serialization.messagepack.writer import MessagePackWriter


@dataclass
class GameModelMesh:
    name: str
    unknown: int
    bone_ids: list[int]
    vertex_layout_type: int
    unknown2: bool
    aabb: AxisAlignedBoundingBox
    is_oriented_bb: bool
    oriented_bounding_box: OrientedBoundingBox
    primitive_type: int
    face_index_count: int
    face_index_type: int
    gpubin_index: int
    face_index_offset: int
    face_index_size: int
    vertex_count: int
    vertex_streams: list[VertexStream]
    vertex_buffer_offset: int
    vertex_buffer_size: int
    instance_number: int
    subgeometry_count: int
    subgeometries: list[GameModelSubgeometry]
    unknown6: int
    unknown_offset: int
    unknown_size: int
    material_hash: int
    draw_priority_offset: int
    unknown7: bool
    unknown8: bool
    lod_near: float
    lod_far: float
    lod_fade: float
    unknown11: bool
    unknown12: bool
    parts_id: int
    parts: list[GameModelMeshPart]
    unknown9: bool
    flags: int
    unknown10: bool
    breakable_bone_index: int
    low_lod_shadow_cascade_no: int

    face_indices: list[int]
    semantics: dict[str, list]

    def __init__(self):
        self.unknown = 0
        self.bone_ids = []
        self.vertex_layout_type = 0  # NULL
        self.unknown2 = False
        self.is_oriented_bb = True
        self.primitive_type = 3
        self.instance_number = 0
        self.subgeometries = [GameModelSubgeometry()]
        self.draw_priority_offset = 0
        self.unknown7 = False
        self.unknown8 = False
        self.lod_near = 0.0
        self.lod_far = 160.0
        self.lod_fade = 0.0
        self.unknown11 = False
        self.unknown12 = False
        self.parts_id = 0
        self.parts = []
        self.unknown9 = False
        self.flags = 263296
        self.unknown10 = True
        self.breakable_bone_index = 4294967295
        self.low_lod_shadow_cascade_no = 2
        self.vertex_streams = []
        self.face_indices = []
        self.semantics = {}

        stream = VertexStream()
        stream.slot = 0
        stream.stride = 12
        stream.type = 0  # Vertex
        element = VertexElement()
        element.format = VertexElementFormat.XYZ32_Float
        element.offset = 0
        element.semantic = "POSITION0"
        stream.elements = [element]
        self.vertex_streams.append(stream)

        stream = VertexStream()
        stream.slot = 1
        stream.stride = 12
        stream.type = 0
        stream.elements = []

        element = VertexElement()
        element.format = VertexElementFormat.XYZW8_SintN
        element.offset = 0
        element.semantic = "NORMAL0"
        stream.elements.append(element)

        element = VertexElement()
        element.format = VertexElementFormat.XYZW8_SintN
        element.offset = 4
        element.semantic = "TANGENT0"
        stream.elements.append(element)

        element = VertexElement()
        element.format = VertexElementFormat.XY16_Float
        element.offset = 8
        element.semantic = "TEXCOORD0"
        stream.elements.append(element)

        self.vertex_streams.append(stream)

    def read(self, reader: MessagePackReader):
        self.name = reader.read()

        if reader.data_version < 20220707:
            self.unknown = reader.read()
            bone_id_count = reader.read()
            self.bone_ids = []
            for i in range(bone_id_count):
                self.bone_ids.append(reader.read())

        self.vertex_layout_type = reader.read()
        self.unknown2 = reader.read()

        self.aabb = AxisAlignedBoundingBox()
        self.aabb.read(reader)

        if reader.data_version >= 20160705:
            self.is_oriented_bb = reader.read()
            self.oriented_bounding_box = OrientedBoundingBox()
            self.oriented_bounding_box.read(reader)

        self.primitive_type = reader.read()
        self.face_index_count = reader.read()
        self.face_index_type = reader.read()

        if reader.data_version >= 20220707:
            self.gpubin_index = reader.read()
        else:
            self.gpubin_index = 0

        self.face_index_offset = reader.read()
        self.face_index_size = reader.read()
        self.vertex_count = reader.read()

        vertex_stream_count = reader.read()
        self.vertex_streams = []
        for i in range(vertex_stream_count):
            vertex_stream = VertexStream()
            vertex_stream.read(reader)
            self.vertex_streams.append(vertex_stream)

        self.vertex_buffer_offset = reader.read()
        self.vertex_buffer_size = reader.read()

        if 20150413 < reader.data_version < 20220707:
            self.instance_number = reader.read()

        self.subgeometry_count = reader.read()
        self.subgeometries = []
        for i in range(self.subgeometry_count):
            subgeometry = GameModelSubgeometry()
            subgeometry.read(reader)
            self.subgeometries.append(subgeometry)

        if reader.data_version >= 20220707:
            self.unknown6 = reader.read()
            self.unknown_offset = reader.read()
            self.unknown_size = reader.read()

        self.material_hash = reader.read()

        if reader.data_version >= 20140623:
            self.draw_priority_offset = reader.read()
            self.unknown7 = reader.read()
            self.unknown8 = reader.read()
            self.lod_near = reader.read()
            self.lod_far = reader.read()
            self.lod_fade = reader.read()

        if 20140814 < reader.data_version < 20220707:
            self.unknown11 = reader.read()
            
        if 20141112 < reader.data_version < 20220707:
            self.unknown12 = reader.read()

        if reader.data_version >= 20140815:
            self.parts_id = reader.read()
            
        if reader.data_version >= 20141115:
            parts_count = reader.read()
            self.parts = []
            for i in range(parts_count):
                part = GameModelMeshPart()
                part.read(reader)
                self.parts.append(part)

        if reader.data_version >= 20150413:
            self.unknown9 = reader.read()
            
        if reader.data_version >= 20150430:
            self.flags = reader.read()
            
        if reader.data_version >= 20150512:
            self.unknown10 = reader.read()
            
        if reader.data_version < 20160420:
            self.breakable_bone_index = 4294967295
            self.low_lod_shadow_cascade_no = 2
        else:
            self.breakable_bone_index = reader.read()
            self.low_lod_shadow_cascade_no = reader.read()

    def write(self, writer: MessagePackWriter):
        writer.write(self.name)
        writer.write(self.unknown)
        writer.write(self.bone_ids)
        writer.write(self.vertex_layout_type)
        writer.write(self.unknown2)

        self.aabb.write(writer)
        writer.write(self.is_oriented_bb)
        self.oriented_bounding_box.write(writer)

        writer.write(self.primitive_type)
        writer.write(self.face_index_count)
        writer.write(self.face_index_type)
        writer.write(self.face_index_offset)
        writer.write(self.face_index_size)

        writer.write(self.vertex_count)
        writer.write(self.vertex_streams)
        writer.write(self.vertex_buffer_offset)
        writer.write(self.vertex_buffer_size)

        writer.write(self.instance_number)

        writer.write(self.subgeometries)

        writer.write(self.material_hash)
        writer.write(self.draw_priority_offset)
        writer.write(self.unknown7)
        writer.write(self.unknown8)
        writer.write(self.lod_near)
        writer.write(self.lod_far)
        writer.write(self.lod_fade)

        writer.write(self.unknown11)
        writer.write(self.unknown12)

        writer.write(self.parts_id)
        writer.write(self.parts)
        writer.write(self.unknown9)
        writer.write(self.flags)
        writer.write(self.unknown10)
        writer.write(self.breakable_bone_index)
        writer.write(self.low_lod_shadow_cascade_no)
