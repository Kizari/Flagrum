import os
from dataclasses import dataclass

import bpy
from bpy.types import Material

from ..graphics.gfxbinheader import GfxbinHeader


@dataclass(init=False)
class ImportContext:
    gfxbin_path: str
    import_lods: bool
    import_vems: bool
    path_without_extension: str
    amdl_path: str
    collection: bpy.types.Collection
    materials: dict[str, Material]
    texture_slots: dict[str, bool]
    base_directory: str
    base_uri: str
    name_without_extension: str

    def __init__(self, gfxbin_file_path, import_lods, import_vems, collection=None):
        gfxbin_file_path = gfxbin_file_path.replace('/', '\\')
        self.gfxbin_path = gfxbin_file_path
        self.import_lods = import_lods
        self.import_vems = import_vems
        self.path_without_extension = gfxbin_file_path.replace(".gmdl.gfxbin", "")
        self.materials = {}
        self.texture_slots = {}

        file_name = gfxbin_file_path.split("\\")[-1]
        group_name = ""
        for string in file_name.split("."):
            if string != "gmdl" and string != "gfxbin":
                if len(group_name) > 0:
                    group_name += "."
                group_name += string

        self.name_without_extension = group_name

        if collection is None:
            self.collection = bpy.data.collections.new(group_name)
        else:
            self.collection = collection

        self._set_amdl_path()

    def _set_amdl_path(self):
        # Check in the same folder as the model
        folder = os.path.dirname(self.gfxbin_path)
        self.amdl_path = self._find_amdl_in_folder(folder)
        if self.amdl_path is None:
            # Check up one folder from the model
            up_folder = os.path.split(folder)[0]
            self.amdl_path = self._find_amdl_in_folder(up_folder)
            if self.amdl_path is None:
                # Check in the common folder if it exists
                common = up_folder + "\\common"
                if os.path.exists(common):
                    self.amdl_path = self._find_amdl_in_folder(common)

    def _find_amdl_in_folder(self, folder: str) -> str:
        result = None

        file_count = 0
        for file in os.listdir(folder):
            if file.endswith(".amdl"):
                file_count += 1
                result = folder + "\\" + file

        if file_count < 2:
            return result

        # There were multiple amdls, find one that matches the name of the model
        names = [
            self.name_without_extension,  # e.g. nh00_010
            self.name_without_extension[:self.name_without_extension.find('_')]  # e.g. nh00
        ]

        for file in os.listdir(folder):
            if file.endswith(".amdl"):
                for name in names:
                    if name in file:
                        return folder + "\\" + file

        return None

    def set_base_directory(self, header: GfxbinHeader):
        # Get the URI of the first gpubin
        gpubin_uri = None
        for key in header.dependencies:
            if header.dependencies[key].endswith(".gpubin"):
                gpubin_uri = header.dependencies[key]
                break

        self.base_uri = gpubin_uri[:gpubin_uri.rfind('/')]
        tokens = self.gfxbin_path.split('\\')[:-1]

        base_directory = ""
        for i in range(len(tokens)):
            base_directory += tokens[i] + '\\'

        self.base_directory = base_directory[:-1]

    def get_absolute_path_from_uri(self, uri: str):
        if uri.endswith(".tif") or uri.endswith(".exr") or uri.endswith(".png") or uri.endswith(".tga") or uri.endswith(
                ".dds") or uri.endswith(".btex"):
            return self._resolve_texture_path(uri)
        else:
            path = self._get_absolute_path_from_uri(uri)

            if not os.path.exists(path):
                print(f"[WARNING] File did not exist at {path}")
                return None
            else:
                return path

    def _get_absolute_path_from_uri(self, uri: str):
        # Get tokens for the part of the URIs that match
        tokens1 = uri.replace("://", "/").split("/")
        tokens2 = self.base_uri.replace("://", "/").split("/")
        target_tokens = []

        for i in range(min(len(tokens1), len(tokens2))):
            if tokens1[i] == tokens2[i]:
                target_tokens.append(tokens1[i])
            else:
                break

        # Get the folder name of the deepest matching folder
        if len(target_tokens) > 0:
            target_token = target_tokens[-1]
        else:
            target_token = ""

        # Get the index of the highest folder the URIs have in common
        index = -1
        counter = 0
        base_tokens = self.base_uri.replace("://", "/").split("/")

        for i in range(len(base_tokens) - 1, -1, -1):
            if base_tokens[i] == target_token:
                index = i
                break

            counter += 1

        if index == -1:
            return None

        # Calculate the absolute path of the highest common folder
        base_path = ""
        base_path_tokens = self.base_directory.split('\\')
        if counter > 0:
            base_path_tokens = base_path_tokens[:-counter]
        for i in range(len(base_path_tokens)):
            base_path += base_path_tokens[i] + "\\"

        base_path = base_path[:-1]

        # Assemble the common URI start
        target = ""
        for i in range(len(target_tokens)):
            target += target_tokens[i]
            if i == 0:
                target += "://"
            else:
                target += "/"

        target = target[:-1]

        # Calculate the final absolute path
        remaining_path = uri.replace(target, "").replace("://", "/").replace("/", "\\")
        return base_path + remaining_path.replace(".gmtl", ".gmtl.gfxbin")

    def _resolve_texture_path(self, uri: str):
        extensions = ["dds", "tga", "png"]

        high = uri[:uri.rfind('.')] + "_$h" + uri[uri.rfind('.'):]
        highest = high.replace("/sourceimages/", "/highimages/")
        medium = uri[:uri.rfind('.')] + "_$m1" + uri[uri.rfind('.'):]
        low = uri

        uris = [highest, high, medium, low]
        paths_checked = []

        for i in range(len(uris)):
            path = self._get_absolute_path_from_uri(uris[i])
            if path is not None:
                for j in range(len(extensions)):
                    without_extension = path[:path.rfind(".")]
                    with_extension = without_extension + "." + extensions[j]
                    paths_checked.append(with_extension)

                    if os.path.exists(with_extension):
                        return with_extension
                    else:
                        name = without_extension.split('\\')[-1]
                        udim = f"{without_extension}\\{name}.1001.{extensions[j]}"
                        paths_checked.append(udim)
                        if os.path.exists(udim):
                            return udim

        # Couldn't find the texture, check near the gmdl
        directory = os.path.dirname(self.gfxbin_path)
        file_name = uri[uri.rfind('/'):uri.rfind('.')]
        highest = f"{directory}\\highimages\\{file_name}_$h"
        high = f"{directory}\\sourceimages\\{file_name}_$h"
        medium = f"{directory}\\sourceimages\\{file_name}_$m1"
        low = f"{directory}\\sourceimages\\{file_name}"
        paths = [highest, high, medium, low]

        for i in range(len(paths)):
            for j in range(len(extensions)):
                with_extension = paths[i] + "." + extensions[j]
                paths_checked.append(with_extension)

                if os.path.exists(with_extension):
                    return with_extension
                else:
                    name = paths[i].split('\\')[-1]
                    udim = f"{paths[i]}\\{name}.1001.{extensions[j]}"
                    paths_checked.append(udim)
                    if os.path.exists(udim):
                        return udim

        print("")
        print(f"[WARNING] Could not find texture for {uri} - checked:")
        for i in range(len(paths_checked)):
            print(f" {paths_checked[i]}")
        print("")
        return None
