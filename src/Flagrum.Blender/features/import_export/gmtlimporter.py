from dataclasses import dataclass

import bpy
from bpy.types import Material

from .import_context import ImportContext
from .materials.combine_normals import setup_combine_normals_group
from .materials.ensure_blue import setup_ensure_blue_group
from ..graphics.msgpack_reader import MessagePackReader
from ...core.graphics.materials.gamematerial import GameMaterial


@dataclass
class GmtlImporter:
    context: ImportContext
    gmtl: GameMaterial

    def __init__(self, context: ImportContext, uri: str):
        self.context = context

        material_path = self.context.get_absolute_path_from_uri(uri)
        if material_path is not None:
            with open(material_path, mode="rb") as file:
                reader = MessagePackReader(file.read())

            self.gmtl = GameMaterial(reader)
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

        emission_strength = gmtl.get_buffer("Emissive0_Power")
        if emission_strength:
            bsdf.inputs["Emission Strength"].default_value = emission_strength.values[0]

        metallic_strength = gmtl.get_buffer("Metallic0_Power")
        metallic_multiply = None
        if metallic_strength and metallic_strength.values[0] != 1.0 and metallic_strength.values[0] != 0.0:
            metallic_multiply = material.node_tree.nodes.new('ShaderNodeMath')
            metallic_multiply.operation = 'MULTIPLY'
            metallic_multiply.inputs[1].default_value = metallic_strength.values[0]
            material.node_tree.links.new(bsdf.inputs['Metallic'], metallic_multiply.outputs['Value'])

        roughness_strength = gmtl.get_buffer("Roughness0_Power")
        roughness_multiply = None
        if roughness_strength and roughness_strength.values[0] != 1.0 and roughness_strength.values[0] != 0.0:
            roughness_multiply = material.node_tree.nodes.new('ShaderNodeMath')
            roughness_multiply.operation = 'MULTIPLY'
            roughness_multiply.inputs[1].default_value = roughness_strength.values[0]
            material.node_tree.links.new(bsdf.inputs['Roughness'], roughness_multiply.outputs['Value'])

        specular_strength = gmtl.get_buffer("Specular0_Power")
        specular_multiply = None
        if specular_strength and specular_strength.values[0] != 1.0 and specular_strength.values[0] != 0.0:
            specular_multiply = material.node_tree.nodes.new('ShaderNodeMath')
            specular_multiply.operation = 'MULTIPLY'
            specular_multiply.inputs[1].default_value = specular_strength.values[0]
            material.node_tree.links.new(bsdf.inputs['Specular IOR Level'], specular_multiply.outputs['Value'])

        multiply = material.node_tree.nodes.new('ShaderNodeMixRGB')
        multiply.blend_type = 'MULTIPLY'
        occlusion_strength = gmtl.get_buffer("Occlusion0_Power")
        if occlusion_strength:
            multiply.inputs[0].default_value = occlusion_strength.values[0]
        else:
            multiply.inputs[0].default_value = 1.0
        material.node_tree.links.new(bsdf.inputs['Base Color'], multiply.outputs['Color'])

        normal_map = material.node_tree.nodes.new('ShaderNodeNormalMap')
        normal_strength = gmtl.get_buffer("Normal0_Power")
        if normal_strength:
            normal_map.inputs['Strength'].default_value = normal_strength.values[0]
        material.node_tree.links.new(bsdf.inputs['Normal'], normal_map.outputs['Normal'])

        uvscale_buffer = gmtl.get_buffer("UVScale")
        needs_scaling = uvscale_buffer is not None and (
                uvscale_buffer.values[0] != 1.0 or (len(uvscale_buffer.values) > 1 and uvscale_buffer.values[1] != 1.0))

        uv_scale = None
        if needs_scaling:
            map1 = material.node_tree.nodes.new('ShaderNodeUVMap')
            map1.uv_map = "map1"
            uv_scale = material.node_tree.nodes.new('ShaderNodeMapping')
            uv_scale.inputs[3].default_value[0] = uvscale_buffer.values[0]
            uv_scale.inputs[3].default_value[1] = uvscale_buffer.values[
                len(uvscale_buffer.values) - 1]
            material.node_tree.links.new(uv_scale.inputs['Vector'], map1.outputs['UV'])

        diffuse_texture = None
        co_texture = None
        aoto_separate_rgb = None
        noto_texture = None
        has_normal1 = False
        has_basecolor = False
        has_emission_texture = False

        for texture_metadata in gmtl.textures:
            if texture_metadata.uri.endswith(".sb"):
                continue

            texture_slot = texture_metadata.shader_gen_name.upper()

            if ("NORMAL1" in texture_slot or "NORMALTEXTURE1" in texture_slot) and not texture_metadata.uri.startswith(
                    "data://shader/defaulttextures"):
                has_normal1 = True
            if "BASECOLOR0" in texture_slot or "BASECOLORTEXTURE0" in texture_slot:
                has_basecolor = True
            if "EMISSIVECOLOR0" in texture_slot or "EMISSIVE0" in texture_slot or "EMISSIVETEXTURE0" in texture_slot:
                if not texture_metadata.uri.upper().endswith("WHITE.TGA"):
                    has_emission_texture = True

        if not has_emission_texture:
            emission_color = gmtl.get_buffer("Emissive0_Color")
            if emission_color:
                bsdf.inputs['Emission Color'].default_value[0] = emission_color.values[0]
                bsdf.inputs['Emission Color'].default_value[1] = emission_color.values[1]
                bsdf.inputs['Emission Color'].default_value[2] = emission_color.values[2]

        combine_normals = None
        if has_normal1:
            detail_normal_strength_buffer = gmtl.get_buffer("Normal1_Power")
            if detail_normal_strength_buffer is None:
                detail_normal_strength = 1
            else:
                detail_normal_strength = detail_normal_strength_buffer.values[0]
            combine_normals = material.node_tree.nodes.new('ShaderNodeGroup')
            combine_normals.node_tree = setup_combine_normals_group()
            combine_normals.inputs[2].default_value = detail_normal_strength
            material.node_tree.links.new(normal_map.inputs['Color'], combine_normals.outputs['Color'])

        # Sort delegate to ensure diffuse is always processed first
        def texture_sort(t):
            t_slot = t.shader_gen_name.upper()
            return "BASECOLOR0" in t_slot or "BASECOLORTEXTURE0" in t_slot

        # Process each texture in the material
        for texture_metadata in sorted(gmtl.textures, key=texture_sort, reverse=True):
            if texture_metadata.uri.endswith(".sb"):
                continue

            if texture_metadata.uri.startswith("data://shader/defaulttextures"):
                continue

            texture_path = self.context.get_absolute_path_from_uri(texture_metadata.uri)
            if texture_path is None:
                continue

            texture_file_name_with_extension = texture_metadata.uri[(texture_metadata.uri.find("/") + 1):]
            texture_file_name = texture_file_name_with_extension[:texture_file_name_with_extension.rfind(".")]
            texture = material.node_tree.nodes.new('ShaderNodeTexImage')
            texture.image = bpy.data.images.load(texture_path, check_existing=True)
            if texture_path.split('\\')[-1].split('.')[-2] == "1001":
                texture.image.source = 'TILED'

            if needs_scaling:
                material.node_tree.links.new(texture.inputs['Vector'], uv_scale.outputs['Vector'])

            texture_slot = texture_metadata.shader_gen_name.upper()

            if "BASECOLOR0" in texture_slot or "BASECOLORTEXTURE0" in texture_slot:
                diffuse_texture = texture
                material.node_tree.links.new(multiply.inputs['Color1'], texture.outputs['Color'])
                if texture_file_name.endswith("_ba") or texture_file_name.endswith("_ba_$h"):
                    material.node_tree.links.new(bsdf.inputs['Alpha'], texture.outputs['Alpha'])
                    material.blend_method = 'CLIP'
            elif "NORMAL0" in texture_slot or "NRT0" in texture_slot or "NRO0" in texture_slot or "NORMALTEXTURE0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                if texture_file_name.endswith("_nrt") or texture_file_name.endswith(
                        "_nrt_$h"):
                    separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateColor')
                    combine_rgb = material.node_tree.nodes.new('ShaderNodeCombineColor')
                    combine_rgb.inputs['Blue'].default_value = 1
                    material.node_tree.links.new(separate_rgb.inputs['Color'], texture.outputs['Color'])
                    material.node_tree.links.new(combine_rgb.inputs['Red'], separate_rgb.outputs['Red'])
                    material.node_tree.links.new(combine_rgb.inputs['Green'], separate_rgb.outputs['Green'])
                    material.node_tree.links.new(normal_map.inputs['Color'], combine_rgb.outputs['Color'])
                    if roughness_multiply:
                        material.node_tree.links.new(roughness_multiply.inputs[0], separate_rgb.outputs['Blue'])
                    else:
                        material.node_tree.links.new(bsdf.inputs['Roughness'], separate_rgb.outputs['Blue'])
                    material.node_tree.links.new(bsdf.inputs['Subsurface'], texture.outputs['Alpha'])
                    bsdf.subsurface_method = 'RANDOM_WALK_FIXED_RADIUS'
                    bsdf.inputs['Subsurface Radius'].default_value[0] = 0.001
                    bsdf.inputs['Subsurface Radius'].default_value[1] = 0.001
                    bsdf.inputs['Subsurface Radius'].default_value[2] = 0.001
                    if diffuse_texture is not None:
                        material.node_tree.links.new(bsdf.inputs['Subsurface Color'], diffuse_texture.outputs['Color'])
                elif texture_file_name.endswith("_nro") or texture_file_name.endswith(
                        "_nro_$h"):
                    separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateColor')
                    combine_rgb = material.node_tree.nodes.new('ShaderNodeCombineColor')
                    combine_rgb.inputs['Blue'].default_value = 1
                    material.node_tree.links.new(separate_rgb.inputs['Color'], texture.outputs['Color'])
                    material.node_tree.links.new(combine_rgb.inputs['Red'], separate_rgb.outputs['Red'])
                    material.node_tree.links.new(combine_rgb.inputs['Green'], separate_rgb.outputs['Green'])
                    material.node_tree.links.new(normal_map.inputs['Color'], combine_rgb.outputs['Color'])
                    material.node_tree.links.new(multiply.inputs['Color2'], texture.outputs['Alpha'])
                    if roughness_multiply:
                        material.node_tree.links.new(roughness_multiply.inputs[0], separate_rgb.outputs['Blue'])
                    else:
                        material.node_tree.links.new(bsdf.inputs['Roughness'], separate_rgb.outputs['Blue'])
                else:
                    ensure_blue = material.node_tree.nodes.new('ShaderNodeGroup')
                    ensure_blue.node_tree = setup_ensure_blue_group()
                    material.node_tree.links.new(ensure_blue.inputs['Color'], texture.outputs['Color'])

                    if has_normal1:
                        material.node_tree.links.new(combine_normals.inputs[0], ensure_blue.outputs['Color'])
                    else:
                        material.node_tree.links.new(normal_map.inputs['Color'], ensure_blue.outputs['Color'])
            elif "METALLIC0TEXTURE" in texture_slot or "METALLICTEXTURE0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                if metallic_multiply:
                    material.node_tree.links.new(metallic_multiply.inputs[0], texture.outputs['Color'])
                else:
                    material.node_tree.links.new(bsdf.inputs['Metallic'], texture.outputs['Color'])
            elif "ROUGHNESS0TEXTURE" in texture_slot or "ROUGHNESSTEXTURE0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                if roughness_multiply:
                    material.node_tree.links.new(roughness_multiply.inputs[0], texture.outputs['Color'])
                else:
                    material.node_tree.links.new(bsdf.inputs['Roughness'], texture.outputs['Color'])
            elif "SPECULAR0TEXTURE" in texture_slot or "SPECULARTEXTURE0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                if specular_multiply:
                    material.node_tree.links.new(specular_multiply.inputs[0], texture.outputs['Color'])
                else:
                    material.node_tree.links.new(bsdf.inputs['Specular IOR Level'], texture.outputs['Color'])
            elif "MRS0" in texture_slot or "MRSTEXTURE0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateColor')
                material.node_tree.links.new(separate_rgb.inputs['Color'], texture.outputs['Color'])
                if metallic_multiply:
                    material.node_tree.links.new(metallic_multiply.inputs[0], separate_rgb.outputs['Red'])
                else:
                    material.node_tree.links.new(bsdf.inputs['Metallic'], separate_rgb.outputs['Red'])
                if roughness_multiply:
                    material.node_tree.links.new(roughness_multiply.inputs[0], separate_rgb.outputs['Green'])
                else:
                    material.node_tree.links.new(bsdf.inputs['Roughness'], separate_rgb.outputs['Green'])
                if specular_multiply:
                    material.node_tree.links.new(specular_multiply.inputs[0], separate_rgb.outputs['Blue'])
                else:
                    material.node_tree.links.new(bsdf.inputs['Specular IOR Level'], separate_rgb.outputs['Blue'])
            elif "MRO_MIX0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateColor')
                material.node_tree.links.new(separate_rgb.inputs['Color'], texture.outputs['Color'])
                material.node_tree.links.new(multiply.inputs['Color2'], separate_rgb.outputs['Blue'])
                if metallic_multiply:
                    material.node_tree.links.new(metallic_multiply.inputs[0], separate_rgb.outputs['Red'])
                else:
                    material.node_tree.links.new(bsdf.inputs['Metallic'], separate_rgb.outputs['Red'])
                if roughness_multiply:
                    material.node_tree.links.new(roughness_multiply.inputs[0], separate_rgb.outputs['Green'])
                else:
                    material.node_tree.links.new(bsdf.inputs['Roughness'], separate_rgb.outputs['Green'])
            elif "EMISSIVECOLOR0" in texture_slot or "EMISSIVE0" in texture_slot or "EMISSIVETEXTURE0" in texture_slot:
                if not texture_metadata.uri.upper().endswith("WHITE.TGA"):
                    material.node_tree.links.new(bsdf.inputs['Emission Color'], texture.outputs['Color'])
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
                aoto_separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateColor')
                material.node_tree.links.new(aoto_separate_rgb.inputs['Color'], texture.outputs['Color'])
                material.node_tree.links.new(bsdf.inputs['Alpha'], aoto_separate_rgb.outputs['Red'])
                material.node_tree.links.new(multiply.inputs['Color2'], aoto_separate_rgb.outputs['Green'])
            elif "NOTO0" in texture_slot:
                # Handle occlusion portion
                texture.image.colorspace_settings.name = 'Non-Color'
                uv_map = material.node_tree.nodes.new('ShaderNodeUVMap')
                if has_light_map:
                    uv_map.uv_map = "mapLM"
                else:
                    uv_map.uv_map = "map1"
                material.node_tree.links.new(texture.inputs['Vector'], uv_map.outputs['UV'])

                separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateColor')
                material.node_tree.links.new(separate_rgb.inputs['Color'], texture.outputs['Color'])
                material.node_tree.links.new(multiply.inputs['Color2'], separate_rgb.outputs['Blue'])

                # Handle normals (need a separate texture node in case the occlusion portion is on mapLM)
                texture = material.node_tree.nodes.new('ShaderNodeTexImage')
                texture.image = bpy.data.images.load(texture_path, check_existing=True)
                texture.image.colorspace_settings.name = 'Non-Color'
                noto_texture = texture

                separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateColor')
                material.node_tree.links.new(separate_rgb.inputs['Color'], texture.outputs['Color'])
                combine_rgb = material.node_tree.nodes.new('ShaderNodeCombineColor')
                material.node_tree.links.new(combine_rgb.inputs['Red'], separate_rgb.outputs['Red'])
                material.node_tree.links.new(combine_rgb.inputs['Green'], separate_rgb.outputs['Green'])
                combine_rgb.inputs['Blue'].default_value = 1.0

                if has_normal1:
                    material.node_tree.links.new(combine_normals.inputs[0], combine_rgb.outputs['Color'])
                else:
                    material.node_tree.links.new(normal_map.inputs['Color'], combine_rgb.outputs['Color'])
            elif "NORMAL1" in texture_slot or "NORMALTEXTURE1" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                ensure_blue = material.node_tree.nodes.new('ShaderNodeGroup')
                ensure_blue.node_tree = setup_ensure_blue_group()
                map1 = material.node_tree.nodes.new('ShaderNodeUVMap')
                map1.uv_map = "map1"
                uv_scale = material.node_tree.nodes.new('ShaderNodeMapping')

                detail_uvscale_buffer = gmtl.get_buffer("DetailUVScale")
                if detail_uvscale_buffer is None:
                    detail_uvscale_buffer = gmtl.get_buffer("Detail_UVScale")
                if detail_uvscale_buffer is None:
                    detail_uvscale_buffer = gmtl.get_buffer("Normal1_UVScale")
                if detail_uvscale_buffer is not None:
                    uv_scale.inputs[3].default_value[0] = detail_uvscale_buffer.values[0]
                    uv_scale.inputs[3].default_value[1] = detail_uvscale_buffer.values[
                        len(detail_uvscale_buffer.values) - 1]

                material.node_tree.links.new(combine_normals.inputs[1], ensure_blue.outputs['Color'])
                material.node_tree.links.new(ensure_blue.inputs['Color'], texture.outputs['Color'])
                material.node_tree.links.new(texture.inputs['Vector'], uv_scale.outputs['Vector'])
                material.node_tree.links.new(uv_scale.inputs['Vector'], map1.outputs['UV'])
            elif "COLORCHIP0" in texture_slot and not has_basecolor:
                co_texture = texture
                material.node_tree.links.new(multiply.inputs['Color1'], co_texture.outputs['Color'])
            elif "DETAILMASK0" in texture_slot and has_normal1:
                texture.image.colorspace_settings.name = 'Non-Color'
                material.node_tree.links.new(combine_normals.inputs[3], texture.outputs['Color'])
            elif "BACKSCATTERMASKTEXTURE" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                material.node_tree.links.new(bsdf.inputs['Subsurface'], texture.outputs['Color'])
                bsdf.subsurface_method = 'RANDOM_WALK_FIXED_RADIUS'
                bsdf.inputs['Subsurface Radius'].default_value[0] = 0.001
                bsdf.inputs['Subsurface Radius'].default_value[1] = 0.001
                bsdf.inputs['Subsurface Radius'].default_value[2] = 0.001
                if diffuse_texture is not None:
                    material.node_tree.links.new(bsdf.inputs['Subsurface Color'], diffuse_texture.outputs['Color'])
            else:
                self.context.texture_slots[texture_slot] = True

        if co_texture is not None and aoto_separate_rgb is not None:
            material.node_tree.links.new(co_texture.inputs['Vector'], aoto_separate_rgb.outputs['Blue'])

        if noto_texture is not None and co_texture is not None:
            material.node_tree.links.new(co_texture.inputs['Vector'], noto_texture.outputs['Alpha'])

        return material
