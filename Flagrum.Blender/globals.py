from bpy.props import BoolProperty
from bpy.types import PropertyGroup


class FlagrumGlobals(PropertyGroup):
    retain_base_armature: BoolProperty(
        name="Retain base armature",
        description="Prevents removal of unused bones from removing base bones even if no vertices are weighted "
                    "to them",
        default=False
    )
