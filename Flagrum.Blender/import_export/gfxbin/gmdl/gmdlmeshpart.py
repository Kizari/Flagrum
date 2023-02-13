from dataclasses import dataclass


@dataclass
class GmdlMeshPart:
    parts_id: int
    start_index: int
    index_count: int
    
    def __init__(self, reader):
        self.parts_id = reader.read()
        self.start_index = reader.read()
        self.index_count = reader.read()
