import bpy
import bpy.path
from bpy.props import StringProperty
from bpy.types import Panel, Operator
from bpy_extras.io_utils import ImportHelper

from .material_data import material_properties


class TextureSlotOperator(Operator, ImportHelper):
    """Select a texture file from the local file system"""
    bl_idname = "flagrum.texture_slot"
    bl_label = "Open"
    filename_ext = ".btex"

    filter_glob: StringProperty(
        default="*.tif;*.tiff;*.png;*.gif;*.bmp;*.jpg;*.btex",
        options={'HIDDEN'}
    )

    property: StringProperty()

    def execute(self, context):
        material = context.view_layer.objects.active.flagrum_material
        data = None
        for property_definition in material.property_collection:
            if property_definition.material_id == material.preset:
                data = property_definition

        setattr(data, self.property, bpy.path.relpath(self.filepath))
        context.area.tag_redraw()
        return {'FINISHED'}


class ClearTextureOperator(Operator):
    """Clears the current texture from the corresponding texture slot"""
    bl_idname = "flagrum.clear_texture"
    bl_label = "Clear"

    property: StringProperty()

    def execute(self, context):
        material = context.view_layer.objects.active.flagrum_material
        data = None
        for property_definition in material.property_collection:
            if property_definition.material_id == material.preset:
                data = property_definition

        setattr(data, self.property, "")
        return {'FINISHED'}


class MaterialResetOperator(Operator):
    """Resets all material properties to their default values"""
    bl_idname = "flagrum.material_reset"
    bl_label = "Reset to Defaults"

    def execute(self, context):
        material = context.view_layer.objects.active.flagrum_material
        active_material_data = None
        for property_definition in material.property_collection:
            if property_definition.material_id == material.preset:
                active_material_data = property_definition

        defaults = material_properties[material.preset]
        for default in defaults:
            setattr(active_material_data, default.property_name, default.default_value)

        return {'FINISHED'}


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
            layout.operator(MaterialResetOperator.bl_idname)
            iterable_properties = active_material_data.property_collection.items().copy()
            iterable_properties.sort(key=lambda p: p[1].importance)
            for empty_string, prop in iterable_properties:
                if prop.is_relevant and prop.property_type == 'TEXTURE':
                    row = layout.row()
                    row.label(text=prop.property_name)
                    row.prop(data=active_material_data, property=prop.property_name, text="")
                    texture_slot = row.operator(TextureSlotOperator.bl_idname, text="", icon='FILEBROWSER')
                    texture_slot.property = prop.property_name
                    texture_slot = row.operator(ClearTextureOperator.bl_idname, text="", icon='X')
                    texture_slot.property = prop.property_name
            for empty_string, prop in iterable_properties:
                if prop.is_relevant and prop.property_type == 'INPUT':
                    layout.prop(data=active_material_data, property=prop.property_name)

            if material.preset != 'NONE':
                layout.prop(data=material, property="show_advanced")

            if material.show_advanced:
                for empty_string, prop in iterable_properties:
                    if not prop.is_relevant and prop.property_type == 'TEXTURE':
                        row = layout.row()
                        row.label(text=prop.property_name)
                        row.prop(data=active_material_data, property=prop.property_name, text="")
                        texture_slot = row.operator(TextureSlotOperator.bl_idname, text="", icon='FILEBROWSER')
                        texture_slot.property = prop.property_name
                        texture_slot = row.operator(ClearTextureOperator.bl_idname, text="", icon='X')
                        texture_slot.property = prop.property_name
                for empty_string, prop in iterable_properties:
                    if not prop.is_relevant and prop.property_type == 'INPUT':
                        layout.prop(data=active_material_data, property=prop.property_name)
