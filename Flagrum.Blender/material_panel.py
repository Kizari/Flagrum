import bpy
from bpy.types import Panel


class MaterialEditorPanel(Panel):
    bl_idname = "OBJECT_PT_flagrum"
    bl_label = "Flagrum"
    bl_context = "material"
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'

    @classmethod
    def poll(cls, context):
        return bpy.context.view_layer.objects.active.type == 'MESH'

    def draw(self, context):
        layout = self.layout
        layout.prop(data=context.view_layer.objects.active.flagrum_material, property="preset")

        for prop in context.view_layer.objects.active.flagrum_material_properties:
            prop_name = prop[0]
            is_basic = prop[1]
            if is_basic:
                layout.prop(data=context.view_layer.objects.active.flagrum_material, property=prop_name)

        if context.view_layer.objects.active.flagrum_material.preset != 'NONE':
            layout.prop(data=context.view_layer.objects.active.flagrum_material, property="show_advanced")

        if context.view_layer.objects.active.flagrum_material.show_advanced:
            for prop in context.view_layer.objects.active.flagrum_material_properties:
                prop_name = prop[0]
                is_basic = prop[1]
                if not is_basic:
                    layout.prop(data=context.view_layer.objects.active.flagrum_material, property=prop_name)
