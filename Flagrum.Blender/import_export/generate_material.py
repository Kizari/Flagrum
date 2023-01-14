import bpy
from bpy.types import NodeTree, Material

from .import_context import ImportContext
from ..entities import MeshData, BlenderTextureData


def generate_material(context: ImportContext, mesh_data: MeshData) -> Material:
    material = bpy.data.materials.new(name=mesh_data.BlenderMaterial.Name)

    material.use_nodes = True
    material.use_backface_culling = True
    bsdf = material.node_tree.nodes["Principled BSDF"]
    multiply = material.node_tree.nodes.new('ShaderNodeMixRGB')
    multiply.blend_type = 'MULTIPLY'
    multiply.inputs[0].default_value = 1.0
    material.node_tree.links.new(bsdf.inputs['Base Color'], multiply.outputs['Color'])

    needs_scaling = mesh_data.BlenderMaterial.UVScale is not None and (
            mesh_data.BlenderMaterial.UVScale[0] != 1.0 or (
            len(mesh_data.BlenderMaterial.UVScale) > 1 and mesh_data.BlenderMaterial.UVScale[1] != 1.0))

    if needs_scaling:
        map1 = material.node_tree.nodes.new('ShaderNodeUVMap')
        map1.uv_map = "map1"
        uv_scale = material.node_tree.nodes.new('ShaderNodeMapping')
        uv_scale.inputs[3].default_value[0] = mesh_data.BlenderMaterial.UVScale[0]
        uv_scale.inputs[3].default_value[1] = mesh_data.BlenderMaterial.UVScale[
            len(mesh_data.BlenderMaterial.UVScale) - 1]
        material.node_tree.links.new(uv_scale.inputs['Vector'], map1.outputs['UV'])

    co_texture = None
    aoto_separate_rgb = None
    noto_texture = None

    has_normal1 = False
    has_basecolor = False
    for t in mesh_data.BlenderMaterial.Textures:
        texture_metadata: BlenderTextureData = t

        # Skip this texture if it doesn't exist in the file system
        if texture_metadata.Path is None:
            continue

        texture_slot = texture_metadata.Slot.upper()
        if "NORMAL1" in texture_slot:
            has_normal1 = True
        if "BASECOLOR0" in texture_slot:
            has_basecolor = True

    if has_normal1:
        bump = material.node_tree.nodes.new('ShaderNodeBump')
        bump.inputs[1].default_value = 0.001
        material.node_tree.links.new(bsdf.inputs['Normal'], bump.outputs['Normal'])

    for t in mesh_data.BlenderMaterial.Textures:
        texture_metadata: BlenderTextureData = t

        # Skip this texture if it doesn't exist in the file system
        if texture_metadata.Path is None:
            continue

        texture = material.node_tree.nodes.new('ShaderNodeTexImage')
        texture.image = bpy.data.images.load(
            texture_metadata.Path,
            check_existing=True)

        if needs_scaling:
            material.node_tree.links.new(texture.inputs['Vector'], uv_scale.outputs['Vector'])

        texture_slot = texture_metadata.Slot.upper()

        if "BASECOLOR0" in texture_slot:
            material.node_tree.links.new(multiply.inputs['Color1'], texture.outputs['Color'])
            if texture_metadata.Name.endswith("_ba") or texture_metadata.Name.endswith("_ba_$h"):
                material.node_tree.links.new(bsdf.inputs['Alpha'], texture.outputs['Alpha'])
                material.blend_method = 'CLIP'
        elif "NORMAL0" in texture_slot or "NRT0" in texture_slot:
            texture.image.colorspace_settings.name = 'Non-Color'
            if texture_metadata.Name.endswith("_nrt") or texture_metadata.Name.endswith(
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
            elif texture_metadata.Name.endswith("_nro") or texture_metadata.Name.endswith(
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
                normalise_group = _setup_normalise_group()
                normalise = material.node_tree.nodes.new('ShaderNodeGroup')
                normalise.node_tree = normalise_group
                material.node_tree.links.new(normalise.inputs['Color'], texture.outputs['Color'])

                if has_normal1:
                    material.node_tree.links.new(bump.inputs['Normal'], normalise.outputs['Normal'])
                else:
                    material.node_tree.links.new(bsdf.inputs['Normal'], normalise.outputs['Normal'])
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
            if not texture_metadata.Path.upper().endswith("WHITE.TGA"):
                material.node_tree.links.new(bsdf.inputs['Emission'], texture.outputs['Color'])
        elif "TRANSPARENCY0" in texture_slot or "OPACITYMASK0" in texture_slot:
            texture.image.colorspace_settings.name = 'Non-Color'
            material.node_tree.links.new(bsdf.inputs['Alpha'], texture.outputs['Color'])
            material.blend_method = 'CLIP'
        elif "OCCLUSION0" in texture_slot:
            texture.image.colorspace_settings.name = 'Non-Color'
            material.node_tree.links.new(multiply.inputs['Color2'], texture.outputs['Color'])
            if len(mesh_data.UVMaps) > 1:
                uv_map = material.node_tree.nodes.new('ShaderNodeUVMap')
                uv_map.uv_map = "mapLM"
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
            if len(mesh_data.UVMaps) > 1:
                uv_map = material.node_tree.nodes.new('ShaderNodeUVMap')
                uv_map.uv_map = "mapLM"
                material.node_tree.links.new(texture.inputs['Vector'], uv_map.outputs['UV'])

            separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
            material.node_tree.links.new(separate_rgb.inputs['Image'], texture.outputs['Color'])
            material.node_tree.links.new(multiply.inputs['Color2'], separate_rgb.outputs['B'])

            # Handle other channels
            texture = material.node_tree.nodes.new('ShaderNodeTexImage')
            texture.image = bpy.data.images.load(texture_metadata.Path, check_existing=True)
            texture.image.colorspace_settings.name = 'Non-Color'
            noto_texture = texture

            normalise_group = _setup_split_normal_group()
            normalise = material.node_tree.nodes.new('ShaderNodeGroup')
            normalise.node_tree = normalise_group

            if has_normal1:
                material.node_tree.links.new(bump.inputs['Normal'], normalise.outputs['Normal'])
            else:
                material.node_tree.links.new(bsdf.inputs['Normal'], normalise.outputs['Normal'])

            material.node_tree.links.new(normalise.inputs['Color'], texture.outputs['Color'])
        elif "NORMAL1" in texture_slot:
            texture.image.colorspace_settings.name = 'Non-Color'
            normalise_group = _setup_normalise_group()
            normalise = material.node_tree.nodes.new('ShaderNodeGroup')
            normalise.node_tree = normalise_group
            map1 = material.node_tree.nodes.new('ShaderNodeUVMap')
            map1.uv_map = "map1"
            uv_scale = material.node_tree.nodes.new('ShaderNodeMapping')

            if hasattr(mesh_data.BlenderMaterial,
                       "DetailUVScale") and mesh_data.BlenderMaterial.DetailUVScale is not None:
                uv_scale.inputs[3].default_value[0] = mesh_data.BlenderMaterial.DetailUVScale[0]
                uv_scale.inputs[3].default_value[1] = mesh_data.BlenderMaterial.DetailUVScale[
                    len(mesh_data.BlenderMaterial.DetailUVScale) - 1]

            material.node_tree.links.new(bump.inputs['Height'], normalise.outputs['Normal'])
            material.node_tree.links.new(normalise.inputs['Color'], texture.outputs['Color'])
            material.node_tree.links.new(texture.inputs['Vector'], uv_scale.outputs['Vector'])
            material.node_tree.links.new(uv_scale.inputs['Vector'], map1.outputs['UV'])
        elif "COLORCHIP0" in texture_slot and not has_basecolor:
            co_texture = texture
            material.node_tree.links.new(multiply.inputs['Color1'], co_texture.outputs['Color'])
        else:
            context.texture_slots[texture_slot] = True

    if co_texture is not None and aoto_separate_rgb is not None:
        material.node_tree.links.new(co_texture.inputs['Vector'], aoto_separate_rgb.outputs['B'])

    if noto_texture is not None and co_texture is not None:
        material.node_tree.links.new(co_texture.inputs['Vector'], noto_texture.outputs['Alpha'])

    return material


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
