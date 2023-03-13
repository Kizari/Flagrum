from dataclasses import dataclass


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
