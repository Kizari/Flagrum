from array import array

from bpy.types import Operator, Mesh


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
