import math
from os.path import exists

import bpy
import numpy
from bpy.types import Material, NodeTree
from mathutils import Vector, Matrix

from ..graphics.fmd.entities import TerrainImportContext, TerrainMetadata

texture_array_map = [
    (0, 0.67742, 0.04839),
    (1, 0.54839, 0.02419),
    (1, 0.58065, 0.01210),
    (2, 0.00000, 0.01210),
    (3, 0.03226, 0.04839),
    (4, 0.35484, 0.04839),
    (5, 0.12903, 0.04839),
    (6, 0.09677, 0.09677),
    (6, 0.64516, 0.04839),
    (7, 0.06452, 0.02419),
    (7, 0.90323, 0.02419),
    (8, 0.45161, 0.02419),
    (9, 0.48387, 0.02419),
    (10, 0.38710, 0.02419),
    (11, 0.41935, 0.04839),
    (12, 0.80645, 0.02419),
    (13, 0.32258, 0.04839),
    (13, 0.70968, 0.02419),
    (14, 0.74194, 0.02419),
    (14, 0.83871, 0.02419),
    (14, 0.87097, 0.01210),
    (15, 0.25806, 0.04839),
    (15, 0.61290, 0.04839),
    (15, 0.93548, 0.04839),
    (16, 0.29032, 0.04839),
    (17, 0.19355, 0.04839),
    (18, 0.22581, 0.04839),
    (19, 0.77419, 0.02419),
    (20, 0.96774, 0.02419),
    (22, 0.51613, 0.02419),
    (25, 0.16129, 0.02419)
]


def generate_terrain(context: TerrainImportContext, data: list[TerrainMetadata]):
    step = int(context.mesh_resolution)
    factor = pow(2, step)
    desired_resolution = int(512 / factor)

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

        w = 512 / min(tile.HeightMap.Width - 1, desired_resolution)  # Distance between vertices horizontally
        h = 512 / min(tile.HeightMap.Height - 1, desired_resolution)  # Distance between vertices vertically
        u = 1.0 / min(tile.HeightMap.Width - 1, desired_resolution)  # Distance between UV coords horizontally
        v = 1.0 / min(tile.HeightMap.Height - 1, desired_resolution)  # Distance between UV coords vertically
        w_step = int((tile.HeightMap.Width - 1) / (512 / w))
        h_step = int((tile.HeightMap.Height - 1) / (512 / h))

        x_counter = w_step - 1
        y_counter = h_step - 1
        total_resolution = 0

        for x in range(tile.HeightMap.Width):
            if x_counter == w_step - 1:
                x_counter = 0
                total_resolution += 1
            else:
                x_counter += 1
                y_counter = h_step - 1
                continue
            for y in range(tile.HeightMap.Height):
                if y_counter == h_step - 1:
                    y_counter = 0
                    vertices.append(
                        [(x / w_step * w) - 256, (y / h_step * h) - 256,
                         tile.HeightMap.Altitudes[(x * tile.HeightMap.Width) + y]])
                    coords.append([x / w_step * u, y / h_step * v])
                else:
                    y_counter += 1
                    continue

        faces = []
        for x in range(total_resolution - 1):
            for y in range(total_resolution - 1):
                tl = x * total_resolution + y
                bl = tl + 1
                tr = tl + total_resolution
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
        bsdf.inputs[7].default_value = 0.3  # Specular
        bsdf.inputs[9].default_value = 0.9  # Roughness

        if context.use_high_materials:
            mask_path = context.directory + "\\" + context.filename_without_extension + "_terrain_textures\\" + tile.Name + "\\merged_mask_map.tga"
            if exists(mask_path):
                material.cycles.displacement_method = 'DISPLACEMENT'
                _setup_texture_splatting(context, material, bsdf, tile.Name)
            else:
                _setup_basic_shader(context, material, bsdf, tile.Name)
        else:
            _setup_basic_shader(context, material, bsdf, tile.Name)

        mesh_object.data.materials.append(material)

        # Move and rotate into position
        mesh_object.location[0] = tile.Position[0] + 256
        mesh_object.location[1] = (tile.Position[2] * -1) - 256
        mesh_object.rotation_euler[2] = math.radians(-90)


def _setup_basic_shader(context: TerrainImportContext, material: Material, bsdf, tile_name: str):
    texture = material.node_tree.nodes.new('ShaderNodeTexImage')
    diffuse_path = context.directory + "\\" + context.filename_without_extension + "_terrain_textures\\" + tile_name + "\\diffuse.tga"
    if exists(diffuse_path):
        texture.image = bpy.data.images.load(diffuse_path, check_existing=True)
    material.node_tree.links.new(bsdf.inputs['Base Color'], texture.outputs['Color'])
    _setup_normal_map(context, material, bsdf, tile_name)


def _setup_normal_map(context: TerrainImportContext, material: Material, bsdf, tile_name: str):
    texture = material.node_tree.nodes.new('ShaderNodeTexImage')
    normal_path = context.directory + "\\" + context.filename_without_extension + "_terrain_textures\\" + tile_name + "\\normal.tga"
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


def _setup_texture_splatting(context: TerrainImportContext, material: Material, bsdf, tile_name: str):
    multiply = material.node_tree.nodes.new('ShaderNodeMixRGB')
    multiply.blend_type = 'MULTIPLY'
    material.node_tree.links.new(multiply.outputs['Color'], bsdf.inputs['Base Color'])

    mix = material.node_tree.nodes.new('ShaderNodeMixRGB')
    material.node_tree.links.new(mix.outputs['Color'], multiply.inputs['Color1'])

    separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
    material.node_tree.links.new(separate_rgb.outputs['R'], mix.inputs['Fac'])
    material.node_tree.links.new(separate_rgb.outputs['G'], multiply.inputs['Fac'])

    splat_map = material.node_tree.nodes.new('ShaderNodeTexImage')
    splat_path = context.directory + "\\" + context.filename_without_extension + "_terrain_textures\\" + tile_name + "\\slope_map.tga"
    if exists(splat_path):
        splat_map.image = bpy.data.images.load(splat_path, check_existing=True)
        splat_map.image.colorspace_settings.name = 'Non-Color'
    material.node_tree.links.new(splat_map.outputs['Color'], separate_rgb.inputs['Image'])

    cliff_texture = material.node_tree.nodes.new('ShaderNodeTexImage')
    cliff_texture.image = bpy.data.images.load(context.directory + "\\common\\diffuse\\0.tga", check_existing=True)
    material.node_tree.links.new(cliff_texture.outputs['Color'], mix.inputs['Color2'])

    mix_arrays = material.node_tree.nodes.new('ShaderNodeMixRGB')
    material.node_tree.links.new(mix_arrays.outputs['Color'], mix.inputs['Color1'])

    diffuse_group = _setup_texture_array_group(context, "Diffuse")
    array_blue = material.node_tree.nodes.new('ShaderNodeGroup')
    array_blue.node_tree = diffuse_group
    material.node_tree.links.new(array_blue.outputs['Color'], mix_arrays.inputs['Color2'])

    array_alpha = material.node_tree.nodes.new('ShaderNodeGroup')
    array_alpha.node_tree = diffuse_group
    material.node_tree.links.new(array_alpha.outputs['Color'], mix_arrays.inputs['Color1'])

    multiply_masks = material.node_tree.nodes.new('ShaderNodeMixRGB')
    multiply_masks.blend_type = 'MULTIPLY'
    multiply_masks.inputs[0].default_value = 1  # Fac
    material.node_tree.links.new(multiply_masks.outputs['Color'], mix_arrays.inputs['Fac'])

    separate_masks = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
    material.node_tree.links.new(separate_masks.outputs['R'], multiply_masks.inputs['Color1'])
    material.node_tree.links.new(separate_masks.outputs['G'], multiply_masks.inputs['Color2'])
    material.node_tree.links.new(separate_masks.outputs['B'], array_blue.inputs['Texture ID'])

    mask_map = material.node_tree.nodes.new('ShaderNodeTexImage')
    mask_path = context.directory + "\\" + context.filename_without_extension + "_terrain_textures\\" + tile_name + "\\merged_mask_map.tga"
    if exists(mask_path):
        mask_map.image = bpy.data.images.load(mask_path, check_existing=True)
        mask_map.image.colorspace_settings.name = 'Non-Color'
    material.node_tree.links.new(mask_map.outputs['Color'], separate_masks.inputs['Image'])
    material.node_tree.links.new(mask_map.outputs['Alpha'], array_alpha.inputs['Texture ID'])

    blur_group = _setup_blur_group()
    blur = material.node_tree.nodes.new('ShaderNodeGroup')
    blur.node_tree = blur_group
    blur.inputs[0].default_value = 0.008  # Blur Amount
    blur.inputs[1].default_value = 39999000  # Blur Quality
    material.node_tree.links.new(blur.outputs['Color'], mask_map.inputs['Vector'])

    mapping = material.node_tree.nodes.new('ShaderNodeMapping')
    mapping.inputs[3].default_value[0] = 100  # X scale
    mapping.inputs[3].default_value[1] = 100  # Y scale
    material.node_tree.links.new(mapping.outputs['Vector'], array_blue.inputs['Vector'])
    material.node_tree.links.new(mapping.outputs['Vector'], array_alpha.inputs['Vector'])
    material.node_tree.links.new(mapping.outputs['Vector'], cliff_texture.inputs['Vector'])

    uv_map = material.node_tree.nodes.new('ShaderNodeUVMap')
    uv_map.uv_map = "map1"
    material.node_tree.links.new(uv_map.outputs['UV'], mapping.inputs['Vector'])

    # Setup blended normals
    normal_map = material.node_tree.nodes.new('ShaderNodeNormalMap')
    material.node_tree.links.new(normal_map.outputs['Normal'], bsdf.inputs['Normal'])

    separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
    combine_rgb = material.node_tree.nodes.new('ShaderNodeCombineRGB')
    less_than = material.node_tree.nodes.new('ShaderNodeMath')
    less_than.operation = 'LESS_THAN'
    less_than.inputs[1].default_value = 0.01
    maximum = material.node_tree.nodes.new('ShaderNodeMath')
    maximum.operation = 'MAXIMUM'
    material.node_tree.links.new(combine_rgb.inputs['R'], separate_rgb.outputs['R'])
    material.node_tree.links.new(combine_rgb.inputs['G'], separate_rgb.outputs['G'])
    material.node_tree.links.new(normal_map.inputs['Color'], combine_rgb.outputs['Image'])
    material.node_tree.links.new(less_than.inputs[0], separate_rgb.outputs['B'])
    material.node_tree.links.new(maximum.inputs[0], separate_rgb.outputs['B'])
    material.node_tree.links.new(maximum.inputs[1], less_than.outputs['Value'])
    material.node_tree.links.new(combine_rgb.inputs['B'], maximum.outputs['Value'])

    mix_normal = material.node_tree.nodes.new('ShaderNodeMixRGB')
    material.node_tree.links.new(mix_normal.outputs['Color'], separate_rgb.inputs['Image'])

    normal_group = _setup_texture_array_group(context, "Normal")
    blue_normal = material.node_tree.nodes.new('ShaderNodeGroup')
    blue_normal.node_tree = normal_group
    material.node_tree.links.new(blue_normal.outputs['Color'], mix_normal.inputs['Color2'])

    alpha_normal = material.node_tree.nodes.new('ShaderNodeGroup')
    alpha_normal.node_tree = normal_group
    material.node_tree.links.new(alpha_normal.outputs['Color'], mix_normal.inputs['Color1'])

    material.node_tree.links.new(multiply_masks.outputs['Color'], mix_normal.inputs['Fac'])
    material.node_tree.links.new(separate_masks.outputs['B'], blue_normal.inputs['Texture ID'])
    material.node_tree.links.new(mask_map.outputs['Alpha'], alpha_normal.inputs['Texture ID'])
    material.node_tree.links.new(mapping.outputs['Vector'], blue_normal.inputs['Vector'])
    material.node_tree.links.new(mapping.outputs['Vector'], alpha_normal.inputs['Vector'])

    output = material.node_tree.nodes["Material Output"]
    displacement = material.node_tree.nodes.new('ShaderNodeDisplacement')
    material.node_tree.links.new(displacement.outputs['Displacement'], output.inputs['Displacement'])

    mix_displacement = material.node_tree.nodes.new('ShaderNodeMixRGB')
    material.node_tree.links.new(mix_displacement.outputs['Color'], displacement.inputs['Height'])

    displacement_group = _setup_texture_array_group(context, "Displacement")
    blue_displacement = material.node_tree.nodes.new('ShaderNodeGroup')
    blue_displacement.node_tree = displacement_group
    material.node_tree.links.new(blue_displacement.outputs['Color'], mix_displacement.inputs['Color2'])

    alpha_displacement = material.node_tree.nodes.new('ShaderNodeGroup')
    alpha_displacement.node_tree = displacement_group
    material.node_tree.links.new(alpha_displacement.outputs['Color'], mix_displacement.inputs['Color1'])

    material.node_tree.links.new(multiply_masks.outputs['Color'], mix_displacement.inputs['Fac'])
    material.node_tree.links.new(separate_masks.outputs['B'], blue_displacement.inputs['Texture ID'])
    material.node_tree.links.new(mask_map.outputs['Alpha'], alpha_displacement.inputs['Texture ID'])
    material.node_tree.links.new(mapping.outputs['Vector'], blue_displacement.inputs['Vector'])
    material.node_tree.links.new(mapping.outputs['Vector'], alpha_displacement.inputs['Vector'])


def _setup_blur_group() -> NodeTree:
    name = "Blur Node"
    group = bpy.data.node_groups.get(name)
    if group:
        return group

    group = bpy.data.node_groups.new(name, 'ShaderNodeTree')
    group_inputs = group.nodes.new('NodeGroupInput')
    group.inputs.new('NodeSocketFloat', "Blur Amount")
    group.inputs.new('NodeSocketFloat', "Blur Quality")

    group_outputs = group.nodes.new('NodeGroupOutput')
    group.outputs.new('NodeSocketColor', "Color")

    add = group.nodes.new('ShaderNodeMixRGB')
    add.blend_type = 'ADD'
    group.links.new(add.outputs['Color'], group_outputs.inputs['Color'])

    uv_map = group.nodes.new('ShaderNodeUVMap')
    uv_map.uv_map = "map1"
    group.links.new(uv_map.outputs['UV'], add.inputs['Color1'])

    subtract = group.nodes.new('ShaderNodeMixRGB')
    subtract.blend_type = 'SUBTRACT'
    subtract.inputs[0].default_value = 1  # Fac
    group.links.new(subtract.outputs['Color'], add.inputs['Color2'])

    noise = group.nodes.new('ShaderNodeTexNoise')
    noise.inputs[3].default_value = 2.45  # Detail
    group.links.new(noise.outputs['Color'], subtract.inputs['Color1'])

    group.links.new(group_inputs.outputs['Blur Amount'], add.inputs['Fac'])
    group.links.new(group_inputs.outputs['Blur Quality'], noise.inputs['Scale'])

    return group


def _setup_texture_array_group(context: TerrainImportContext, array_type: str) -> NodeTree:
    name = "Terrain " + array_type + " Array"
    group = bpy.data.node_groups.get(name)
    if group:
        return group

    directory = context.directory + "\\common\\" + array_type.lower()
    group = bpy.data.node_groups.new("Terrain " + array_type + " Array", 'ShaderNodeTree')

    group_inputs = group.nodes.new('NodeGroupInput')
    group.inputs.new('NodeSocketFloat', "Texture ID")
    group.inputs.new('NodeSocketVector', "Vector")

    group_outputs = group.nodes.new('NodeGroupOutput')
    group.outputs.new('NodeSocketColor', "Color")

    base_texture = group.nodes.new('ShaderNodeTexImage')
    base_texture.image = bpy.data.images.load(directory + "\\11.tga", check_existing=True)
    group.links.new(group_inputs.outputs['Vector'], base_texture.inputs['Vector'])

    prev_lighten = None
    indices = [t[0] for t in texture_array_map]

    for i in reversed(range(26)):
        if i not in indices:
            continue
        texture = group.nodes.new('ShaderNodeTexImage')
        texture.image = bpy.data.images.load(directory + "\\" + str(i) + ".tga", check_existing=True)
        group.links.new(group_inputs.outputs['Vector'], texture.inputs['Vector'])

        if i == 25:
            lighten = group.nodes.new('ShaderNodeMixRGB')
            lighten.blend_type = 'LIGHTEN'
            group.links.new(lighten.outputs['Color'], group_outputs.inputs['Color'])
            compare = group.nodes.new('ShaderNodeMath')
            compare.operation = 'COMPARE'
            compare.inputs[1].default_value = texture_array_map[-1][1]
            compare.inputs[2].default_value = texture_array_map[-1][2]
            group.links.new(group_inputs.outputs['Texture ID'], compare.inputs[0])
            group.links.new(compare.outputs['Value'], lighten.inputs['Fac'])
            group.links.new(texture.outputs['Color'], lighten.inputs['Color2'])
            prev_lighten = lighten
        else:
            values = [t for t in texture_array_map if t[0] == i]
            for value in values:
                lighten = group.nodes.new('ShaderNodeMixRGB')
                lighten.blend_type = 'LIGHTEN'
                group.links.new(lighten.outputs['Color'], prev_lighten.inputs['Color1'])
                compare = group.nodes.new('ShaderNodeMath')
                compare.operation = 'COMPARE'
                compare.inputs[1].default_value = value[1]
                compare.inputs[2].default_value = value[2]
                group.links.new(group_inputs.outputs['Texture ID'], compare.inputs[0])
                group.links.new(compare.outputs['Value'], lighten.inputs['Fac'])
                group.links.new(texture.outputs['Color'], lighten.inputs['Color2'])
                prev_lighten = lighten

        if i == 0:
            group.links.new(base_texture.outputs['Color'], prev_lighten.inputs['Color1'])

    return group
