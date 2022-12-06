using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Gfxbin.Gmtl.Data;

namespace Flagrum.Blender;

public class Importer
{
    private readonly Model _model;
    private readonly string _rootDirectory;
    private readonly string _rootUri;
    private Dictionary<int, string>? _boneTable;

    public Importer(string inputPath)
    {
        // Load the model from disk
        var gfxbin = File.ReadAllBytes(inputPath);
        var gpubin = File.ReadAllBytes(inputPath.Replace(".gmdl.gfxbin", ".gpubin"));
        var reader = new ModelReader(gfxbin, gpubin);
        _model = reader.Read();

        // Calculate paths
        var gpubinUri = _model.Header.Dependencies.First(d => d.Path.EndsWith(".gpubin")).Path;
        _rootUri = gpubinUri[..gpubinUri.LastIndexOf('/')];
        _rootDirectory = string.Join('\\', inputPath.Split('\\')[..^1]);

        LoadBoneTable();
    }

    public Gpubin GetData()
    {
        return new Gpubin
        {
            BoneTable = _boneTable,
            Parts = _model.Parts.ToDictionary(p => p.Id, p => p.Name),
            Meshes = _model.MeshObjects.SelectMany(o => o.Meshes
                .Where(m => m.LodNear == 0)
                .Select(MeshToGpubinMesh))
        };
    }

    private string? UriToRelativePath(string uri)
    {
        // Get path tokens
        var target = GetMatchingUriStart(uri, _rootUri);
        var rootUriTokens = _rootUri.Replace("data://", "data/").Split('/');
        var rootPathTokens = _rootDirectory.Split('\\');
        var targetTokens = target.Replace("data://", "data/").Split('/');
        var targetToken = targetTokens.Last();

        // Find index of the first folder the URIs have in common
        var index = -1;
        var counter = 0;
        for (var i = rootUriTokens.Length - 1; i >= 0; i--)
        {
            if (rootUriTokens[i].Equals(targetToken, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                break;
            }

            counter++;
        }

        if (index == -1)
        {
            return null;
        }

        // Calculate the final physical path
        var root = string.Join('\\', rootPathTokens[..^counter]);
        var remainingPath = uri.Replace(target, "").Replace("://", "/");
        var path = $@"{root}{remainingPath.Replace('/', '\\')}";

        // Calculate the true extension
        path = path.Replace(".gmtl", ".gmtl.gfxbin");

        return path;
    }

    private string GetMatchingUriStart(string first, string second)
    {
        var tokens1 = first.Replace("data://", "data/").Split('/');
        var tokens2 = second.Replace("data://", "data/").Split('/');
        var tokens = new List<string>();

        var length = Math.Min(tokens1.Length, tokens2.Length);
        for (var i = 0; i < length; i++)
        {
            if (tokens1[i].Equals(tokens2[i], StringComparison.OrdinalIgnoreCase))
            {
                tokens.Add(tokens1[i]);
            }
            else
            {
                break;
            }
        }

        return string.Join('/', tokens).Replace("data/", "data://");
    }

    private GpubinMesh MeshToGpubinMesh(Mesh mesh)
    {
        var materialPath = UriToRelativePath(_model.Header.Dependencies
            .First(d => d.PathHash == mesh.DefaultMaterialHash.ToString()).Path);
        var material = ReadMaterialData(mesh.DefaultMaterialHash, materialPath);

        var gpubinMesh = new GpubinMesh
        {
            Name = mesh.Name,
            FaceIndices = mesh.FaceIndices,
            VertexPositions = mesh.VertexPositions,
            ColorMaps = mesh.ColorMaps,
            Normals = mesh.Normals,
            UVMaps = mesh.UVMaps.Select(m => new UVMap32
            {
                UVs = m.UVs.Select(uv => new UV32
                {
                    U = (float)uv.U,
                    V = (float)uv.V
                }).ToList()
            }).ToList(),
            WeightIndices = mesh.WeightIndices,
            WeightValues = mesh.WeightValues
                .Select(n => n.Select(o => o.Select(p => (int)p).ToArray()).ToList())
                .ToList(),
            MeshParts = mesh.MeshParts.ToList(),
            BlenderMaterial = material
        };

        return gpubinMesh;
    }

    private BlenderMaterialData? ReadMaterialData(ulong materialHash, string? materialPath)
    {
        if (materialPath == null || !File.Exists(materialPath))
        {
            Console.WriteLine($"[WARNING] Could not find material file {materialPath}");
            return null;
        }

        var material = new MaterialReader(materialPath).Read();

        return new BlenderMaterialData
        {
            Hash = materialHash.ToString(),
            Name = material.Name,
            DetailUVScale = material.InterfaceInputs
                                .FirstOrDefault(i => i.ShaderGenName == "Detail_UVScale")
                                ?.Values
                            ?? material.InterfaceInputs
                                .FirstOrDefault(i => i.ShaderGenName == "Normal1_UVScale")
                                ?.Values,
            Textures = material.Textures
                .Where(t => t.ResourceFileHash > 0 && t.Path != null && t.Path.Contains('.'))
                .Select(MaterialTextureToBlenderTextureData)
                .Where(t => t != null)
        };
    }

    private BlenderTextureData? MaterialTextureToBlenderTextureData(MaterialTexture texture)
    {
        var fileName = texture.Path.Split('/').Last();
        var fileNameWithoutExtension = fileName[..fileName.LastIndexOf('.')];

        var texturePath = ResolveTexturePath(texture.Path);
        if (texturePath == null)
        {
            Console.WriteLine($"[WARNING] Could not find texture for {texture.Path}");
            return null;
        }
        
        return new BlenderTextureData
        {
            Hash = texture.ResourceFileHash.ToString(),
            Name = fileNameWithoutExtension,
            Path = texturePath,
            Slot = texture.ShaderGenName
        };
    }

    private string? ResolveTexturePath(string uri)
    {
        var extensions = new[] {"dds", "tga", "png"};
        var high = uri.Insert(uri.LastIndexOf('.'), "_$h");
        var highest = high.Replace("/sourceimages/", "/highimages/");

        var highestPath = UriToRelativePath(highest);
        if (highestPath != null)
        {
            foreach (var extension in extensions)
            {
                var withoutExtension = highestPath[..highestPath.LastIndexOf('.')];
                var withExtension = $"{withoutExtension}.{extension}";

                if (File.Exists(withExtension))
                {
                    return withExtension;
                }
            }
        }

        var highPath = UriToRelativePath(high);
        if (highPath != null)
        {
            foreach (var extension in extensions)
            {
                var withoutExtension = highPath[..highPath.LastIndexOf('.')];
                var withExtension = $"{withoutExtension}.{extension}";

                if (File.Exists(withExtension))
                {
                    return withExtension;
                }
            }
        }

        var lowPath = UriToRelativePath(uri);
        if (lowPath != null)
        {
            foreach (var extension in extensions)
            {
                var withoutExtension = lowPath[..lowPath.LastIndexOf('.')];
                var withExtension = $"{withoutExtension}.{extension}";

                if (File.Exists(withExtension))
                {
                    return withExtension;
                }
            }
        }

        return null;
    }
    
    private void LoadBoneTable()
    {
        if (_model.BoneHeaders.Count(b => b.UniqueIndex == ushort.MaxValue) > 1)
        {
            // Probably a broken MO gfxbin with all IDs set to this value
            var arbitraryIndex = 0;
            _boneTable = _model.BoneHeaders.ToDictionary(b => arbitraryIndex++, b => b.Name);
        }
        else
        {
            _boneTable = _model.BoneHeaders
                .ToDictionary(b => (int)(b.UniqueIndex == 65535 ? 0 : b.UniqueIndex),
                    b => b.Name);
        }
    }
}