from dataclasses import dataclass

from .gmdlbone import GmdlBone
from .gmdlmeshobject import GmdlMeshObject
from .gmdlmodelpart import GmdlModelPart
from .gmdlnode import GmdlNode
from ..gfxbinheader import GfxbinHeader


@dataclass
class Gmdl:
    header: GfxbinHeader
    aabb: list[list[float]]
    instance_name_format: int
    shader_class_format: int
    shader_sampler_description_format: int
    shader_parameter_list_format: int
    child_class_format: int
    bones: list[GmdlBone]
    nodes: list[GmdlNode]
    unknown: float
    gpubin_count: int
    gpubin_hashes: list[int]
    mesh_objects: list[GmdlMeshObject]
    unknown2: bool
    name: str
    parts: list[GmdlModelPart]
    unknown3: float
    unknown4: float
    unknown5: float
    unknown6: float
    unknown7: float
    gpubin_size_count: int
    gpubin_sizes: list[int]

    def __init__(self, reader):
        self.header = GfxbinHeader(reader)
        self.aabb = [[reader.read(), reader.read(), reader.read()], [reader.read(), reader.read(), reader.read()]]

        if self.header.version < 20220707:
            self.instance_name_format = reader.read()
            self.shader_class_format = reader.read()
            self.shader_sampler_description_format = reader.read()
            self.shader_parameter_list_format = reader.read()
            self.child_class_format = reader.read()

        bone_count = reader.read()
        self.bones = []
        for i in range(bone_count):
            self.bones.append(GmdlBone(reader, self.header.version))

        node_count = reader.read()
        self.nodes = []
        for i in range(node_count):
            self.nodes.append(GmdlNode(reader, i == 0, self.header.version))

        if self.header.version >= 20220707:
            self.unknown = reader.read()
            self.gpubin_count = reader.read()
        else:
            self.gpubin_count = 1

        self.gpubin_hashes = []
        for i in range(self.gpubin_count):
            self.gpubin_hashes.append(reader.read())

        mesh_object_count = reader.read()
        self.mesh_objects = []
        for i in range(mesh_object_count):
            self.mesh_objects.append(GmdlMeshObject(reader, i == 0, self.header.version))

        self.unknown2 = reader.read()
        self.name = reader.read()

        parts_count = reader.read()
        self.parts = []
        for i in range(parts_count):
            self.parts.append(GmdlModelPart(reader))

        if self.header.version >= 20220707:
            self.unknown3 = reader.read()
            self.unknown4 = reader.read()
            self.unknown5 = reader.read()
            self.unknown6 = reader.read()
            self.unknown7 = reader.read()

            self.gpubin_size_count = reader.read()
            self.gpubin_sizes = []
            for i in range(self.gpubin_size_count):
                self.gpubin_sizes.append(reader.read())
