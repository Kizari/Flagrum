import bpy
from bpy.types import Panel, Operator


class DeleteUnusedVGroupsOperator(Operator):
    bl_idname = "flagrum.cleanup_delete_unused_vgroups"
    bl_label = "Unused Vert. Groups"
    bl_description = "Deletes all vertex groups that are not weighted to any vertices in the active mesh"

    @classmethod
    def poll(cls, context):
        return context.view_layer.objects.active is not None and context.view_layer.objects.active.type == 'MESH'

    def execute(self, context):
        mesh = context.view_layer.objects.active

        groups = {i: False for i, k in enumerate(mesh.vertex_groups)}

        for vertex in mesh.data.vertices:
            for group in vertex.groups:
                if group.weight > 0:
                    groups[group.group] = True

        for index, used in sorted(groups.items(), reverse=True):
            if not used:
                mesh.vertex_groups.remove(mesh.vertex_groups[index])

        return {'FINISHED'}


class DeleteUnusedBonesOperator(Operator):
    bl_idname = "flagrum.cleanup_delete_unused_bones"
    bl_label = "Unused Bones"
    bl_description = "Deletes all bones in the active armature that do not have any vertices weighted to them"

    @classmethod
    def poll(cls, context):
        return context.view_layer.objects.active is not None and context.view_layer.objects.active.type == 'ARMATURE'

    def execute(self, context):
        armature = context.view_layer.objects.active

        meshes = []
        for obj in bpy.data.objects:
            if obj.type == 'MESH' and obj.parent == armature:
                meshes.append(obj)

        bones_to_keep = ["C_Hip"]

        for mesh in meshes:
            groups = {i: False for i, k in enumerate(mesh.vertex_groups)}

            for vertex in mesh.data.vertices:
                for group in vertex.groups:
                    if group.weight > 0:
                        groups[group.group] = True

            for index, used in sorted(groups.items(), reverse=True):
                if used:
                    bones_to_keep.append(mesh.vertex_groups[index].name)

        current_mode = context.object.mode
        bpy.ops.object.mode_set(mode='EDIT')

        for bone in armature.data.edit_bones:
            if bone.name not in bones_to_keep:
                armature.data.edit_bones.remove(bone)

        bpy.ops.object.mode_set(mode=current_mode)

        return {'FINISHED'}


class CleanupPanel(Panel):
    bl_idname = "VIEW_3D_PT_flagrum_cleanup"
    bl_label = "Cleanup"
    bl_category = "Flagrum"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'

    def draw(self, context):
        layout = self.layout
        layout.operator(DeleteUnusedBonesOperator.bl_idname)
        layout.operator(DeleteUnusedVGroupsOperator.bl_idname)
