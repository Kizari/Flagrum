import bpy
from mathutils import Matrix, Vector
from numpy.linalg import solve


def createEmptyTree(armature, context):
    miniscene = {}
    createRootNub(miniscene)
    for ix, bone in enumerate(armature):
        if ix not in miniscene:
            createNub(ix, bone, armature, miniscene)
    miniscene[255].name = 'Armature'
    miniscene[255]["Type"] = "SkeletonRoot"
    context.armature = miniscene
    return


def createRootNub():
    o = bpy.data.objects.new("Root", None)
    bpy.context.scene.objects.link(o)
    o.show_wire = True
    o.show_x_ray = True
    return o


def createNub(bone, parent=None):
    # raise ValueError(bone.keys())
    # o = bpy.data.objects.new("Bone.%03d"%ix, None )
    o = bpy.data.objects.new("%s" % bone.name, None)  # ix
    bpy.context.scene.objects.link(o)
    o.show_wire = False
    o.show_x_ray = True
    o.show_bounds = False
    o.empty_draw_size = 0.01
    o.empty_draw_type = "SPHERE"
    transformedMatrix = bone.matrix
    # transformedMatrix = Matrix.Identity(4)
    # transformedMatrix.translation = transformedMatrix2.translation
    o.parent = parent
    bpy.context.scene.update()
    o.matrix_world = Matrix.Identity(4) if parent is None else transformedMatrix
    bpy.context.scene.update()
    if bone.name == "C_Spine1":
        print(o.matrix_local)
        print()
    for child in bone.children:
        createNub(child, o)
    return o


def visitBones(bone, visited):
    visited.add(bone.id)
    for child in bone.children:
        if child.id in visited:
            raise ValueError("Cycle Detected at bone %d:%s" % (bone.id, bone.name))
        visitBones(child, visited)
    return visited


def detectCycles(root, miniscene):
    for rootb in root:
        visited = set()
        visitBones(rootb, visited)
    for bid in miniscene:
        if bid not in visited:
            bone = miniscene[bid]
            # print(bone.name, bone.parent if not hasattr(bone.parent,"id") else bone.parent.name)
            raise ValueError("Disconnected Bone %d:%s" % (bone.id, bone.name))
    return


def processArmatureData(armature_data):
    root = []
    miniscene = {}
    for bone in armature_data.bones:
        if bone.id in miniscene:
            raise KeyError("Duplicated ID %d" % bone.id)
        else:
            bone.children = []
            miniscene[bone.id] = bone
            bone.matrix = Matrix(solve(bone.transformation_matrix, Matrix.Identity(4)))
    for bone in armature_data.bones:
        if bone.id:
            bone.parent = miniscene[armature_data.parent_IDs[bone.id - 1]]
            miniscene[armature_data.parent_IDs[bone.id - 1]].children.append(bone)
        else:
            root.append(bone)
            bone.parent = None
    detectCycles(root, miniscene)
    return root


def _generate_armature(armature_data):
    world = createRootNub()
    root = processArmatureData(armature_data)
    for rootb in root:
        rootEmpty = createNub(rootb)
        rootEmpty.parent = world
    return world


def distance(point, origin, line):
    pointPrime = point - origin
    a = pointPrime.project(line).magnitude
    c = pointPrime.magnitude
    # a2+b2 = c2
    return c ** 2 - a ** 2


def minimizeDistance(origin, transform, points):
    line = transform @ Vector([0, 1, 0])
    minima = -1
    minimizer = None
    dst = None
    for point in points:
        if point.matrix.translation == origin:
            continue
        d = distance(point.matrix.translation, origin, line)
        if 0 < d < minima or minima == -1:
            minima = d
            minimizer = point
            dst = (point.matrix.translation - origin).magnitude
            # return point.matrix.translation
            break
    if minimizer is None:
        return None
    return origin + (transform @ Vector([0, dst, 0, 0])).to_3d()


def matGen(ixlist):
    m = [[1 if j == i else (-1 if j == i % 10 else 0) for j in range(len(ixlist))] for i in ixlist]
    return Matrix(m)


def createBone(bone, armature, parent=None, per=[1, 2, 0, 3]):
    new_bone = armature.edit_bones.new(bone.name)
    new_bone["ID"] = bone.id
    transformedMatrix = bone.matrix
    if parent:
        new_bone.parent = parent
        correction = matGen(per)
        transform = transformedMatrix @ correction
    else:
        correction = matGen(per)
        transform = transformedMatrix @ correction
    new_bone.head = transform.translation
    preferred = minimizeDistance(transform.translation, transform, bone.children)
    if preferred is None:
        if parent is not None:
            delta = transform @ Vector([0, min(0.01, parent.length), 0, 0])
            preferred = transform.translation + delta.to_3d()
            # print(bone.name,"Parent Length",transform.determinant(),delta.length)
        else:
            # print(bone.name,"Arbitrary Length")
            preferred = transform.translation + transform @ Vector([0, 0.01, 0])
    if preferred == transform.translation:
        # print(bone.name,"Arbitrary Length - Fixed")
        preferred = transform.translation + Vector([0, 0.01, 0])
    new_bone.tail = preferred
    new_bone.matrix = transform
    for child in bone.children:
        createBone(child, armature, new_bone, per)


def generate_armature(context, armature_data):
    armature_name = "Armature"
    armature = bpy.data.armatures.new(armature_name)
    armature_object = bpy.data.objects.new(armature_name, armature)
    armature_object.data.name = "Armature"
    bpy.context.scene.collection.children.link(context.collection)
    context.collection.objects.link(armature_object);
    bpy.context.view_layer.objects.active = armature_object
    bpy.ops.object.mode_set(mode='EDIT')

    root = processArmatureData(armature_data)
    for rootb in root:
        createBone(rootb, armature)
    bpy.ops.object.mode_set(mode='OBJECT')
