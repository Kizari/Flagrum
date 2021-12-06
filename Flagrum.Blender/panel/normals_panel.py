from array import array

import bpy
from bpy.props import IntProperty, IntVectorProperty
from bpy.types import Panel, Operator, Mesh, PropertyGroup
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


class UseCustomNormalsOperator(Operator):
    bl_idname = "flagrum.use_custom_normals"
    bl_label = "Use Custom Normals"
    bl_description = "Enables custom normals for the selected meshes"

    @classmethod
    def poll(cls, context):
        return context.view_layer.objects.active is not None \
               and context.view_layer.objects.active.type == 'MESH'

    def execute(self, context):
        for mesh_object in context.view_layer.objects.selected:
            mesh: Mesh = mesh_object.data
            mesh.use_auto_smooth = True
            mesh.normals_split_custom_set_from_vertices([vert.normal for vert in mesh.vertices])

            try:
                mesh.calc_tangents()
            except:
                pass

            normals = {}
            tangents = {}

            for loop in mesh.loops:
                if loop.vertex_index not in normals:
                    normals[loop.vertex_index] = []
                    tangents[loop.vertex_index] = []
                normals[loop.vertex_index].append(loop.normal)
                tangents[loop.vertex_index].append(loop.tangent)

            mesh.update(calc_edges=True)

            clnors = array('f', [0.0] * (len(mesh.loops) * 3))
            mesh.loops.foreach_get("normal", clnors)
            mesh.polygons.foreach_set("use_smooth", [True] * len(mesh.polygons))

            final_normals = []
            for i in range(len(normals)):
                # x = []
                # y = []
                # z = []
                # for j in range(len(normals[i])):
                #     x.append(normals[i][j][0])
                #     y.append(normals[i][j][0])
                #     z.append(normals[i][j][0])
                # final_x = 0
                # final_y = 0
                # final_z = 0
                # for j in range(len(x)):
                #     final_x += x[j]
                #     final_y += y[j]
                #     final_z += z[j]
                # final_normal = [final_x / len(x), final_y / len(y), final_z / len(z)]
                final_normals.append(normals[i][0])

            mesh.normals_split_custom_set_from_vertices(final_normals)
            mesh.use_auto_smooth = True

            return {'FINISHED'}


class NormalsPanel(Panel):
    bl_idname = "VIEW_3D_PT_flagrum_normals"
    bl_label = "Custom Normals"
    bl_category = "Flagrum"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'

    def draw(self, context):
        layout = self.layout
        layout.operator(UseCustomNormalsOperator.bl_idname)
        layout.operator(TransferFCNDOperator.bl_idname)
