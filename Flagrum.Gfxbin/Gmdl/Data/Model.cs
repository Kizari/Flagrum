using System.Collections.Generic;
using Flagrum.Gfxbin.Data;

namespace Flagrum.Gfxbin.Gmdl.Data
{
    public class Model
    {
        public GfxbinHeader Header { get; } = new();

        public string Name { get; set; }
        public ulong AssetHash { get; set; }

        public (Vector3 Min, Vector3 Max) Aabb { get; set; }

        public byte InstanceNameFormat { get; set; }
        public byte ShaderClassFormat { get; set; }
        public byte ShaderSamplerDescriptionFormat { get; set; }
        public byte ShaderParameterListFormat { get; set; }
        public byte ChildClassFormat { get; set; }

        public List<BoneHeader> BoneHeaders { get; } = new();
        public List<NodeInformation> NodeTable { get; } = new();
        public List<MeshObject> MeshObjects { get; } = new();
        public IEnumerable<ModelPart> Parts { get; set; }

        public bool HasPsdPath { get; set; }
        public ulong PsdPathHash { get; set; }


        // NOTE: Class has unknowns
        public bool Unknown1 { get; set; }
    }
}