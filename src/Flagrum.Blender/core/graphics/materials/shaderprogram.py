from dataclasses import dataclass

from ...serialization.messagepack.writer import MessagePackWriter


@dataclass
class ShaderProgram:
    low_key: int
    high_key: int
    cs_binary_index: int
    vs_binary_index: int
    hs_binary_index: int
    ds_binary_index: int
    gs_binary_index: int
    ps_binary_index: int

    def __init__(self, reader):
        self.low_key = reader.read()
        self.high_key = reader.read()
        self.cs_binary_index = reader.read()
        self.vs_binary_index = reader.read()
        self.hs_binary_index = reader.read()
        self.ds_binary_index = reader.read()
        self.gs_binary_index = reader.read()
        self.ps_binary_index = reader.read()

    def write(self, writer: MessagePackWriter):
        writer.write(self.low_key)
        writer.write(self.high_key)
        writer.write(self.cs_binary_index)
        writer.write(self.vs_binary_index)
        writer.write(self.hs_binary_index)
        writer.write(self.ds_binary_index)
        writer.write(self.gs_binary_index)
        writer.write(self.ps_binary_index)
