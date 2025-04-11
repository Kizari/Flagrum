namespace Flagrum.Abstractions.ModManager.Instructions;

/// <summary>
/// Represents a factory that creates instances of <see cref="IModBuildInstruction" />.
/// </summary>
public interface IModBuildInstructionFactory
{
    /// <summary>
    /// Creates a new instruction of the given type.
    /// </summary>
    /// <typeparam name="TInstruction">
    /// Type of the modification, must implement <see cref="IModBuildInstruction" />.
    /// </typeparam>
    TInstruction Create<TInstruction>() where TInstruction : IModBuildInstruction;

    /// <summary>
    /// Injects services into a <see cref="IModBuildInstruction" /> instance.
    /// </summary>
    /// <param name="instruction">Instruction to inject services into.</param>
    /// <remarks>
    /// Service properties must be marked with <see cref="FactoryInjectAttribute" /> to
    /// be populated by this method.
    /// </remarks>
    void Inject(IModBuildInstruction instruction);
}