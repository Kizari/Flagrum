from dataclasses import dataclass

from .gamemodelmesh import GameModelMesh
from ...serialization.messagepack.reader import MessagePackReader
from ...serialization.messagepack.writer import MessagePackWriter


@dataclass
class GameModelMeshObject:
    unknown: bool
    name: str
    clusters: list[str]
    meshes: list[GameModelMesh]
    is_first: bool  # MessagePackWriter will set this where needed

    def __init__(self):
        self.name = "Default"
        self.clusters = ["CLUSTER_NAME"]
        self.meshes = []

    def read(self, reader: MessagePackReader, is_first: bool):
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
            mesh = GameModelMesh()
            mesh.read(reader)
            self.meshes.append(mesh)

    def write(self, writer: MessagePackWriter):
        # if self.is_first:
        #    writer.write(self.unknown)

        writer.write(self.name)
        writer.write(self.clusters)
        writer.write(self.meshes)
