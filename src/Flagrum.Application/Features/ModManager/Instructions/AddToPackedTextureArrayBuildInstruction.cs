using System.IO;
using Flagrum.Abstractions;
using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Archive;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Mod;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Instructions;

[MemoryPackable]
public partial class AddToPackedTextureArrayBuildInstruction 
    : PackedAssetBuildInstruction, IAddToPackedTextureArrayBuildInstruction
{
    public override bool ShouldShowInBuildList => Premium.IsClientWhitelisted;

    public override void Apply(IFlagrumProject mod, IEbonyArchive archive, IFlagrumProjectArchive projectArchive)
    {
        var hash = Cryptography.HashFileUri64(Uri);
        var cachePath = Path.Combine(Profile.CacheDirectory, $"{mod.Identifier}{hash}.ffg");

        var fragment = new FmodFragment();
        fragment.Read(FilePath.EndsWith(".ffg")
            ? FilePath
            : cachePath);

        var unprocessedData = EbonyArchiveFile.GetUnprocessedData(fragment.Flags,
            fragment.OriginalSize, fragment.Key, fragment.Data);

        using var sourceArchive =
            new EbonyArchive(Path.Combine(Profile.GameDataDirectory, projectArchive.RelativePath));
        var textureArray = sourceArchive[Uri].GetReadableData();
        var textureArrayBinary = new BlackTexture(LuminousGame.FFXV);
        textureArrayBinary.Read(textureArray);
        var textureBinary = new BlackTexture(LuminousGame.FFXV);
        textureBinary.Read(unprocessedData);
        textureArrayBinary.AppendTextureToArray(textureBinary);
        var result = textureArrayBinary.Write();

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