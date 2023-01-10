import bpy
from bpy.types import EditBone
from mathutils import Matrix, Vector, Quaternion

from .proto.lik_pb2 import IKRig, IKJoint


def print_joints_recursively(rig: IKRig, joint: IKJoint, indent: int):
    prefix = ""
    for i in range(indent):
        prefix = prefix + "—"

    bind = joint.bindPose
    position = bind.position
    rotation = bind.rotation

    print(prefix + joint.modelBoneName + ": position=(" + str(position.x) + ", " + str(position.y) + ", " + str(
        position.z) + ") rotation=(" + str(rotation.x) + ", " + str(rotation.y) + ", " + str(rotation.z) + ", " + str(
        rotation.w) + ")")

    for childIndex in joint.children:
        print_joints_recursively(rig, rig.joints[childIndex], indent + 1)


def process_bones_recursively(rig: IKRig, joint: IKJoint, edit_bones, parent: EditBone):
    bind = joint.bindPose
    position = bind.position
    rotation = bind.rotation

    # Matrix that corrects the axes from FBX coordinate system
    correction_matrix = Matrix([
        [1, 0, 0],
        [0, 0, -1],
        [0, 1, 0]
    ])

    bone: EditBone = edit_bones.new(joint.modelBoneName)
    bone.head = [position.x, position.y, position.z]
    bone.tail = bone.head + Vector([0.1, 0, 0])
    rotation_matrix = correction_matrix.to_4x4() @ Quaternion(
        [rotation.w, rotation.x, rotation.y, rotation.z]).to_matrix().to_4x4()
    translation_matrix = correction_matrix.to_4x4() @ Matrix.Translation(Vector([position.x, position.y, position.z]))
    transform_matrix = translation_matrix @ rotation_matrix
    bone.matrix = transform_matrix

    if parent is not None:
        bone.parent = parent

    for childIndex in joint.children:
        process_bones_recursively(rig, rig.joints[childIndex], edit_bones, bone)


def import_lik_from_file(path: str):
    # Read the IK rig from the LIK file
    rig = IKRig()
    with open(path, "rb") as file:
        rig.ParseFromString(file.read())

    # Create a new armature
    collection = bpy.data.collections.new("Test")
    armature_name = "Test_IK"
    armature = bpy.data.armatures.new(armature_name)
    armature_object = bpy.data.objects.new(armature_name, armature)
    armature_object.data.name = armature_name
    bpy.context.scene.collection.children.link(collection)
    collection.objects.link(armature_object)
    bpy.context.view_layer.objects.active = armature_object

    # Create the bones
    bpy.ops.object.mode_set(mode='EDIT', toggle=False)
    process_bones_recursively(rig, rig.joints[0], armature.edit_bones, None)
    bpy.ops.object.mode_set(mode='OBJECT')
