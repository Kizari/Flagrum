import json
import math
import os
from types import SimpleNamespace

import bpy
from bpy.props import StringProperty, FloatProperty, BoolProperty
from bpy.types import Operator
from bpy_extras.io_utils import ImportHelper, ExportHelper

from .export_context import ExportContext
from .generate_armature import generate_armature
from .generate_mesh import generate_mesh
from .import_context import ImportContext
from .interop import Interop
from .pack_mesh import pack_mesh
from .read_armature_data import import_armature_data
from ..entities import EnvironmentModelMetadata, Gpubin, BlenderTextureData


class ImportOperator(Operator, ImportHelper):
    """Imports data from a Luminous Engine model"""
    bl_idname = "flagrum.gfxbin_import"
    bl_label = "Import gfxbin"
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
            generate_mesh(import_context.collection, mesh, mesh_data.BoneTable.__dict__)

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

    smooth_normals: BoolProperty(
        name="Smooth Normals on Doubles",
        description="When the exporter splits edges for compatibility with FFXV, this functionality will smooth "
                    "the seams caused by the edge-splitting",
        default=False
    )

    distance: FloatProperty(
        name="Distance",
        description="The maximum distance between doubles for which to merge normals",
        default=0.0001,
        min=0.0001,
        precision=4
    )

    def draw(self, context):
        layout = self.layout
        layout.label(text="FMD Export Options")
        layout.prop(self, property="smooth_normals")
        layout.prop(self, property="distance")

    def execute(self, context):
        export_context = ExportContext(self.smooth_normals, self.distance)
        data = pack_mesh(export_context)
        Interop.export_mesh(self.filepath, data)

        return {'FINISHED'}


class ImportEnvironmentOperator(Operator, ImportHelper):
    """Imports data from a Flagrum Environment Pack"""
    bl_idname = "flagrum.environment_import"
    bl_label = "Import Flagrum Environment"
    filename_ext = ".fed"

    filter_glob: StringProperty(
        default="*.fed",
        options={'HIDDEN'}
    )

    def execute(self, context):
        environment_path = self.filepath
        directory = os.path.dirname(environment_path)

        import_file = open(environment_path, mode='r')
        import_data = import_file.read()

        data: list[EnvironmentModelMetadata] = json.loads(import_data, object_hook=lambda d: SimpleNamespace(**d))
        materials = {}
        meshes = {}
        texture_slots = {}

        for model in data:
            if model.Index < 1:
                continue

            mesh_data = None
            if model.Index in meshes:
                if model.PrefabName not in bpy.context.scene.collection.children:
                    collection = bpy.data.collections.new(model.PrefabName)
                    bpy.context.scene.collection.children.link(collection)
                else:
                    collection = bpy.data.collections[model.PrefabName]

                for original_mesh in meshes[model.Index]:
                    mesh = original_mesh.copy()
                    collection.objects.link(mesh)
                    mesh["Model URI"] = model.Path
                    mesh.scale = [model.Scale, model.Scale, model.Scale]
                    mesh.rotation_euler = [math.radians(model.Rotation[0]), math.radians(abs(model.Rotation[2] * -1)),
                                           math.radians(model.Rotation[1])]
                    mesh.location = [model.Position[0], model.Position[2] * -1, model.Position[1]]
            else:
                try:
                    path = directory + "\\models\\" + str(model.Index) + ".json"
                    mesh_file = open(path, mode='r')
                    mesh_file_data = mesh_file.read()
                    mesh_data: Gpubin = json.loads(mesh_file_data, object_hook=lambda d: SimpleNamespace(**d))
                except:
                    print("Failed to load " + model.Path)

                if mesh_data is not None:
                    self.import_mesh(mesh_data, model, materials, directory, meshes, texture_slots)

        for texture_slot in texture_slots:
            print(texture_slot)

        return {'FINISHED'}

    def import_mesh(self, mesh_data: Gpubin, model: EnvironmentModelMetadata, materials, directory, meshes, texture_slots):
        for mesh_metadata in mesh_data.Meshes:
            # Create or get the collection for this prefab
            if model.PrefabName not in bpy.context.scene.collection.children:
                collection = bpy.data.collections.new(model.PrefabName)
                bpy.context.scene.collection.children.link(collection)
            else:
                collection = bpy.data.collections[model.PrefabName]

            # Import the mesh and position it according to the level data
            mesh = generate_mesh(collection, mesh_metadata, [])
            mesh["Model URI"] = model.Path
            mesh.scale = [model.Scale, model.Scale, model.Scale]
            mesh.rotation_euler = [math.radians(model.Rotation[0]), math.radians(abs(model.Rotation[2] * -1)),
                                   math.radians(model.Rotation[1])]
            mesh.location = [model.Position[0], model.Position[2] * -1, model.Position[1]]

            # Skip the material if we have no material data
            if mesh_metadata.BlenderMaterial is None or mesh_metadata.BlenderMaterial.Name is None:
                continue

            if mesh_metadata.BlenderMaterial.Hash in materials:
                material = materials[mesh_metadata.BlenderMaterial.Hash]
            else:
                material = bpy.data.materials.new(name=mesh_metadata.BlenderMaterial.Name)

                material.use_nodes = True
                bsdf = material.node_tree.nodes["Principled BSDF"]
                multiply = material.node_tree.nodes.new('ShaderNodeMixRGB')
                multiply.blend_type = 'MULTIPLY'
                multiply.inputs[0].default_value = 1.0
                material.node_tree.links.new(bsdf.inputs['Base Color'], multiply.outputs['Color'])

                for t in mesh_metadata.BlenderMaterial.Textures:
                    texture_metadata: BlenderTextureData = t
                    texture = material.node_tree.nodes.new('ShaderNodeTexImage')
                    texture.image = bpy.data.images.load(
                        directory + "\\textures\\" + texture_metadata.Name + ".tga",
                        check_existing=True)

                    texture_slot = texture_metadata.Slot.upper()
                    if texture_metadata.Name == "le_ar_gqshop1_room_02_glass_ba":
                        print(texture_slot)

                    if "BASECOLOR0" in texture_slot:
                        material.node_tree.links.new(multiply.inputs['Color1'], texture.outputs['Color'])
                        if texture_metadata.Name.endswith("_ba") or texture_metadata.Name.endswith("_ba_$h"):
                            material.node_tree.links.new(bsdf.inputs['Alpha'], texture.outputs['Alpha'])
                            material.blend_method = 'CLIP'
                    elif "NORMAL0" in texture_slot or "NRT0" in texture_slot:
                        texture.image.colorspace_settings.name = 'Non-Color'
                        separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
                        combine_rgb = material.node_tree.nodes.new('ShaderNodeCombineRGB')
                        norm_map = material.node_tree.nodes.new('ShaderNodeNormalMap')
                        material.node_tree.links.new(separate_rgb.inputs['Image'], texture.outputs['Color'])
                        material.node_tree.links.new(combine_rgb.inputs['R'], separate_rgb.outputs['R'])
                        material.node_tree.links.new(combine_rgb.inputs['G'], separate_rgb.outputs['G'])
                        material.node_tree.links.new(norm_map.inputs['Color'], combine_rgb.outputs['Image'])
                        material.node_tree.links.new(bsdf.inputs['Normal'], norm_map.outputs['Normal'])
                        if texture_metadata.Name.endswith("_nrt") or texture_metadata.Name.endswith(
                                "_nrt_$h"):
                            rgb = material.node_tree.nodes.new('ShaderNodeRGB')
                            rgb.outputs[0].default_value = (1.0, 1.0, 1.0, 1.0)
                            material.node_tree.links.new(bsdf.inputs['Transmission'], texture.outputs['Alpha'])
                            material.node_tree.links.new(bsdf.inputs['Roughness'], separate_rgb.outputs['B'])
                            material.node_tree.links.new(combine_rgb.inputs['B'], rgb.outputs['Color'])
                        elif texture_metadata.Name.endswith("_nro") or texture_metadata.Name.endswith(
                                "_nro_$h"):
                            rgb = material.node_tree.nodes.new('ShaderNodeRGB')
                            rgb.outputs[0].default_value = (1.0, 1.0, 1.0, 1.0)
                            material.node_tree.links.new(multiply.inputs['Color2'], texture.outputs['Alpha'])
                            material.node_tree.links.new(bsdf.inputs['Roughness'], separate_rgb.outputs['B'])
                            material.node_tree.links.new(combine_rgb.inputs['B'], rgb.outputs['Color'])
                        else:
                            invert = material.node_tree.nodes.new('ShaderNodeInvert')
                            material.node_tree.links.new(invert.inputs['Color'], separate_rgb.outputs['B'])
                            material.node_tree.links.new(combine_rgb.inputs['B'], invert.outputs['Color'])
                    elif "MRS0" in texture_slot:
                        texture.image.colorspace_settings.name = 'Non-Color'
                        separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
                        material.node_tree.links.new(separate_rgb.inputs['Image'], texture.outputs['Color'])
                        material.node_tree.links.new(bsdf.inputs['Metallic'], separate_rgb.outputs['R'])
                        material.node_tree.links.new(bsdf.inputs['Roughness'], separate_rgb.outputs['G'])
                        material.node_tree.links.new(bsdf.inputs['Specular'], separate_rgb.outputs['B'])
                    elif "MRO_MIX0" in texture_slot:
                        texture.image.colorspace_settings.name = 'Non-Color'
                        separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
                        material.node_tree.links.new(separate_rgb.inputs['Image'], texture.outputs['Color'])
                        material.node_tree.links.new(multiply.inputs['Color2'], separate_rgb.outputs['B'])
                        material.node_tree.links.new(bsdf.inputs['Metallic'], separate_rgb.outputs['R'])
                        material.node_tree.links.new(bsdf.inputs['Roughness'], separate_rgb.outputs['G'])
                    elif "EMISSIVECOLOR0" in texture_slot or "EMISSIVE0" in texture_slot:
                        material.node_tree.links.new(bsdf.inputs['Emission'], texture.outputs['Color'])
                    elif "TRANSPARENCY0" in texture_slot:
                        texture.image.colorspace_settings.name = 'Non-Color'
                        material.node_tree.links.new(bsdf.inputs['Alpha'], texture.outputs['Color'])
                    elif "OCCLUSION0" in texture_slot:
                        texture.image.colorspace_settings.name = 'Non-Color'
                        material.node_tree.links.new(multiply.inputs['Color2'], texture.outputs['Color'])
                    else:
                        texture_slots[texture_slot] = 1

                materials[mesh_metadata.BlenderMaterial.Hash] = material

            mesh.data.materials.append(material)

            if model.Index in meshes:
                meshes[model.Index].append(mesh)
            else:
                meshes[model.Index] = []
                meshes[model.Index].append(mesh)