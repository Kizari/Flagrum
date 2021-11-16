import bpy
from mathutils import Matrix, Vector

from .entities import MeshData


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
    for face in mesh_data.Faces:
        face[0], face[2] = face[2], face[0]

    # Create the mesh
    mesh = bpy.data.meshes.new(mesh_data.Name)
    mesh.from_pydata(vertices, [], mesh_data.Faces)

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
        for coord in uv_data.Coords:
            # The V coordinate is set as 1-V to flip from FBX coordinate system
            coords.append([coord.X, 1 - coord.Y])

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
    context.collection.objects.link(mesh_object)
    mesh.polygons.foreach_set("use_smooth", [True] * len(mesh.polygons))

    # Thanks Sai for the fix here
    layer = bpy.context.view_layer
    layer.update()

    # Add the vertex weights from each weight map
    for weight_map in mesh_data.WeightMaps:
        for weight in weight_map:
            bone_name = bone_table[str(weight.BoneIndex)]
            vertex_group = mesh_object.vertex_groups.get(bone_name)

            if not vertex_group:
                vertex_group = mesh_object.vertex_groups.new(name=bone_name)

            # Weights are divided by 255 to convert from 0-255 to 0.0 - 1.0
            vertex_group.add([weight.VertexIndex], weight.Weight / 255.0, 'ADD')

    # Link the mesh to the armature
    mod = mesh_object.modifiers.new(
        type="ARMATURE", name="Armature")
    mod.use_vertex_groups = True

    armature = bpy.data.objects["Armature"]
    mod.object = armature

    mesh_object.parent = armature
