from datetime import datetime

from bpy.props import StringProperty, EnumProperty
from bpy.types import Operator
from bpy_extras.io_utils import ImportHelper, ExportHelper

from .generate_armature import generate_armature
from .generate_mesh import generate_mesh
from .import_context import ImportContext
from .interop import Interop
from .pack_mesh import pack_mesh
from .read_armature_data import import_armature_data


class ImportOperator(Operator, ImportHelper):
    """Imports gfxbin files into Blender"""
    bl_idname = "flagrum.gfxbin_import"
    bl_label = "Import gfxbin"
    filename_ext = ".gfxbin"
    filter_glob = StringProperty(
        default="*.gfxbin",
        options={'HIDDEN'}
    )

    def execute(self, context):
        import_context = ImportContext(self.filepath)

        armature_data = import_armature_data(import_context)
        generate_armature(import_context, armature_data)

        mesh_data = Interop.import_mesh(import_context.gfxbin_path)

        for mesh in mesh_data.Meshes:
            generate_mesh(import_context, mesh, mesh_data.BoneTable.__dict__)

        return {'FINISHED'}


class ExportOperator(Operator, ExportHelper):
    """Exports Blender files to mod archives"""
    bl_idname = "flagrum.gfxbin_export"
    bl_label = "Export FFXV Mod"
    filename_ext = ".ffxvbinmod"
    filter_glob = StringProperty(
        default="*.ffxvbinmod",
        options={'HIDDEN'}
    )

    character: EnumProperty(
        items=(
            ('NOCTIS', "Noctis", ""),
            ('PROMPTO', "Prompto", ""),
            ('IGNIS', "Ignis", ""),
            ('GLADIOLUS', "Gladiolus", "")
        ),
        name="Character",
        description="Choose the character this model will be applied to",
        default=0,
        options={'ANIMATABLE'}
    )

    uuid: StringProperty(name="UUID")
    title: StringProperty(name="Title")

    def draw(self, context):
        layout = self.layout
        layout.prop(data=self, property="title")
        layout.prop(data=self, property="character")
        layout.prop(data=self, property="uuid")

    def execute(self, context):
        print(datetime.now())
        print("Packing Data...")
        data = pack_mesh()
        data.Title = self.title
        data.Target = self.character
        data.Uuid = self.uuid
        Interop.export_mesh(self.filepath, data)

        return {'FINISHED'}
