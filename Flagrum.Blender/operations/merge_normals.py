from bpy.types import Mesh
from mathutils import Vector


def merge_normals(mesh: Mesh, distance: float):
    new_normals = {}

    # Split vertices up into a grid by the set merge distance
    grid = {}
    for vertex in mesh.vertices:
        x = vertex.co[0] // distance
        y = vertex.co[1] // distance
        z = vertex.co[2] // distance

        if x not in grid:
            grid[x] = {}
        if y not in grid[x]:
            grid[x][y] = {}
        if z not in grid[x][y]:
            grid[x][y][z] = []

        grid[x][y][z].append(vertex)

    # Merge custom normals for vertices in range
    vertices = set(vertex for vertex in mesh.vertices)
    while vertices:
        vertex = vertices.pop()
        vertex_normal = Vector()
        x = vertex.co[0] // distance
        y = vertex.co[1] // distance
        z = vertex.co[2] // distance

        # Find list of all vertices in adjacent parts of the grid
        adjacent = []
        for i in [x - 1, x, x + 1]:
            for j in [y - 1, y, y + 1]:
                for k in [z - 1, z, z + 1]:
                    if i in grid and j in grid[i] and k in grid[i][j]:
                        adjacent.extend(grid[i][j][k])

        # Remove vertices that are out of the merge distance
        in_range = [v for v in adjacent if (v.co - vertex.co).length_squared <= distance ** 2]

        # Average all the normals of the vertices in range
        for other in in_range:
            vertex_normal += other.normal

        vertex_normal.normalize()

        # Add new normals for merged vertices
        for other in in_range:
            new_normals[other.index] = vertex_normal

        # Remove merged vertices from the set
        processed = [v for v in in_range]
        vertices.difference_update(processed)

    # Apply new normals to the mesh
    normals = []
    for i in range(len(mesh.vertices)):
        if i in new_normals:
            normals.append([new_normals[i].x, new_normals[i].y, new_normals[i].z])
        else:
            normals.append(mesh.vertices[i].normal)

    mesh.normals_split_custom_set_from_vertices(normals)
    mesh.use_auto_smooth = True
