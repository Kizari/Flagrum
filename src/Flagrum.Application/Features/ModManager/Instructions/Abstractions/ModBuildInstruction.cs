using System;
using System.Runtime.CompilerServices;
using Flagrum.Abstractions.ModManager.Instructions;
using MemoryPack;
using MemoryPack.Formatters;

namespace Flagrum.Application.Features.ModManager.Instructions.Abstractions;

[MemoryPackable]
[MemoryPackUnion(0, typeof(AddLooseFileBuildInstruction))]
[MemoryPackUnion(1, typeof(EnableHookFeatureBuildInstruction))]
[MemoryPackUnion(2, typeof(ReplaceLooseFileBuildInstruction))]
public abstract partial class ModBuildInstruction : IModBuildInstruction
{
    [MemoryPackIgnore] public abstract bool ShouldShowInBuildList { get; }
    [MemoryPackIgnore] public abstract string FilterableName { get; }
}

/// <summary>
/// Specialized formatter for <see cref="IModBuildInstruction"/>,
/// needed to map interfaces back to their packable types.
/// </summary>
public class ModBuildInstructionFormatter : MemoryPackFormatter<IModBuildInstruction>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, 
        scoped ref IModBuildInstruction value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullUnionHeader();
                break;
            case IAddLooseFileBuildInstruction:
                writer.WriteUnionHeader(0);
                writer.WritePackable(Unsafe.As<IModBuildInstruction, AddLooseFileBuildInstruction>(ref value));
                break;
            case IEnableHookFeatureBuildInstruction:
                writer.WriteUnionHeader(1);
                writer.WritePackable(Unsafe.As<IModBuildInstruction, EnableHookFeatureBuildInstruction>(ref value));
                break;
            case IReplaceLooseFileBuildInstruction:
                writer.WriteUnionHeader(2);
                writer.WritePackable(Unsafe.As<IModBuildInstruction, ReplaceLooseFileBuildInstruction>(ref value));
                break;
            default:
                throw new NotSupportedException($"Instruction type {value.GetType().Name} not supported.");
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, scoped ref IModBuildInstruction value)
    {
        if (reader.TryReadUnionHeader(out var tag))
        {
            value = tag switch
            {
                0 => reader.ReadPackable<AddLooseFileBuildInstruction>(),
                1 => reader.ReadPackable<EnableHookFeatureBuildInstruction>(),
                2 => reader.ReadPackable<ReplaceLooseFileBuildInstruction>(),
                _ => throw new NotSupportedException($"Instruction type {value.GetType().Name} not supported.")
            };
        }
        else
        {
            throw new MemoryPackSerializationException("Failed to read union header.");
        }
    }
}