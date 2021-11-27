import ctypes
import json
from zipfile import ZipFile, ZIP_STORED
from datetime import datetime
from os.path import dirname, join, splitext
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
    def export_mesh(target_path, data: Gpubin):
        json_data = json.dumps(data, default=lambda o: o.__dict__, sort_keys=True, indent=0)

        with ZipFile(target_path, mode='w', compression=ZIP_STORED, allowZip64=True, compresslevel=None) as fmd:
            fmd.writestr("data.json", json_data)
            for mesh in data.Meshes:
                if mesh.Material:
                    for texture_id in mesh.Material.Textures:
                        texture_path = mesh.Material.Textures[texture_id]
                        if texture_path != '':
                            filename, file_extension = splitext(texture_path)
                            fmd.write(texture_path, arcname=mesh.Name + "/" + texture_id + file_extension)