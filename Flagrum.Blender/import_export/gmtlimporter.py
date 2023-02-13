import os
from dataclasses import dataclass

import bpy
from bpy.types import Material, NodeTree

from .gfxbin.gmtl.gmtl import Gmtl
from .gfxbin.msgpack_reader import MessagePackReader
from .import_context import ImportContext


@dataclass
class GmtlImporter:
    context: ImportContext
    gmtl: Gmtl

    def __init__(self, context: ImportContext, uri: str):
        self.context = context

        material_path = self.context.get_absolute_path_from_uri(uri)
        if material_path is not None:
            with open(material_path, mode="rb") as file:
                reader = MessagePackReader(file.read())

            self.gmtl = Gmtl(reader)
        else:
            self.gmtl = None

    def generate_material(self, has_light_map: bool) -> Material:
        if self.gmtl is None:
            return None

        gmtl = self.gmtl
        material = bpy.data.materials.new(name=gmtl.name)

        material.use_nodes = True
        material.use_backface_culling = True
        bsdf = material.node_tree.nodes["Principled BSDF"]
        multiply = material.node_tree.nodes.new('ShaderNodeMixRGB')
        multiply.blend_type = 'MULTIPLY'
        multiply.inputs[0].default_value = 1.0
        material.node_tree.links.new(bsdf.inputs['Base Color'], multiply.outputs['Color'])

        uvscale_buffer = gmtl.get_buffer("UVScale")

        needs_scaling = uvscale_buffer is not None and (
                uvscale_buffer.values[0] != 1.0 or (len(uvscale_buffer.values) > 1 and uvscale_buffer.values[1] != 1.0))

        uv_scale = None
        if needs_scaling:
            map1 = material.node_tree.nodes.new('ShaderNodeUVMap')
            map1.uv_map = "map1"
            uv_scale = material.node_tree.nodes.new('ShaderNodeMapping')
            uv_scale.inputs[3].default_value[0] = uvscale_buffer[0]
            uv_scale.inputs[3].default_value[1] = uvscale_buffer[
                len(uvscale_buffer) - 1]
            material.node_tree.links.new(uv_scale.inputs['Vector'], map1.outputs['UV'])

        co_texture = None
        aoto_separate_rgb = None
        noto_texture = None

        has_normal1 = False
        has_basecolor = False
        for texture_metadata in gmtl.textures:
            if texture_metadata.uri.endswith(".sb"):
                continue

            texture_slot = texture_metadata.shader_gen_name.upper()

            if "NORMAL1" in texture_slot or "NORMALTEXTURE1" in texture_slot:
                has_normal1 = True
            if "BASECOLOR0" in texture_slot or "BASECOLORTEXTURE0" in texture_slot:
                has_basecolor = True

        bump = None
        if has_normal1:
            bump = material.node_tree.nodes.new('ShaderNodeBump')
            bump.inputs[1].default_value = 0.001
            material.node_tree.links.new(bsdf.inputs['Normal'], bump.outputs['Normal'])

        for texture_metadata in gmtl.textures:
            if texture_metadata.uri.endswith(".sb"):
                continue

            texture_path = self.context.get_absolute_path_from_uri(texture_metadata.uri)
            if texture_path is None:
                continue

            texture = material.node_tree.nodes.new('ShaderNodeTexImage')
            texture.image = bpy.data.images.load(texture_path, check_existing=True)
            if texture_path.split('\\')[-1].split('.')[-2] == "1001":
                texture.image.source = 'TILED'

            if needs_scaling:
                material.node_tree.links.new(texture.inputs['Vector'], uv_scale.outputs['Vector'])

            texture_slot = texture_metadata.shader_gen_name.upper()

            if "BASECOLOR0" in texture_slot or "BASECOLORTEXTURE0" in texture_slot:
                material.node_tree.links.new(multiply.inputs['Color1'], texture.outputs['Color'])
                if texture_metadata.name.endswith("_ba") or texture_metadata.name.endswith("_ba_$h"):
                    material.node_tree.links.new(bsdf.inputs['Alpha'], texture.outputs['Alpha'])
                    material.blend_method = 'CLIP'
            elif "NORMAL0" in texture_slot or "NRT0" in texture_slot or "NORMALTEXTURE0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                if texture_metadata.name.endswith("_nrt") or texture_metadata.name.endswith(
                        "_nrt_$h"):
                    norm_map = material.node_tree.nodes.new('ShaderNodeNormalMap')
                    material.node_tree.links.new(bsdf.inputs['Normal'], norm_map.outputs['Normal'])
                    rgb = material.node_tree.nodes.new('ShaderNodeRGB')
                    rgb.outputs[0].default_value = (1.0, 1.0, 1.0, 1.0)
                    invert = material.node_tree.nodes.new('ShaderNodeInvert')
                    separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
                    combine_rgb = material.node_tree.nodes.new('ShaderNodeCombineRGB')
                    material.node_tree.links.new(separate_rgb.inputs['Image'], texture.outputs['Color'])
                    material.node_tree.links.new(combine_rgb.inputs['R'], separate_rgb.outputs['R'])
                    material.node_tree.links.new(combine_rgb.inputs['G'], separate_rgb.outputs['G'])
                    material.node_tree.links.new(norm_map.inputs['Color'], combine_rgb.outputs['Image'])
                    material.node_tree.links.new(invert.inputs['Color'], texture.outputs['Alpha'])
                    material.node_tree.links.new(bsdf.inputs['Transmission'], invert.outputs['Color'])
                    material.node_tree.links.new(bsdf.inputs['Roughness'], separate_rgb.outputs['B'])
                    material.node_tree.links.new(combine_rgb.inputs['B'], rgb.outputs['Color'])
                elif texture_metadata.name.endswith("_nro") or texture_metadata.name.endswith(
                        "_nro_$h"):
                    norm_map = material.node_tree.nodes.new('ShaderNodeNormalMap')
                    material.node_tree.links.new(bsdf.inputs['Normal'], norm_map.outputs['Normal'])
                    rgb = material.node_tree.nodes.new('ShaderNodeRGB')
                    rgb.outputs[0].default_value = (1.0, 1.0, 1.0, 1.0)
                    separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
                    combine_rgb = material.node_tree.nodes.new('ShaderNodeCombineRGB')
                    material.node_tree.links.new(separate_rgb.inputs['Image'], texture.outputs['Color'])
                    material.node_tree.links.new(combine_rgb.inputs['R'], separate_rgb.outputs['R'])
                    material.node_tree.links.new(combine_rgb.inputs['G'], separate_rgb.outputs['G'])
                    material.node_tree.links.new(norm_map.inputs['Color'], combine_rgb.outputs['Image'])
                    material.node_tree.links.new(multiply.inputs['Color2'], texture.outputs['Alpha'])
                    material.node_tree.links.new(bsdf.inputs['Roughness'], separate_rgb.outputs['B'])
                    material.node_tree.links.new(combine_rgb.inputs['B'], rgb.outputs['Color'])
                else:
                    normalise_group = self._setup_normalise_group()
                    normalise = material.node_tree.nodes.new('ShaderNodeGroup')
                    normalise.node_tree = normalise_group
                    material.node_tree.links.new(normalise.inputs['Color'], texture.outputs['Color'])

                    if has_normal1:
                        material.node_tree.links.new(bump.inputs['Normal'], normalise.outputs['Normal'])
                    else:
                        material.node_tree.links.new(bsdf.inputs['Normal'], normalise.outputs['Normal'])
            elif "METALLICTEXTURE0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                material.node_tree.links.new(bsdf.inputs['Metallic'], texture.outputs['Color'])
            elif "ROUGHNESSTEXTURE0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                material.node_tree.links.new(bsdf.inputs['Roughness'], texture.outputs['Color'])
            elif "SPECULARTEXTURE0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                material.node_tree.links.new(bsdf.inputs['Specular'], texture.outputs['Color'])
            elif "MRS0" in texture_slot or "MRSTEXTURE0" in texture_slot:
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
            elif "EMISSIVECOLOR0" in texture_slot or "EMISSIVE0" in texture_slot or "EMISSIVETEXTURE0" in texture_slot:
                if not texture_metadata.uri.upper().endswith("WHITE.TGA"):
                    material.node_tree.links.new(bsdf.inputs['Emission'], texture.outputs['Color'])
            elif "TRANSPARENCY0" in texture_slot or "OPACITYMASK0" in texture_slot or "OPACITYMASKTEXTURE0" in texture_slot or "TRANSPARENCYTEXTURE0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                material.node_tree.links.new(bsdf.inputs['Alpha'], texture.outputs['Color'])
                material.blend_method = 'CLIP'
            elif "OCCLUSION0" in texture_slot or "OCCLUSIONTEXTURE0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                material.node_tree.links.new(multiply.inputs['Color2'], texture.outputs['Color'])
                uv_map = material.node_tree.nodes.new('ShaderNodeUVMap')
                if has_light_map:
                    uv_map.uv_map = "mapLM"
                else:
                    uv_map.uv_map = "map1"
                material.node_tree.links.new(texture.inputs['Vector'], uv_map.outputs['UV'])
            elif "AOTO0" in texture_slot:
                material.blend_method = 'CLIP'
                texture.image.colorspace_settings.name = 'Non-Color'
                aoto_separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
                material.node_tree.links.new(aoto_separate_rgb.inputs['Image'], texture.outputs['Color'])
                material.node_tree.links.new(bsdf.inputs['Alpha'], aoto_separate_rgb.outputs['R'])
                material.node_tree.links.new(multiply.inputs['Color2'], aoto_separate_rgb.outputs['G'])
            elif "NOTO0" in texture_slot:
                # Handle occlusion portion
                texture.image.colorspace_settings.name = 'Non-Color'
                uv_map = material.node_tree.nodes.new('ShaderNodeUVMap')
                if has_light_map > 1:
                    uv_map.uv_map = "mapLM"
                else:
                    uv_map.uv_map = "map1"
                material.node_tree.links.new(texture.inputs['Vector'], uv_map.outputs['UV'])

                separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
                material.node_tree.links.new(separate_rgb.inputs['Image'], texture.outputs['Color'])
                material.node_tree.links.new(multiply.inputs['Color2'], separate_rgb.outputs['B'])

                # Handle other channels
                texture = material.node_tree.nodes.new('ShaderNodeTexImage')
                texture.image = bpy.data.images.load(texture_path, check_existing=True)
                texture.image.colorspace_settings.name = 'Non-Color'
                noto_texture = texture

                normalise_group = self._setup_split_normal_group()
                normalise = material.node_tree.nodes.new('ShaderNodeGroup')
                normalise.node_tree = normalise_group

                if has_normal1:
                    material.node_tree.links.new(bump.inputs['Normal'], normalise.outputs['Normal'])
                else:
                    material.node_tree.links.new(bsdf.inputs['Normal'], normalise.outputs['Normal'])

                material.node_tree.links.new(normalise.inputs['Color'], texture.outputs['Color'])
            elif "NORMAL1" in texture_slot or "NORMALTEXTURE1" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                normalise_group = self._setup_normalise_group()
                normalise = material.node_tree.nodes.new('ShaderNodeGroup')
                normalise.node_tree = normalise_group
                map1 = material.node_tree.nodes.new('ShaderNodeUVMap')
                map1.uv_map = "map1"
                uv_scale = material.node_tree.nodes.new('ShaderNodeMapping')

                detail_uvscale_buffer = gmtl.get_buffer("DetailUVScale")
                if detail_uvscale_buffer is not None:
                    uv_scale.inputs[3].default_value[0] = detail_uvscale_buffer[0]
                    uv_scale.inputs[3].default_value[1] = detail_uvscale_buffer[
                        len(detail_uvscale_buffer) - 1]

                material.node_tree.links.new(bump.inputs['Height'], normalise.outputs['Normal'])
                material.node_tree.links.new(normalise.inputs['Color'], texture.outputs['Color'])
                material.node_tree.links.new(texture.inputs['Vector'], uv_scale.outputs['Vector'])
                material.node_tree.links.new(uv_scale.inputs['Vector'], map1.outputs['UV'])
            elif "COLORCHIP0" in texture_slot and not has_basecolor:
                co_texture = texture
                material.node_tree.links.new(multiply.inputs['Color1'], co_texture.outputs['Color'])
            else:
                self.context.texture_slots[texture_slot] = True

        if co_texture is not None and aoto_separate_rgb is not None:
            material.node_tree.links.new(co_texture.inputs['Vector'], aoto_separate_rgb.outputs['B'])

        if noto_texture is not None and co_texture is not None:
            material.node_tree.links.new(co_texture.inputs['Vector'], noto_texture.outputs['Alpha'])

        return material

    @staticmethod
    def _setup_normalise_group() -> NodeTree:
        name = "Normalise"
        group = bpy.data.node_groups.get(name)
        if group:
            return group

        group = bpy.data.node_groups.new(name, 'ShaderNodeTree')
        group_inputs = group.nodes.new('NodeGroupInput')
        group.inputs.new('NodeSocketColor', "Color")

        group_outputs = group.nodes.new('NodeGroupOutput')
        group.outputs.new('NodeSocketVector', "Normal")

        separate_rgb = group.nodes.new('ShaderNodeSeparateRGB')
        combine_rgb = group.nodes.new('ShaderNodeCombineRGB')
        less_than = group.nodes.new('ShaderNodeMath')
        less_than.operation = 'LESS_THAN'
        less_than.inputs[1].default_value = 0.01
        maximum = group.nodes.new('ShaderNodeMath')
        maximum.operation = 'MAXIMUM'
        normal_map = group.nodes.new('ShaderNodeNormalMap')

        group.links.new(separate_rgb.inputs['Image'], group_inputs.outputs['Color'])
        group.links.new(combine_rgb.inputs['R'], separate_rgb.outputs['R'])
        group.links.new(combine_rgb.inputs['G'], separate_rgb.outputs['G'])
        group.links.new(normal_map.inputs['Color'], combine_rgb.outputs['Image'])
        group.links.new(less_than.inputs[0], separate_rgb.outputs['B'])
        group.links.new(maximum.inputs[0], separate_rgb.outputs['B'])
        group.links.new(maximum.inputs[1], less_than.outputs['Value'])
        group.links.new(combine_rgb.inputs['B'], maximum.outputs['Value'])
        group.links.new(group_outputs.inputs['Normal'], normal_map.outputs['Normal'])

        return group

    @staticmethod
    def _setup_split_normal_group() -> NodeTree:
        name = "RG Normals"
        group = bpy.data.node_groups.get(name)
        if group:
            return group

        group = bpy.data.node_groups.new(name, 'ShaderNodeTree')
        group_inputs = group.nodes.new('NodeGroupInput')
        group.inputs.new('NodeSocketColor', "Color")

        group_outputs = group.nodes.new('NodeGroupOutput')
        group.outputs.new('NodeSocketVector', "Normal")

        separate_rgb = group.nodes.new('ShaderNodeSeparateRGB')
        combine_rgb = group.nodes.new('ShaderNodeCombineRGB')
        combine_rgb.inputs[2].default_value = 1.0
        normal_map = group.nodes.new('ShaderNodeNormalMap')

        group.links.new(separate_rgb.inputs['Image'], group_inputs.outputs['Color'])
        group.links.new(combine_rgb.inputs['R'], separate_rgb.outputs['R'])
        group.links.new(combine_rgb.inputs['G'], separate_rgb.outputs['G'])
        group.links.new(normal_map.inputs['Color'], combine_rgb.outputs['Image'])
        group.links.new(group_outputs.inputs['Normal'], normal_map.outputs['Normal'])

        return group
