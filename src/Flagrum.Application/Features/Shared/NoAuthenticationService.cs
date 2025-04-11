using Flagrum.Abstractions;

namespace Flagrum.Application.Features.Shared;

/// <summary>
/// Authentication service implementation for the open-source edition of Flagrum that does not integrate with
/// the official servers. Does not perform authentication, simply exists to ensure the application functions
/// correctly even when not connected to the official servers.
/// </summary>
public class NoAuthenticationService : IAuthenticationService
{
    /// <inheritdoc />
    public bool IsAuthenticated => false;
}