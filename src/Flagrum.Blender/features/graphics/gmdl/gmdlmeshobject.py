from dataclasses import dataclass

from .gmdlmesh import GmdlMesh


@dataclass
class GmdlMeshObject:
    unknown: bool
    name: str
    clusters: list[str]
    meshes: list[GmdlMesh]

    def __init__(self, reader, is_first, version: int):
        if is_first:
            self.unknown = False
        else:
            self.unknown = reader.read()

        self.name = reader.read()

        cluster_count = reader.read()
        self.clusters = []
        for i in range(cluster_count):
            self.clusters.append(reader.read())

        mesh_count = reader.read()
        self.meshes = []
        for i in range(mesh_count):
            self.meshes.append(GmdlMesh(reader, version))
