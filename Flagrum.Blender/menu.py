from bpy.types import Operator
from bpy_extras.io_utils import ImportHelper

from .generate_armature import generate_armature
from .generate_mesh import generate_mesh
from .import_context import ImportContext
from .interop import Interop
from .read_armature_data import import_armature_data


class ImportOperator(Operator, ImportHelper):
    """Imports gfxbin files into Blender"""
    bl_idname = "import_test.some_data"
    bl_label = "Import gfxbin"
    filename_ext = ".gfxbin"

    def execute(self, context):
        import_context = ImportContext(self.filepath)

        armature_data = import_armature_data(import_context)
        generate_armature(import_context, armature_data)

        mesh_data = Interop.import_mesh(import_context.gfxbin_path)
        for mesh in mesh_data.Meshes:
            generate_mesh(import_context, mesh, mesh_data.BoneTable.__dict__)

        return {'FINISHED'}
