import os
from dataclasses import dataclass

import bpy


@dataclass(init=False)
class ImportContext:
    gfxbin_path: str
    amdl_path: str
    collection: bpy.types.Collection

    def __init__(self, gfxbin_file_path):
        self.gfxbin_path = gfxbin_file_path

        path_name = os.path.dirname(gfxbin_file_path)
        p0 = os.path.split(path_name)
        p1 = p0[0]
        f_idx = p1.rfind("\\")
        self.amdl_path = p1 + "\\" + p1[f_idx + 1:] + ".amdl"

        file_name = gfxbin_file_path.split("\\")[-1]
        group_name = ""
        for string in file_name.split("."):
            if string != "gmdl" and string != "gfxbin":
                if len(group_name) > 0:
                    group_name += "."
                group_name += string

        self.collection = bpy.data.collections.new(group_name)
