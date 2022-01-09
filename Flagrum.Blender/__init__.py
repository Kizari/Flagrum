import bpy
from bpy.props import PointerProperty, CollectionProperty, BoolProperty, IntProperty
from bpy.types import AddonPreferences
from bpy.utils import register_class, unregister_class

from . import addon_updater_ops
from .import_export.menu import ImportOperator, ExportOperator
from .panel.cleanup_panel import CleanupPanel, DeleteUnusedBonesOperator, DeleteUnusedVGroupsOperator, \
    NormaliseWeightsOperator
from .panel.material_data import MaterialSettings, FlagrumMaterialProperty, FlagrumMaterialPropertyCollection
from .panel.material_panel import MaterialEditorPanel, MaterialResetOperator, TextureSlotOperator, \
    ClearTextureOperator, MaterialImportOperator
from .panel.normals_panel import UseCustomNormalsOperator, TransferFCNDOperator, FCNDSettings, NormalsPanel

bl_info = {
    "name": "Flagrum",
    "version": (1, 0, 2),
    "blender": (2, 80, 0),
    "location": "File > Import-Export",
    "description": "Blender add-on for Flagrum",
    "category": "Import-Export",
}


@addon_updater_ops.make_annotations
class FlagrumPreferences(AddonPreferences):
    bl_idname = __package__

    auto_check_update = BoolProperty(
        name="Auto-check for Update",
        description="If enabled, auto-check for updates using an interval",
        default=True,
    )

    updater_interval_months = IntProperty(
        name='Months',
        description="Number of months between checking for updates",
        default=0,
        min=0
    )

    updater_interval_days = IntProperty(
        name='Days',
        description="Number of days between checking for updates",
        default=0,
        min=0,
    )

    updater_interval_hours = IntProperty(
        name='Hours',
        description="Number of hours between checking for updates",
        default=1,
        min=0,
        max=23
    )

    updater_interval_minutes = IntProperty(
        name='Minutes',
        description="Number of minutes between checking for updates",
        default=0,
        min=0,
        max=59
    )

    def draw(self, context):
        addon_updater_ops.update_settings_ui(self, context)


classes = (
    FlagrumPreferences,
    ImportOperator,
    ExportOperator,
    FlagrumMaterialProperty,
    FlagrumMaterialPropertyCollection,
    TextureSlotOperator,
    ClearTextureOperator,
    MaterialResetOperator,
    MaterialImportOperator,
    MaterialEditorPanel,
    MaterialSettings,
    FCNDSettings,
    TransferFCNDOperator,
    UseCustomNormalsOperator,
    DeleteUnusedBonesOperator,
    DeleteUnusedVGroupsOperator,
    NormaliseWeightsOperator,
    CleanupPanel,
    NormalsPanel
)


def import_menu_item(self, context):
    self.layout.operator(ImportOperator.bl_idname,
                         text="FFXV Model (.gfxbin)")


def export_menu_item(self, context):
    self.layout.operator(ExportOperator.bl_idname,
                         text="Flagrum (.fmd)")


def register():
    addon_updater_ops.register(bl_info)
    for cls in classes:
        register_class(cls)
    bpy.types.TOPBAR_MT_file_import.append(import_menu_item)
    bpy.types.TOPBAR_MT_file_export.append(export_menu_item)
    bpy.types.Object.flagrum_material = PointerProperty(type=MaterialSettings)
    bpy.types.Object.flagrum_custom_normal_data = CollectionProperty(type=FCNDSettings)


def unregister():
    del bpy.types.Object.flagrum_custom_normal_data
    del bpy.types.Object.flagrum_material
    bpy.types.TOPBAR_MT_file_import.remove(export_menu_item)
    bpy.types.TOPBAR_MT_file_import.remove(import_menu_item)
    for cls in reversed(classes):
        unregister_class(cls)
    addon_updater_ops.unregister()


if __name__ == "__main__":
    register()
