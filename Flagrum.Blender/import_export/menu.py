from bpy.props import StringProperty, FloatProperty, BoolProperty
from bpy.types import Operator
from bpy_extras.io_utils import ImportHelper, ExportHelper

from .export_context import ExportContext
from .generate_armature import generate_armature
from .generate_mesh import generate_mesh
from .import_context import ImportContext
from .interop import Interop
from .pack_mesh import pack_mesh
from .read_armature_data import import_armature_data


class ImportOperator(Operator, ImportHelper):
    """Imports data from a Luminous Engine model"""
    bl_idname = "flagrum.gfxbin_import"
    bl_label = "Import gfxbin"
    filename_ext = ".gfxbin"

    filter_glob: StringProperty(
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
    """Exports a mod data package for use with Flagrum"""
    bl_idname = "flagrum.gfxbin_export"
    bl_label = "Export to Flagrum"
    filename_ext = ".fmd"

    filter_glob: StringProperty(
        default="*.fmd",
        options={'HIDDEN'}
    )

    smooth_normals: BoolProperty(
        name="Smooth Normals on Doubles",
        description="When the exporter splits edges for compatibility with FFXV, this functionality will smooth "
                    "the seams caused by the edge-splitting",
        default=False
    )

    distance: FloatProperty(
        name="Distance",
        description="The maximum distance between doubles for which to merge normals",
        default=0.0001,
        min=0.0001,
        precision=4
    )

    def draw(self, context):
        layout = self.layout
        layout.label(text="FMD Export Options")
        layout.prop(self, property="smooth_normals")
        layout.prop(self, property="distance")

    def execute(self, context):
        export_context = ExportContext(self.smooth_normals, self.distance)
        data = pack_mesh(export_context)
        Interop.export_mesh(self.filepath, data)

        return {'FINISHED'}
