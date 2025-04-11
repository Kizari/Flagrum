from dataclasses import dataclass


@dataclass
class GfxbinHeader:
    version: int
    dependencies: dict[str, str]
    hashes: list[int]

    def __init__(self, reader):
        self.version = reader.read()
        self.dependencies = reader.read()

        hash_count = reader.read()
        self.hashes = []
        for i in range(hash_count):
            self.hashes.append(reader.read())
