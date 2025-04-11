namespace Flagrum.Abstractions;

public interface IProfileService : IDisposable
{
    List<IProfileViewModel> Profiles { get; }
    string ClientId { get; }
    IProfile Current { get; }
    string LastVersionNotes { get; set; }
    Version LastVersion { get; }
    bool DidMigrateThisSession { get; }
    bool IsReady { get; }
    string FlagrumDirectory { get; }
    string DatabasePath { get; }
    string FileIndexPath { get; }
    string ImagesDirectory { get; }
    string ModThumbnailWebDirectory { get; }
    string CacheDirectory { get; }
    string ModStagingDirectory { get; }

    /// <summary>
    /// Absolute path to the patch folder inside the game's data directory.
    /// </summary>
    string PatchDirectory { get; }

    /// <summary>
    /// Absolute path to the directory that holds mod project files for the current profile.
    /// </summary>
    string ModFilesDirectory { get; }

    string EarcModThumbnailDirectory { get; }
    string EarcModBackupsDirectory { get; }

    /// <summary>
    /// Folder inside the main Flagrum directory for holding temporary files.
    /// </summary>
    /// <remarks>
    /// Nothing in here is cleaned up automatically, so must be handled by the consumer.
    /// </remarks>
    string TemporaryDirectory { get; }

    /// <summary>
    /// The absolute path to the Steam executable.
    /// </summary>
    string SteamExePath { get; }

    string BinmodDirectory { get; }
    string WorkshopDirectory { get; }
    string GameDataDirectory { get; }
    string GameDirectory { get; }
    string ModStatePath { get; }
    bool IsGamePathAvailable(string path);
    void SetPatreonToken(string? token, string? refreshToken, DateTime expiry);
    void SetGiftToken(string token);
    void SetCurrentProfile(string id);
    void Add(IProfileViewModel profile);
    void Update(IProfileViewModel profile);
    void Delete(IProfileViewModel profile);
    bool IsGameRunning();
}