import bpy

from .menu import ImportOperator

bl_info = {
    "name": "GFXBIN format",
    "version": (1, 1, 0),
    "blender": (2, 80, 0),
    "location": "File > Import-Export",
    "description": "Import a Luminous Engine model",
    "category": "Import-Export",
}


def menu_func_import(self, context):
    self.layout.operator(ImportOperator.bl_idname,
                         text="Luminous Engine (.gfxbin)")


def register():
    bpy.utils.register_class(ImportOperator)
    bpy.types.TOPBAR_MT_file_import.append(menu_func_import)


def unregister():
    bpy.types.TOPBAR_MT_file_import.remove(menu_func_import)
    bpy.utils.unregister_class(ImportOperator)


if __name__ == "__main__":
    register()
    bpy.ops.import_test.some_data('INVOKE_DEFAULT')
