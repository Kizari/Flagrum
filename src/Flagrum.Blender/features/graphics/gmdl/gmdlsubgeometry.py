from dataclasses import dataclass


@dataclass
class GmdlSubgeometry:
    AABB: list[list[float]]
    start_index: int
    primitive_count: int
    cluster_index_bitflag: int
    draw_order: int

    def __init__(self, reader):
        self.AABB = [[reader.read(), reader.read(), reader.read()], [reader.read(), reader.read(), reader.read()]]
        self.start_index = reader.read()
        self.primitive_count = reader.read()
        self.cluster_index_bitflag = reader.read()
        self.draw_order = reader.read()
