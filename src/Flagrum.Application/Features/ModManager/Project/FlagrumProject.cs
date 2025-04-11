using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Persistence;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Resources;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Project;

[MemoryPackable]
public partial class FlagrumProject : IFlagrumProject
{
    public Guid Identifier { get; set; }
    public ModFlags Flags { get; set; }
    
    [Display(Name = nameof(DisplayNameResource.ModName), ResourceType = typeof(DisplayNameResource))]
    [Required(ErrorMessageResourceName = nameof(ErrorMessageResource.RequiredError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    [StringLength(37, ErrorMessageResourceName = nameof(ErrorMessageResource.MaxLengthError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    public string Name { get; set; }

    [Display(Name = nameof(DisplayNameResource.Author), ResourceType = typeof(DisplayNameResource))]
    [Required(ErrorMessageResourceName = nameof(ErrorMessageResource.RequiredError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    [StringLength(32, ErrorMessageResourceName = nameof(ErrorMessageResource.MaxLengthError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    public string Author { get; set; }

    [Display(Name = nameof(DisplayNameResource.Description), ResourceType = typeof(DisplayNameResource))]
    [Required(ErrorMessageResourceName = nameof(ErrorMessageResource.RequiredError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    [StringLength(1000, ErrorMessageResourceName = nameof(ErrorMessageResource.MaxLengthError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    public string Description { get; set; }

    public string Readme { get; set; }

    [MemoryPackIgnore]
    public IEnumerable<IModBuildInstruction> AllInstructions => Archives
        .SelectMany(a => a.Instructions)
        .Cast<PackedBuildInstruction>()
        .Union(Instructions);

    // This specifically only targets packed instructions since loose files don't need to be cached
    [MemoryPackIgnore]
    public bool HaveFilesChanged => Archives
        .SelectMany(archive => archive.Instructions.OfType<PackedAssetBuildInstruction>())
        .Any(file => file.FileLastModified != File.GetLastWriteTime(file.FilePath).Ticks);

    public List<IFlagrumProjectArchive> Archives { get; set; } = [];
    public List<IModBuildInstruction> Instructions { get; set; } = [];

    public Task Save(string path)
    {
        foreach (var archive in Archives.Where(a => !a.Instructions.Any()).ToList())
        {
            Archives.Remove(archive);
        }

        return MemoryPackHelper.SerializeCompressedAsync(path, this);
    }

    /// <inheritdoc />
    public bool ContainsLooseFile(string relativePath) => Instructions
        .Any(i => i is AddLooseFileBuildInstruction instruction
                  && instruction.RelativePath == relativePath);

    public List<string> GetDeadFiles()
    {
        return Archives.SelectMany(e => e.Instructions.OfType<PackedAssetBuildInstruction>()
                .Where(i => !File.Exists(i.FilePath)))
            .Select(i => i.FilePath)
            .Union(Instructions.OfType<LooseAssetBuildInstruction>()
                .Where(i => !File.Exists(i.FilePath))
                .Select(i => i.FilePath))
            .ToList();
    }

    // This specifically only targets packed instructions since loose files don't need to be cached
    public void UpdateLastModifiedTimestamps()
    {
        foreach (var change in Archives.SelectMany(
                     archive => archive.Instructions.OfType<PackedAssetBuildInstruction>()))
        {
            change.FileLastModified = File.GetLastWriteTime(change.FilePath).Ticks;
        }
    }

    public bool AreReferencesValid()
    {
        if (Archives.Any(a => a.Instructions
                .Any(i => i.Uri == "data://menu/pause/script/menusentry_pausemenue.ebex")))
        {
            return false;
        }

        var references = Archives.SelectMany(a => a.Instructions.OfType<AddReferenceBuildInstruction>()
            .Select(r => new {Archive = a, r.Uri})).ToList();
        return references.All(r => !IsSelfReference(r.Archive, r.Uri) && HasValidReferenceExtension(r.Uri));
    }

    private static bool HasValidReferenceExtension(string uri)
    {
        var extensions = new[] {".max", ".sax", ".bk2", ".ebex@", ".prefab@", ".htpk", ".tif", ".tga", ".heb"};
        return extensions.Any(uri.EndsWith);
    }

    private static bool IsSelfReference(IFlagrumProjectArchive archive, string uri) =>
        uri == "data://" + archive.RelativePath.Replace('\\', '/').Replace(".earc", ".ebex@").ToLower()
        || uri == "data://" + archive.RelativePath.Replace('\\', '/').Replace(".earc", ".prefab@").ToLower();

    public class Formatter : MemoryPackFormatter<IFlagrumProject>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer,
            scoped ref IFlagrumProject value) => writer.WritePackable((FlagrumProject)value);

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IFlagrumProject value) =>
            value = reader.ReadPackable<FlagrumProject>();
    }
}