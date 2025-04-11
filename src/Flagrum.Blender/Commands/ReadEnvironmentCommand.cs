using System.Globalization;
using System.Numerics;
using Flagrum.Blender.Commands.Data;
using Flagrum.Core.Mathematics;
using Flagrum.Core.Scripting.Ebex;
using Flagrum.Core.Scripting.Ebex.Configuration;
using Flagrum.Core.Scripting.Ebex.Data;
using Newtonsoft.Json;
using Quaternion = System.Numerics.Quaternion;

namespace Flagrum.Blender.Commands;

public class ReadEnvironmentCommand : IConsoleCommand
{
    private readonly List<string> _staticModelTypes = new()
    {
        "Black.Entity.Actor.TaggedStaticModelEntity",
        "Black.Entity.Shape.OceanFlowMapEntity",
        "Black.Entity.Shape.OceanPatchEntity",
        "Black.Entity.HairModelEntity",
        "Black.Entity.OceanFloatingModelEntity",
        "Black.Entity.SkeletalModelEntity",
        "Black.Entity.StaticModelEntity"
    };

    public string Command => "environment";

    public void Execute(string[] args)
    {
        XmlUtility.Dummy();

        var dataDirectory = args[0];
        var inputPath = args[1];

        // Need to set to invariant culture as some cultures don't handle the
        // exponent portion when parsing floats
        var previousCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        Project.DataRoot = dataDirectory.Replace('\\', '/').ToLower();
        using var reader = new EbexReader(null);
        var ebex = reader.Read(inputPath);
        var root = new EnvironmentGroup {Name = ebex.Name};

        TraverseScriptHierarchy(
            root,
            ebex["entities_"].Children,
            null,
            null,
            1f,
            new List<float[]>(),
            ebex is Prefab);

        root.RemoveEmptyGroups();

        Console.Out.Write(JsonConvert.SerializeObject(root));

        Thread.CurrentThread.CurrentCulture = previousCulture;
    }

    private void TraverseScriptHierarchy(EnvironmentGroup group, List<DataItem> children,
        float[]? prefabPosition, float[]? prefabRotation, float prefabScale,
        List<float[]> prefabRotations, bool isPrefab)
    {
        // Set root translation if this is the first recursion
        if (prefabPosition == null)
        {
            if (isPrefab)
            {
                // Don't want to use the root transform if the root ebex is a prefab file
                // See https://github.com/Kizari/Flagrum/issues/51
                prefabPosition = new[] {0.0f, 0.0f, 0.0f, 0.0f};
                prefabRotation = new[] {0.0f, 0.0f, 0.0f, 0.0f};
                prefabScale = 1.0f;
            }
            else
            {
                var element = children.First();
                prefabPosition = element.Position;
                prefabRotation = element.Rotation;
                prefabScale = element.Scaling;
            }
        }

        // Change rotation into quaternion
        var quaternion = Quaternion.CreateFromYawPitchRoll(
            prefabRotation![1].ToRadians(),
            prefabRotation[0].ToRadians(),
            prefabRotation[2].ToRadians());

        foreach (var element in children)
        {
            var type = element.DataType.Name;

            if (element is EntityPackageReference or EntityGroup)
            {
                EnvironmentGroup nextGroup;

                if (element is EntityPackageReference reference)
                {
                    nextGroup = new EnvironmentGroup {Name = element.Name};
                    group.Children.Add(nextGroup);
                }
                else
                {
                    nextGroup = group;
                }

                if (element.GetBool("hasTransform_"))
                {
                    var position = Vector3.Add(
                        Vector3.Transform(new Vector3(element.Position), quaternion),
                        new Vector3(prefabPosition)
                    );

                    var i = 0;
                    var rotation = element.Rotation;
                    var rotationAltered = rotation.Select(r => r + prefabRotation[i++]).ToArray();
                    var scaling = element.Scaling * prefabScale;

                    var newPrefabRotations = new List<float[]>(prefabRotations);
                    if (!(rotation[0] is > -0.0001f and < 0.0001f
                          && rotation[1] is > -0.0001f and < 0.0001f
                          && rotation[2] is > -0.0001f and < 0.0001f))
                    {
                        newPrefabRotations.Add(rotation);
                    }

                    TraverseScriptHierarchy(
                        nextGroup,
                        element["entities_"].Children,
                        new[] {position.X, position.Y, position.Z},
                        rotationAltered,
                        scaling,
                        newPrefabRotations,
                        isPrefab);
                }
                else
                {
                    TraverseScriptHierarchy(
                        nextGroup,
                        element["entities_"].Children,
                        prefabPosition,
                        prefabRotation,
                        1f * prefabScale,
                        prefabRotations,
                        isPrefab);
                }
            }
            else if (_staticModelTypes.Contains(type))
            {
                var relativePath = element.GetString("sourcePath_");
                var scaling = element.Scaling * prefabScale;
                var position = Vector3.Add(
                    Vector3.Transform(new Vector3(element.Position), quaternion),
                    new Vector3(prefabPosition)
                );

                group.Children.Add(new EnvironmentProp
                {
                    Name = element.Name,
                    Uri = "data://" + relativePath.Replace('\\', '/').ToLower(),
                    RelativePath = relativePath + ".gfxbin",
                    Position = new[] {position.X, position.Y, position.Z},
                    Rotation = element.Rotation,
                    PrefabRotations = prefabRotations,
                    Scale = scaling
                });
            }
        }
    }
}