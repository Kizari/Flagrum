using Flagrum.Abstractions;
using Flagrum.Application.Utilities;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Instructions.Abstractions;

public abstract class GlobalBuildInstruction : ModBuildInstruction
{
    [FactoryInject] protected IProfileService Profile { get; set; }
    [FactoryInject] protected IAuthenticationService Authentication { get; set; }
    [FactoryInject] protected IPremiumService Premium { get; set; }

    [MemoryPackIgnore] public abstract string InstructionGroupName { get; }

    public abstract void Apply();
    public abstract void Revert();
}