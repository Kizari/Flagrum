using Flagrum.Abstractions.ModManager;

namespace Flagrum.Abstractions;

/// <summary>
/// Represents a service that handles premium features for the application.
/// </summary>
public interface IPremiumService
{
    /// <summary>
    /// Whether the user is whitelisted to access experimental features.
    /// </summary>
    bool IsClientWhitelisted { get; }

    /// <summary>
    /// Gets the <see cref="Type" /> of a premium component.
    /// </summary>
    /// <param name="component"><see cref="PremiumComponentType" /> member that represents the desired component.</param>
    /// <returns><see cref="Type" /> of the desired component.</returns>
    /// <remarks>Typically used with <c>DynamicComponent</c> to render the desired component.</remarks>
    Type? GetComponentType(PremiumComponentType component);

    /// <summary>
    /// Gets the type of build that the game executable for the current profile is.
    /// </summary>
    /// <returns>The game executable type.</returns>
    /// <remarks>
    /// The game executable will be hashed if a current hash is not stored in the persisted profile data.
    /// </remarks>
    GameExecutableType GetGameExecutableType();
}