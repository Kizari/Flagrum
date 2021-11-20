import bpy
from bpy.types import Object, Mesh
from mathutils import Matrix, Vector

from .entities import Gpubin, UV, Vector3, MeshData, UVMap, ColorMap, Color4, Normal

# Matrix that converts the axes back to FBX coordinate system
conversion_matrix = Matrix([
    [1, 0, 0],
    [0, 0, 1],
    [0, -1, 0]
])


def pack_mesh():
    mesh_data = Gpubin()
    mesh_data.Meshes = []

    bone_table, reverse_bone_table = _pack_bone_table()
    mesh_data.BoneTable = bone_table

    for obj in bpy.data.objects:
        if obj.type == 'MESH':
            mesh = MeshData()
            mesh.Name = obj.name
            mesh.VertexPositions = _pack_vertex_positions(obj)
            mesh.FaceIndices = _pack_faces(obj)
            mesh.UVMaps = _pack_uv_maps(obj)
            # mesh.ColorMaps = _pack_color_maps(obj)
            weight_indices, weight_values = _pack_weight_maps(obj, reverse_bone_table)
            mesh.WeightIndices = weight_indices
            mesh.WeightValues = weight_values
            normals, tangents = _pack_normals_and_tangents(obj)
            mesh.Normals = normals
            mesh.Tangents = tangents
            mesh_data.Meshes.append(mesh)

    return mesh_data


def _pack_normals_and_tangents(mesh: Object):
    normals: list[Normal] = []
    tangents: list[Normal] = []
    mesh_data: Mesh = mesh.data

    mesh_data.calc_tangents()

    normals_dict = {}
    tangents_dict = {}

    for face in mesh_data.polygons:
        for vertex in [mesh_data.loops[i] for i in face.loop_indices]:
            normal = Normal()
            tangent = Normal()
            converted_normal = conversion_matrix @ Vector(vertex.normal)
            converted_tangent = conversion_matrix @ Vector(vertex.tangent)
            normal.X = int(converted_normal[0] * 127.0)
            normal.Y = int(converted_normal[1] * 127.0)
            normal.Z = int(converted_normal[2] * 127.0)
            normal.W = 0
            tangent.X = int(converted_tangent[0] * 127.0)
            tangent.Y = int(converted_tangent[1] * 127.0)
            tangent.Z = int(converted_tangent[2] * 127.0)
            tangent.W = -127
            normals_dict[vertex.vertex_index] = normal
            tangents_dict[vertex.vertex_index] = tangent

    for i in range(len(mesh_data.vertices)):
        normals.append(normals_dict[i])
        tangents.append(tangents_dict[i])

    return normals, tangents


def _pack_bone_table():
    bone_table = {}
    reverse_bone_table = {}
    armature = bpy.data.armatures[0]

    for i in range(len(armature.bones)):
        bone_table[i] = armature.bones[i].name
        reverse_bone_table[armature.bones[i].name] = i

    return bone_table, reverse_bone_table


def _pack_weight_maps(mesh: Object, bone_table):
    weight_indices: list[list[list[int]]] = [[]]
    weight_values: list[list[list[float]]] = [[]]

    mesh_data: Mesh = mesh.data

    for i in range(len(mesh_data.vertices)):
        weight_indices[0].append([])
        weight_values[0].append([])
        counter = 0
        for group in mesh_data.vertices[i].groups:
            bone_name = mesh.vertex_groups[group.group].name
            weight_indices[0][i].append(bone_table[bone_name])
            weight_values[0][i].append(int(group.weight * 255.0))
            counter += 1
            if counter > 3:
                break

    return weight_indices, weight_values


def _pack_color_maps(mesh: Object):
    color_maps: list[ColorMap] = []
    mesh_data: Mesh = mesh.data

    for color_layer in mesh_data.vertex_colors:
        color_map = ColorMap()
        color_map.Colors = []
        for loop in color_layer.data:
            color = Color4()
            color.R = int(loop.color[0] * 255)
            color.G = int(loop.color[1] * 255)
            color.B = int(loop.color[2] * 255)
            color.A = int(loop.color[3] * 255)
            color_map.Colors.append(color)
        color_maps.append(color_map)

    return color_maps


def _pack_uv_maps(mesh: Object):
    uv_maps: list[UVMap] = []
    mesh_data: Mesh = mesh.data

    # for uv_layer in mesh_data.uv_layers:
    #     uv_map = UVMap()
    #     uv_map.UVs = []
    #     for loop in uv_layer.data:
    #         for i in range(len(loop.uv)):
    #             if (i + 1) % 2 == 0:
    #                 continue
    #             uv = UV()
    #             uv.U = loop.uv[i]
    #             uv.V = 1 - loop.uv[i + 1]  # Flip V for FBX coord system
    #             uv_map.UVs.append(uv)
    #     uv_maps.append(uv_map)

    coords = {}
    layer = mesh_data.uv_layers["map1"]
    for loop in mesh_data.loops:
        uv = UV()
        temp = layer.data[loop.index].uv
        uv.U = temp[0]
        uv.V = 1 - temp[1]
        coords[loop.vertex_index] = uv

    uv_map = UVMap()
    uv_map.UVs = []
    for i in range(len(mesh_data.vertices)):
        uv_map.UVs.append(coords[i])
    uv_maps.append(uv_map)

    return uv_maps


def _pack_faces(mesh: Object):
    faces: list[list[int]] = []
    mesh_data: Mesh = mesh.data

    for poly in mesh_data.polygons:
        # Reverse winding order so normals face correct way in-game
        face = [poly.vertices[2], poly.vertices[1], poly.vertices[0]]
        faces.append(face)

    return faces


def _pack_vertex_positions(mesh: Object):
    vertex_positions: list[Vector3] = []
    mesh_data: Mesh = mesh.data
    for vertex in mesh_data.vertices:
        converted_position = conversion_matrix @ vertex.co
        position = Vector3()
        position.X = converted_position[0]
        position.Y = converted_position[1]
        position.Z = converted_position[2]
        vertex_positions.append(position)

    return vertex_positions
