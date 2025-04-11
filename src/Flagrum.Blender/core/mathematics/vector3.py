from dataclasses import dataclass


@dataclass(init=False)
class Vector3:
    x: float
    y: float
    z: float
