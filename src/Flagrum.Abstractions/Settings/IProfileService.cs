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
    
    /// <summary>
    /// Directory within temporary local application data that holds temporary files for Flagrum.
    /// </summary>
    string TemporaryDirectory { get; }
    
    /// <summary>
    /// Temporary directory that holds cached mod build files.
    /// </summary>
    string CacheDirectory { get; }
    
    /// <summary>
    /// Temporary directory that holds files while constructing mod archives.
    /// </summary>
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