import bpy

from bpy.types import NodeTree


def setup_ensure_blue_group() -> NodeTree:
    name = "Ensure Blue Normal Map"
    tree = bpy.data.node_groups.get(name)
    if tree:
        return tree

    tree = bpy.data.node_groups.new(name, 'ShaderNodeTree')
    group_inputs = tree.nodes.new('NodeGroupInput')
    tree.interface.new_socket(name="Color", in_out='INPUT', socket_type='NodeSocketColor')

    group_outputs = tree.nodes.new('NodeGroupOutput')
    tree.interface.new_socket(name="Color", in_out='OUTPUT', socket_type='NodeSocketColor')

    separate_rgb = tree.nodes.new('ShaderNodeSeparateColor')
    combine_rgb = tree.nodes.new('ShaderNodeCombineColor')
    less_than = tree.nodes.new('ShaderNodeMath')
    less_than.operation = 'LESS_THAN'
    less_than.inputs[1].default_value = 0.01
    maximum = tree.nodes.new('ShaderNodeMath')
    maximum.operation = 'MAXIMUM'

    tree.links.new(separate_rgb.inputs['Color'], group_inputs.outputs['Color'])
    tree.links.new(combine_rgb.inputs['Red'], separate_rgb.outputs['Red'])
    tree.links.new(combine_rgb.inputs['Green'], separate_rgb.outputs['Green'])
    tree.links.new(less_than.inputs[0], separate_rgb.outputs['Blue'])
    tree.links.new(maximum.inputs[0], separate_rgb.outputs['Blue'])
    tree.links.new(maximum.inputs[1], less_than.outputs['Value'])
    tree.links.new(combine_rgb.inputs['Blue'], maximum.outputs['Value'])
    tree.links.new(group_outputs.inputs['Color'], combine_rgb.outputs['Color'])

    return tree
