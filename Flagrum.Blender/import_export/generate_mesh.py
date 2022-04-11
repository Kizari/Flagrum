from array import array

import bpy
from bpy.types import Collection
from mathutils import Matrix, Vector

from .import_context import ImportContext
from ..entities import MeshData, BlenderTextureData


def generate_mesh(context: ImportContext, collection: Collection, mesh_data: MeshData, bone_table,
                  use_blue_normals: bool):
    # Matrix that corrects the axes from FBX coordinate system
    correction_matrix = Matrix([
        [1, 0, 0],
        [0, 0, -1],
        [0, 1, 0]
    ])

    # Correct the vertex positions
    vertices = []
    for vertex in mesh_data.VertexPositions:
        vertices.append(correction_matrix @ Vector([vertex.X, vertex.Y, vertex.Z]))

    # Reverse the winding order of the faces so the normals face the right direction
    for face in mesh_data.FaceIndices:
        face[0], face[2] = face[2], face[0]

    # Create the mesh
    mesh = bpy.data.meshes.new(mesh_data.Name)
    mesh.from_pydata(vertices, [], mesh_data.FaceIndices)

    # Generate each of the UV Maps
    for i in range(len(mesh_data.UVMaps)):
        if i == 0:
            new_name = "map1"
        elif i == 1:
            new_name = "mapLM"
        else:
            new_name = "map" + str(i + 1)
        mesh.uv_layers.new(name=new_name)

        uv_data = mesh_data.UVMaps[i]

        coords = []
        for coord in uv_data.UVs:
            # The V coordinate is set as 1-V to flip from FBX coordinate system
            coords.append([coord.U, 1 - coord.V])

        uv_dictionary = {i: uv for i, uv in enumerate(coords)}
        per_loop_list = [0.0] * len(mesh.loops)

        for loop in mesh.loops:
            per_loop_list[loop.index] = uv_dictionary.get(loop.vertex_index)

        per_loop_list = [uv for pair in per_loop_list for uv in pair]

        mesh.uv_layers[i].data.foreach_set("uv", per_loop_list)

    # Generate each of the color maps
    for i in range(len(mesh_data.ColorMaps)):
        vertex_colors = mesh_data.ColorMaps[i].Colors

        colors = []
        for color in vertex_colors:
            # Colors are divided by 255 to convert from 0-255 to 0.0 - 1.0
            colors.append([color.R / 255.0, color.G / 255.0, color.B / 255.0, color.A / 255.0])

        per_loop_list = [0.0] * len(mesh.loops)

        for loop in mesh.loops:
            if loop.vertex_index < len(vertex_colors):
                per_loop_list[loop.index] = colors[loop.vertex_index]

        per_loop_list = [colors for pair in per_loop_list for colors in pair]
        new_name = "colorSet"
        if i > 0:
            new_name += str(i)
        mesh.vertex_colors.new(name=new_name)
        mesh.vertex_colors[i].data.foreach_set("color", per_loop_list)

    mesh.validate()
    mesh.update()

    mesh_object = bpy.data.objects.new(mesh_data.Name, mesh)
    collection.objects.link(mesh_object)

    # Import custom normals
    mesh.update(calc_edges=True)

    clnors = array('f', [0.0] * (len(mesh.loops) * 3))
    mesh.loops.foreach_get("normal", clnors)
    mesh.polygons.foreach_set("use_smooth", [True] * len(mesh.polygons))

    normals = []
    for normal in mesh_data.Normals:
        result = correction_matrix @ Vector([normal.X / 127.0, normal.Y / 127.0, normal.Z / 127.0])
        result.normalize()
        normals.append(result)

    mesh.normals_split_custom_set_from_vertices(normals)
    mesh.use_auto_smooth = True

    layer = bpy.context.view_layer
    layer.update()

    # Add the vertex weights from each weight map
    if len(bone_table) > 0:
        for i in range(len(mesh_data.WeightValues)):
            for j in range(len(mesh_data.WeightValues[i])):
                for k in range(4):
                    # No need to import zero weights
                    if mesh_data.WeightValues[i][j][k] == 0:
                        continue

                    bone_name = bone_table[str(mesh_data.WeightIndices[i][j][k])]
                    vertex_group = mesh_object.vertex_groups.get(bone_name)

                    if not vertex_group:
                        vertex_group = mesh_object.vertex_groups.new(name=bone_name)

                    # Weights are divided by 255 to convert from 0-255 to 0.0 - 1.0
                    vertex_group.add([j], mesh_data.WeightValues[i][j][k] / 255.0, 'ADD')

    # Link the mesh to the armature
    if len(bone_table) > 0:
        mod = mesh_object.modifiers.new(
            type="ARMATURE", name=collection.name)
        mod.use_vertex_groups = True

        armature = bpy.data.objects[collection.name]
        mod.object = armature

        mesh_object.parent = armature
    else:
        # Collection wasn't linked on armature set, so do it now
        if collection.name not in bpy.context.scene.collection.children:
            bpy.context.scene.collection.children.link(collection)

    # Skip the material if we have no material data
    if mesh_data.BlenderMaterial is None or mesh_data.BlenderMaterial.Name is None:
        return mesh_object

    if mesh_data.BlenderMaterial.Hash in context.materials:
        material = context.materials[mesh_data.BlenderMaterial.Hash]
    else:
        material = bpy.data.materials.new(name=mesh_data.BlenderMaterial.Name)

        material.use_nodes = True
        bsdf = material.node_tree.nodes["Principled BSDF"]
        multiply = material.node_tree.nodes.new('ShaderNodeMixRGB')
        multiply.blend_type = 'MULTIPLY'
        multiply.inputs[0].default_value = 1.0
        material.node_tree.links.new(bsdf.inputs['Base Color'], multiply.outputs['Color'])

        for t in mesh_data.BlenderMaterial.Textures:
            texture_metadata: BlenderTextureData = t

            # Skip this texture if it doesn't exist in the file system
            if texture_metadata.Path is None:
                continue

            texture = material.node_tree.nodes.new('ShaderNodeTexImage')
            texture.image = bpy.data.images.load(
                texture_metadata.Path,
                check_existing=True)

            texture_slot = texture_metadata.Slot.upper()

            if "BASECOLOR0" in texture_slot:
                material.node_tree.links.new(multiply.inputs['Color1'], texture.outputs['Color'])
                if texture_metadata.Name.endswith("_ba") or texture_metadata.Name.endswith("_ba_$h"):
                    material.node_tree.links.new(bsdf.inputs['Alpha'], texture.outputs['Alpha'])
                    material.blend_method = 'CLIP'
            elif "NORMAL0" in texture_slot or "NRT0" in texture_slot:
                texture.image.colorspace_settings.name = 'Non-Color'
                norm_map = material.node_tree.nodes.new('ShaderNodeNormalMap')
                material.node_tree.links.new(bsdf.inputs['Normal'], norm_map.outputs['Normal'])
                if texture_metadata.Name.endswith("_nrt") or texture_metadata.Name.endswith(
                        "_nrt_$h"):
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
                    if use_blue_normals:
                        material.node_tree.links.new(norm_map.inputs['Color'], texture.outputs['Color'])
                    else:
                        separate_rgb = material.node_tree.nodes.new('ShaderNodeSeparateRGB')
                        combine_rgb = material.node_tree.nodes.new('ShaderNodeCombineRGB')
                        invert = material.node_tree.nodes.new('ShaderNodeInvert')
                        material.node_tree.links.new(separate_rgb.inputs['Image'], texture.outputs['Color'])
                        material.node_tree.links.new(combine_rgb.inputs['R'], separate_rgb.outputs['R'])
                        material.node_tree.links.new(combine_rgb.inputs['G'], separate_rgb.outputs['G'])
                        material.node_tree.links.new(norm_map.inputs['Color'], combine_rgb.outputs['Image'])
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
                if len(mesh_data.UVMaps) > 1:
                    uv_map = material.node_tree.nodes.new('ShaderNodeUVMap')
                    uv_map.uv_map = "mapLM"
                    material.node_tree.links.new(texture.inputs['Vector'], uv_map.outputs['UV'])
            else:
                context.texture_slots[texture_slot] = True

        context.materials[mesh_data.BlenderMaterial.Hash] = material

    mesh_object.data.materials.append(material)
    return mesh_object
