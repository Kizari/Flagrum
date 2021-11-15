from dataclasses import dataclass


@dataclass(init=False)
class Vector2:
    X: int
    Y: int


@dataclass(init=False)
class Vector3:
    X: float
    Y: float
    Z: float


@dataclass(init=False)
class Vector4:
    X: int
    Y: int
    Z: int
    W: int


@dataclass(init=False)
class Color4:
    R: int
    G: int
    B: int
    A: int


@dataclass(init=False)
class VertexWeight:
    VertexIndex: int
    BoneIndex: int
    Weight: int


@dataclass(init=False)
class ColorMap:
    Colors: list[Color4]


@dataclass(init=False)
class UVMap:
    Coords: list[Vector2]


@dataclass(init=False)
class MeshData:
    Name: str
    VertexPositions: list[Vector3]
    Faces: list[list[int]]
    Normals: list[Vector4]
    ColorMaps: list[ColorMap]
    WeightMaps: list[list[VertexWeight]]
    UVMaps: list[UVMap]


@dataclass(init=False)
class Gpubin:
    Meshes: list[MeshData]
    BoneTable: dict[int, str]


class ArmatureData:
    def __init__(self):
        self.parent_IDs = []
        self.bones = []


class BoneData:
    def __init__(self):
        self.id = 0
        self.name = ""
        self.head_position_matrix = 0
