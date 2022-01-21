import bpy
from mathutils import Matrix, Vector

conversion_matrix = Matrix([
    [1, 0, 0],
    [0, 0, 1],
    [0, -1, 0]
])

normals = {}
tangents = {}
bitangents = {}

bpy.data.objects["EyelashShape"].data.calc_tangents()
for loop in bpy.data.objects["EyelashShape"].data.loops:
    converted = conversion_matrix @ loop.normal
    normal = [
        converted[0],
        converted[1],
        converted[2]
    ]
    loop_tangent = loop.tangent
    loop_tangent.normalize()
    converted = conversion_matrix @ loop_tangent
    tangent = [
        converted[0],
        converted[1],
        converted[2]
    ]
    if loop.vertex_index not in normals:
        normals[loop.vertex_index] = Vector(normal)
    if loop.vertex_index not in tangents:
        tangents[loop.vertex_index] = Vector(tangent)
        bitangents[loop.vertex_index] = loop.bitangent_sign

file = open(r"C:\Modding\normals.txt", "w+")
for index in range(len(normals)):
    normal = normals[index]
    tangent = tangents[index]
    bitangent = bitangents[index]
    # normal.normalize()
    normal = [
        int(normal[0] * 127.0),
        int(normal[1] * 127.0),
        int(normal[2] * 127.0),
        0
    ]
    tangent = [
        int(tangent[0] * 127.0),
        int(tangent[1] * 127.0),
        int(tangent[2] * 127.0),
        int(bitangent * 127.0)
    ]
    file.write("[" + str(tangent[0]) + ", " + str(tangent[1]) + ", " + str(tangent[2]) + ", " + str(tangent[3]) + "], ")
file.close()
