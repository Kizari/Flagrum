namespace Flagrum.Abstractions;

/// <summary>
/// Represents a service that offers functionality relating to user authentication for premium feature access.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Whether the user is authenticated with premium access.
    /// </summary>
    bool IsAuthenticated { get; }
}