import bpy

from bpy.types import NodeTree


def setup_spherical_to_vector_group() -> NodeTree:
    name = "Spherical to Vector"
    group = bpy.data.node_groups.get(name)
    if group:
        return group

    group = bpy.data.node_groups.new(name, 'ShaderNodeTree')
    group_inputs = group.nodes.new('NodeGroupInput')
    group.interface.new_socket(socket_type='NodeSocketFloat', name="Theta", in_out='INPUT')
    group.interface.new_socket(socket_type='NodeSocketFloat', name="Phi", in_out='INPUT')

    group_outputs = group.nodes.new('NodeGroupOutput')
    group.interface.new_socket(socket_type='NodeSocketVector', name="Vector", in_out='OUTPUT')

    combine_xyz = group.nodes.new('ShaderNodeCombineXYZ')
    combine_xyz.inputs[2].default_value = 1

    rotate_y = group.nodes.new('ShaderNodeVectorRotate')
    rotate_y.rotation_type = 'Y_AXIS'

    rotate_z = group.nodes.new('ShaderNodeVectorRotate')
    rotate_z.rotation_type = 'Z_AXIS'

    group.links.new(rotate_y.inputs['Vector'], combine_xyz.outputs['Vector'])
    group.links.new(rotate_y.inputs['Angle'], group_inputs.outputs['Theta'])
    group.links.new(rotate_z.inputs['Vector'], rotate_y.outputs['Vector'])
    group.links.new(rotate_z.inputs['Angle'], group_inputs.outputs['Phi'])
    group.links.new(group_outputs.inputs['Vector'], rotate_z.outputs['Vector'])

    return group
