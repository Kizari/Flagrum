import bmesh
import bpy
import bpy.path
from bpy.props import IntProperty, StringProperty, CollectionProperty
from bpy.types import Panel, PropertyGroup, Operator, UIList, Mesh, MeshVertex, Attribute, AttributeGroup

reserved_attribute_names = ["position", "radius", "id", "material_index", "crease", "sharp_face", "resolution",
                            "cyclic", "handle_left", "handle_right"]


class PartsVertex(PropertyGroup):
    vertex: MeshVertex = None


class PartsGroup(PropertyGroup):
    name: StringProperty()
    vertices: list[MeshVertex] = []


class PartsSettings(PropertyGroup):
    def active_parts_group_changed(self, context):
        items = [a for a in context.view_layer.objects.active.data.attributes if
                 a.domain == 'FACE' and a.data_type == 'BOOLEAN' and not a.name.startswith(
                     ".") and a.name not in reserved_attribute_names]
        self.active_parts_group_name = items[self.active_parts_group].name

    active_parts_group: IntProperty(default=-1, update=active_parts_group_changed)
    active_parts_group_name: StringProperty()
    parts_groups: CollectionProperty(type=PartsGroup)


class AddPartsGroupOperator(Operator):
    """Creates a new parts group for this object"""
    bl_idname = "flagrum.parts_add"
    bl_label = ""

    def execute(self, context):
        active_object = context.view_layer.objects.active
        active_object.data.attributes.new(name="Parts", type='BOOLEAN', domain='FACE')
        return {'FINISHED'}


class RemovePartsGroupOperator(Operator):
    """Removes an existing parts group from this object"""
    bl_idname = "flagrum.parts_remove"
    bl_label = ""

    @classmethod
    def poll(cls, context):
        parts: PartsSettings = context.view_layer.objects.active.flagrum_parts
        return parts.active_parts_group > -1

    def execute(self, context):
        active_object = context.view_layer.objects.active
        parts: PartsSettings = active_object.flagrum_parts
        attributes: AttributeGroup = active_object.data.attributes
        attributes.remove(attributes[parts.active_parts_group])
        return {'FINISHED'}


class AssignPartsGroupOperator(Operator):
    bl_idname = "flagrum.parts_assign"
    bl_label = "Assign"

    def execute(self, context):
        # Must do this in object mode or the parts attributes will be empty
        face_layers = {}
        bpy.ops.object.mode_set(mode='OBJECT')
        parts: PartsSettings = context.view_layer.objects.active.flagrum_parts
        active_mesh: Mesh = context.view_layer.objects.active.data

        # Get state of all parts layers
        for i in range(len(active_mesh.attributes)):
            layer = active_mesh.attributes[i]
            if layer.domain == 'FACE' and layer.data_type == 'BOOLEAN':
                faces = [False] * len(active_mesh.polygons)
                layer.data.foreach_get("value", faces)
                face_layers[i] = faces

        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(active_mesh)
        bm.faces.ensure_lookup_table()

        for key in face_layers:
            layer = active_mesh.attributes[key]
            if layer.domain == 'FACE' and layer.data_type == 'BOOLEAN':
                for i in range(len(bm.faces)):
                    face = bm.faces[i]
                    if face.select:
                        face_layers[key][i] = key == parts.active_parts_group

        bpy.ops.object.mode_set(mode='OBJECT')
        for key in face_layers:
            layer = active_mesh.attributes[key]
            layer.data.foreach_set("value", face_layers[key])

        bpy.ops.object.mode_set(mode='EDIT')

        return {'FINISHED'}


class UnassignPartsGroupOperator(Operator):
    bl_idname = "flagrum.parts_unassign"
    bl_label = "Remove"

    def execute(self, context):
        # Must do this in object mode or the parts attributes will be empty
        bpy.ops.object.mode_set(mode='OBJECT')
        parts: PartsSettings = context.view_layer.objects.active.flagrum_parts
        active_mesh: Mesh = context.view_layer.objects.active.data
        layer = active_mesh.attributes[parts.active_parts_group]

        faces = [False] * len(active_mesh.polygons)
        layer.data.foreach_get("value", faces)

        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(active_mesh)
        bm.faces.ensure_lookup_table()

        for i in range(len(bm.faces)):
            face = bm.faces[i]
            if face.select:
                faces[i] = False

        bpy.ops.object.mode_set(mode='OBJECT')
        layer = active_mesh.attributes[parts.active_parts_group]
        layer.data.foreach_set("value", faces)
        bpy.ops.object.mode_set(mode='EDIT')

        return {'FINISHED'}


class SelectPartsGroupOperator(Operator):
    bl_idname = "flagrum.parts_select"
    bl_label = "Select"

    def execute(self, context):
        # Must do this in object mode or the parts attributes will be empty
        bpy.ops.object.mode_set(mode='OBJECT')
        active_mesh: Mesh = context.view_layer.objects.active.data
        parts: PartsSettings = context.view_layer.objects.active.flagrum_parts
        parts_layer = active_mesh.attributes[parts.active_parts_group_name]
        faces = [False] * len(active_mesh.polygons)
        parts_layer.data.foreach_get("value", faces)

        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(active_mesh)
        bm.faces.ensure_lookup_table()

        for i in range(len(bm.faces)):
            if faces[i]:
                bm.faces[i].select_set(True)

        bm.select_flush(True)
        bmesh.update_edit_mesh(active_mesh)
        return {'FINISHED'}


class DeselectPartsGroupOperator(Operator):
    bl_idname = "flagrum.parts_deselect"
    bl_label = "Deselect"

    def execute(self, context):
        # Must do this in object mode or the parts attributes will be empty
        bpy.ops.object.mode_set(mode='OBJECT')
        parts: PartsSettings = context.view_layer.objects.active.flagrum_parts
        active_mesh: Mesh = context.view_layer.objects.active.data
        parts_layer = active_mesh.attributes[parts.active_parts_group]
        faces = [False] * len(active_mesh.polygons)
        parts_layer.data.foreach_get("value", faces)

        bpy.ops.object.mode_set(mode='EDIT')
        bm = bmesh.from_edit_mesh(active_mesh)
        bm.faces.ensure_lookup_table()

        for i in range(len(bm.faces)):
            if faces[i]:
                bm.faces[i].select_set(False)

        bm.select_flush(True)
        bmesh.update_edit_mesh(active_mesh)
        return {'FINISHED'}


class PartsGroupsList(UIList):
    """UI List for Parts Groups"""
    bl_idname = "OBJECT_UL_flagrum_parts_groups_list"

    def draw_item(self,
                  context: 'Context',
                  layout: 'UILayout',
                  data: 'AnyType',
                  item: Attribute,
                  icon: int,
                  active_data: 'AnyType',
                  active_property: str,
                  index: int = 0,
                  flt_flag: int = 0):
        layout.prop(item, property="name", text="", emboss=False, icon='CLIPUV_DEHLT')

    def filter_items(self, context, data, propname):
        attributes = getattr(data, propname)
        helper_funcs = bpy.types.UI_UL_list

        filter_flags = [self.bitflag_filter_item] * len(attributes)

        for i, attribute in enumerate(attributes):
            if (attribute.domain != 'FACE' or attribute.data_type != 'BOOLEAN' or attribute.name.startswith(".")
                    or attribute.name in reserved_attribute_names):
                filter_flags[i] &= ~self.bitflag_filter_item

        filter_new_order = helper_funcs.sort_items_by_name(attributes, "name")

        return filter_flags, filter_new_order


class PartsSystemPanel(Panel):
    bl_idname = "OBJECT_PT_flagrum_parts_system"
    bl_label = "Parts Groups"
    bl_context = "data"
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'

    @classmethod
    def poll(cls, context):
        return bpy.context.view_layer.objects.active.type == 'MESH'

    def draw(self, context):
        layout = self.layout
        active_object = context.view_layer.objects.active
        parts = active_object.flagrum_parts

        row = layout.split(factor=0.95)
        row.template_list(PartsGroupsList.bl_idname, "", active_object.data, "attributes", parts, "active_parts_group")
        column = row.column()
        top_column = column.column(align=True)
        top_column.operator(AddPartsGroupOperator.bl_idname, icon='ADD', text="")
        top_column.operator(RemovePartsGroupOperator.bl_idname, icon='REMOVE', text="")
        # bottom_column = column.column(align=True)
        # bottom_column.operator(AddPartsGroupOperator.bl_idname, icon='TRIA_UP', text="")
        # bottom_column.operator(AddPartsGroupOperator.bl_idname, icon='TRIA_DOWN', text="")
        if context.view_layer.objects.active.mode == 'EDIT' and parts.active_parts_group > -1:
            row = layout.row()
            left_row = row.row(align=True)
            left_row.operator(AssignPartsGroupOperator.bl_idname)
            left_row.operator(UnassignPartsGroupOperator.bl_idname)
            right_row = row.row(align=True)
            right_row.operator(SelectPartsGroupOperator.bl_idname)
            right_row.operator(DeselectPartsGroupOperator.bl_idname)
