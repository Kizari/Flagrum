import json
import math
import os
from types import SimpleNamespace

import bpy
from bpy.props import StringProperty, EnumProperty, BoolProperty
from bpy.types import Operator, Menu, Mesh
from bpy_extras.io_utils import ImportHelper, ExportHelper
from mathutils import Matrix, Euler, Vector

from .generate_armature import generate_armature
from .generate_mesh import generate_mesh
from .generate_terrain import generate_terrain
from .import_context import ImportContext
from .interop import Interop
from .pack_mesh import pack_mesh
from .read_armature_data import import_armature_data
from ..entities import EnvironmentModelMetadata, Gpubin, TerrainMetadata, TerrainImportContext
from ..helpers import draw_lines


class ImportOperator(Operator, ImportHelper):
    """Imports data from a Luminous Engine model"""
    bl_idname = "flagrum.gfxbin_import"
    bl_label = "Import FFXV Model (.gfxbin)"
    filename_ext = ".gfxbin"

    filter_glob: StringProperty(
        default="*.gfxbin",
        options={'HIDDEN'}
    )

    def execute(self, context):
        import_context = ImportContext(self.filepath)

        mesh_data = Interop.import_mesh(import_context.gfxbin_path)

        if len(mesh_data.BoneTable.__dict__) > 0:
            armature_data = import_armature_data(import_context)
            generate_armature(import_context, armature_data)

        for mesh in mesh_data.Meshes:
            generate_mesh(import_context,
                          collection=import_context.collection,
                          mesh_data=mesh,
                          bone_table=mesh_data.BoneTable.__dict__,
                          parts=mesh_data.Parts.__dict__)

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
        data = pack_mesh(self.preserve_normals)
        Interop.export_mesh(self.filepath, data)

        return {'FINISHED'}


class ImportEnvironmentOperator(Operator, ImportHelper):
    """Imports data from a Flagrum Environment Pack"""
    bl_idname = "flagrum.environment_import"
    bl_label = "Import Flagrum Environment (.fed)"
    filename_ext = ".fed"

    filter_glob: StringProperty(
        default="*.fed",
        options={'HIDDEN'}
    )

    def execute(self, context):
        environment_path = self.filepath
        directory = os.path.dirname(environment_path)
        filename_without_extension = environment_path.split("\\")[-1].replace(".fed", "")
        context = ImportContext(environment_path)

        import_file = open(environment_path, mode='r')
        import_data = import_file.read()

        data: list[EnvironmentModelMetadata] = json.loads(import_data, object_hook=lambda d: SimpleNamespace(**d))
        meshes = {}

        for model in data:
            mesh_data = None
            if model.Index in meshes:
                if model.PrefabName not in bpy.context.scene.collection.children:
                    collection = bpy.data.collections.new(model.PrefabName)
                    bpy.context.scene.collection.children.link(collection)
                else:
                    collection = bpy.data.collections[model.PrefabName]

                for original_mesh in meshes[model.Index]:
                    mesh: Mesh = original_mesh.copy()
                    collection.objects.link(mesh)
                    mesh["Model URI"] = model.Path
                    self.transform_mesh(mesh, model)
            else:
                try:
                    path = directory + "\\" + filename_without_extension + "_models\\" + str(model.Index) + ".json"
                    mesh_file = open(path, mode='r')
                    mesh_file_data = mesh_file.read()
                    mesh_data: Gpubin = json.loads(mesh_file_data, object_hook=lambda d: SimpleNamespace(**d))
                except:
                    print("Failed to load " + model.Path)

                if mesh_data is not None:
                    self.import_mesh(context, mesh_data, model, meshes)

        for texture_slot in context.texture_slots:
            print(texture_slot)

        return {'FINISHED'}

    def import_mesh(self, context: ImportContext, mesh_data: Gpubin, model: EnvironmentModelMetadata, meshes):
        for mesh_metadata in mesh_data.Meshes:
            # Create or get the collection for this prefab
            if model.PrefabName not in bpy.context.scene.collection.children:
                collection = bpy.data.collections.new(model.PrefabName)
                bpy.context.scene.collection.children.link(collection)
            else:
                collection = bpy.data.collections[model.PrefabName]

            # Import the mesh and position it according to the level data
            mesh = generate_mesh(context, collection, mesh_metadata, [], {}, use_correction_matrix=False)
            mesh["Model URI"] = model.Path
            self.transform_mesh(mesh, model)

            if model.Index in meshes:
                meshes[model.Index].append(mesh)
            else:
                meshes[model.Index] = []
                meshes[model.Index].append(mesh)

    def transform_mesh(self, mesh: Mesh, model: EnvironmentModelMetadata):
        # Matrix that corrects the axes from FBX coordinate system
        correction_matrix = Matrix([
            [1, 0, 0],
            [0, 0, -1],
            [0, 1, 0]
        ])

        # Compose a transformation matrix from the model node transforms
        scale_vector = Vector([model.Scale, model.Scale, model.Scale])
        rotation_euler = Vector([math.radians(model.Rotation[0]),
                                 math.radians(model.Rotation[1]),
                                 math.radians(model.Rotation[2])])
        position_vector = Vector([model.Position[0], model.Position[1], model.Position[2]])

        # If the model node doesn't have a rotation, we need to plug the first prefab transformation in here instead
        # Otherwise things end up rotated incorrectly
        skip_first = False
        if -0.01 < model.Rotation[0] < 0.01 and -0.01 < model.Rotation[1] < 0.01 and -0.01 < model.Rotation[2] < 0.01:
            for rotation in reversed(model.PrefabRotations):
                rotation_euler = Vector([math.radians(rotation[0]),
                                         math.radians(rotation[1]),
                                         math.radians(rotation[2])])
                skip_first = True
                break

        # Apply the matrix to the mesh
        matrix = Matrix.LocRotScale(position_vector, Euler(rotation_euler), scale_vector)
        mesh.matrix_world = correction_matrix.to_4x4() @ matrix

        rotation_x = 0
        rotation_y = 0
        rotation_z = 0

        # Add all the prefab rotations together
        passed_first = False
        for rotation in reversed(model.PrefabRotations):
            # Skip the first prefab rotation if it was plugged into the matrix above
            if skip_first and not passed_first:
                passed_first = True
                continue
            rotation_x += rotation[0]
            rotation_y += rotation[1]
            rotation_z += rotation[2]

        # Convert the combined prefab rotation to Blender space and apply to the mesh
        mesh.rotation_euler[0] += math.radians(rotation_x)
        mesh.rotation_euler[1] += math.radians(rotation_z * -1)
        mesh.rotation_euler[2] += math.radians(rotation_y)


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
