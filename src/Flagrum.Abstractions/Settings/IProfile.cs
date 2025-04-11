namespace Flagrum.Abstractions;

public partial interface IProfile
{
    Guid Id { get; set; }
    LuminousGame Type { get; set; }
    string Name { get; set; }
    string GamePath { get; set; }
    string BinmodListPath { get; set; }
    Version LastSeenVersion { get; set; }
    string GameExeHash { get; set; }
    long GameExeHashTime { get; set; }
    bool HasUpgradedToSteppedMigrations { get; set; }
    void SetConfiguration(IConfiguration configuration);
    bool HasMigrated(Guid migration);
    void SetMigrated(Guid migration);
    void SetMigratedNoSave(IEnumerable<Guid> migrations);
}

public interface IProfileViewModel
{
    string Id { get; set; }
    LuminousGame Type { get; set; }
    string Name { get; set; }
    string GamePath { get; set; }
    string BinmodListPath { get; set; }
}