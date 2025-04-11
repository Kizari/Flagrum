from dataclasses import dataclass

from .gmdlmeshpart import GmdlMeshPart
from .gmdlsubgeometry import GmdlSubgeometry
from .gmdlvertexstream import GmdlVertexStream


@dataclass
class GmdlMesh:
    name: str
    unknown: int
    bone_ids: list[int]
    vertex_layout_type: int
    unknown2: bool
    AABB: list[list[float]]
    is_oriented_bb: bool
    orientedBB: list[list[float]]
    primitive_type: int
    face_index_count: int
    face_index_type: int
    gpubin_index: int
    face_index_offset: int
    face_index_size: int
    vertex_count: int
    vertex_streams: list[GmdlVertexStream]
    vertex_buffer_offset: int
    vertex_buffer_size: int
    instance_number: int
    subgeometry_count: int
    subgeometries: list[GmdlSubgeometry]
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
    parts: list[GmdlMeshPart]
    unknown9: bool
    flags: int
    unknown10: bool
    breakable_bone_index: int
    low_lod_shadow_cascade_no: int

    def __init__(self, reader, version: int):
        self.name = reader.read()

        if version < 20220707:
            self.unknown = reader.read()
            bone_id_count = reader.read()
            self.bone_ids = []
            for i in range(bone_id_count):
                self.bone_ids.append(reader.read())

        self.vertex_layout_type = reader.read()
        self.unknown2 = reader.read()

        self.AABB = [
            [reader.read(), reader.read(), reader.read()],
            [reader.read(), reader.read(), reader.read()]
        ]

        self.is_oriented_bb = reader.read()

        self.orientedBB = [
            [reader.read(), reader.read(), reader.read()],
            [reader.read(), reader.read(), reader.read()],
            [reader.read(), reader.read(), reader.read()],
            [reader.read(), reader.read(), reader.read()]
        ]

        self.primitive_type = reader.read()
        self.face_index_count = reader.read()
        self.face_index_type = reader.read()

        if version >= 20220707:
            self.gpubin_index = reader.read()
        else:
            self.gpubin_index = 0

        self.face_index_offset = reader.read()
        self.face_index_size = reader.read()
        self.vertex_count = reader.read()

        vertex_stream_count = reader.read()
        self.vertex_streams = []
        for i in range(vertex_stream_count):
            self.vertex_streams.append(GmdlVertexStream(reader))

        self.vertex_buffer_offset = reader.read()
        self.vertex_buffer_size = reader.read()

        if version < 20220707:
            self.instance_number = reader.read()

        self.subgeometry_count = reader.read()
        self.subgeometries = []
        for i in range(self.subgeometry_count):
            self.subgeometries.append(GmdlSubgeometry(reader))

        if version >= 20220707:
            self.unknown6 = reader.read()
            self.unknown_offset = reader.read()
            self.unknown_size = reader.read()

        self.material_hash = reader.read()
        self.draw_priority_offset = reader.read()
        self.unknown7 = reader.read()
        self.unknown8 = reader.read()
        self.lod_near = reader.read()
        self.lod_far = reader.read()
        self.lod_fade = reader.read()

        if version < 20220707:
            self.unknown11 = reader.read()
            self.unknown12 = reader.read()

        self.parts_id = reader.read()
        parts_count = reader.read()
        self.parts = []
        for i in range(parts_count):
            self.parts.append(GmdlMeshPart(reader))

        self.unknown9 = reader.read()
        self.flags = reader.read()
        self.unknown10 = reader.read()
        self.breakable_bone_index = reader.read()
        self.low_lod_shadow_cascade_no = reader.read()
