import ctypes
import json
from types import SimpleNamespace

from .entities import Gpubin

interop = ctypes.cdll.LoadLibrary("C:\\Code\\Flagrum\\Flagrum.Interop\\bin\\Debug\\net6.0\\Flagrum.InteropNE.dll")


class Interop:

    @staticmethod
    def import_mesh(gfxbin_path):
        interop.Import.restype = ctypes.c_char_p

        import_path = interop.Import(gfxbin_path)
        import_file = open(import_path, mode='r')
        import_data = import_file.read()

        data: Gpubin = json.loads(import_data, object_hook=lambda d: SimpleNamespace(**d))
        return data
