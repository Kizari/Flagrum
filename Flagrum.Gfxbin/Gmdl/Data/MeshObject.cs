﻿using System.Collections.Generic;

namespace Flagrum.Gfxbin.Gmdl.Data
{
    public class MeshObject
    {
        public string Name { get; set; }

        public List<Mesh> Meshes { get; } = new();


        // NOTE: Currently unused, unsure what its purpose is. Doesn't appear relevant to ClusterName or any data
        // Seems to always be set to 1
        public int ClusterCount { get; set; }

        // NOTE: Currently unused, unsure what its purpose is. Doesn't appear relevant to ClusterCount or any data
        // Seems to always be set to CLUSTER_NAME
        public string ClusterName { get; set; }
    }
}