namespace Flagrum.Abstractions;

public interface IConfiguration
{
    Guid ClientId { get; set; }
    Guid CurrentProfile { get; set; }
    string LatestVersionNotes { get; set; }
    Guid GiftToken { get; set; }
    string PatreonToken { get; set; }
    string PatreonRefreshToken { get; set; }
    DateTime PatreonTokenExpiry { get; set; }
    AuthenticationType AuthenticationType { get; set; }
    bool ShouldMigratePreProfilesData { get; set; }
    List<IProfile> Profiles { get; set; }
    Guid LucentClientId { get; }
    void AddProfile(IProfile profile);
    void UpdateProfile(IProfile profile);
    void DeleteProfile(Guid id);
    void Save();
    bool ContainsKey(StateKey key);
    TValue Get<TValue>(StateKey key);
    void Set<TValue>(StateKey key, TValue value);
    bool HasMigrated(Guid migration);
    void SetMigrated(Guid migration);
    void SetMigratedNoSave(IEnumerable<Guid> migrations);

    /// <summary>
    /// Should be run when Flagrum is first installed to prevent previous version migrations from running
    /// </summary>
    void OnFreshInstall(IEnumerable<Guid> applicationSteps, IEnumerable<Guid> profileSteps);
}