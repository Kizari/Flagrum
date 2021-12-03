import bpy
from bpy.props import IntProperty, IntVectorProperty
from bpy.types import Operator, Mesh, PropertyGroup
from mathutils import Vector


class FCNDSettings(PropertyGroup):
    vertex_index: IntProperty()
    normal: IntVectorProperty(size=4)
    tangent: IntVectorProperty(size=4)


class TransferFCNDOperator(Operator):
    bl_idname = "flagrum.transfer_fcnd"
    bl_label = "Transfer FCDN"
    bl_description = "Transfers preserved normal/tangent data from selected vertices of one mesh to another"

    @classmethod
    def poll(cls, context):
        return context.view_layer.objects.active is not None \
               and context.view_layer.objects.active.type == 'MESH' \
               and context.view_layer.objects.active.mode == 'EDIT' \
               and len(context.view_layer.objects.selected) > 1

    def execute(self, context):
        source_object = context.view_layer.objects.selected[1]
        target_object = context.view_layer.objects.active
        source_data = source_object.flagrum_custom_normal_data
        target_data = target_object.flagrum_custom_normal_data
        source: Mesh = source_object.data
        target: Mesh = target_object.data

        print("Source mesh: " + source_object.name)
        print("Target mesh: " + target_object.name)

        # Need to return to object mode to commit selected vertices
        mode = target_object.mode
        bpy.ops.object.mode_set(mode='OBJECT')

        source_verts = []
        target_verts = []

        for i in range(len(source.vertices)):
            vertex = source.vertices[i]
            if vertex.select:
                source_verts.append((i, vertex))

        for i in range(len(target.vertices)):
            vertex = target.vertices[i]
            if vertex.select:
                target_verts.append((i, vertex))

        print("Source vertices count: " + str(len(source_verts)))
        print("Target vertices count: " + str(len(target_verts)))

        for i in range(len(source_verts)):
            source_vertex = source_verts[i][1]
            for j in range(len(target_verts)):
                target_vertex = target_verts[j][1]
                if -0.001 < (Vector(source_vertex.co) - Vector(target_vertex.co)).length < 0.001:
                    for item in source_data:
                        if item.vertex_index == i:
                            source_fcnd = item
                            break
                    target_fcnd = target_data.add()
                    target_fcnd.vertex_index = target_verts[j][0]
                    target_fcnd.normal = source_fcnd.normal
                    target_fcnd.tangent = source_fcnd.tangent
                    print("Copied custom data to target.")

        bpy.ops.object.mode_set(mode=mode)

        return {'FINISHED'}
