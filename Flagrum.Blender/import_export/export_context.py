from dataclasses import dataclass


@dataclass
class ExportContext:
    smooth_normals: bool
    distance: float
