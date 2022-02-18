from array import array

import bpy
from mathutils import Matrix, Vector

from ..entities import MeshData


def generate_mesh(context, mesh_data: MeshData, bone_table):
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
    color_map_counter = 0
    for i in range(len(mesh_data.ColorMaps)):
        vertex_colors = mesh_data.ColorMaps[i].Colors
        if len(vertex_colors) < 1:
            continue

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
        mesh.vertex_colors[color_map_counter].data.foreach_set("color", per_loop_list)
        color_map_counter += 1

    mesh.validate()
    mesh.update()

    mesh_object = bpy.data.objects.new(mesh_data.Name, mesh)
    context.collection.objects.link(mesh_object)

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
    mod = mesh_object.modifiers.new(
        type="ARMATURE", name=context.collection.name)
    mod.use_vertex_groups = True

    armature = bpy.data.objects[context.collection.name]
    mod.object = armature

    mesh_object.parent = armature
