using System.Collections.Generic;
using Flagrum.Core.Gfxbin.Data;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;

namespace Flagrum.Core.Gfxbin.Gmdl.Components;

public class Model
{
    public GfxbinHeader Header { get; set; } = new();

    public string Name { get; set; }
    public ulong AssetHash { get; set; }

    public Aabb Aabb { get; set; }

    public byte InstanceNameFormat { get; set; }
    public byte ShaderClassFormat { get; set; }
    public byte ShaderSamplerDescriptionFormat { get; set; }
    public byte ShaderParameterListFormat { get; set; }
    public byte ChildClassFormat { get; set; }

    public List<BoneHeader> BoneHeaders { get; set; } = new();
    public List<NodeInformation> NodeTable { get; set; } = new();
    public List<MeshObject> MeshObjects { get; set; } = new();
    public IEnumerable<ModelPart> Parts { get; set; }

    public bool HasPsdPath { get; set; }
    public ulong PsdPathHash { get; set; }


    // NOTE: Class has unknowns
    public bool Unknown1 { get; set; }
}