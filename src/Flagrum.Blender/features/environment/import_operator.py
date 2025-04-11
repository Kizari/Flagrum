import math
import os

import bpy
from bpy.props import StringProperty
from bpy.types import Operator, Mesh, Object, Collection
from bpy_extras.io_utils import ImportHelper
from mathutils import Vector, Matrix, Euler

from .group import Group
from .prop import Prop
from ..import_export.gmdlimporter import GmdlImporter
from ..import_export.import_context import ImportContext
from ..import_export.interop import Interop


class ImportEnvironmentOperator(Operator, ImportHelper):
    """Imports model data from a Luminous environment script"""
    bl_idname = "flagrum.environment_import"
    bl_label = "Import Luminous Environment (.xml)"
    bl_options = {'REGISTER'}
    filename_ext = ".xml"

    filter_glob: StringProperty(
        default="*.xml",
        options={'HIDDEN'}
    )

    def execute(self, context):
        # Ensure the game data path is set in the add-on settings
        data_directory: str = context.preferences.addons["Flagrum"].preferences.game_data_directory
        if data_directory is None or data_directory == '':
            self.report({'ERROR'}, "The game data directory must be set in the add-on settings")
            return {'FINISHED'}

        # Import the environment via the console application
        data_directory = data_directory.rstrip('\\')
        root: Group = Interop.import_environment(data_directory, self.filepath)
        meshes: dict[str, list[Mesh]] = {}
        self.traverse_group(root, bpy.context.scene.collection, meshes, data_directory)

        return {'FINISHED'}

    def traverse_group(self, group: Group, parent_collection: Collection, meshes: dict[str, list[Mesh]],
                       data_directory: str):
        # Create a collection for this group and link it to the parent collection
        collection = bpy.data.collections.new(group.name)
        parent_collection.children.link(collection)

        # Process each prop in this group
        for prop in group.children:
            if prop.type == "EnvironmentProp":
                # Create a collection named for this prop
                prop_collection = bpy.data.collections.new(prop.name)
                collection.children.link(prop_collection)

                if prop.relative_path in meshes:
                    # The meshes have already been imported, make copies and link to this group
                    for original_mesh in meshes[prop.relative_path]:
                        mesh: Mesh = original_mesh.copy()
                        prop_collection.objects.link(mesh)
                        mesh["Model URI"] = prop.uri
                        self.transform_mesh(mesh, prop)
                else:
                    # The meshes haven't been imported yet, do that now
                    path = f'{data_directory}\\{prop.relative_path}'
                    if os.path.exists(path):
                        try:
                            self.import_mesh(prop_collection, prop, path, meshes)
                        except:
                            print(f"Failed to read {path}")

        # Process each subgroup in this group
        for subgroup in group.children:
            if subgroup.type == "EnvironmentGroup":
                self.traverse_group(subgroup, collection, meshes, data_directory)

    def import_mesh(self, parent_collection: Collection, prop: Prop, filepath: str, meshes: dict[str, list[Mesh]]):
        # Import the mesh and position it according to the level data
        import_context = ImportContext(filepath, False, False, parent_collection)
        importer = GmdlImporter(import_context)
        importer.import_gfxbin()
        importer.generate_bone_table()
        importer.bone_table = {}
        imported_meshes = importer.import_meshes(use_correction_matrix=False)

        for mesh in imported_meshes:
            mesh["Model URI"] = prop.uri
            self.transform_mesh(mesh, prop)

            if prop.relative_path in meshes:
                meshes[prop.relative_path].append(mesh)
            else:
                meshes[prop.relative_path] = []
                meshes[prop.relative_path].append(mesh)

    def transform_mesh(self, mesh: Object, prop: Prop):
        # Matrix that corrects the axes from FBX coordinate system
        correction_matrix = Matrix([
            [1, 0, 0],
            [0, 0, -1],
            [0, 1, 0]
        ])

        # Compose a transformation matrix from the model node transforms
        scale_vector = Vector([prop.scale, prop.scale, prop.scale])
        rotation_euler = Vector([math.radians(prop.rotation[0]),
                                 math.radians(prop.rotation[1]),
                                 math.radians(prop.rotation[2])])
        position_vector = Vector([prop.position[0], prop.position[1], prop.position[2]])

        # If the model node doesn't have a rotation, we need to plug the first prefab transformation in here instead
        # Otherwise things end up rotated incorrectly
        skip_first = False
        if -0.01 < prop.rotation[0] < 0.01 and -0.01 < prop.rotation[1] < 0.01 and -0.01 < prop.rotation[2] < 0.01:
            for rotation in reversed(prop.prefab_rotations):
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
        for rotation in reversed(prop.prefab_rotations):
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
