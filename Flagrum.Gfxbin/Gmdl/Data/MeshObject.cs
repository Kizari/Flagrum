using System.Collections.Generic;

namespace Flagrum.Gfxbin.Gmdl.Data
{
    public class MeshObject
    {
        public string Name { get; set; }

        public List<Mesh> Meshes { get; } = new();


        // NOTE: Currently unused, unsure what its purpose is. Doesn't appear relevant to ClusterName or any data
        public int ClusterCount { get; set; }

        // NOTE: Currently unused, unsure what its purpose is. Doesn't appear relevant to ClusterCount or any data
        public string ClusterName { get; set; }
    }
}