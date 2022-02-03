from array import array

import bmesh
import bpy
from bpy.types import Panel, Operator, Mesh

from ..operations.merge_normals import merge_normals


class UseCustomNormalsOperator(Operator):
    bl_idname = "flagrum.use_custom_normals"
    bl_label = "Use Custom Normals"
    bl_description = "Enables custom normals for the selected meshes"

    @classmethod
    def poll(cls, context):
        return context.view_layer.objects.active is not None \
               and context.view_layer.objects.active.type == 'MESH' \
               and not context.view_layer.objects.active.data.has_custom_normals

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
                final_normals.append(normals[i][0])

            mesh.normals_split_custom_set_from_vertices(final_normals)
            mesh.use_auto_smooth = True

            return {'FINISHED'}


class SplitEdgesOperator(Operator):
    bl_idname = "flagrum.split_edges"
    bl_label = "Split Edges"
    bl_description = "Creates doubles along UV boundaries for compatibility with Luminous"

    @classmethod
    def poll(cls, context):
        selected_meshes = []
        for obj in context.view_layer.objects.selected:
            if obj.type == 'MESH':
                selected_meshes.append(obj)
        return len(selected_meshes) > 0

    def execute(self, context):
        for obj in context.view_layer.objects.selected:
            if obj.type == 'MESH':
                # Make sure all verts are selected otherwise some of the bmesh operations shit themselves
                for vertex in obj.data.vertices:
                    vertex.select = True

                bpy.ops.object.mode_set(mode='EDIT')
                bmesh_copy = bmesh.from_edit_mesh(obj.data)

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

                # Split edges
                bpy.ops.uv.seams_from_islands()
                island_boundaries = [edge for edge in bmesh_copy.edges if edge.seam and len(edge.link_faces) == 2]
                bmesh.ops.split_edges(bmesh_copy, edges=island_boundaries)

                # Apply the changes to the mesh
                bmesh.update_edit_mesh(obj.data)
                bpy.ops.object.mode_set(mode='OBJECT')

        return {'FINISHED'}


class MergeNormalsOperator(Operator):
    bl_idname = "flagrum.merge_normals"
    bl_label = "Merge Normals"
    bl_description = "Averages normals across doubles on selected meshes to smooth out the shading along seams"

    @classmethod
    def poll(cls, context):
        selected_meshes = []
        for obj in context.view_layer.objects.selected:
            if obj.type == 'MESH':
                selected_meshes.append(obj)
        return len(selected_meshes) > 0

    def execute(self, context):
        for obj in context.view_layer.objects.selected:
            if obj.type == 'MESH':
                merge_normals(obj.data, 0.0001)

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
        layout.operator(SplitEdgesOperator.bl_idname)
        layout.operator(MergeNormalsOperator.bl_idname)
