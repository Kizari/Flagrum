from bpy.types import Panel

from .transfer_fcnd import TransferFCNDOperator


class FlagrumPanel(Panel):
    bl_idname = "flagrum"
    bl_label = "Flagrum"
    bl_category = "Flagrum"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'

    def draw(self, context):
        layout = self.layout
        layout.operator(TransferFCNDOperator.bl_idname)
