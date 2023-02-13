from dataclasses import dataclass


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
