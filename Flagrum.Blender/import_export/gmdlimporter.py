from array import array
from dataclasses import dataclass

import bpy
import numpy as np
from bpy.types import Collection
from mathutils import Matrix, Vector

from .gfxbin.gmdl.gmdl import Gmdl
from .gfxbin.gmdl.gmdlmesh import GmdlMesh
from .gfxbin.gmdl.gmdlvertexelementformat import ElementFormat
from .gfxbin.msgpack_reader import MessagePackReader
from .gmtlimporter import GmtlImporter
from .import_context import ImportContext
from ..utilities.timer import Timer


@dataclass
class GmdlImporter:
    gpubins: dict[int, bytes]
    context: ImportContext
    game_model: Gmdl
    correction_matrix: Matrix
    bone_table: dict[int, str]
    timer: Timer
    format_strings: dict[ElementFormat, str]
    lods: dict[float, Collection]
    has_vems: bool
    vems: Collection

    def __init__(self, context):
        self.gpubins = {}
        self.context = context
        self.correction_matrix = Matrix([
            [1, 0, 0],
            [0, 0, -1],
            [0, 1, 0]
        ])

        self.format_strings = {
            ElementFormat.XYZ32_Float: "fff",
            ElementFormat.XY16_SintN: "hh",
            ElementFormat.XY16_UintN: "HH",
            ElementFormat.XY16_Float: "f2f2",
            ElementFormat.XYZW8_UintN: "BBBB",
            ElementFormat.XYZW8_SintN: "bbbb",
            ElementFormat.XYZW16_Uint: "HHHH",
            ElementFormat.XYZW32_Uint: "IIII"
        }

    def run(self):
        timer = Timer()
        self.timer = Timer()
        self._import_gfxbin()
        self.timer.print("Importing gfxbin")
        self.context.set_base_directory(self.game_model.header)
        self._generate_bone_table()
        self.timer.print("Generating bone table")
        self._import_meshes()
        timer.print("Overall import")

    def _import_gfxbin(self):
        with open(self.context.gfxbin_path, mode="rb") as file:
            reader = MessagePackReader(file.read())
        self.game_model = Gmdl(reader)

    def _generate_bone_table(self):
        self.bone_table = {}
        counter = 0
        if self.game_model.header.version >= 20220707:
            for bone in self.game_model.bones:
                self.bone_table[counter] = bone.name
                counter += 1
        else:
            for bone in self.game_model.bones:
                self.bone_table[bone.unique_index] = bone.name

    def _import_meshes(self):
        if self.context.import_lods:
            self.lods = {}
            for mesh_object in self.game_model.mesh_objects:
                for mesh in mesh_object.meshes:
                    self.lods[mesh.lod_near] = None

            counter = 0
            for key in self.lods:
                self.lods[key] = bpy.data.collections.new("LOD" + str(counter))
                self.context.collection.children.link(self.lods[key])
                counter += 1

        if self.context.import_vems:
            self.has_vems = False
            for mesh_object in self.game_model.mesh_objects:
                for mesh in mesh_object.meshes:
                    if (mesh.flags & 67108864) > 0:
                        self.has_vems = True

            if self.has_vems:
                self.vems = bpy.data.collections.new("VEMs")
                self.context.collection.children.link(self.vems)

        for mesh_object in self.game_model.mesh_objects:
            for mesh in mesh_object.meshes:
                self._import_mesh(mesh)

        layer = bpy.context.view_layer
        layer.update()

    def _import_mesh(self, mesh_data: GmdlMesh):
        context = self.context
        game_model = self.game_model

        # Skip LODs if setting is not checked
        if not context.import_lods and mesh_data.lod_near != 0:
            return

        # Skip VEMs if setting is not checked
        if not context.import_vems and (mesh_data.flags & 67108864) > 0:
            return

        print("")
        print(f"Importing {mesh_data.name}...")
        buffer = self._get_gpubin_buffer(mesh_data.gpubin_index)

        # Get face indices
        if mesh_data.face_index_type == 0:
            data_type = "<H"
        else:
            data_type = "<I"

        face_indices = np.frombuffer(buffer, dtype=data_type, offset=mesh_data.face_index_offset,
                                     count=mesh_data.face_index_count) \
            .reshape((int(mesh_data.face_index_count / 3), 3))

        self.timer.print("Reading face indices")

        # Reverse the winding order of the faces so the normals face the right direction
        faces = []
        for face in face_indices:
            faces.append([face[2], face[1], face[0]])

        self.timer.print("Unwinding triangles")

        # Get the vertex semantics
        semantics = {}
        for stream in mesh_data.vertex_streams:
            for element in stream.elements:
                semantics[element.semantic] = []

        # Read the vertex streams
        # for stream in mesh_data.vertex_streams:
        #     format_string = "<"
        # 
        #     elements = []
        #     current_index = 0
        #     for element in stream.elements:
        #         if element.format not in self.format_strings:
        #             print(f"")
        #             continue
        # 
        #         format_string += self.format_strings[element.format]
        #         element_data_type = ElementFormat.get_data_type(element)
        #         element_count = ElementFormat.get_count(element)
        #         element_items = np.zeros((mesh_data.vertex_count, element_count), dtype=element_data_type)
        #         elements.append((element_items, current_index, int(current_index + element_count)))
        #         current_index += element_count
        # 
        #     base_offset = mesh_data.vertex_buffer_offset + stream.offset
        #     elements_range = range(len(elements))
        #     for i in range(mesh_data.vertex_count):
        #         stream_offset = base_offset + (stream.stride * i)
        #         for j in elements_range:
        #             vertex_array = np.array(struct.unpack_from(format_string, buffer, stream_offset)).transpose()
        #             elements[j][0][i] = np.column_stack(vertex_array[elements[j][1]:elements[j][2]])
        # 
        #     # current_index = 0
        #     # for element in stream.elements:
        #     #     element_count = ElementFormat.get_count(element)
        #     #     print(f"[{current_index}:{(current_index + element_count)}]")
        #     #     semantics[element.semantic] = np.column_stack(vertex_array[current_index:(current_index + element_count)])
        #     #     current_index += element_count
        # 
        # print(semantics["POSITION0"])
        # self.timer.print("Reading vertex streams")
        # return

        for stream in mesh_data.vertex_streams:
            for element in stream.elements:
                data_type = ElementFormat.get_data_type(element)
                element_count = ElementFormat.get_count(element)

                if data_type is None or element_count is None:
                    continue

                elements = np.zeros((mesh_data.vertex_count, element_count), dtype=data_type)
                base_offset = mesh_data.vertex_buffer_offset + stream.offset + element.offset
                for i in range(mesh_data.vertex_count):
                    element_offset = base_offset + stream.stride * i
                    if element.format == ElementFormat.XYZ32_Float:
                        elements[i] = self.correction_matrix @ Vector(
                            np.frombuffer(buffer, dtype="<f", offset=element_offset, count=3))
                    elif element.format == ElementFormat.XY16_SintN:
                        elements[i] = np.frombuffer(buffer, dtype="<h", offset=element_offset, count=2).astype(
                            np.float32) * (1 / 0x7FFF)
                    elif element.format == ElementFormat.XY16_UintN:
                        elements[i] = np.frombuffer(buffer, dtype="<H", offset=element_offset, count=2).astype(
                            np.float32) / 0xFFFF
                    elif element.format == ElementFormat.XY16_Float:
                        elements[i] = np.frombuffer(buffer, dtype=np.float16, offset=element_offset, count=2)
                    elif element.format == ElementFormat.XY32_Float:
                        elements[i] = np.frombuffer(buffer, dtype="<f", offset=element_offset, count=2)
                    elif element.format == ElementFormat.XYZW8_UintN:
                        elements[i] = np.frombuffer(buffer, dtype="<B", offset=element_offset, count=4).astype(
                            np.float32) / 0xFF
                    elif element.format == ElementFormat.XYZW8_Uint:
                        elements[i] = np.frombuffer(buffer, dtype="<B", offset=element_offset, count=4)
                    elif element.format == ElementFormat.XYZW8_SintN:
                        elements[i] = np.frombuffer(buffer, dtype="<b", offset=element_offset, count=4).astype(
                            np.float32) * (1 / 0x7F)
                    elif element.format == ElementFormat.XYZW8_Sint:
                        elements[i] = np.frombuffer(buffer, dtype="<b", offset=element_offset, count=4)
                    elif element.format == ElementFormat.XYZW16_Uint:
                        elements[i] = np.frombuffer(buffer, dtype="<H", offset=element_offset, count=4)
                    elif element.format == ElementFormat.XYZW32_Uint:
                        elements[i] = np.frombuffer(buffer, dtype="<I", offset=element_offset, count=4)
                semantics[element.semantic] = elements

        self.timer.print("Reading vertex streams")

        # Create the mesh
        mesh = bpy.data.meshes.new(mesh_data.name)
        mesh.from_pydata(semantics["POSITION0"], [], faces)

        self.timer.print("Creating mesh")

        # Generate each of the UV Maps
        has_light_map = False
        uv_map_index = 0
        for i in range(8):
            key = "TEXCOORD" + str(i)
            if key not in semantics:
                continue

            if i == 0:
                new_name = "map1"
            elif i == 1:
                new_name = "mapLM"
                has_light_map = True
            else:
                new_name = "map" + str(i + 1)
            mesh.uv_layers.new(name=new_name)

            uv_data = semantics[key]

            coords = np.zeros((mesh_data.vertex_count, 2), np.float32)
            for c in range(len(uv_data)):
                if game_model.header.version >= 20220707:
                    u = uv_data[c][0]
                    v = uv_data[c][1]
                    if v >= 0:
                        v_tile = int(v)
                        coords[c][0] = u + (v_tile % 10)
                        coords[c][1] = (v_tile + int(v_tile / 10) + 1) - v
                    else:
                        coords[c][0] = u
                        coords[c][1] = 1 - v
                else:
                    # The V coordinate is set as 1-V to flip from FBX coordinate system
                    coords[c][0] = uv_data[c][0]
                    coords[c][1] = 1 - uv_data[c][1]

            uv_dictionary = {i: uv for i, uv in enumerate(coords)}
            per_loop_list = [0.0] * len(mesh.loops)

            for loop in mesh.loops:
                per_loop_list[loop.index] = uv_dictionary.get(loop.vertex_index)

            per_loop_list = [uv for pair in per_loop_list for uv in pair]

            mesh.uv_layers[uv_map_index].data.foreach_set("uv", per_loop_list)
            uv_map_index += 1

        self.timer.print("Generating UV maps")

        # Generate each of the color maps
        for i in range(4):
            key = "COLOR" + str(i)
            if key not in semantics:
                continue

            colors = semantics[key]

            per_loop_list = [0.0] * len(mesh.loops)

            for loop in mesh.loops:
                if loop.vertex_index < len(colors):
                    per_loop_list[loop.index] = colors[loop.vertex_index]

            per_loop_list = [colors for pair in per_loop_list for colors in pair]
            new_name = "colorSet"
            if i > 0:
                new_name += str(i)
            mesh.vertex_colors.new(name=new_name)
            mesh.vertex_colors[i].data.foreach_set("color", per_loop_list)

        self.timer.print("Generating vertex colours")

        mesh.validate()
        mesh.update()

        self.timer.print("Validating and updating mesh")

        mesh_object = bpy.data.objects.new(mesh_data.name, mesh)

        if (mesh_data.flags & 67108864) > 0:
            self.vems.objects.link(mesh_object)
        elif context.import_lods:
            self.lods[mesh_data.lod_near].objects.link(mesh_object)
        else:
            context.collection.objects.link(mesh_object)

        self.timer.print("Linking mesh object")

        # Add the parts system
        model_parts = {}
        for model_part in game_model.parts:
            model_parts[model_part.id] = model_part.name

        for parts_group in mesh_data.parts:
            parts_layer = mesh.attributes.new(name=model_parts[parts_group.parts_id],
                                              type='BOOLEAN',
                                              domain='FACE')

            sequence = []
            start_index = int(parts_group.start_index / 3)
            index_count = int(parts_group.index_count / 3)
            end_index = start_index + index_count
            for i in range(len(mesh.polygons)):
                sequence.append(start_index <= i < end_index)

            parts_layer.data.foreach_set("value", sequence)

        self.timer.print("Generating parts data")

        # Import custom normals
        mesh.update(calc_edges=True)

        clnors = array('f', [0.0] * (len(mesh.loops) * 3))
        mesh.loops.foreach_get("normal", clnors)
        mesh.polygons.foreach_set("use_smooth", [True] * len(mesh.polygons))

        normals = []
        for normal in semantics["NORMAL0"]:
            result = self.correction_matrix @ Vector([normal[0], normal[1], normal[2]])
            result.normalize()
            normals.append(result)

        mesh.normals_split_custom_set_from_vertices(normals)
        mesh.use_auto_smooth = True

        self.timer.print("Generating custom normals")

        # Add the vertex weights from each weight map
        weights = {}
        for key in self.bone_table:
            weights[key] = []

        if len(self.bone_table) > 0:
            for i in range(2):
                if "BLENDWEIGHT" + str(i) not in semantics:
                    continue

                blend_weight = semantics["BLENDWEIGHT" + str(i)]
                blend_indices = semantics["BLENDINDICES" + str(i)]
                for j in range(len(blend_weight)):
                    for k in range(4):
                        # No need to import zero weights
                        if blend_weight[j][k] == 0:
                            continue
                        weights[blend_indices[j][k]].append(([j], blend_weight[j][k]))

            for key in weights:
                bone_name = self.bone_table[key]
                vertex_group = mesh_object.vertex_groups.new(name=bone_name)
                for item in weights[key]:
                    vertex_group.add(item[0], item[1], 'ADD')

        self.timer.print("Generating weight data")

        # Link the mesh to the armature
        if len(self.bone_table) > 0:
            mod = mesh_object.modifiers.new(
                type="ARMATURE", name=context.collection.name)
            mod.use_vertex_groups = True

            armature = bpy.data.objects[context.collection.name]
            mod.object = armature

            mesh_object.parent = armature
        else:
            # Collection wasn't linked on armature set, so do it now
            if context.collection.name not in bpy.context.scene.collection.children:
                bpy.context.scene.collection.children.link(context.collection)

        self.timer.print("Linking mesh to armature")

        # Process the material
        material_uri = self._get_material_uri(mesh_data.material_hash)

        try:
            material_importer = GmtlImporter(self.context, material_uri)
            material = material_importer.generate_material(has_light_map)

            if material is not None:
                mesh_object.data.materials.append(material)
        except:
            print(f"[ERROR] Failed to import GMTL data from {material_uri}")

        self.timer.print("Generating automatic materials")

    def _get_gpubin_buffer(self, index: int) -> bytes:
        if self.game_model.header.version < 20220707:
            file_path = self.context.path_without_extension + ".gpubin"
        else:
            file_path = self.context.path_without_extension + "_" + str(index) + ".gpubin"

        if index not in self.gpubins:
            with open(file_path, mode="rb") as file:
                self.gpubins[index] = file.read()

        return self.gpubins[index]

    def _get_material_uri(self, uri_hash: int):
        return self.game_model.header.dependencies[str(uri_hash)]
