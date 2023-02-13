using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
using Flagrum.Core.Gfxbin.Gmtl.Data;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Types;
using Flagrum.Web.Services;
using Newtonsoft.Json;

namespace Flagrum.Web.Features.WorkshopMods.Data;

public class FmdData
{
    public Gpubin Gpubin { get; set; }
    public ConcurrentBag<FmdTexture> Textures { get; } = new();
    public ConcurrentDictionary<string, Material> Materials { get; } = new();
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
    private readonly LuminousGame _profile;

    public WorkshopModBuildContext(
        Action stateChanged,
        LuminousGame profile)
    {
        _stateChanged = stateChanged;
        _profile = profile;
    }

    public WorkshopModBuildContextFlags Flags { get; set; }

    public byte[] PreviewImage { get; private set; }
    public byte[] PreviewBtex { get; private set; }
    public byte[] ThumbnailImage { get; private set; }
    public byte[] ThumbnailBtex { get; set; }
    public FmdData[] Fmds { get; } = new FmdData[2];
    public bool[] NeedsWaitFmd { get; set; } = new bool[2];

    public async Task WaitForPreviewData(bool needsThumbnail)
    {
        while (PreviewBtex == null || needsThumbnail && ThumbnailBtex == null)
        {
            await Task.Delay(100);
        }
    }

    public async Task WaitForBuildData(bool needsThumbnail)
    {
        while (NeedsWaitFmd[0] && Fmds[0] == null || NeedsWaitFmd[1] && Fmds[1] == null || PreviewBtex == null ||
               needsThumbnail && ThumbnailBtex == null)
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
            fmd.Gpubin = JsonConvert.DeserializeObject<Gpubin>(json);

            var textures = new List<dynamic>();
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

                    textures.Add(new
                    {
                        Mesh = mesh.Name,
                        TextureSlot = textureId,
                        Name =
                            $"{mesh.Name.ToSafeString()}_{textureId.ToSafeString()}{BtexHelper.GetSuffix(textureId)}",
                        Extension = path.Split('\\', '/').Last().Split('.').Last(),
                        Type = BtexHelper.GetType(textureId),
                        Data = textureBytes
                    });
                }
            }

            var materials = new Dictionary<string, byte[]>();
            foreach (var materialEntry in archive.Entries.Where(e => e.FullName.Contains("materials/")))
            {
                await using var materialStream = materialEntry.Open();
                var materialBytes = new byte[materialEntry.Length];
                await materialStream.ReadAsync(materialBytes);
                materials.Add(materialEntry.Name.Replace(".json", ""), materialBytes);
            }

            foreach (var data in textures)
            {
                var texture = new FmdTexture();
                var converter = new TextureConverter(_profile);
                texture.Mesh = data.Mesh;
                texture.TextureSlot = data.TextureSlot;
                texture.Data = converter.ToBtex(data.Name, data.Extension, data.Type, data.Data);
                fmd.Textures.Add(texture);
            }

            Parallel.ForEach(materials, material =>
            {
                var json = Encoding.UTF8.GetString(material.Value);
                var materialObject = JsonConvert.DeserializeObject<Material>(json);
                fmd.Materials.TryAdd(material.Key, materialObject);
            });

            Fmds[index] = fmd;
        });
    }

    public async void ProcessPreviewImage(string file, Func<Task> onUpdate)
    {
        Flags |= WorkshopModBuildContextFlags.PreviewImageChanged;

        await Task.Run(async () =>
        {
            await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            var buffer = new byte[stream.Length];
            stream.Read(buffer);

            await File.WriteAllBytesAsync($"{IOHelper.GetWebRoot()}\\images\\current_preview.png", buffer);
            await onUpdate();

            var converter = new TextureConverter(_profile);
            PreviewBtex = converter.ConvertPreview("$preview", buffer, out var jpeg);
            PreviewImage = jpeg;
        });
    }

    public async void ProcessPreviewImage(byte[] image)
    {
        await Task.Run(() =>
        {
            var converter = new TextureConverter(_profile);
            PreviewBtex = converter.ConvertPreview("$preview", image, out var jpeg);
            PreviewImage = jpeg;
        });
    }

    public async void ProcessThumbnailImage(string file, Func<Task> onUpdate)
    {
        ThumbnailBtex = null;
        Flags |= WorkshopModBuildContextFlags.PreviewImageChanged;

        await Task.Run(async () =>
        {
            await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            var buffer = new byte[stream.Length];
            stream.Read(buffer);
            ThumbnailImage = buffer;

            await File.WriteAllBytesAsync($"{IOHelper.GetWebRoot()}\\images\\current_thumbnail.png", buffer);
            await onUpdate();

            var converter = new TextureConverter(_profile);
            ThumbnailBtex = converter.ConvertThumbnail("default", buffer);
        });
    }

    public async void ProcessThumbnailImage(byte[] image)
    {
        await Task.Run(() =>
        {
            var converter = new TextureConverter(_profile);
            ThumbnailImage = image;
            ThumbnailBtex = converter.ConvertThumbnail("default", image);
        });
    }
}