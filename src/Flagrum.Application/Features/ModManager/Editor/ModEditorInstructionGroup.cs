using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flagrum.Abstractions.ModManager;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;

namespace Flagrum.Application.Features.ModManager.Editor;

public class ModEditorInstructionGroup : IModEditorInstructionGroup
{
    public const string LooseFiles = "Loose Files";
    public const string HookFeatures = "Hook Features";
    public IEnumerable<IModBuildInstruction> Instructions { get; set; }

    public string Text { get; set; }
    public IFlagrumProjectArchive Archive { get; set; }
    public ModEditorInstructionGroupType Type { get; set; }
    public bool IsExpanded { get; set; }

    public static List<IModEditorInstructionGroup> CreateGroups(List<IFlagrumProjectArchive> archives,
        List<IModBuildInstruction> instructions)
    {
        var groups = Assembly.GetAssembly(typeof(GlobalBuildInstruction))!.GetTypes()
            .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(GlobalBuildInstruction)))
            .Select(t => ((GlobalBuildInstruction)Activator.CreateInstance(t))!.InstructionGroupName)
            .Distinct()
            .Select(IModEditorInstructionGroup (name) => new ModEditorInstructionGroup
            {
                Text = name,
                Instructions = instructions.OfType<GlobalBuildInstruction>()
                    .Where(i => i.ShouldShowInBuildList && i.InstructionGroupName == name)
            })
            .ToList();

        groups.AddRange(archives.Select(a => new ModEditorInstructionGroup
        {
            Text = a.RelativePath,
            Archive = a,
            Type = ModEditorInstructionGroupType.Archive,
            Instructions = a.Instructions.Where(i => i.ShouldShowInBuildList)
        }));

        return groups;
    }
}