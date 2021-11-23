import bpy
from bpy.utils import register_class, unregister_class

from .material_panel import MaterialEditorPanel
from .material_data import MaterialSettings
from .menu import ImportOperator, ExportOperator

bl_info = {
    "name": "GFXBIN format",
    "version": (1, 1, 0),
    "blender": (2, 80, 0),
    "location": "File > Import-Export",
    "description": "Import a Luminous Engine model",
    "category": "Import-Export",
}

classes = (
    ImportOperator,
    ExportOperator,
    MaterialEditorPanel,
    MaterialSettings
)


def import_menu_item(self, context):
    self.layout.operator(ImportOperator.bl_idname,
                         text="Luminous Engine (.gfxbin)")


def export_menu_item(self, context):
    self.layout.operator(ExportOperator.bl_idname,
                         text="Luminous Engine (.gfxbin)")


def register():
    for cls in classes:
        register_class(cls)
    bpy.types.TOPBAR_MT_file_import.append(import_menu_item)
    bpy.types.TOPBAR_MT_file_export.append(export_menu_item)
    bpy.types.Object.flagrum_material = bpy.props.PointerProperty(type=MaterialSettings)
    bpy.types.Object.flagrum_material_properties = []


def unregister():
    bpy.types.Object.flagrum_material = None
    bpy.types.TOPBAR_MT_file_import.remove(export_menu_item)
    bpy.types.TOPBAR_MT_file_import.remove(import_menu_item)
    for cls in reversed(classes):
        unregister_class(cls)


if __name__ == "__main__":
    register()
