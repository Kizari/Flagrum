import json
import math
import os
from os.path import exists
from types import SimpleNamespace

import bpy
import numpy
from bpy.props import StringProperty
from bpy.types import Operator, Menu, Mesh
from bpy_extras.io_utils import ImportHelper, ExportHelper
from mathutils import Matrix, Euler, Vector

from .generate_armature import generate_armature
from .generate_mesh import generate_mesh
from .import_context import ImportContext
from .interop import Interop
from .pack_mesh import pack_mesh
from .read_armature_data import import_armature_data
from ..entities import EnvironmentModelMetadata, Gpubin, TerrainMetadata


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
                          bone_table=mesh_data.BoneTable.__dict__)

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

    def execute(self, context):
        data = pack_mesh()
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
            mesh = generate_mesh(context, collection, mesh_metadata, [], use_correction_matrix=False)
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

    def execute(self, context):
        terrain_path = self.filepath
        directory = os.path.dirname(terrain_path)
        filename_without_extension = terrain_path.split("\\")[-1].replace(".ftd", "")
        import_file = open(terrain_path, mode='r')
        import_data = import_file.read()

        data: list[TerrainMetadata] = json.loads(import_data, object_hook=lambda d: SimpleNamespace(**d))

        for tile in data:
            if tile.HeightMap is None:
                continue

            if tile.PrefabName not in bpy.data.collections:
                collection = bpy.data.collections.new(tile.PrefabName)
                bpy.context.scene.collection.children.link(collection)
            else:
                collection = bpy.data.collections[tile.PrefabName]

            vertices = []
            coords = []
            w = 512 / (tile.HeightMap.Width - 1)
            h = 512 / (tile.HeightMap.Height - 1)
            u = 1.0 / (tile.HeightMap.Width - 1)
            v = 1.0 / (tile.HeightMap.Height - 1)

            for x in range(tile.HeightMap.Width):
                for y in range(tile.HeightMap.Height):
                    vertices.append(
                        [(x * w) - 256, (y * h) - 256, tile.HeightMap.Altitudes[(x * tile.HeightMap.Width) + y]])
                    coords.append([x * u, y * v])

            faces = []
            for x in range(tile.HeightMap.Width - 1):
                for y in range(tile.HeightMap.Height - 1):
                    tl = x * tile.HeightMap.Height + y
                    bl = tl + 1
                    tr = tl + tile.HeightMap.Height
                    br = tr + 1
                    faces.append([tl, tr, bl])
                    faces.append([tr, br, bl])

            mesh = bpy.data.meshes.new(tile.Name)
            mesh.from_pydata(vertices, [], faces)

            for face in mesh.polygons:
                face.use_smooth = True

            map1 = mesh.uv_layers.new(name="map1")
            uv_dictionary = {i: uv for i, uv in enumerate(coords)}
            per_loop_list = [0.0] * len(mesh.loops)

            for loop in mesh.loops:
                per_loop_list[loop.index] = uv_dictionary.get(loop.vertex_index)

            per_loop_list = numpy.asarray([uv for pair in per_loop_list for uv in pair])
            pivot = Vector((0.5, 0.5))
            angle = numpy.radians(-90)
            aspect_ratio = 1
            rotation = Matrix((
                (numpy.cos(angle), numpy.sin(angle) / aspect_ratio),
                (-aspect_ratio * numpy.sin(angle), numpy.cos(angle)),
            ))

            uvs = numpy.dot(per_loop_list.reshape((-1, 2)) - pivot, rotation) + pivot

            map1.data.foreach_set("uv", uvs.ravel())

            mesh.validate()
            mesh.update()

            mesh_object = bpy.data.objects.new(tile.Name, mesh)
            collection.objects.link(mesh_object)

            layer = bpy.context.view_layer
            layer.update()

            material = bpy.data.materials.new(name=tile.Name)

            material.use_nodes = True
            material.use_backface_culling = True
            bsdf = material.node_tree.nodes["Principled BSDF"]
            bsdf.inputs[9].default_value = 0.8  # Roughness

            texture = material.node_tree.nodes.new('ShaderNodeTexImage')
            diffuse_path = directory + "\\" + filename_without_extension + "_terrain_textures\\" + tile.Name + "\\diffuse.tga"
            if exists(diffuse_path):
                texture.image = bpy.data.images.load(diffuse_path, check_existing=True)
            material.node_tree.links.new(bsdf.inputs['Base Color'], texture.outputs['Color'])

            texture = material.node_tree.nodes.new('ShaderNodeTexImage')
            normal_path = directory + "\\" + filename_without_extension + "_terrain_textures\\" + tile.Name + "\\normal.tga"
            if exists(normal_path):
                texture.image = bpy.data.images.load(normal_path, check_existing=True)
                texture.image.colorspace_settings.name = 'Non-Color'
            norm_map = material.node_tree.nodes.new('ShaderNodeNormalMap')
            material.node_tree.links.new(bsdf.inputs['Normal'], norm_map.outputs['Normal'])
            separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
            combine_rgb = material.node_tree.nodes.new('ShaderNodeCombineRGB')
            less_than = material.node_tree.nodes.new('ShaderNodeMath')
            less_than.operation = 'LESS_THAN'
            less_than.inputs[1].default_value = 0.01
            maximum = material.node_tree.nodes.new('ShaderNodeMath')
            maximum.operation = 'MAXIMUM'
            material.node_tree.links.new(separate_rgb.inputs['Image'], texture.outputs['Color'])
            material.node_tree.links.new(combine_rgb.inputs['R'], separate_rgb.outputs['R'])
            material.node_tree.links.new(combine_rgb.inputs['G'], separate_rgb.outputs['G'])
            material.node_tree.links.new(norm_map.inputs['Color'], combine_rgb.outputs['Image'])
            material.node_tree.links.new(less_than.inputs[0], separate_rgb.outputs['B'])
            material.node_tree.links.new(maximum.inputs[0], separate_rgb.outputs['B'])
            material.node_tree.links.new(maximum.inputs[1], less_than.outputs['Value'])
            material.node_tree.links.new(combine_rgb.inputs['B'], maximum.outputs['Value'])
            mesh_object.data.materials.append(material)

            mesh_object.location[0] = tile.Position[0] + 256
            mesh_object.location[1] = (tile.Position[2] * -1) - 256
            mesh_object.rotation_euler[2] = math.radians(-90)

        return {'FINISHED'}


class FlagrumImportMenu(Menu):
    bl_idname = "TOPBAR_MT_flagrum_import"
    bl_label = "Flagrum"

    def draw(self, context):
        layout = self.layout
        layout.operator(ImportOperator.bl_idname)
        layout.operator(ImportEnvironmentOperator.bl_idname)
        layout.operator(ImportTerrainOperator.bl_idname)
