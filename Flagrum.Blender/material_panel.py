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
        active_object = context.view_layer.objects.active
        material = active_object.flagrum_material

        active_material_data = None
        for property_definition in material.property_collection:
            if property_definition.material_id == material.preset:
                active_material_data = property_definition

        layout.prop(data=material, property="preset")

        if active_material_data is not None:
            iterable_properties = active_material_data.property_collection.items().copy()
            iterable_properties.sort(key=lambda p: p[1].importance)
            for empty_string, prop in iterable_properties:
                if prop.is_relevant and prop.property_type == 'TEXTURE':
                    layout.prop(data=active_material_data, property=prop.property_name)
            for empty_string, prop in iterable_properties:
                if prop.is_relevant and prop.property_type == 'INPUT':
                    layout.prop(data=active_material_data, property=prop.property_name)

            if material.preset != 'NONE':
                layout.prop(data=material, property="show_advanced")

            if material.show_advanced:
                for empty_string, prop in iterable_properties:
                    if not prop.is_basic and prop.property_type == 'TEXTURE':
                        layout.prop(data=active_material_data, property=prop.property_name)
                for empty_string, prop in iterable_properties:
                    if not prop.is_basic and prop.property_type == 'INPUT':
                        layout.prop(data=active_material_data, property=prop.property_name)
