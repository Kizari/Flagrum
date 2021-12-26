import bpy
from bpy.types import Panel, Operator, Mesh


class NormaliseWeightsOperator(Operator):
    bl_idname = "flagrum.cleanup_normalise_weights"
    bl_label = "Normalise Weights"
    bl_description = "Limits weights to 4 per vertex and normalises existing weights to ensure a consistent result " \
                     "with the exporter "

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
                mesh_data: Mesh = obj.data
                for vertex in mesh_data.vertices:
                    weights = vertex.groups.items().copy()
                    weights.sort(key=lambda g: g[1].weight, reverse=True)
                    total_weight = 0
                    for i in range(len(weights)):
                        group = weights[i][1]
                        total_weight += group.weight
                        if i == 3:
                            break
                    for i in range(len(weights)):
                        group = weights[i][1]
                        if i > 3:
                            obj.vertex_groups[group.group].remove([vertex.index])
                            continue
                        if group.weight > 0:
                            normalised_weight = group.weight / total_weight
                            obj.vertex_groups[group.group].add([vertex.index], normalised_weight, 'REPLACE')
                        else:
                            obj.vertex_groups[group.group].remove([vertex.index])

        return {'FINISHED'}


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
        layout.operator(NormaliseWeightsOperator.bl_idname)
