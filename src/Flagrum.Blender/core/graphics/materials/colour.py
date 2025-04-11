from dataclasses import dataclass

from ...serialization.messagepack.writer import MessagePackWriter


@dataclass
class Colour:
    R: float
    G: float
    B: float
    A: float

    def __init__(self, reader):
        self.R = reader.read()
        self.G = reader.read()
        self.B = reader.read()
        self.A = reader.read()

    def write(self, writer: MessagePackWriter):
        writer.write(self.R)
        writer.write(self.G)
        writer.write(self.B)
        writer.write(self.A)
