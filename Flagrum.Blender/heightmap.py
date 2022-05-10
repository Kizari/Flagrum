import json
import math
from types import SimpleNamespace

import bpy

import_file = open("C:\\Modding\\HebTest\\Leide.json", mode='r')
import_data = import_file.read()
tiles = json.loads(import_data, object_hook=lambda d: SimpleNamespace(**d))

collection = bpy.data.collections.new("Terrain")
bpy.context.scene.collection.children.link(collection)

for data in tiles:
    vertices = []
    for x in range(data.Width):
        for y in range(data.Height):
            vertices.append([x - 256, y - 256, data.Pixels[(x * data.Width) + y]])

    faces = []
    for x in range(data.Width - 1):
        for y in range(data.Height - 1):
            tl = x * data.Height + y
            bl = tl + 1
            tr = tl + data.Height
            br = tr + 1
            faces.append([tl, tr, bl])
            faces.append([tr, br, bl])

    mesh = bpy.data.meshes.new("HeightPlane")
    mesh.from_pydata(vertices, [], faces)

    mesh.validate()
    mesh.update()

    mesh_object = bpy.data.objects.new("HeightPlane", mesh)
    collection.objects.link(mesh_object)

    layer = bpy.context.view_layer
    layer.update()

    mesh_object.location[0] = data.Position[0] + 256
    mesh_object.location[1] = (data.Position[2] * -1) - 256
    mesh_object.rotation_euler[2] = math.radians(-90)
