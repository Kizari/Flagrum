﻿import bpy
import bpy.path
from bpy.props import StringProperty
from bpy.types import Panel, Operator
from bpy_extras.io_utils import ImportHelper

from .material_data import material_properties, MaterialSettings, material_weight_limit
from ... import addon_updater_ops
from ..import_export.interop import Interop


class TextureSlotOperator(Operator, ImportHelper):
    """Select a texture file from the local file system"""
    bl_idname = "flagrum.texture_slot"
    bl_label = "Open"
    filename_ext = ".btex"

    filter_glob: StringProperty(
        default="*.tif;*.tiff;*.png;*.gif;*.bmp;*.jpg;*.tga;*.dds;*.btex",
        options={'HIDDEN'}
    )

    property: StringProperty()

    def execute(self, context):
        material = context.view_layer.objects.active.flagrum_material
        data = None
        for property_definition in material.property_collection:
            if property_definition.material_id == material.preset:
                data = property_definition

        setattr(data, self.property, self.filepath)
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


class MaterialImportOperator(Operator, ImportHelper):
    """Select a material from the local file system"""
    bl_idname = "flagrum.material_import"
    bl_label = "Import Defaults from GMTL"
    filename_ext = ".gmtl.gfxbin"

    filter_glob: StringProperty(
        default="*.gmtl.gfxbin",
        options={'HIDDEN'}
    )

    def execute(self, context):
        import_dict = Interop.import_material_inputs(self.filepath)

        material = context.view_layer.objects.active.flagrum_material

        for input_name in import_dict:
            values = import_dict[input_name]

            data = None
            for property_definition in material.property_collection:
                if property_definition.material_id == material.preset:
                    data = property_definition

            if data is not None:
                if len(values) > 1:
                    setattr(data, input_name, values)
                else:
                    setattr(data, input_name, values[0])

        # context.area.tag_redraw()
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


class MaterialCopyOperator(Operator):
    """Copies data from the active Flagrum material to the Flagrum material clipboard"""
    bl_idname = "flagrum.material_copy"
    bl_label = "Copy"

    def execute(self, context):
        clipboard = context.window_manager.flagrum_material_clipboard
        material: MaterialSettings = context.view_layer.objects.active.flagrum_material
        active_material_data = None
        for property_definition in material.property_collection:
            if property_definition.material_id == material.preset:
                active_material_data = property_definition

        for attribute in dir(active_material_data):
            if not attribute.startswith("__") and attribute != "property_collection" and attribute != "rna_type":
                setattr(clipboard, attribute, getattr(active_material_data, attribute))

        return {'FINISHED'}


class MaterialPasteOperator(Operator):
    """Pastes data from the Flagrum material clipboard to the active Flagrum material"""
    bl_idname = "flagrum.material_paste"
    bl_label = "Paste"

    @classmethod
    def poll(cls, context):
        material_id = context.window_manager.flagrum_material_clipboard.material_id
        material: MaterialSettings = context.view_layer.objects.active.flagrum_material
        active_material_data = None
        for property_definition in material.property_collection:
            if property_definition.material_id == material.preset:
                active_material_data = property_definition
        return material_id is not None and material_id != '' and active_material_data.material_id == material_id

    def execute(self, context):
        clipboard = context.window_manager.flagrum_material_clipboard
        material: MaterialSettings = context.view_layer.objects.active.flagrum_material
        active_material_data = None
        for property_definition in material.property_collection:
            if property_definition.material_id == material.preset:
                active_material_data = property_definition

        for attribute in dir(clipboard):
            if not attribute.startswith("__") and attribute != "property_collection" and attribute != "rna_type":
                setattr(active_material_data, attribute, getattr(clipboard, attribute))

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

        addon_updater_ops.check_for_update_background()

        active_object = context.view_layer.objects.active
        material = active_object.flagrum_material

        active_material_data = None
        for property_definition in material.property_collection:
            if property_definition.material_id == material.preset:
                active_material_data = property_definition

        addon_updater_ops.update_notice_box_ui(self, context)

        layout.prop(data=material, property="preset")

        if active_material_data is not None and material.preset != 'NONE':
            limit = material_weight_limit[material.preset]
            row = layout.row()
            row.alignment = 'RIGHT'
            row.label(text="Supports up to " + str(limit) + " weights per vertex")
            row = layout.row()
            row.operator(MaterialCopyOperator.bl_idname, icon='COPYDOWN')
            row.operator(MaterialPasteOperator.bl_idname, icon='PASTEDOWN')
            row.operator(MaterialResetOperator.bl_idname)

            iterable_properties = active_material_data.property_collection.items().copy()
            iterable_properties.sort(key=lambda p: (p[1].importance, p[1].property_name))
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
