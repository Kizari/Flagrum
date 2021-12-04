import bpy
from bpy.props import PointerProperty
from bpy.utils import register_class, unregister_class

from .material_data import MaterialSettings, FlagrumMaterialProperty, FlagrumMaterialPropertyCollection
from .material_panel import MaterialEditorPanel
from .menu import ImportOperator, ExportOperator

bl_info = {
    "name": "Flagrum Blender Plugin",
    "version": (1, 0, 0),
    "blender": (2, 80, 0),
    "location": "File > Import-Export",
    "description": "Build mod data for Flagrum",
    "category": "Import-Export",
}

classes = (
    ImportOperator,
    ExportOperator,
    FlagrumMaterialProperty,
    FlagrumMaterialPropertyCollection,
    MaterialEditorPanel,
    MaterialSettings
)


def import_menu_item(self, context):
    self.layout.operator(ImportOperator.bl_idname,
                         text="FFXV Model (.gfxbin)")


def export_menu_item(self, context):
    self.layout.operator(ExportOperator.bl_idname,
                         text="Flagrum (.fmd)")


def register():
    for cls in classes:
        register_class(cls)
    bpy.types.TOPBAR_MT_file_import.append(import_menu_item)
    bpy.types.TOPBAR_MT_file_export.append(export_menu_item)
    bpy.types.Object.flagrum_material = PointerProperty(type=MaterialSettings)


def unregister():
    bpy.types.Object.flagrum_material = None
    bpy.types.TOPBAR_MT_file_import.remove(export_menu_item)
    bpy.types.TOPBAR_MT_file_import.remove(import_menu_item)
    for cls in reversed(classes):
        unregister_class(cls)


if __name__ == "__main__":
    register()
