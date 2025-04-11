from dataclasses import dataclass

from ..mathematics.vector3 import Vector3


@dataclass
class Matrix3x4:
    rows: list[Vector3]

    def __init__(self):
        self.rows = []
