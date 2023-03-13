from dataclasses import dataclass


@dataclass
class ShaderBinary:
    resource_file_hash: int
    uri_offset: int

    def __init__(self, reader):
        self.resource_file_hash = reader.read()
        self.uri_offset = reader.read()
