import json
import os
import subprocess
import tempfile
from os.path import dirname, join, splitext
from types import SimpleNamespace
from zipfile import ZipFile, ZIP_STORED

from ..entities import Gpubin


class Interop:

    @staticmethod
    def import_mesh(gfxbin_path):
        tempfile_path = tempfile.NamedTemporaryFile().name + ".json"
        flagrum_path = join(dirname(__file__), "..", "lib", "Flagrum.Blender.exe")
        command = "\"" + flagrum_path + "\" import -i \"" + gfxbin_path + "\" -o \"" + tempfile_path + "\""
        process = subprocess.Popen(command, stdout=subprocess.PIPE, shell=True)
        (output, err) = process.communicate()
        status = process.wait()
        print(output)
        print(err)

        import_file = open(tempfile_path, mode='r')
        import_data = import_file.read()

        data: Gpubin = json.loads(import_data, object_hook=lambda d: SimpleNamespace(**d))

        import_file.close()
        os.remove(tempfile_path)
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
