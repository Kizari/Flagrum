import numpy
import os
import struct
from mathutils import Matrix

from ..graphics.fmd.entities import ArmatureData, BoneData


def import_armature_data(context):
    if os.path.exists(context.amdl_path):
        amdl_file = open(context.amdl_path, "rb")
        armature_data = _read_armature_data(amdl_file)
        amdl_file.close()
        return armature_data
    else:
        return None


def _read_armature_data(amdl_file):
    armature_data = ArmatureData()

    is_duscae = _is_amdl_from_episode_duscae(amdl_file)
    is_root_bone_trans = True
    bone_names = []

    if is_duscae:
        relative_offsets = [128, 112, 96, 80, 64, 48, 32]
    else:
        relative_offsets = [112, 96, 80, 64, 48, 32, 16]

    amdl_file.seek(0, 0)  # Return to start of file
    struct.unpack("<L", amdl_file.read(4))[0]  # Skip over the file size
    # Skip the next 4 bytes (not sure what these are)
    struct.unpack("<L", amdl_file.read(4))[0]
    struct.unpack("<L", amdl_file.read(4))[0]  # Skip the block2 offset

    offset_flag = struct.unpack("<L", amdl_file.read(4))[0]
    offset = relative_offsets[offset_flag]

    if is_duscae:
        amdl_file.seek(840, 0)
    else:
        amdl_file.seek(296, 0)

    offset_to_end_of_names = struct.unpack(
        "<L", amdl_file.read(4))[0] + offset

    amdl_file.seek(156, 1)

    names_start = struct.unpack("<L", amdl_file.read(4))[
                      0] + relative_offsets[0]

    # Skip the next 4 bytes (not sure what these are)
    struct.unpack("<L", amdl_file.read(4))[0]

    bone_count = struct.unpack("<H", amdl_file.read(2))[0]
    amdl_file.seek(names_start, 0)

    for i in range(bone_count):
        bone_name = _read_string(amdl_file)
        if i == 0 and bone_name != "Trans":
            is_root_bone_trans = False

        sc = len(bone_name) + 1
        amdl_file.seek(48 - sc, 1)

    trans_header = _read_armature_header(amdl_file, is_duscae)

    if not is_duscae:
        amdl_file.seek(trans_header["count_0"] * 2, 1)  # block 1  uint16
        amdl_file.seek(trans_header["count_0"] * 2, 1)  # block 2  uint16

    if is_duscae:
        parent_count = trans_header["parentID_count"]
    else:
        parent_count = trans_header["count_1"]

    parent_IDs_start = amdl_file.tell()
    for p in range(parent_count):  # block 3  uint16
        id = struct.unpack("<H", amdl_file.read(2))[0]
        if id == 65535:
            continue  # excluded in parentID_count

    if is_duscae:
        # Episode Duscae only has blocks 3-6
        skip = ((parent_count * 2) * 2) + (parent_count * 4)
        amdl_file.seek(skip, 1)
        amdl_file.seek(_align(amdl_file.tell(), 16), 0)
    else:
        amdl_file.seek(trans_header["count_1"] * 2, 1)  # block 4  uint16
        amdl_file.seek(trans_header["count_1"] * 2, 1)  # block 5  uint16

        # block 6  uint32
        amdl_file.seek(trans_header["count_1"] * 4, 1)
        amdl_file.seek(_align(amdl_file.tell(), 16), 0)

        # block 7  4x fp32
        amdl_file.seek(trans_header["count_1"] * 16, 1)
        amdl_file.seek(_align(amdl_file.tell(), 16), 0)

        # block 8  4x fp32
        amdl_file.seek(trans_header["count_1"] * 16, 1)
        amdl_file.seek(_align(amdl_file.tell(), 16), 0)

    transforms_offset = amdl_file.tell()
    amdl_file.seek(names_start, 0)

    if is_duscae:
        name_count = parent_count
    else:
        name_count = bone_count

    for i in range(name_count):
        bone_name = _read_string(amdl_file)
        bone_names.append(bone_name)
        sc = len(bone_name) + 1
        amdl_file.seek(48 - sc, 1)

    parent_IDs = []
    amdl_file.seek(parent_IDs_start, 0)
    for p in range(parent_count):
        id = struct.unpack("<H", amdl_file.read(2))[0]
        if id == 65535:
            continue
        if not is_root_bone_trans:
            if id < (parent_count - 1) and p != 0:
                id += 1
        parent_IDs.append(id)

    armature_data.parent_IDs = parent_IDs
    amdl_file.seek(transforms_offset, 0)

    for i in range(parent_count):
        temporary_matrix = numpy.fromfile(
            amdl_file, dtype='<f', count=16).reshape((4, 4))

        if i == 0 and not is_root_bone_trans:
            amdl_file.seek(-64, 1)

        temporary_matrix[:, [1, 2]] = temporary_matrix[:, [2, 1]]
        temporary_matrix[1:3] = numpy.flipud(temporary_matrix[1:3])
        temporary_matrix = temporary_matrix.transpose()

        for row in range(3):
            temporary_matrix[row][1] *= -1

        bone_data = BoneData()
        bone_data.id = i
        bone_data.name = bone_names[i]
        bone_data.transformation_matrix = Matrix(temporary_matrix)
        armature_data.bones.append(bone_data)

    return armature_data


def _read_armature_header(amdl_file, is_duscae):
    ab = {}
    ex_st = amdl_file.tell()
    external_files_header_size = struct.unpack("<L", amdl_file.read(4))[0]
    amdl_file.seek(ex_st + external_files_header_size, 0)

    if is_duscae:
        ab["parentID_count"] = struct.unpack("<H", amdl_file.read(2))[0]
    else:
        ab["count_0"] = struct.unpack("<L", amdl_file.read(4))[0]
        ab["count_1"] = struct.unpack("<L", amdl_file.read(4))[0]
        ab["xfrm_count"] = struct.unpack("<H", amdl_file.read(2))[0]
        ab["parentID_count"] = struct.unpack("<H", amdl_file.read(2))[0]
        if ab["parentID_count"] == 0:
            ab["parentID_count"] = ab["xfrm_count"]
        unk_count = struct.unpack("<L", amdl_file.read(4))[0]
        ab["start_offset"] = amdl_file.tell()
    return ab


def _read_string(file, number_of_bytes=200):
    ascii_byte = file.read(1)
    string_value = ascii_byte
    byte_count = 0
    while ascii_byte and ord(ascii_byte) != 0 and byte_count < number_of_bytes:
        ascii_byte = file.read(1)
        string_value += ascii_byte
        byte_count += 1
    return string_value[:-1].decode('ascii', errors='ignore')


def _align(ptr, alignment):
    alignment -= 1
    return (ptr + alignment) & ~alignment


def _is_amdl_from_episode_duscae(amdl_file):
    is_episode_duscae = True
    amdl_file.seek(160, 0)
    for x in range(112):
        if struct.unpack("<L", amdl_file.read(4))[0] != 0:
            is_episode_duscae = False
            break

    return is_episode_duscae
