using System.Collections.Generic;

namespace Flagrum.Core.Gfxbin.Gmdl.Components;

public class MeshObject
{
    public string Name { get; set; }

    public List<Mesh> Meshes { get; set; } = new();


    // NOTE: Currently unused, unsure what its purpose is. Doesn't appear relevant to ClusterName or any data
    // Seems to always be set to 1
    public int ClusterCount { get; set; }

    // NOTE: Currently unused, unsure what its purpose is. Doesn't appear relevant to ClusterCount or any data
    // Seems to always be set to CLUSTER_NAME
    public string ClusterName { get; set; }
}