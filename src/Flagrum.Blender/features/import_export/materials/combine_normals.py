import math

import bpy
from bpy.types import NodeTree

from .spherical_to_vector import setup_spherical_to_vector_group
from .vector_to_spherical import setup_vector_to_spherical_group


def setup_combine_normals_group() -> NodeTree:
    name = "Combine Normals"
    group = bpy.data.node_groups.get(name)
    if group:
        return group

    group = bpy.data.node_groups.new(name, 'ShaderNodeTree')
    group_inputs = group.nodes.new('NodeGroupInput')
    group.interface.new_socket(name="Normal Map", in_out='INPUT', socket_type='NodeSocketColor')
    detail_normal_map = group.interface.new_socket(socket_type='NodeSocketColor', name="Detail Normal Map",
                                                   in_out='INPUT')
    detail_normal_map.default_value = [0.5, 0.5, 1.0, 1.0]
    group.interface.new_socket(socket_type='NodeSocketFloat', name="Detail Normal Strength", in_out='INPUT')
    detail_normal_mask = group.interface.new_socket(socket_type='NodeSocketColor', name="Detail Normal Mask",
                                                    in_out='INPUT')
    detail_normal_mask.default_value = [1.0, 1.0, 1.0, 1.0]

    group_outputs = group.nodes.new('NodeGroupOutput')
    group.interface.new_socket(socket_type='NodeSocketColor', name="Color", in_out='OUTPUT')

    normalize_output = _setup_normalise_nodes(group, group_inputs.outputs[0])
    normalize_output_detail = _setup_normalise_nodes(group, group_inputs.outputs[1])

    vector_to_spherical_detail = group.nodes.new('ShaderNodeGroup')
    vector_to_spherical_detail.node_tree = setup_vector_to_spherical_group()
    group.links.new(vector_to_spherical_detail.inputs['Vector'], normalize_output_detail)

    multiply = group.nodes.new('ShaderNodeMath')
    multiply.operation = 'MULTIPLY'
    group.links.new(multiply.inputs[0], vector_to_spherical_detail.outputs['Theta'])
    group.links.new(multiply.inputs[1], group_inputs.outputs[2])

    clamp = group.nodes.new('ShaderNodeClamp')
    clamp.inputs[2].default_value = math.pi / 2
    group.links.new(clamp.inputs[0], multiply.outputs['Value'])

    spherical_to_vector = group.nodes.new('ShaderNodeGroup')
    spherical_to_vector.node_tree = setup_spherical_to_vector_group()
    group.links.new(spherical_to_vector.inputs['Theta'], clamp.outputs['Result'])
    group.links.new(spherical_to_vector.inputs['Phi'], vector_to_spherical_detail.outputs['Phi'])

    multiply_mask = group.nodes.new('ShaderNodeVectorMath')
    multiply_mask.operation = 'MULTIPLY'
    group.links.new(multiply_mask.inputs[0], spherical_to_vector.outputs['Vector'])
    group.links.new(multiply_mask.inputs[1], group_inputs.outputs[3])

    vector_to_spherical = group.nodes.new('ShaderNodeGroup')
    vector_to_spherical.node_tree = setup_vector_to_spherical_group()
    group.links.new(vector_to_spherical.inputs['Vector'], normalize_output)

    rotate_z_invert = group.nodes.new('ShaderNodeVectorRotate')
    rotate_z_invert.rotation_type = 'Z_AXIS'
    rotate_z_invert.invert = True
    group.links.new(rotate_z_invert.inputs['Vector'], multiply_mask.outputs['Vector'])
    group.links.new(rotate_z_invert.inputs['Angle'], vector_to_spherical.outputs['Phi'])

    rotate_y = group.nodes.new('ShaderNodeVectorRotate')
    rotate_y.rotation_type = 'Y_AXIS'
    group.links.new(rotate_y.inputs['Vector'], rotate_z_invert.outputs['Vector'])
    group.links.new(rotate_y.inputs['Angle'], vector_to_spherical.outputs['Theta'])

    rotate_z = group.nodes.new('ShaderNodeVectorRotate')
    rotate_z.rotation_type = 'Z_AXIS'
    group.links.new(rotate_z.inputs['Vector'], rotate_y.outputs['Vector'])
    group.links.new(rotate_z.inputs['Angle'], vector_to_spherical.outputs['Phi'])

    separate_xyz = group.nodes.new('ShaderNodeSeparateXYZ')
    group.links.new(separate_xyz.inputs['Vector'], rotate_z.outputs['Vector'])

    clamp = group.nodes.new('ShaderNodeClamp')
    group.links.new(clamp.inputs[0], separate_xyz.outputs['Z'])

    combine_xyz = group.nodes.new('ShaderNodeCombineXYZ')
    group.links.new(combine_xyz.inputs['X'], separate_xyz.outputs['X'])
    group.links.new(combine_xyz.inputs['Y'], separate_xyz.outputs['Y'])
    group.links.new(combine_xyz.inputs['Z'], clamp.outputs['Result'])

    normalize = group.nodes.new('ShaderNodeVectorMath')
    normalize.operation = 'NORMALIZE'
    group.links.new(normalize.inputs['Vector'], combine_xyz.outputs['Vector'])

    add = group.nodes.new('ShaderNodeVectorMath')
    add.operation = 'ADD'
    add.inputs[1].default_value[0] = 1
    add.inputs[1].default_value[1] = 1
    add.inputs[1].default_value[2] = 1
    group.links.new(add.inputs[0], normalize.outputs['Vector'])

    divide = group.nodes.new('ShaderNodeVectorMath')
    divide.operation = 'DIVIDE'
    divide.inputs[1].default_value[0] = 2
    divide.inputs[1].default_value[1] = 2
    divide.inputs[1].default_value[2] = 2
    group.links.new(divide.inputs[0], add.outputs['Vector'])
    group.links.new(group_outputs.inputs['Color'], divide.outputs['Vector'])

    return group


def _setup_normalise_nodes(group: NodeTree, input_connection):
    multiply = group.nodes.new('ShaderNodeVectorMath')
    multiply.operation = 'MULTIPLY'
    multiply.inputs[1].default_value[0] = 2
    multiply.inputs[1].default_value[1] = 2
    multiply.inputs[1].default_value[2] = 2

    subtract = group.nodes.new('ShaderNodeVectorMath')
    subtract.operation = 'SUBTRACT'
    subtract.inputs[1].default_value[0] = 1
    subtract.inputs[1].default_value[1] = 1
    subtract.inputs[1].default_value[2] = 1

    normalize = group.nodes.new('ShaderNodeVectorMath')
    normalize.operation = 'NORMALIZE'

    group.links.new(multiply.inputs[0], input_connection)
    group.links.new(subtract.inputs[0], multiply.outputs['Vector'])
    group.links.new(normalize.inputs[0], subtract.outputs['Vector'])
    return normalize.outputs['Vector']
