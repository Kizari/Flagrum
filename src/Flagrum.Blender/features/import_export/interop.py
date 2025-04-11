import json
import os
import subprocess
import tempfile
from os.path import dirname, join, splitext
from zipfile import ZipFile, ZIP_STORED

from ..environment.group import Group
from ..graphics.fmd.entities import Gpubin


class Interop:

    @staticmethod
    def run_command(command) -> bytes:
        flagrum_path = join(dirname(__file__), "..", "..", "lib", "Flagrum.Blender.exe")
        command = "\"" + flagrum_path + "\" " + command
        process = subprocess.Popen(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=False)
        (output, error) = process.communicate()
        status = process.wait()

        if error is not None and status != 0:
            print(error.decode('UTF-8'))
            raise RuntimeError("An exception occured during the operation of Flagrum.Blender.exe")
        elif status == 0 and output != b'':
            return output

    @staticmethod
    def import_material_inputs(gfxbin_path) -> dict[str, list[float]]:
        tempfile_path = tempfile.NamedTemporaryFile().name + ".json"
        command = "material -i \"" + gfxbin_path + "\" -o \"" + tempfile_path + "\""
        Interop.run_command(command)

        import_file = open(tempfile_path, mode='r')
        import_data = import_file.read()

        data: dict[str, list[float]] = json.loads(import_data)

        import_file.close()
        os.remove(tempfile_path)
        return data

    @staticmethod
    def import_gpubin(gfxbin_path: str, import_lods: bool, import_vems: bool) -> bytes:
        tempfile_path = tempfile.NamedTemporaryFile().name + ".bin"
        command = f"gpubin \"{gfxbin_path}\" \"{tempfile_path}\" {str(import_lods)} {str(import_vems)}"
        Interop.run_command(command)

        import_file = open(tempfile_path, mode='rb')
        buffer = import_file.read()
        import_file.close()
        os.remove(tempfile_path)
        return buffer

    @staticmethod
    def import_environment(data_directory: str, xml_path: str) -> Group:
        command = f"environment \"{data_directory}\" \"{xml_path}\""
        buffer = Interop.run_command(command)
        return Group.from_dict(json.loads(buffer))

    @staticmethod
    def export_mesh(target_path, data: Gpubin):
        # Serialize the model data
        json_data = json.dumps(data, default=lambda o: o.__dict__, sort_keys=True, indent=0)

        with ZipFile(target_path, mode='w', compression=ZIP_STORED, allowZip64=True, compresslevel=None) as fmd:
            # Add model data to the zip
            fmd.writestr("data.json", json_data)
            templates = []

            for mesh in data.Meshes:
                if mesh.Material:
                    # Add material template if not already in the zip
                    template_path = join(dirname(__file__), "..", "lib", "templates", mesh.Material.Id + ".json")
                    if mesh.Material.Id not in templates:
                        fmd.write(template_path, arcname="materials/" + mesh.Material.Id + ".json")
                        templates.append(mesh.Material.Id)

                    # Add textures to the zip
                    for texture_id in mesh.Material.Textures:
                        texture_path = mesh.Material.Textures[texture_id]
                        if texture_path != '':
                            filename, file_extension = splitext(texture_path)
                            fmd.write(texture_path, arcname=mesh.Name + "/" + texture_id + file_extension)
