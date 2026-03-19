using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Application.Features.AssetExplorer.Base;
using Flagrum.Application.Features.AssetExplorer.Data;
using Flagrum.Application.Services;
using Flagrum.Components.Controls;
using Flagrum.Components.Modals;
using Flagrum.Core.Graphics.Materials;
using Flagrum.Core.Graphics.Models;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Graphics.Textures.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Flagrum.Application.Features.AssetExplorer.Previews;

public partial class ModelPreview
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IAssetExplorerNode? _previousItem;
    private string? _rootDirectory;
    private string? _rootUri;

    [Inject] private AppStateService AppState { get; set; }
    [Inject] private IStringLocalizer<App> AppLocalizer { get; set; }
    [Inject] private IConfiguration Configuration { get; set; }
    [Inject] private IAlertService Alert { get; set; }
    [Inject] private IProfileService Profile { get; set; }

    [CascadingParameter] public IAssetExplorerParent Parent { get; set; }

    [Parameter] public IAssetExplorerNode Item { get; set; }

    private ModelPreviewSettingsModal? ModelPreviewSettingsModal { get; set; }
    private Viewport3D? Viewport { get; set; }
    private bool IsLoading { get; set; }
    private int LodLevel { get; set; }
    private int LodLevels { get; set; } = 1;

    protected override async Task OnParametersSetAsync()
    {
        if (Viewport != null && Item.Path != _previousItem?.Path)
        {
            LodLevel = 0;
            await LoadModelAsync();
        }
    }

    public void CallStateHasChanged() => StateHasChanged();
    private void OpenSettingsModal() => ModelPreviewSettingsModal?.Open();
    private Task PreviousLod() => ChangeLod(LodLevel - 1);
    private Task NextLod() => ChangeLod(LodLevel + 1);

    private Viewport3D.Control GetAction(StateKey key)
    {
        if (!Configuration.ContainsKey(key))
        {
            return key switch
            {
                StateKey.ViewportLeftClickAction => Viewport3D.Control.Pan,
                StateKey.ViewportMiddleClickAction => Viewport3D.Control.Rotate,
                StateKey.ViewportRightClickAction => Viewport3D.Control.Dolly,
                _ => throw new NotSupportedException($"'{key}' is not a viewport action")
            };
        }

        return Configuration.Get<Viewport3D.Control>(key);
    }

    private string GetKeyBindingString(Viewport3D.Control control)
    {
        if (GetAction(StateKey.ViewportLeftClickAction) == control)
        {
            return "Left Click";
        }
        
        if (GetAction(StateKey.ViewportMiddleClickAction) == control)
        {
            return "Middle Click";
        }
        
        if (GetAction(StateKey.ViewportRightClickAction) == control)
        {
            return "Right Click";
        }

        return "None";
    }

    private async Task ChangeLod(int lodLevel)
    {
        LodLevel = lodLevel;
        // PERF: Don't reload GameModel each time, also reuse materials/textures
        await LoadModelAsync();
    }

    private async Task LoadModelAsync()
    {
        // Show loading indicator
        IsLoading = true;
        StateHasChanged();

        // Remove existing meshes from scene if any
        await Viewport!.ClearMeshesAsync();

        // Load the model
        if (!TryLoadModel(out var model))
        {
            Alert.Open("Warning", "Missing Model Data",
                "This model doesn't appear to have a corresponding .gpubin file, so there is nothing to show.");
            IsLoading = false;
            StateHasChanged();
            return;
        }

        // Add the meshes to the scene
        await Parallel.ForEachAsync(model.LodMeshes[LodLevel], async (mesh, cancellation) =>
        {
            var positions = (IList<float[]>)mesh.Semantics[VertexElementSemantic.Position0];
            var normals = (IList<float[]>)mesh.Semantics[VertexElementSemantic.Normal0];
            var uvs = (IList<float[]>)mesh.Semantics[VertexElementSemantic.TexCoord0];

            // Reverse winding order of faces and flatten
            var indices = new uint[mesh.FaceIndexCount];
            for (var i = 0; i < indices.Length / 3; i++)
            {
                indices[i * 3 + 0] = mesh.FaceIndices[i, 2];
                indices[i * 3 + 1] = mesh.FaceIndices[i, 1];
                indices[i * 3 + 2] = mesh.FaceIndices[i, 0];
            }

            // Load textures
            TryLoadTextures(model, mesh, out var diffuse, out var normalMap);

            // Add mesh to scene
            await _lock.WaitAsync(cancellation);
            await Viewport.AddMeshAsync(
                positions.SelectMany(x => x).ToArray(),
                indices,
                normals.SelectMany(x => x.Take(3)).ToArray(),
                uvs.SelectMany(uv => new[] {uv[0], 1 - uv[1]}).ToArray(),
                diffuse,
                normalMap);
            _lock.Release();
        });

        // Adjust the camera
        var min = model.AxisAlignedBoundingBox.Start;
        var max = model.AxisAlignedBoundingBox.End;
        await Viewport.FrameModelAsync(min.X, min.Y, min.Z, max.X, max.Y, max.Z);

        // Hide loading indicator
        _previousItem = Item;
        IsLoading = false;
        StateHasChanged();
    }

    private bool TryLoadModel(out GameModel model)
    {
        // Read the model metadata
        model = new GameModel();
        model.Read(Item.Data);
        LodLevels = model.LodLevels;

        // Get the GPU binaries for the model
        var gpubinUris = model.Dependencies.Values
            .Where(d => d.EndsWith(".gpubin"))
            .ToArray();

        // Warn user and abort if unable to locate gpubin file
        if (gpubinUris.Length == 0)
        {
            return false;
        }

        // Compute paths
        var gpubinUri = gpubinUris[0];
        _rootUri = gpubinUri[..(gpubinUri.LastIndexOf('/') + 1)];
        _rootDirectory = Path.GetDirectoryName(Item.Path);

        // Read the GPU binaries
        model.ReadVertexData(gpubinUris
            .Select(uri => Item.Parent.Children
                .Single(c => c.Path.Split('\\', '/').Last()
                    .Equals(uri.Split('\\', '/').Last(), StringComparison.OrdinalIgnoreCase)))
            .OrderBy(f => f.Name)
            .Select(gpubin => gpubin.Data)
            .ToArray());

        return true;
    }

    private bool TryLoadTextures(
        GameModel model,
        GameModelMesh mesh,
        out byte[]? diffuse,
        out byte[]? normal)
    {
        diffuse = normal = null;

        // Get the texture fidelity settings
        var fidelityInt = Configuration.Get<int>(StateKey.ViewportTextureFidelity);
        var fidelity = fidelityInt == -1 ? ModelViewerTextureFidelity.Low : (ModelViewerTextureFidelity)fidelityInt;
        if (fidelity == ModelViewerTextureFidelity.None
            || !model.Dependencies.TryGetValue(mesh.MaterialHash.ToString(), out var materialUri)
            || !TryGetDataByUri(materialUri, fidelity, out var materialData))
        {
            return false;
        }

        // Get texture URIs from the material
        var material = new GameMaterial();
        material.Read(materialData);
        var diffuseUri = material.Textures
            .FirstOrDefault(t =>
                t.ShaderGenName.Contains("BaseColor0") || t.ShaderGenName.Contains("BaseColorTexture0"))
            ?.Uri;

        var normalUri = material.Textures
            .FirstOrDefault(t =>
                t.ShaderGenName.Contains("Normal0") || t.ShaderGenName.Contains("NormalTexture0"))
            ?.Uri;

        // Load textures into memory
        if (diffuseUri != null && TryGetDataByUri(diffuseUri, fidelity, out var diffuseBtex))
        {
            diffuse = new BlackTexture(diffuseBtex).Save(0, ImageFileFormat.Png);
        }

        if (normalUri != null && TryGetDataByUri(normalUri, fidelity, out var normalBtex))
        {
            normal = new BlackTexture(normalBtex).Save(0, ImageFileFormat.Png);
        }

        return true;
    }

    private bool TryGetDataByUri(string sourceUri,
        ModelViewerTextureFidelity desiredFidelity,
        [NotNullWhen(true)] out byte[]? sourceData)
    {
        sourceData = null;
        var high = sourceUri.Insert(sourceUri.LastIndexOf('.'), "_$h");
        var highest = high.Replace("/sourceimages/", "/highimages/");
        var map = new Dictionary<ModelViewerTextureFidelity, string>
        {
            [ModelViewerTextureFidelity.Highest] = highest,
            [ModelViewerTextureFidelity.High] = high,
            [ModelViewerTextureFidelity.Low] = sourceUri
        };

        // Handle "Game View" texture path resolution
        if (Parent.CurrentView == AssetExplorerView.GameView)
        {
            foreach (var (fidelity, uri) in map)
            {
                if (desiredFidelity >= fidelity)
                {
                    var data = AppState.GetFileByUri(uri);
                    if (data.Length > 0)
                    {
                        sourceData = data;
                        return true;
                    }
                }
            }

            return false;
        }

        // Handle "File System" view texture path resolution
        var baseUri = new Uri(_rootUri!);
        var extensions = new[] {"dds", "tga", "png", "btex"};

        if (sourceUri.EndsWith(".gmtl"))
        {
            var targetUri = new Uri(sourceUri);
            var relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(targetUri).ToString());
            var path = Path.Combine(_rootDirectory!, relativePath) + ".gfxbin";
            if (File.Exists(path))
            {
                sourceData = File.ReadAllBytes(path);
                return true;
            }

            return false;
        }

        foreach (var (fidelity, uri) in map)
        {
            if (desiredFidelity >= fidelity)
            {
                var targetUri = new Uri(uri);
                var relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(targetUri).ToString());
                var path = Path.Combine(_rootDirectory!, relativePath);
                var withoutExtension = path[..path.LastIndexOf('.')];
                foreach (var finalPath in extensions
                             .Select(extension => $"{withoutExtension}.{extension}")
                             .Where(File.Exists))
                {
                    sourceData = File.ReadAllBytes(finalPath);
                    return true;
                }
            }
        }

        return false;
    }
}