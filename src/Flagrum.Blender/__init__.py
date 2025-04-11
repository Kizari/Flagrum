import bpy
from bpy.props import PointerProperty, BoolProperty, IntProperty, StringProperty
from bpy.types import AddonPreferences
from bpy.utils import register_class, unregister_class

from .features.environment.import_operator import ImportEnvironmentOperator
from .features.import_export.menu import ImportOperator, ExportOperator, FlagrumImportMenu, ImportTerrainOperator
from .features.panel.cleanup_panel import CleanupPanel, DeleteUnusedBonesOperator, DeleteUnusedVGroupsOperator, \
    NormaliseWeightsOperator
from .features.panel.material_data import MaterialSettings, FlagrumMaterialProperty, FlagrumMaterialPropertyCollection
from .features.panel.material_panel import MaterialEditorPanel, MaterialResetOperator, TextureSlotOperator, \
    ClearTextureOperator, MaterialImportOperator, MaterialCopyOperator, MaterialPasteOperator
from .features.panel.normals_panel import UseCustomNormalsOperator, NormalsPanel, SplitEdgesOperator
from .features.panel.parts_panel import PartsSettings, PartsSystemPanel, PartsVertex, PartsGroup, AddPartsGroupOperator, \
    PartsGroupsList, RemovePartsGroupOperator, SelectPartsGroupOperator, DeselectPartsGroupOperator, \
    AssignPartsGroupOperator, UnassignPartsGroupOperator
from .features.panel.rendering_panel import ToggleEmissionOperator, RenderingPanel, SetEmissionOperator
from . import addon_updater_ops
from .utilities.globals import FlagrumGlobals

bl_info = {
    "name": "Flagrum",
    "version": (1, 4, 0),
    "blender": (3, 0, 0),
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

    game_data_directory: StringProperty(
        name="Game Data Directory",
        description="The path to the unpacked game files",
        subtype='DIR_PATH'
    )

    def draw(self, context):
        addon_updater_ops.update_settings_ui(self, context)
        self.layout.prop(self, "game_data_directory")


classes = (
    FlagrumPreferences,
    ImportOperator,
    ExportOperator,
    ImportEnvironmentOperator,
    ImportTerrainOperator,
    FlagrumImportMenu,
    FlagrumMaterialProperty,
    FlagrumMaterialPropertyCollection,
    TextureSlotOperator,
    ClearTextureOperator,
    MaterialResetOperator,
    MaterialImportOperator,
    MaterialCopyOperator,
    MaterialPasteOperator,
    MaterialEditorPanel,
    MaterialSettings,
    UseCustomNormalsOperator,
    SplitEdgesOperator,
    DeleteUnusedBonesOperator,
    DeleteUnusedVGroupsOperator,
    NormaliseWeightsOperator,
    CleanupPanel,
    NormalsPanel,
    ToggleEmissionOperator,
    SetEmissionOperator,
    RenderingPanel,
    FlagrumGlobals,
    PartsVertex,
    PartsGroup,
    PartsSettings,
    AddPartsGroupOperator,
    RemovePartsGroupOperator,
    PartsGroupsList,
    AssignPartsGroupOperator,
    UnassignPartsGroupOperator,
    SelectPartsGroupOperator,
    DeselectPartsGroupOperator,
    PartsSystemPanel
)


def import_menu_item(self, context):
    self.layout.menu(FlagrumImportMenu.bl_idname)


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
    bpy.types.Object.flagrum_parts = PointerProperty(type=PartsSettings)
    bpy.types.WindowManager.flagrum_material_clipboard = PointerProperty(type=FlagrumMaterialPropertyCollection)
    bpy.types.WindowManager.flagrum_globals = PointerProperty(type=FlagrumGlobals)


def unregister():
    del bpy.types.WindowManager.flagrum_globals
    del bpy.types.WindowManager.flagrum_material_clipboard
    del bpy.types.Object.flagrum_material
    bpy.types.TOPBAR_MT_file_import.remove(export_menu_item)
    bpy.types.TOPBAR_MT_file_import.remove(import_menu_item)
    for cls in reversed(classes):
        unregister_class(cls)
    addon_updater_ops.unregister()


if __name__ == "__main__":
    register()
