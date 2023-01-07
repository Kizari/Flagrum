using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DirectXTexNet;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Utilities;
using Flagrum.Web.Services;

namespace Flagrum.Console.Scripts.Terrain;

public static class HeightMapScripts
{
    /// <summary>
    /// Puts together a massive image of all heightmaps of one continent normalised across the whole game
    /// and stitched together. See comments inside method for extra settings.
    /// </summary>
    public static void BuildHeightmap()
    {
        // Set to "world/" for Luchil, or "world_train/" for Terraverde
        var typeToProcess = "world/";

        var settings = new SettingsService();
        var hebPaths = Directory.GetFiles(settings.GameDataDirectory, "*.heb", SearchOption.AllDirectories)
            .Where(p => p.Contains("\\lod") && p.Contains("\\world\\"))
            .ToList();
        var hebPaths2 = Directory.GetFiles(settings.GameDataDirectory, "*.heb", SearchOption.AllDirectories)
            .Where(p => p.Contains("\\lod") && p.Contains("\\world_train\\"))
            .ToList();

        var results = new Dictionary<string, HeightMapScriptData>();

        foreach (var path in hebPaths2)
        {
            var name = path.Split("\\").Last().Split(".").First();
            var tokens = name.Split('_');
            var x = int.Parse(tokens[1][1..]);
            var y = int.Parse(tokens[2][1..]);
            results.TryAdd("world_train/" + name, new HeightMapScriptData {X = x, Y = y});
        }

        foreach (var path in hebPaths)
        {
            var name = path.Split("\\").Last().Split(".").First();
            var tokens = name.Split('_');
            var x = int.Parse(tokens[1][1..]);
            var y = int.Parse(tokens[2][1..]);
            results.TryAdd("world/" + name, new HeightMapScriptData {X = x, Y = y});
        }

        var maxX = results.Where(r => r.Key.StartsWith(typeToProcess)).Max(kvp => kvp.Value.X) + 1;
        var maxY = results.Where(r => r.Key.StartsWith(typeToProcess)).Max(kvp => kvp.Value.Y) + 1;

        System.Console.WriteLine($"Max: {maxX}, {maxY}");

        var fullImage =
            TexHelper.Instance.Initialize2D(DXGI_FORMAT.D16_UNORM, maxX * 513, maxY * 513, 1, 1, CP_FLAGS.NONE);

        foreach (var (name, data) in results)
        {
            var tokens = name.Split('/');
            var paths = tokens[0] == "world" ? hebPaths : hebPaths2;

            var lodIndex = 0;
            while (!paths.Any(p => p.Contains($@"\lod0{lodIndex}\{tokens[1]}.heb")))
            {
                lodIndex++;
                if (lodIndex > 6)
                {
                    break;
                }
            }

            if (lodIndex > 6)
            {
                continue;
            }

            var matches = paths.Where(p => p.Contains($@"\lod0{lodIndex}\{tokens[1]}.heb"));
            var heb = HebHeader.FromData(File.ReadAllBytes(matches.First()));
            var heightMapHeader = heb.ImageHeaders.FirstOrDefault(h => h.Type == HebImageType.TYPE_HEIGHT_MAP);

            if (heightMapHeader == null)
            {
                results.Remove(name);
                continue;
            }

            var size = heightMapHeader.TextureWidth * heightMapHeader.TextureHeight;
            data.HeightData = new ushort[size];
            data.HeightMapHeader = heightMapHeader;
            for (var i = 0; i < size; i++)
            {
                data.HeightData[i] = BitConverter.ToUInt16(heightMapHeader.DdsData, i * 2);
            }
        }

        var min = results.Min(kvp => kvp.Value.HeightData.Min(d => d));
        var max = results.Max(kvp => kvp.Value.HeightData.Max(d => d));

        foreach (var (name, _) in results)
        {
            if (!name.StartsWith(typeToProcess))
            {
                results.Remove(name);
            }
        }

        System.Console.WriteLine($"Min height: {min}");
        System.Console.WriteLine($"Max height: {max}");

        // Used to normalise the height from 0-1 across both continents
        var multiplier = ushort.MaxValue / (double)max;

        foreach (var (_, data) in results)
        {
            // Normalise the height data
            var pixelData = data.HeightData.SelectMany(d => BitConverter.GetBytes((ushort)(d * multiplier))).ToArray();

            // Build a DDS file
            var ddsHeader = new DdsHeader
            {
                Height = data.HeightMapHeader.TextureHeight,
                Width = data.HeightMapHeader.TextureWidth,
                PitchOrLinearSize = data.HeightMapHeader.TextureSizeBytes,
                Depth = 1,
                MipMapCount = data.HeightMapHeader.MipCount > 0 ? data.HeightMapHeader.MipCount : 1u,
                Flags = DDSFlags.Texture | DDSFlags.Pitch | DDSFlags.Depth | DDSFlags.MipMapCount,
                PixelFormat = new PixelFormat(),
                DX10Header = new DX10
                {
                    ArraySize = 1,
                    Format = DxgiFormat.D16_UNORM,
                    ResourceDimension = 3
                }
            };

            var ddsData = BtexConverter.WriteDds(ddsHeader, pixelData);
            var pinnedData = GCHandle.Alloc(ddsData, GCHandleType.Pinned);
            var pointer = pinnedData.AddrOfPinnedObject();
            var image = TexHelper.Instance.LoadFromDDSMemory(pointer, ddsData.Length, DDS_FLAGS.NONE);
            pinnedData.Free();

            if (data.HeightMapHeader.TextureWidth < 513)
            {
                var previous = image;
                image = image.Resize(513, 513, TEX_FILTER_FLAGS.DEFAULT);
                previous.Dispose();
            }

            TexHelper.Instance.CopyRectangle(image.GetImage(0), 0, 0,
                513, 513,
                fullImage.GetImage(0), TEX_FILTER_FLAGS.DEFAULT, 513 * data.X, 513 * (maxY - 1 - data.Y));

            image.Dispose();
        }

        var previousImage = fullImage;

        // Use this one if you want compressed 8bpp single channel
        // fullImage = fullImage.Compress(DXGI_FORMAT.BC4_UNORM, TEX_COMPRESS_FLAGS.PARALLEL, 0.5f);

        // Use this one if you want uncompressed 8bpp single channel
        // fullImage = fullImage.Convert(DXGI_FORMAT.R8_UNORM, TEX_FILTER_FLAGS.DEFAULT, 0.5f);

        // Use this one if you want uncompressed 16bpp single channel (this matches the original fidelity)
        // This file will be difficult to work with due to the pixel format and size
        // Currently I have only had luck opening it with GIMP
        fullImage = fullImage.Convert(DXGI_FORMAT.R16_UNORM, TEX_FILTER_FLAGS.DEFAULT, 0.5f);

        previousImage.Dispose();

        // DDS will support the widest range of options
        fullImage.SaveToDDSFile(DDS_FLAGS.NONE, $@"C:\Users\Kieran\Desktop\{typeToProcess[..^1]}.dds");

        // TGA may be easier to work with, but seems to only support R8_UNORM out of the above options
        //fullImage.SaveToTGAFile(0, $@"C:\Users\Kieran\Desktop\{typeToProcess[..^1]}.tga");
    }

    /// <summary>
    /// Puts together a massive image of all terrain diffuse maps of one continent stitched together.
    /// See comments inside method for extra settings.
    /// </summary>
    public static void BuildDiffuseMap()
    {
        // Set to "world/" for Luchil or "world_train/" for Terraverde
        var typeToProcess = "world_train/";

        var settings = new SettingsService();
        var hebPaths = Directory.GetFiles(settings.GameDataDirectory, "*.heb", SearchOption.AllDirectories)
            .Where(p => p.Contains("\\diffuse") && p.Contains("\\world\\"))
            .ToList();
        var hebPaths2 = Directory.GetFiles(settings.GameDataDirectory, "*.heb", SearchOption.AllDirectories)
            .Where(p => p.Contains("\\diffuse") && p.Contains("\\world_train\\"))
            .ToList();

        var results = new Dictionary<string, HeightMapScriptData>();

        foreach (var path in hebPaths2)
        {
            var name = path.Split("\\").Last().Split(".").First();
            var tokens = name.Split('_');
            var x = int.Parse(tokens[1][1..]);
            var y = int.Parse(tokens[2][1..]);
            results.TryAdd("world_train/" + name, new HeightMapScriptData {X = x, Y = y});
        }

        foreach (var path in hebPaths)
        {
            var name = path.Split("\\").Last().Split(".").First();
            var tokens = name.Split('_');
            var x = int.Parse(tokens[1][1..]);
            var y = int.Parse(tokens[2][1..]);
            results.TryAdd("world/" + name, new HeightMapScriptData {X = x, Y = y});
        }

        // This grabs the highest available resolution for each tile that exists in the game files
        foreach (var (name, data) in results)
        {
            var tokens = name.Split('/');
            var paths = tokens[0] == "world" ? hebPaths : hebPaths2;

            var lodIndex = 1024;
            while (!paths.Any(p => p.Contains($@"\{tokens[1]}.{lodIndex}.heb")))
            {
                lodIndex /= 2;
                if (lodIndex < 256)
                {
                    break;
                }
            }

            if (lodIndex < 256)
            {
                continue;
            }

            var matches = paths.Where(p => p.Contains($@"\{tokens[1]}.{lodIndex}.heb"));
            var heb = HebHeader.FromData(File.ReadAllBytes(matches.First()));
            var heightMapHeader = heb.ImageHeaders.FirstOrDefault();

            if (heightMapHeader == null)
            {
                results.Remove(name);
                continue;
            }

            data.HeightMapHeader = heightMapHeader;
        }

        foreach (var (name, _) in results)
        {
            if (!name.StartsWith(typeToProcess))
            {
                results.Remove(name);
            }
        }

        var minX = results.Where(r => r.Key.StartsWith(typeToProcess)).Min(kvp => kvp.Value.X);
        var minY = results.Where(r => r.Key.StartsWith(typeToProcess)).Min(kvp => kvp.Value.Y);
        var maxX = results.Where(r => r.Key.StartsWith(typeToProcess)).Max(kvp => kvp.Value.X) + 1;
        var maxY = results.Where(r => r.Key.StartsWith(typeToProcess)).Max(kvp => kvp.Value.Y) + 1;

        System.Console.WriteLine($"Max: {maxX}, {maxY}");

        var fullImage = TexHelper.Instance.Initialize2D(DXGI_FORMAT.R8G8B8A8_UNORM, (maxX - minX) * 1024,
            (maxY - minY) * 1024, 1, 1, CP_FLAGS.NONE);

        foreach (var (_, data) in results)
        {
            // Build a DDS file
            var ddsHeader = new DdsHeader
            {
                Height = data.HeightMapHeader.TextureHeight,
                Width = data.HeightMapHeader.TextureWidth,
                PitchOrLinearSize = data.HeightMapHeader.TextureSizeBytes,
                Depth = 1,
                MipMapCount = data.HeightMapHeader.MipCount > 0 ? data.HeightMapHeader.MipCount : 1u,
                Flags = DDSFlags.Texture | DDSFlags.Pitch | DDSFlags.Depth | DDSFlags.MipMapCount,
                PixelFormat = new PixelFormat(),
                DX10Header = new DX10
                {
                    ArraySize = 1,
                    Format = BtexConverter.FormatMap[data.HeightMapHeader.TextureFormat],
                    ResourceDimension = 3
                }
            };

            var ddsData = BtexConverter.WriteDds(ddsHeader, data.HeightMapHeader.DdsData);
            var pinnedData = GCHandle.Alloc(ddsData, GCHandleType.Pinned);
            var pointer = pinnedData.AddrOfPinnedObject();
            var image = TexHelper.Instance.LoadFromDDSMemory(pointer, ddsData.Length, DDS_FLAGS.NONE);
            pinnedData.Free();

            if (true)
            {
                var previous = image;
                image = image.Decompress(DXGI_FORMAT.R8G8B8A8_UNORM);
                previous.Dispose();
            }

            // Scales up any tiles that didn't have a 1024 resolution available so they still line up
            if (data.HeightMapHeader.TextureWidth < 1024)
            {
                var previous = image;
                image = image.Resize(1024, 1024, TEX_FILTER_FLAGS.DEFAULT);
                previous.Dispose();
            }

            TexHelper.Instance.CopyRectangle(image.GetImage(0), 0, 0,
                1024, 1024,
                fullImage.GetImage(0), TEX_FILTER_FLAGS.DEFAULT, 1024 * (data.X - minX),
                1024 * (maxY - minY - 1 - (data.Y - minY)));

            image.Dispose();
        }

        // R8G8B8A8_UNORM is compatible with most any image format, so select whatever you like
        //fullImage.SaveToDDSFile(DDS_FLAGS.NONE, $@"C:\Users\Kieran\Desktop\{typeToProcess[..^1]}.dds");
        fullImage.SaveToTGAFile(0, $@"C:\Users\Kieran\Desktop\{typeToProcess[..^1]}.tga");
    }

    /// <summary>
    /// Stitches together all minimap tiles for a given continent into one image.
    /// See internal comments for extra options.
    /// </summary>
    public static void BuildMinimap()
    {
        // Use "luchil" for Luchil, or "terabelde" for Terraverde
        const string folder = "terabelde";

        // Set this to the directory you want to save the resulting image in
        const string outputDirectory = @"C:\Modding\Maps\Output";

        // This process requires that the contents of data://menu/map_resource/luchil/level0
        // and data://menu/map_resource/terabelde/level0 are extracted from the game files.
        // You can use Flagrum's "Export Folder" feature to do this.
        // Make sure the texture output format is set to BTEX.
        const string extractLocation = @"C:\Modding\Maps\Export";

        // Grab the paths of all tiles for the selected continent
        var results = new Dictionary<string, (int X, int Y)>();
        var tiles = Directory.GetFiles($@"{extractLocation}\{folder}\level0", "*.btex").ToList();

        // Parse the file names to get the array indices
        foreach (var tile in tiles)
        {
            var tokens = tile.Split('\\').Last().Split('.').First().Split('_');
            var x = int.Parse(tokens[0][1..]);
            var y = int.Parse(tokens[1][1..]);
            results.Add(tile, (x, y));
        }

        // Get the minimum and maximum X and Y values
        var minX = results.Min(r => r.Value.X);
        var minY = results.Min(r => r.Value.Y);
        var maxX = results.Max(r => r.Value.X) + 1;
        var maxY = results.Max(r => r.Value.Y) + 1;

        // Create a giant empty image of the appropriate size
        var fullImage = TexHelper.Instance.Initialize2D(DXGI_FORMAT.R8G8B8A8_UNORM, (maxX - minX) * 1024,
            (maxY - minY) * 1024, 1, 1, CP_FLAGS.NONE);

        // Process each tile
        foreach (var (path, coordinates) in results)
        {
            // Load the BTEX and convert it to DDS
            var ddsData = BtexConverter.BtexToDds(File.ReadAllBytes(path));
            var pinnedData = GCHandle.Alloc(ddsData, GCHandleType.Pinned);
            var pointer = pinnedData.AddrOfPinnedObject();
            var image = TexHelper.Instance.LoadFromDDSMemory(pointer, ddsData.Length, DDS_FLAGS.NONE);
            pinnedData.Free();

            // Decompress the image so it can be manipulated
            var previous = image;
            image = image.Decompress(DXGI_FORMAT.R8G8B8A8_UNORM);
            previous.Dispose();

            // Copy the image into the appropriate place on the final canvas
            TexHelper.Instance.CopyRectangle(image.GetImage(0), 0, 0,
                1024, 1024,
                fullImage.GetImage(0), TEX_FILTER_FLAGS.DEFAULT, 1024 * (coordinates.X - minX),
                1024 * (maxY - minY - 1 - (coordinates.Y - minY)));

            image.Dispose();
        }

        // Save the final image to 
        fullImage.SaveToTGAFile(0, $@"{outputDirectory}\{folder}.tga");
    }
}

public class HeightMapScriptData
{
    public int X { get; set; }
    public int Y { get; set; }
    public HebImageHeader HeightMapHeader { get; set; }
    public ushort[] HeightData { get; set; }
}