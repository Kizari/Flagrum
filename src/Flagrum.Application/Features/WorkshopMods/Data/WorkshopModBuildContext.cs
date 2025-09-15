using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.WorkshopMods.Data.Model;
using Flagrum.Application.Utilities;
using Flagrum.Core.Graphics.Materials;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Graphics.Textures.Luminous.Builder;
using Flagrum.Core.Graphics.Textures.Luminous.DataSources;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace Flagrum.Application.Features.WorkshopMods.Data;

public class FmdData
{
    public BinmodModelData Gpubin { get; set; }
    public ConcurrentBag<FmdTexture> Textures { get; } = new();
    public ConcurrentDictionary<string, GameMaterial> Materials { get; } = new();
}

public class FmdTexture
{
    public string Mesh { get; set; }
    public string TextureSlot { get; set; }
    public byte[] Data { get; set; }
}

[Flags]
public enum WorkshopModBuildContextFlags
{
    None = 0,
    PreviewImageChanged = 1,
    NeedsBuild = 2
}

public class WorkshopModBuildContext
{
    private readonly Action _stateChanged;

    public WorkshopModBuildContext(
        Action stateChanged) =>
        _stateChanged = stateChanged;

    public WorkshopModBuildContextFlags Flags { get; set; }

    public byte[] PreviewImage { get; private set; }
    public byte[] PreviewBtex { get; private set; }
    public byte[] ThumbnailImage { get; private set; }
    public byte[] ThumbnailBtex { get; set; }
    public FmdData[] Fmds { get; } = new FmdData[2];
    public bool[] NeedsWaitFmd { get; set; } = new bool[2];

    public async Task WaitForPreviewData(bool needsThumbnail)
    {
        while (PreviewBtex == null || (needsThumbnail && ThumbnailBtex == null))
        {
            await Task.Delay(100);
        }
    }

    public async Task WaitForBuildData(bool needsThumbnail)
    {
        while ((NeedsWaitFmd[0] && Fmds[0] == null) || (NeedsWaitFmd[1] && Fmds[1] == null) || PreviewBtex == null ||
               (needsThumbnail && ThumbnailBtex == null))
        {
            await Task.Delay(100);
        }
    }

    public async void ProcessFmd(int index, string path)
    {
        NeedsWaitFmd[index] = true;
        Flags |= WorkshopModBuildContextFlags.NeedsBuild;
        _stateChanged();

        await Task.Run(async () =>
        {
            var fmd = new FmdData();

            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var dataEntry = archive.GetEntry("data.json");
            await using var dataStream = dataEntry.Open();
            var dataBytes = new byte[dataEntry.Length];
            await dataStream.ReadAsync(dataBytes);

            var json = Encoding.UTF8.GetString(dataBytes);
            fmd.Gpubin = JsonConvert.DeserializeObject<BinmodModelData>(json);

            var textures = new List<WorkshopModTexture>();
            foreach (var mesh in fmd.Gpubin.Meshes)
            {
                foreach (var (textureId, path) in mesh.Material.Textures
                             .Where(t => !string.IsNullOrEmpty(t.Value)))
                {
                    var textureEntry =
                        archive.Entries.FirstOrDefault(e => e.FullName.Contains($"{mesh.Name}/{textureId}"));
                    await using var textureStream = textureEntry.Open();
                    var textureBytes = new byte[textureEntry.Length];
                    await textureStream.ReadAsync(textureBytes);

                    textures.Add(new WorkshopModTexture
                    {
                        Mesh = mesh.Name,
                        TextureSlot = textureId,
                        Name =
                            $"{mesh.Name.ToSafeString()}_{textureId.ToSafeString()}{BinmodTextureHelper.GetSuffix(textureId)}",
                        Extension = path.Split('\\', '/').Last().Split('.').Last(),
                        Type = BinmodTextureHelper.GetType(textureId),
                        Data = textureBytes
                    });
                }
            }

            var materials = new Dictionary<string, byte[]>();
            foreach (var materialEntry in archive.Entries.Where(e => e.FullName.Contains("materials/")))
            {
                await using var materialStream = materialEntry.Open();
                var materialBytes = new byte[materialEntry.Length];
                _ = await materialStream.ReadAsync(materialBytes);
                materials.Add(materialEntry.Name.Replace(".json", ""), materialBytes);
            }

            foreach (var data in textures)
            {
                var info = Image.Identify(data.Data);
                var texture = new FmdTexture
                {
                    Mesh = data.Mesh,
                    TextureSlot = data.TextureSlot,
                    Data = new BlackTextureBuilder(data.Name, info.Width, info.Height, 0, data.Type switch
                        {
                            TextureType.Normal => BlackTexturePixelFormat.BC5_UNORM,
                            TextureType.AmbientOcclusion or TextureType.Opacity => BlackTexturePixelFormat.BC4_UNORM,
                            TextureType.MenuSprites => BlackTexturePixelFormat.B8G8R8A8_UNORM,
                            _ => BlackTexturePixelFormat.BC1_UNORM
                        }, data.Type == TextureType.BaseColor)
                        .AddRasterImage(new RasterImageDataSource(data.Data, info.Width, info.Height,
                            BlackTexturePixelFormat.R8G8B8A8_UNORM))
                        .Build()
                };

                fmd.Textures.Add(texture);
            }

            Parallel.ForEach(materials, material =>
            {
                var materialTemplateJson = Encoding.UTF8.GetString(material.Value);
                var legacyMaterial = JsonConvert.DeserializeObject<WorkshopModGameMaterial>(materialTemplateJson);
                fmd.Materials.TryAdd(material.Key, legacyMaterial.ToGameMaterial());
            });

            Fmds[index] = fmd;
        });
    }

    public async Task ProcessPreviewImage(string file, Func<Task> onUpdate)
    {
        Flags |= WorkshopModBuildContextFlags.PreviewImageChanged;

        await Task.Run(async () =>
        {
            var path = Path.Combine(IOHelper.GetWebRoot(), "images", "current_preview.png");
            File.Copy(file, path, true);
            await onUpdate();

            await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            (PreviewImage, PreviewBtex) = ProcessPreviewImage(stream);
        });
    }

    public async Task ProcessPreviewImage(byte[] image)
    {
        await Task.Run(() =>
        {
            using var stream = new MemoryStream(image);
            (PreviewImage, PreviewBtex) = ProcessPreviewImage(stream);
        });
    }

    private (byte[] Image, byte[] Texture) ProcessPreviewImage(Stream source)
    {
        var configuration = Configuration.Default.Clone();
        configuration.PreferContiguousImageBuffers = true;
        using var image = Image.Load<Rgba32>(new DecoderOptions {Configuration = configuration}, source);
        using var final = (Image<Rgba32>)image.ResizeFit(600, 600);
        final.DangerousTryGetSinglePixelMemory(out var memory);
        var span = MemoryMarshal.Cast<Rgba32, byte>(memory.Span);
        return (final.EncodeJpeg(953673),
            PreviewBtex = new BlackTextureBuilder("$preview", 600, 600, 1,
                    BlackTexturePixelFormat.BC1_UNORM, true)
                .AddRasterImage(new RasterImageDataSource(span.ToArray(), 600, 600,
                    BlackTexturePixelFormat.R8G8B8A8_UNORM))
                .Build());
    }

    public async Task ProcessThumbnailImage(string file, Func<Task> onUpdate)
    {
        ThumbnailBtex = null;
        Flags |= WorkshopModBuildContextFlags.PreviewImageChanged;

        await Task.Run(async () =>
        {
            var path = Path.Combine(IOHelper.GetWebRoot(), "images", "current_thumbnail.png");
            File.Copy(file, path, true);
            await onUpdate();

            var buffer = await File.ReadAllBytesAsync(file);
            ThumbnailImage = buffer;
            using var stream = new MemoryStream(buffer);
            ThumbnailBtex = ProcessThumbnailImage(stream);
        });
    }

    public async Task ProcessThumbnailImage(byte[] image)
    {
        await Task.Run(() =>
        {
            ThumbnailImage = image;
            using var stream = new MemoryStream(image);
            ThumbnailBtex = ProcessThumbnailImage(stream);
        });
    }

    private byte[] ProcessThumbnailImage(Stream source)
    {
        var configuration = Configuration.Default.Clone();
        configuration.PreferContiguousImageBuffers = true;
        using var image = Image.Load(new DecoderOptions {Configuration = configuration}, source);
        using var final = (Image<Rgba32>)image.ResizeFill(168, 242);
        final.DangerousTryGetSinglePixelMemory(out var memory);
        var span = MemoryMarshal.Cast<Rgba32, byte>(memory.Span);
        return PreviewBtex = new BlackTextureBuilder("default", 168, 242, 1,
                BlackTexturePixelFormat.R8G8B8A8_UNORM, true)
            .AddRasterImage(new RasterImageDataSource(span.ToArray(), 168, 242,
                BlackTexturePixelFormat.R8G8B8A8_UNORM))
            .Build();
    }
}