import traceback
from array import array
from dataclasses import dataclass

import bpy
import numpy as np
from bpy.types import Collection, Object
from mathutils import Matrix, Vector

from .gmtlimporter import GmtlImporter
from .import_context import ImportContext
from .interop import Interop
from ..graphics.gmdl.gmdl import Gmdl
from ..graphics.gmdl.gmdlmesh import GmdlMesh
from ..graphics.msgpack_reader import MessagePackReader


loaded_materials = {}


@dataclass
class GmdlImporter:
    context: ImportContext
    game_model: Gmdl
    correction_matrix: Matrix
    bone_table: dict[int, str]
    lods: dict[float, Collection]
    has_vems: bool
    vems: Collection
    buffer: bytes
    buffer_offset: int

    def __init__(self, context):
        self.context = context
        self.correction_matrix = Matrix([
            [1, 0, 0],
            [0, 0, -1],
            [0, 1, 0]
        ])

    def import_gfxbin(self):
        with open(self.context.gfxbin_path, mode="rb") as file:
            reader = MessagePackReader(file.read())
        self.game_model = Gmdl(reader)
        self.context.set_base_directory(self.game_model.header)

    def generate_bone_table(self):
        self.bone_table = {}
        counter = 0
        if self.game_model.header.version >= 20220707:
            for bone in self.game_model.bones:
                self.bone_table[counter] = bone.name
                counter += 1
        else:
            max_count = 0
            for bone in self.game_model.bones:
                if bone.unique_index == 65535:
                    max_count += 1
                if max_count > 1:
                    break
            if max_count > 1:
                for bone in self.game_model.bones:
                    self.bone_table[counter] = bone.name
                    counter += 1
            else:
                for bone in self.game_model.bones:
                    if bone.unique_index == 65535:
                        self.bone_table[0] = bone.name
                    else:
                        self.bone_table[bone.unique_index] = bone.name

    def import_meshes(self, use_correction_matrix=True) -> list[Object]:
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

        # Get the gpubin buffer
        self.buffer_offset = 0
        self.buffer = Interop.import_gpubin(self.context.gfxbin_path, self.context.import_lods,
                                            self.context.import_vems)

        meshes = []
        for mesh_object in self.game_model.mesh_objects:
            for mesh in mesh_object.meshes:
                # Skip LODs if setting is not checked
                if not self.context.import_lods and mesh.lod_near != 0:
                    continue
                # Skip VEMs if setting is not checked
                if not self.context.import_vems and (mesh.flags & 67108864) > 0:
                    continue
                meshes.append(self._import_mesh(mesh, use_correction_matrix))

        layer = bpy.context.view_layer
        layer.update()

        return meshes

    def _import_mesh(self, mesh_data: GmdlMesh, use_correction_matrix) -> Object:
        context = self.context
        game_model = self.game_model
        buffer = self.buffer

        # Get face indices
        face_indices = np.frombuffer(buffer, dtype="<I", offset=self.buffer_offset, count=mesh_data.face_index_count) \
            .reshape((int(mesh_data.face_index_count / 3), 3))
        self.buffer_offset += mesh_data.face_index_count * 4

        # Reverse the winding order of the faces so the normals face the right direction
        faces = []
        for face in face_indices:
            faces.append([face[2], face[1], face[0]])

        # Get the vertex semantics
        semantics = {}
        for stream in mesh_data.vertex_streams:
            for element in stream.elements:
                semantics[element.semantic] = []

        # Populate the semantics with data from the buffer
        for semantic in sorted(semantics.keys()):
            if semantic == "POSITION0":
                vertices = []
                elements = np.frombuffer(buffer, dtype="<f", offset=self.buffer_offset,
                                         count=mesh_data.vertex_count * 3) \
                    .reshape((mesh_data.vertex_count, 3))
                self.buffer_offset += mesh_data.vertex_count * 3 * 4
                for i in range(mesh_data.vertex_count):
                    if use_correction_matrix:
                        vertices.append(self.correction_matrix @ Vector(elements[i]))
                    else:
                        vertices.append(Vector(elements[i]))
                semantics[semantic] = vertices
            elif semantic == "NORMAL0":
                elements = np.frombuffer(buffer, dtype="<f", offset=self.buffer_offset,
                                         count=mesh_data.vertex_count * 3) \
                    .reshape((mesh_data.vertex_count, 3))
                self.buffer_offset += mesh_data.vertex_count * 3 * 4
                semantics[semantic] = elements
            elif semantic.startswith("TEXCOORD"):
                elements = np.frombuffer(buffer, dtype="<f", offset=self.buffer_offset,
                                         count=mesh_data.vertex_count * 2) \
                    .reshape((mesh_data.vertex_count, 2))
                self.buffer_offset += mesh_data.vertex_count * 2 * 4
                semantics[semantic] = elements
            elif semantic.startswith("COLOR"):
                elements = np.frombuffer(buffer, dtype="<f", offset=self.buffer_offset,
                                         count=mesh_data.vertex_count * 4) \
                    .reshape((mesh_data.vertex_count, 4))
                self.buffer_offset += mesh_data.vertex_count * 4 * 4
                semantics[semantic] = elements
            elif semantic.startswith("BLENDWEIGHT"):
                elements = np.frombuffer(buffer, dtype="<f", offset=self.buffer_offset,
                                         count=mesh_data.vertex_count * 4) \
                    .reshape((mesh_data.vertex_count, 4))
                self.buffer_offset += mesh_data.vertex_count * 4 * 4
                semantics[semantic] = elements
            elif semantic.startswith("BLENDINDICES"):
                elements = np.frombuffer(buffer, dtype="<I", offset=self.buffer_offset,
                                         count=mesh_data.vertex_count * 4) \
                    .reshape((mesh_data.vertex_count, 4))
                self.buffer_offset += mesh_data.vertex_count * 4 * 4
                semantics[semantic] = elements

        # Create the mesh
        mesh = bpy.data.meshes.new(mesh_data.name)
        mesh.from_pydata(semantics["POSITION0"], [], faces)

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

        mesh.validate()
        mesh.update()

        mesh_object = bpy.data.objects.new(mesh_data.name, mesh)

        if (mesh_data.flags & 67108864) > 0:
            self.vems.objects.link(mesh_object)
        elif context.import_lods:
            self.lods[mesh_data.lod_near].objects.link(mesh_object)
        else:
            context.collection.objects.link(mesh_object)

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

        # Import custom normals
        mesh.update(calc_edges=True)

        clnors = array('f', [0.0] * (len(mesh.loops) * 3))
        mesh.loops.foreach_get("normal", clnors)
        mesh.polygons.foreach_set("use_smooth", [True] * len(mesh.polygons))

        normals = []
        for normal in semantics["NORMAL0"]:
            if use_correction_matrix:
                result = self.correction_matrix @ Vector([normal[0], normal[1], normal[2]])
            else:
                result = Vector([normal[0], normal[1], normal[2]])
            result.normalize()
            normals.append(result)

        mesh.normals_split_custom_set_from_vertices(normals)
        mesh.use_auto_smooth = True

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

        # Link the mesh to the armature
        if len(self.bone_table) > 0:
            mod = mesh_object.modifiers.new(
                type="ARMATURE", name=context.collection.name)
            mod.use_vertex_groups = True

            armature = bpy.data.objects[context.collection.name]
            mod.object = armature

            mesh_object.parent = armature

        # Process the material
        material_uri = self._get_material_uri(mesh_data.material_hash)

        try:
            if material_uri in loaded_materials:
                material = loaded_materials[material_uri]
            else:
                material_importer = GmtlImporter(self.context, material_uri)
                material = material_importer.generate_material(has_light_map)
                if material is not None:
                    loaded_materials[material_uri] = material

            if material is not None:
                mesh_object.data.materials.append(material)
        except Exception:
            print(f"[ERROR] Failed to import GMTL data from {material_uri}")
            traceback.print_exc()

        return mesh_object

    def _get_material_uri(self, uri_hash: int):
        return self.game_model.header.dependencies[str(uri_hash)]
