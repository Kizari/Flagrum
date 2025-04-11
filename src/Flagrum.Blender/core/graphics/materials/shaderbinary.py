from dataclasses import dataclass

from ...serialization.messagepack.writer import MessagePackWriter


@dataclass
class ShaderBinary:
    uri_hash: int
    uri_offset: int

    uri: str

    def __init__(self, reader):
        self.uri_hash = reader.read()
        self.uri_offset = reader.read()

    def write(self, writer: MessagePackWriter):
        writer.write(self.uri_hash)
        writer.write(self.uri_offset)
