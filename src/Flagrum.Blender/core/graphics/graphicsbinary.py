from dataclasses import dataclass

from ..serialization.messagepack.reader import MessagePackReader
from ..serialization.messagepack.writer import MessagePackWriter


@dataclass(init=False)
class GraphicsBinary:
    version: int
    unknown: int
    dependencies: dict[str, str]
    hashes: list[int]

    def __init__(self):
        self.version = 20160705
        self.dependencies = {}
        self.hashes = []

    def read(self, reader: MessagePackReader):
        self.version = reader.read()
        reader.data_version = self.version

        if reader.data_version <= 20141115:
            self.unknown = reader.read()

        self.dependencies = reader.read()

        hash_count = reader.read()
        self.hashes = []
        for i in range(hash_count):
            self.hashes.append(reader.read())

    def write(self, writer: MessagePackWriter):
        writer.write(self.version)
        writer.write(self.dependencies)
        writer.write(self.hashes)
