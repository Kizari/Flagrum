using System;
using System.IO;
using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Mod;
using Flagrum.Core.Archive;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Graphics.Textures.Luminous.Builder;
using Flagrum.Core.Utilities;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Instructions;

[MemoryPackable]
public partial class AddToPackedTextureArrayBuildInstruction
    : PackedAssetBuildInstruction, IAddToPackedTextureArrayBuildInstruction
{
    public override bool ShouldShowInBuildList => Premium.IsClientWhitelisted;

    public override void Apply(IFlagrumProject mod, IEbonyArchive archive, IFlagrumProjectArchive projectArchive)
    {
        // Read the cached image from disk
        var hash = Cryptography.HashFileUri64(Uri);
        var cachePath = Path.Combine(Profile.CacheDirectory, $"{mod.Identifier}{hash}.ffg");
        var fragment = new FmodFragment();
        fragment.Read(FilePath.EndsWith(".ffg")
            ? FilePath
            : cachePath);
        var unprocessedData = EbonyArchiveFile.GetUnprocessedData(fragment.Flags,
            fragment.OriginalSize, fragment.Key, fragment.Data);

        // Get a copy of the original texture array from disk
        using var sourceArchive =
            new EbonyArchive(Path.Combine(Profile.GameDataDirectory, projectArchive.RelativePath));
        var textureArray = sourceArchive[Uri].GetReadableData();
        var source = new BlackTexture(textureArray);

        // Append the texture to the array
        var texture = new BlackTexture(unprocessedData);
        var newImageSurfaces = texture.GetImageDataSources();
        if (newImageSurfaces.Count > 1)
        {
            throw new InvalidOperationException("Unexpected texture array. Should be a single image.");
        }

        var result = new BlackTextureBuilder(source)
            .AddImage(newImageSurfaces[0])
            .Build();

        archive.AddFile(Uri, fragment.Flags & ~EbonyArchiveFileFlags.Autoload, result);
    }

    public override void Revert(IEbonyArchive archive, IFlagrumProjectArchive projectArchive)
    {
        if (archive.HasFile(Uri))
        {
            archive.RemoveFile(Uri);
        }
    }
}