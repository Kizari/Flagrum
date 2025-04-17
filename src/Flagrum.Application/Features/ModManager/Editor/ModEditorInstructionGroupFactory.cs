using System.Linq;
using Flagrum.Abstractions.ModManager;
using Flagrum.Abstractions.ModManager.Project;
using Injectio.Attributes;

namespace Flagrum.Application.Features.ModManager.Editor;

/// <inheritdoc />
[RegisterSingleton<IModEditorInstructionGroupFactory>]
public class ModEditorInstructionGroupFactory : IModEditorInstructionGroupFactory
{
    /// <inheritdoc />
    public IModEditorInstructionGroup Create(
        ModEditorInstructionGroupType type,
        string name,
        IFlagrumProjectArchive archive,
        bool isExpanded) =>
        new ModEditorInstructionGroup
        {
            Text = name,
            Archive = archive,
            Type = type,
            IsExpanded = isExpanded,
            Instructions = archive.Instructions.Where(i => i.ShouldShowInBuildList)
        };
}