from dataclasses import dataclass


@dataclass(init=False)
class UV:
    U: float
    V: float


@dataclass(init=False)
class Vector3:
    X: float
    Y: float
    Z: float


@dataclass(init=False)
class Normal:
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
    UVs: list[UV]


@dataclass
class MaterialPropertyMetadata:
    property_name: str
    is_relevant: bool
    importance: int
    property_type: str
    default_value: object


@dataclass(init=False)
class MaterialData:
    Id: str
    Name: str
    Textures: dict[str, str]
    Inputs: dict[str, list[float]]


@dataclass(init=False)
class MeshData:
    Name: str
    VertexPositions: list[Vector3]
    FaceIndices: list[list[int]]
    Normals: list[Normal]
    # ColorMaps: list[ColorMap]
    WeightIndices: list[list[list[int]]]
    WeightValues: list[list[list[float]]]
    UVMaps: list[UVMap]
    Material: MaterialData


@dataclass(init=False)
class Gpubin:
    Target: str
    Uuid: str
    Title: str
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
