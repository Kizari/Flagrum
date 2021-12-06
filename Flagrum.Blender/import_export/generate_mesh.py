import bpy
from mathutils import Matrix, Vector, kdtree

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

    # Add normal data as a custom data if present
    size = len(mesh_data.Normals)
    if size > 0:
        print("Custom normal data detected")

        kd = kdtree.KDTree(size)
        for i in range(size):
            vertex = mesh.vertices[i]
            kd.insert(vertex.co, i)

        kd.balance()

        for i in range(size):
            vertex = vertices[i]
            for (co, index, distance) in kd.find_range([vertex[0], vertex[1], vertex[2]], 0.001):
                match = index
                break
            normal = mesh_data.Normals[i]
            tangent = mesh_data.Tangents[i]
            fcnd = mesh_object.flagrum_custom_normal_data.add()
            fcnd.vertex_index = match
            fcnd.normal = [normal.X, normal.Y, normal.Z, normal.W]
            fcnd.tangent = [tangent.X, tangent.Y, tangent.Z, tangent.W]

    # Thanks Sai for the fix here
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
        type="ARMATURE", name="Armature")
    mod.use_vertex_groups = True

    armature = bpy.data.objects["Armature"]
    mod.object = armature

    mesh_object.parent = armature