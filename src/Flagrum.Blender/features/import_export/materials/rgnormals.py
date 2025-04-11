import bpy

from bpy.types import NodeTree


def setup_rgnormals_group() -> NodeTree:
    name = "RG to RGB Normal Map"
    group = bpy.data.node_groups.get(name)
    if group:
        return group

    group = bpy.data.node_groups.new(name, 'ShaderNodeTree')
    group_inputs = group.nodes.new('NodeGroupInput')
    group.interface.new_socket(socket_type='NodeSocketColor', name="Color", in_out='INPUT')

    group_outputs = group.nodes.new('NodeGroupOutput')
    group.interface.new_socket(socket_type='NodeSocketColor', name="Color", in_out='OUTPUT')

    separate_rgb = group.nodes.new('ShaderNodeSeparateColor')
    combine_rgb = group.nodes.new('ShaderNodeCombineColor')
    combine_rgb.inputs['Blue'].default_value = 1.0

    group.links.new(separate_rgb.inputs['Color'], group_inputs.outputs['Color'])
    group.links.new(combine_rgb.inputs['Red'], separate_rgb.outputs['Red'])
    group.links.new(combine_rgb.inputs['Green'], separate_rgb.outputs['Green'])
    group.links.new(group_outputs.inputs['Color'], combine_rgb.outputs['Color'])

    return group
