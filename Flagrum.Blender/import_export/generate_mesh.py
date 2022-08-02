from array import array

import bpy
from bpy.types import Collection, IntAttribute
from mathutils import Matrix, Vector

from .import_context import ImportContext
from ..entities import MeshData, BlenderTextureData


def generate_mesh(context: ImportContext, collection: Collection, mesh_data: MeshData, bone_table, parts,
                  use_correction_matrix: bool = True):
    # Matrix that corrects the axes from FBX coordinate system
    correction_matrix = Matrix([
        [1, 0, 0],
        [0, 0, -1],
        [0, 1, 0]
    ])

    # Correct the vertex positions
    vertices = []
    if use_correction_matrix:
        for vertex in mesh_data.VertexPositions:
            vertices.append(correction_matrix @ Vector([vertex.X, vertex.Y, vertex.Z]))
    else:
        for vertex in mesh_data.VertexPositions:
            vertices.append(Vector([vertex.X, vertex.Y, vertex.Z]))

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

    # Add the parts system
    if len(parts) > 0:
        for parts_group in mesh_data.MeshParts:
            parts_layer: IntAttribute = mesh.attributes.new(name=parts[str(parts_group.PartsId)], type='BOOLEAN',
                                                            domain='FACE')

            sequence = []
            start_index = int(parts_group.StartIndex / 3)
            index_count = int(parts_group.IndexCount / 3)
            end_index = start_index + index_count
            for i in range(len(mesh.polygons)):
                sequence.append(start_index <= i < end_index)

            parts_layer.data.foreach_set("value", sequence)

            # new_group = mesh_object.flagrum_parts.parts_groups.add()
            # new_group.name = parts[str(parts_group.PartsId)]
            # for i in range(parts_group.StartIndex, parts_group.StartIndex + parts_group.IndexCount, 3):
            #     matching_face = mesh.polygons[int(i / 3)]
            #     for vertex_index in matching_face.vertices:
            #         vertex = mesh.vertices[vertex_index]
            #         new_group.vertices.append(vertex)
            # new_vertex = new_group.vertices.add()
            # new_vertex.vertex = vertex

    # Import custom normals
    mesh.update(calc_edges=True)

    clnors = array('f', [0.0] * (len(mesh.loops) * 3))
    mesh.loops.foreach_get("normal", clnors)
    mesh.polygons.foreach_set("use_smooth", [True] * len(mesh.polygons))

    normals = []
    for normal in mesh_data.Normals:
        if use_correction_matrix:
            result = correction_matrix @ Vector([normal.X / 127.0, normal.Y / 127.0, normal.Z / 127.0])
        else:
            result = Vector([normal.X / 127.0, normal.Y / 127.0, normal.Z / 127.0])
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
            else:
                context.texture_slots[texture_slot] = True

        context.materials[mesh_data.BlenderMaterial.Hash] = material

    mesh_object.data.materials.append(material)
    return mesh_object
