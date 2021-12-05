from bpy.types import Panel

from .custom_normals import UseCustomNormalsOperator
from .transfer_fcnd import TransferFCNDOperator


class FlagrumPanel(Panel):
    bl_idname = "flagrum"
    bl_label = "Flagrum"
    bl_category = "Flagrum"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'

    def draw(self, context):
        layout = self.layout
        layout.operator(UseCustomNormalsOperator.bl_idname)
        layout.operator(TransferFCNDOperator.bl_idname)
