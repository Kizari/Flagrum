import bpy
from bpy.types import Panel, Operator, Mesh

from .material_data import material_weight_limit


class NormaliseWeightsOperator(Operator):
    bl_idname = "flagrum.cleanup_normalise_weights"
    bl_label = "Normalise Weights"
    bl_description = "Normalises vertex weights to the limits defined by the selected Flagrum materials to " \
                     "ensure a consistent result with the FMD exporter"

    @classmethod
    def poll(cls, context):
        selected_meshes = []
        for obj in context.view_layer.objects.selected:
            if obj.type == 'MESH':
                if obj.flagrum_material.preset is None or obj.flagrum_material.preset == 'NONE':
                    return False
                selected_meshes.append(obj)
        return len(selected_meshes) > 0

    def execute(self, context):
        for obj in context.view_layer.objects.selected:
            if obj.type == 'MESH':
                material = obj.flagrum_material
                mesh_data: Mesh = obj.data
                limit = material_weight_limit[material.preset]
                for vertex in mesh_data.vertices:
                    weights = vertex.groups.items().copy()
                    weights.sort(key=lambda g: g[1].weight, reverse=True)
                    total_weight = 0
                    for i in range(len(weights)):
                        group = weights[i][1]
                        total_weight += group.weight
                        if i == (limit - 1):
                            break
                    for i in range(len(weights)):
                        group = weights[i][1]
                        if i > (limit - 1):
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
        if context.window_manager.flagrum_globals.retain_base_armature:
            bones_to_keep = ["C_Hip", "C_Spine1", "C_Spine1Sub", "C_Spine2", "C_Spine2W", "C_Spine3", "C_Spine3W",
                             "C_Neck1", "C_Head", "Facial_A", "C_HairRoot", "C_HeadEnd", "Facial_B", "C_Throat_B",
                             "C_NeckSub", "C_NeckSubEnd", "C_Neck1W", "C_Neck1WEnd", "L_Shoulder", "L_Forearm",
                             "L_Hand", "L_Socket", "L_Thumb1", "L_Thumb2", "L_Thumb3", "L_ThumbEnd", "L_Index1",
                             "L_Index2", "L_Index3", "L_IndexBulge", "L_Middle1", "L_Middle2", "L_Middle3",
                             "L_MiddleBulge", "L_RingMeta", "L_Ring1", "L_Ring2", "L_Ring3", "L_RingBulge", "L_RingSub",
                             "L_PinkyMeta", "L_Pinky1", "L_Pinky2", "L_Pinky3", "L_PinkyBulge", "L_PinkySub",
                             "L_IndexSub", "L_MiddleSub", "L_ForearmrollA", "L_ForearmrollB", "L_ForearmrollC",
                             "L_Wrist", "L_sleeveSub", "L_DeltoidA", "L_DeltoidB", "L_DeltoidC", "L_Elbow", "L_Bust",
                             "L_armpit", "L_ShoulderSub", "R_Shoulder", "R_UpperArm", "R_Forearm", "R_Hand", "R_Socket",
                             "R_Thumb1", "R_Thumb2", "R_Thumb3", "R_ThumbEnd", "R_Index1", "R_Index2", "R_Index3",
                             "R_IndexBulge", "R_Middle1", "R_Middle2", "R_Middle3", "R_MiddleBulge", "R_RingMeta",
                             "R_Ring1", "R_Ring2", "R_Ring3", "R_RingSub", "R_PinkyMeta", "R_Pinky1", "R_Pinky2",
                             "R_Pinky3", "R_PinkyBulge", "R_PinkySub", "R_IndexSub", "R_MiddleSub", "R_ForearmrollA",
                             "R_ForearmrollB", "R_ForearmrollC", "R_Wrist", "R_sleeveSub", "R_DeltoidA", "R_DeltoidB",
                             "R_DeltoidC", "R_Elbow", "R_Bust", "R_ShoulderSub", "R_armpit", "L_UpperLeg", "L_Foreleg",
                             "L_Foot", "L_Toe", "L_ToeEnd", "L_CalfB", "L_CalfF", "L_ankle", "L_ankleB", "L_FemorisA",
                             "L_FemorisAsub", "L_FemorisB", "L_FemorisC", "L_Knee", "L_UpperLegSub", "L_UpperLegSubEnd",
                             "R_UpperLeg", "R_Foreleg", "R_Foot", "R_Toe", "R_ToeEnd", "R_CalfB", "R_CalfF", "R_ankle",
                             "R_FemorisA", "R_FemorisAsub", "R_FemorisB", "R_FemorisC", "R_Knee", "C_HipW", "C_Spine1W",
                             "C_Spine1WEnd", "L_Hip", "L_Hipback", "L_HipSub", "R_Hip", "R_Hipback", "R_HipSub",
                             "R_HipSubEnd", "c_BeltKdi"]

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
        row = layout.row(align=True)
        row.operator(DeleteUnusedBonesOperator.bl_idname)
        row.prop(context.window_manager.flagrum_globals, property="retain_base_armature", icon='OUTLINER_OB_ARMATURE',
                 icon_only=True)
        layout.operator(DeleteUnusedVGroupsOperator.bl_idname)
        layout.operator(NormaliseWeightsOperator.bl_idname)
