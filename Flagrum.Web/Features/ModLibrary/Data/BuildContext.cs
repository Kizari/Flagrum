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
using Flagrum.Core.Utilities;
using Flagrum.Web.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Flagrum.Web.Features.ModLibrary.Data;

public class FmdData
{
    public Gpubin Gpubin { get; set; }
    public ConcurrentBag<FmdTexture> Textures { get; } = new();
}

public class FmdTexture
{
    public string Mesh { get; set; }
    public string TextureSlot { get; set; }
    public byte[] Data { get; set; }
}

[Flags]
public enum BuildContextFlags
{
    None = 0,
    PreviewImageChanged = 1,
    NeedsBuild = 2
}

public class BuildContext
{
    private readonly ILogger _logger;
    private readonly Action _stateChanged;

    public BuildContext(
        ILogger logger,
        Action stateChanged)
    {
        _logger = logger;
        _stateChanged = stateChanged;
    }

    public BuildContextFlags Flags { get; set; }

    public byte[] PreviewImage { get; private set; }
    public byte[] PreviewBtex { get; private set; }
    public byte[] ThumbnailImage { get; private set; }
    public byte[] ThumbnailBtex { get; private set; }
    public FmdData[] Fmds { get; } = new FmdData[2];

    public async Task WaitForPreviewData(bool needsThumbnail)
    {
        while (PreviewBtex == null || needsThumbnail && ThumbnailBtex == null)
        {
            await Task.Delay(100);
        }
    }

    public async Task WaitForBuildData(bool needsThumbnail)
    {
        while (Fmds[0] == null || PreviewBtex == null || needsThumbnail && ThumbnailBtex == null)
        {
            await Task.Delay(100);
        }
    }

    public async void ProcessFmd(int index, string path)
    {
        Flags |= BuildContextFlags.NeedsBuild;
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

            Parallel.ForEach(textures, data =>
            {
                var texture = new FmdTexture();
                var converter = new TextureConverter();
                texture.Mesh = data.Mesh;
                texture.TextureSlot = data.TextureSlot;
                texture.Data = converter.Convert(data.Name, data.Extension, data.Type, data.Data);
                fmd.Textures.Add(texture);
            });

            Fmds[index] = fmd;
        });
    }

    public async void ProcessPreviewImage(string file, Func<byte[], Task> onUpdate)
    {
        Flags |= BuildContextFlags.PreviewImageChanged;

        await Task.Run(async () =>
        {
            await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            var buffer = new byte[stream.Length];
            stream.Read(buffer);
            PreviewImage = buffer;

            await File.WriteAllBytesAsync($"{IOHelper.GetWebRoot()}\\images\\current_preview.png", buffer);
            await onUpdate(buffer);

            var converter = new TextureConverter();
            PreviewBtex = converter.Convert(
                "$preview",
                file.Split('.').Last(),
                TextureType.Thumbnail,
                buffer);
        });
    }

    public async void ProcessPreviewImage(byte[] image)
    {
        await Task.Run(() =>
        {
            var converter = new TextureConverter();
            PreviewImage = image;
            PreviewBtex = converter.Convert(
                "$preview",
                "png",
                TextureType.Thumbnail,
                image);
        });
    }

    public async void ProcessThumbnailImage(string file, Func<byte[], Task> onUpdate)
    {
        Flags |= BuildContextFlags.PreviewImageChanged;

        await Task.Run(async () =>
        {
            await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            var buffer = new byte[stream.Length];
            stream.Read(buffer);
            ThumbnailImage = buffer;

            await File.WriteAllBytesAsync($"{IOHelper.GetWebRoot()}\\images\\current_thumbnail.png", buffer);
            await onUpdate(buffer);

            var converter = new TextureConverter();
            ThumbnailBtex = converter.Convert(
                "default",
                file.Split('.').Last(),
                TextureType.Thumbnail,
                buffer);
        });
    }

    public async void ProcessThumbnailImage(byte[] image)
    {
        await Task.Run(() =>
        {
            var converter = new TextureConverter();
            ThumbnailImage = image;
            ThumbnailBtex = converter.Convert(
                "default",
                "png",
                TextureType.Thumbnail,
                image);
        });
    }
}