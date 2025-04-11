import json
import os
from types import SimpleNamespace

import bpy
from bpy.props import StringProperty, EnumProperty, BoolProperty
from bpy.types import Operator, Menu
from bpy_extras.io_utils import ImportHelper, ExportHelper

from .generate_armature import generate_armature
from .generate_terrain import generate_terrain
from .gmdlimporter import GmdlImporter
from .import_context import ImportContext
from .interop import Interop
from .pack_mesh import FmdExporter
from .read_armature_data import import_armature_data
from ..environment.import_operator import ImportEnvironmentOperator
from ..graphics.fmd.entities import TerrainMetadata, TerrainImportContext
from ...utilities.helpers import draw_lines


class ImportOperator(Operator, ImportHelper):
    """Imports data from a Luminous Engine model"""
    bl_idname = "flagrum.gfxbin_import"
    bl_label = "Import FFXV Model (.gfxbin)"
    filename_ext = ".gfxbin"

    filter_glob: StringProperty(
        default="*.gfxbin",
        options={'HIDDEN'}
    )

    import_lods: BoolProperty(
        name="Import LODs",
        description="If checked, Flagrum will import all LOD meshes with this model.",
        default=False
    )

    import_vems: BoolProperty(
        name="Import VEMs",
        description="If checked, Flagrum will import all VEM meshes with this model.",
        default=False
    )

    def draw(self, context):
        layout = self.layout
        layout.label(text="GMDL Import Options")
        layout.prop(data=self, property="import_lods")
        layout.prop(data=self, property="import_vems")

    def execute(self, context):
        import_context = ImportContext(self.filepath, self.import_lods, self.import_vems)

        importer = GmdlImporter(import_context)
        importer.import_gfxbin()
        importer.generate_bone_table()

        if len(importer.bone_table) > 0 and import_context.amdl_path is None:
            self.report({'ERROR'}, "Unable to import due to missing armature (amdl) file. Please ensure you have "
                                   "the correct amdl file exported. If you have multiple amdl files in the same "
                                   "folder, make sure that the name matches the model you are trying to import.")
            return {'FINISHED'}

        if len(importer.bone_table) > 0:
            armature_data = import_armature_data(import_context)
            if armature_data is not None:
                generate_armature(import_context, armature_data)
        else:
            bpy.context.scene.collection.children.link(import_context.collection)

        importer.import_meshes()

        return {'FINISHED'}


class ExportOperator(Operator, ExportHelper):
    """Exports a mod data package for use with Flagrum"""
    bl_idname = "flagrum.gfxbin_export"
    bl_label = "Export to Flagrum"
    filename_ext = ".fmd"

    filter_glob: StringProperty(
        default="*.fmd",
        options={'HIDDEN'}
    )

    preserve_normals: BoolProperty(
        name="Autocorrect Seam Normals",
        description="Automatically corrects seam normals after edge-splitting for Luminous. May cause issues for double-sided meshes.",
        default=True
    )

    def draw(self, context):
        layout = self.layout
        layout.label(text="FMD Export Options")
        layout.prop(data=self, property="preserve_normals")

    def execute(self, context):
        exporter = FmdExporter(self)
        data = exporter.pack_mesh(self.preserve_normals)

        if data is not None:
            Interop.export_mesh(self.filepath, data)

        return {'FINISHED'}


class ImportTerrainOperator(Operator, ImportHelper):
    """Imports data from a Flagrum Terrain Pack"""
    bl_idname = "flagrum.terrain_import"
    bl_label = "Import Flagrum Terrain (.ftd)"
    filename_ext = ".ftd"

    filter_glob: StringProperty(
        default="*.ftd",
        options={'HIDDEN'}
    )

    mesh_resolution: EnumProperty(
        items=(
            ('0', '512x512', ''),
            ('1', '256x256', ''),
            ('2', '128x128', ''),
            ('3', '64x64', ''),
            ('4', '32x32', ''),
            ('5', '16x16', '')
        ),
        name="Quads",
        description="The resolution of each terrain tile, measured in quads",
        default='0'
    )

    use_high_textures: BoolProperty(
        name="Use Experimental Shader",
        description="Import with texture splatting shader setup"
    )

    def draw(self, context):
        layout = self.layout
        layout.label(text="FTD Import Options")
        layout.prop(self, property="mesh_resolution")

        layout.label(text=" ")
        layout.label(text="!!! WARNING: Experimental !!!")
        draw_lines(layout,
                   text="This feature is experimental and will only run in cycles. The shader will only run on extremely good hardware.")
        layout.prop(self, property="use_high_textures")

    def execute(self, context):
        terrain_path = self.filepath
        directory = os.path.dirname(terrain_path)
        filename_without_extension = terrain_path.split("\\")[-1].replace(".ftd", "")
        import_file = open(terrain_path, mode='r')
        import_data = import_file.read()
        data: list[TerrainMetadata] = json.loads(import_data, object_hook=lambda d: SimpleNamespace(**d))
        context = TerrainImportContext(directory, filename_without_extension, self.mesh_resolution,
                                       self.use_high_textures)
        generate_terrain(context, data)
        return {'FINISHED'}


class FlagrumImportMenu(Menu):
    bl_idname = "TOPBAR_MT_flagrum_import"
    bl_label = "Flagrum"

    def draw(self, context):
        layout = self.layout
        layout.operator(ImportOperator.bl_idname)
        layout.operator(ImportEnvironmentOperator.bl_idname)
        layout.operator(ImportTerrainOperator.bl_idname)
