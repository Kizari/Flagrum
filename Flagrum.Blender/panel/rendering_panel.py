import bpy
from bpy.types import Panel, Operator


class ToggleEmissionOperator(Operator):
    bl_idname = "flagrum.rendering_toggle_emission"
    bl_label = "Toggle Emission"
    bl_description = "Disconnects/reconnects emission textures for all materials in the scene"

    def execute(self, context):
        for material in bpy.data.materials:
            if material.node_tree is not None:
                bsdf = material.node_tree.nodes["Principled BSDF"]
                emission_link = None
                for link in material.node_tree.links:
                    if link.to_node.type == 'BSDF_PRINCIPLED' and link.to_socket.name == "Emission":
                        emission_link = link
                        break

                if emission_link is None:
                    emission_texture = None
                    for node in material.node_tree.nodes:
                        if "was_emission" in node:
                            emission_texture = node
                            break
                    if emission_texture is not None:
                        material.node_tree.links.new(bsdf.inputs['Emission'], emission_texture.outputs['Color'])
                else:
                    emission_link.from_node["was_emission"] = True
                    material.node_tree.links.remove(emission_link)

                # This is needed to force the material to update in the viewport
                # Basically just need some kind of change that triggers an update since the
                # Emission texture connection/disconnection doesn't do it
                bsdf.inputs['Specular'].default_value = 0.5

        return {'FINISHED'}


class SetEmissionOperator(Operator):
    bl_idname = "flagrum.rendering_set_emission"
    bl_label = "Set Emission"
    bl_description = "Sets the emission strength of all materials in the scene"

    def execute(self, context):
        for material in bpy.data.materials:
            if material.node_tree is not None:
                bsdf = material.node_tree.nodes["Principled BSDF"]
                bsdf.inputs[20].default_value = context.window_manager.flagrum_globals.emission_strength

        return {'FINISHED'}


class RenderingPanel(Panel):
    bl_idname = "VIEW_3D_PT_flagrum_rendering"
    bl_label = "Rendering"
    bl_category = "Flagrum"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'

    def draw(self, context):
        layout = self.layout
        layout.operator(ToggleEmissionOperator.bl_idname)
        box = layout.box()
        box.prop(context.window_manager.flagrum_globals, "emission_strength")
        box.operator(SetEmissionOperator.bl_idname)
