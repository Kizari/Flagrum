using System;
using System.Runtime.CompilerServices;
using Flagrum.Abstractions;
using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Application.Utilities;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Instructions.Abstractions;

[MemoryPackable]
[MemoryPackUnion(0, typeof(AddPackedFileBuildInstruction))]
[MemoryPackUnion(1, typeof(ReplacePackedFileBuildInstruction))]
[MemoryPackUnion(2, typeof(RemovePackedFileBuildInstruction))]
[MemoryPackUnion(3, typeof(AddReferenceBuildInstruction))]
[MemoryPackUnion(4, typeof(AddToPackedTextureArrayBuildInstruction))]
public abstract partial class PackedBuildInstruction : ModBuildInstruction, IPackedBuildInstruction
{
    [FactoryInject] protected IProfileService Profile { get; set; }
    [FactoryInject] protected IAuthenticationService Authentication { get; set; }
    [FactoryInject] protected IPremiumService Premium { get; set; }
    
    /// <inheritdoc />
    public override string FilterableName => Uri;
    
    /// <inheritdoc />
    public string Uri { get; set; }

    /// <inheritdoc />
    public EbonyArchiveFileFlags Flags { get; set; }

    /// <inheritdoc />
    public abstract void Apply(IFlagrumProject mod, IEbonyArchive archive, IFlagrumProjectArchive projectArchive);

    /// <inheritdoc />
    public abstract void Revert(IEbonyArchive archive, IFlagrumProjectArchive projectArchive);
}

/// <summary>
/// Specialized formatter for <see cref="IPackedBuildInstruction"/>,
/// needed to map interfaces back to their packable types.
/// </summary>
public class PackedBuildInstructionFormatter : MemoryPackFormatter<IPackedBuildInstruction>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, 
        scoped ref IPackedBuildInstruction value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullUnionHeader();
                break;
            case IAddPackedFileBuildInstruction:
                writer.WriteUnionHeader(0);
                writer.WritePackable(Unsafe.As<IPackedBuildInstruction, AddPackedFileBuildInstruction>(ref value));
                break;
            case IReplacePackedFileBuildInstruction:
                writer.WriteUnionHeader(1);
                writer.WritePackable(Unsafe.As<IPackedBuildInstruction, ReplacePackedFileBuildInstruction>(ref value));
                break;
            case IRemovePackedFileBuildInstruction:
                writer.WriteUnionHeader(2);
                writer.WritePackable(Unsafe.As<IPackedBuildInstruction, RemovePackedFileBuildInstruction>(ref value));
                break;
            case IAddReferenceBuildInstruction:
                writer.WriteUnionHeader(3);
                writer.WritePackable(Unsafe.As<IPackedBuildInstruction, AddReferenceBuildInstruction>(ref value));
                break;
            case IAddToPackedTextureArrayBuildInstruction:
                writer.WriteUnionHeader(4);
                writer.WritePackable(Unsafe.As<IPackedBuildInstruction, AddToPackedTextureArrayBuildInstruction>(ref value));
                break;
            default:
                throw new NotSupportedException($"Instruction type {value.GetType().Name} not supported.");
        }
    }
    
    public override void Deserialize(ref MemoryPackReader reader, scoped ref IPackedBuildInstruction value)
    {
        if (reader.TryReadUnionHeader(out var tag))
        {
            value = tag switch
            {
                0 => reader.ReadPackable<AddPackedFileBuildInstruction>(),
                1 => reader.ReadPackable<ReplacePackedFileBuildInstruction>(),
                2 => reader.ReadPackable<RemovePackedFileBuildInstruction>(),
                3 => reader.ReadPackable<AddReferenceBuildInstruction>(),
                4 => reader.ReadPackable<AddToPackedTextureArrayBuildInstruction>(),
                _ => throw new NotSupportedException($"Instruction type {value.GetType().Name} not supported.")
            };
        }
        else
        {
            throw new MemoryPackSerializationException("Failed to read union header.");
        }
    }
}