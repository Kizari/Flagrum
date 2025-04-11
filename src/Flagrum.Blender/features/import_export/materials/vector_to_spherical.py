import bpy

from bpy.types import NodeTree


def setup_vector_to_spherical_group() -> NodeTree:
    name = "Vector to Spherical"
    group = bpy.data.node_groups.get(name)
    if group:
        return group

    group = bpy.data.node_groups.new(name, 'ShaderNodeTree')
    group_inputs = group.nodes.new('NodeGroupInput')
    group.interface.new_socket(socket_type='NodeSocketVector', name="Vector", in_out='INPUT')

    group_outputs = group.nodes.new('NodeGroupOutput')
    group.interface.new_socket(socket_type='NodeSocketFloat', name="Theta", in_out='OUTPUT')
    group.interface.new_socket(socket_type='NodeSocketFloat', name="Phi", in_out='OUTPUT')

    separate_xyz = group.nodes.new('ShaderNodeSeparateXYZ')
    combine_xyz = group.nodes.new('ShaderNodeCombineXYZ')
    length = group.nodes.new('ShaderNodeVectorMath')
    length.operation = 'LENGTH'
    theta = group.nodes.new('ShaderNodeMath')
    theta.operation = 'ARCTAN2'
    phi = group.nodes.new('ShaderNodeMath')
    phi.operation = 'ARCTAN2'

    group.links.new(separate_xyz.inputs['Vector'], group_inputs.outputs['Vector'])
    group.links.new(combine_xyz.inputs['X'], separate_xyz.outputs['X'])
    group.links.new(combine_xyz.inputs['Y'], separate_xyz.outputs['Y'])
    group.links.new(theta.inputs[1], separate_xyz.outputs['Z'])
    group.links.new(length.inputs['Vector'], combine_xyz.outputs['Vector'])
    group.links.new(theta.inputs[0], length.outputs['Value'])
    group.links.new(phi.inputs[0], separate_xyz.outputs['Y'])
    group.links.new(phi.inputs[1], separate_xyz.outputs['X'])
    group.links.new(group_outputs.inputs['Theta'], theta.outputs['Value'])
    group.links.new(group_outputs.inputs['Phi'], phi.outputs['Value'])

    return group
