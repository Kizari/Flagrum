namespace Flagrum.Abstractions;

/// <summary>
/// Authentication type used by the current user to verify premium features.
/// </summary>
public enum AuthenticationType
{
    /// <summary>
    /// User either has not attempted to authenticate yet, or their authentication method is no longer valid.
    /// </summary>
    None,

    /// <summary>
    /// User last authenticated via Patreon and has persisted information for this method.
    /// </summary>
    Patreon,

    /// <summary>
    /// User last authenticated via a gift token and has persisted information for this method.
    /// </summary>
    GiftToken
}