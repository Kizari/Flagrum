import bmesh
import bpy
from bpy.types import Object, Mesh
from mathutils import Matrix, Vector

from .entities import Gpubin, UV, Vector3, MeshData, UVMap, ColorMap, Color4, Normal, MaterialData
from .material_data import original_name_dictionary

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
            mesh.Material = _pack_material(obj)

            # Clone the mesh so we don't mess with the original
            mesh_copy: Object = obj.copy()
            mesh_copy.data = obj.data.copy()
            bpy.context.collection.objects.link(mesh_copy)
            bpy.context.view_layer.objects.active = mesh_copy
            bpy.ops.object.mode_set(mode='EDIT')
            bmesh_copy = bmesh.from_edit_mesh(mesh_copy.data)

            # Clear seams as we need to use them for splitting
            for edge in bmesh_copy.edges:
                if edge.seam:
                    edge.seam = False

            # Select all UV verts as seams_from_islands relies on this to function
            uv_layer = bmesh_copy.loops.layers.uv.verify()
            for face in bmesh_copy.faces:
                for loop in face.loops:
                    loop_uv = loop[uv_layer]
                    loop_uv.select = True

            # Split edges as Luminous relies on tangents calculated this way for correct lighting
            bpy.ops.uv.seams_from_islands()
            island_boundaries = [edge for edge in bmesh_copy.edges if edge.seam and len(edge.link_faces) == 2]
            bmesh.ops.split_edges(bmesh_copy, edges=island_boundaries)

            # Triangulate faces as Luminous only supports tris
            bmesh.ops.triangulate(bmesh_copy, faces=bmesh_copy.faces, quad_method='FIXED', ngon_method='EAR_CLIP')

            # Apply the changes to the cloned mesh
            bmesh.update_edit_mesh(mesh_copy.data)
            bpy.ops.object.mode_set(mode='OBJECT')

            mesh.VertexPositions = _pack_vertex_positions(mesh_copy)
            mesh.FaceIndices = _pack_faces(mesh_copy)
            mesh.UVMaps = _pack_uv_maps(mesh_copy)
            mesh.ColorMaps = _pack_color_maps(mesh_copy)
            weight_indices, weight_values = _pack_weight_maps(mesh_copy, reverse_bone_table)
            mesh.WeightIndices = weight_indices
            mesh.WeightValues = weight_values
            normals, tangents = _pack_normals_and_tangents(mesh_copy)
            mesh.Normals = normals
            mesh.Tangents = tangents
            mesh_data.Meshes.append(mesh)

            # Unlink and delete the clone
            bpy.data.objects.remove(mesh_copy, do_unlink=True)

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


def _take_value(element):
    return element[1]


def _pack_weight_maps(mesh: Object, bone_table):
    weight_indices: list[list[list[int]]] = [[], []]
    weight_values: list[list[list[float]]] = [[], []]

    mesh_data: Mesh = mesh.data

    for i in range(len(mesh_data.vertices)):
        weight_indices[0].append([])
        weight_values[0].append([])
        weight_indices[1].append([])
        weight_values[1].append([])
        counter = 0
        current = []
        for group in mesh_data.vertices[i].groups:
            subgroup = []
            bone_name = mesh.vertex_groups[group.group].name
            if counter < 8:
                if bone_name not in bone_table:
                    continue
                subgroup.append(bone_table[bone_name])
                subgroup.append(int(group.weight * 255.0))
            else:
                break  # Luminous can only handle up to 8 weights per vertex
            counter += 1
            current.append(subgroup)
        record_counter = 0
        current.sort(key=_take_value, reverse=True)
        for record in current:
            if record_counter < 4:
                weight_indices[0][i].append(record[0])
                weight_values[0][i].append(record[1])
            elif record_counter < 8:
                weight_indices[1][i].append(record[0])
                weight_values[1][i].append(record[1])
            else:
                break
            record_counter += 1

    return weight_indices, weight_values


def _pack_color_maps(mesh: Object):
    color_maps: list[ColorMap] = []
    mesh_data: Mesh = mesh.data
    colors = []
    counter = 0

    for color_layer in mesh_data.vertex_colors:
        colors.append({})
        for poly in mesh_data.polygons:
            for index, vertex_index in enumerate(poly.vertices):
                loop_index = poly.loop_indices[index]
                loop = color_layer.data[loop_index]
                color = Color4()
                color.R = int(loop.color[0] * 255)
                color.G = int(loop.color[1] * 255)
                color.B = int(loop.color[2] * 255)
                color.A = int(loop.color[3] * 255)
                colors[counter][vertex_index] = color
        color_map = ColorMap()
        color_map.Colors = []
        for i in range(len(mesh_data.vertices)):
            color_map.Colors.append(colors[counter][i])
        color_maps.append(color_map)
        counter += 1

    return color_maps


def _pack_uv_maps(mesh: Object):
    uv_maps: list[UVMap] = []
    mesh_data: Mesh = mesh.data
    coords = []
    counter = 0

    for layer in mesh_data.uv_layers:
        coords.append({})
        for loop in mesh_data.loops:
            uv = UV()
            temp = layer.data[loop.index].uv
            uv.U = temp[0]
            uv.V = 1 - temp[1]
            coords[counter][loop.vertex_index] = uv
        uv_map = UVMap()
        uv_map.UVs = []
        for i in range(len(mesh_data.vertices)):
            uv_map.UVs.append(coords[counter][i])
        uv_maps.append(uv_map)
        counter += 1

    return uv_maps


def _pack_faces(mesh: Object):
    faces: list[list[int]] = []
    mesh_data: Mesh = mesh.data

    for poly in mesh_data.polygons:
        # Reverse winding order as the game handles them this way
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


def _pack_material(mesh: Object):
    material = mesh.flagrum_material

    for property_definition in material.property_collection:
        if property_definition.material_id == material.preset:
            material_data = property_definition

    data = MaterialData()
    data.Id = material.preset
    data.Name = original_name_dictionary[material.preset]
    data.Inputs = {}
    data.Textures = {}

    for prop in material_data.property_collection:
        property_value = getattr(material_data, prop.property_name)

        if prop.property_type == 'TEXTURE':
            if property_value == '':
                data.Textures[prop.property_name] = property_value
            else:
                data.Textures[prop.property_name] = bpy.path.abspath(property_value)
        else:  # If it's not a texture, it's an input
            if type(property_value) is not float:
                array = []
                for value in property_value:
                    array.append(value)
                data.Inputs[prop.property_name] = array
            else:
                # Put in an array to make handling the JSON easier
                data.Inputs[prop.property_name] = [property_value]

    return data
