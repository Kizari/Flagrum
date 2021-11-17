import ctypes
import json
from datetime import datetime
from os.path import dirname, join
from types import SimpleNamespace

from .entities import Gpubin

interop = ctypes.cdll.LoadLibrary(join(dirname(__file__), "lib", "Flagrum.InteropNE.dll"))
interop.Import.restype = ctypes.c_char_p
interop.Export.restype = ctypes.c_char_p


class Interop:

    @staticmethod
    def import_mesh(gfxbin_path):
        import_path = interop.Import(gfxbin_path)
        import_file = open(import_path, mode='r')
        import_data = import_file.read()

        data: Gpubin = json.loads(import_data, object_hook=lambda d: SimpleNamespace(**d))
        return data

    @staticmethod
    def export_mesh(target_path, data):
        print(datetime.now())
        print("Serializing data...")
        json_data = json.dumps(data, default=lambda o: o.__dict__, sort_keys=True, indent=0)
        output_path = target_path + ".json"

        print(datetime.now())
        print("Writing file...")
        with open(output_path, 'w') as file:
            file.write(json_data)

        print(datetime.now())
        print("Calling .NET")
        success = interop.Export(output_path)

        print(datetime.now())
        print("Done!")
