import bpy
from bpy.types import Object, Mesh
from mathutils import Matrix

from .entities import Gpubin, Vector2, Vector3, MeshData, UVMap, ColorMap, Color4, VertexWeight


def pack_mesh():
    mesh_data = Gpubin()
    mesh_data.Meshes = []
    mesh_data.BoneTable = _pack_bone_table()

    for obj in bpy.data.objects:
        if obj.type == 'MESH':
            mesh = MeshData()
            mesh.Name = obj.name
            mesh.VertexPositions = _pack_vertex_positions(obj)
            mesh.Faces = _pack_faces(obj)
            mesh.UVMaps = _pack_uv_maps(obj)
            mesh.ColorMaps = _pack_color_maps(obj)
            mesh.WeightMaps = _pack_weight_maps(obj)
            mesh_data.Meshes.append(mesh)

    return mesh_data


def _pack_bone_table():
    bone_table = {}
    armature = bpy.data.armatures[0]

    for i in range(len(armature.bones)):
        bone_table[i] = armature.bones[i].name

    return bone_table


def _pack_weight_maps(mesh: Object):
    weight_maps: list[list[VertexWeight]] = []
    mesh_data: Mesh = mesh.data

    for i in range(len(mesh_data.vertices)):
        vertex_weights: list[VertexWeight] = []
        for group in mesh_data.vertices[i].groups:
            vertex_weight = VertexWeight()
            vertex_weight.VertexIndex = i
            vertex_weight.BoneIndex = group.group
            vertex_weight.Weight = int(group.weight * 255)
            vertex_weights.append(vertex_weight)
        weight_maps.append(vertex_weights)

    return weight_maps


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

    for uv_layer in mesh_data.uv_layers:
        uv_map = UVMap()
        uv_map.Coords = []
        for loop in uv_layer.data:
            for i in range(len(loop.uv)):
                if (i + 1) % 2 == 0:
                    continue
                uv = Vector2()
                uv.X = loop.uv[i]
                uv.Y = 1 - loop.uv[i + 1]  # Flip V for FBX coord system
                uv_map.Coords.append(uv)
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
    # Matrix that converts the axes back to FBX coordinate system
    conversion_matrix = Matrix([
        [1, 0, 0],
        [0, 0, -1],
        [0, 1, 0]
    ])

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
