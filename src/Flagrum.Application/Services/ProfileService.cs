using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Flagrum.Abstractions;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.Settings.Data;
using Microsoft.Win32;

namespace Flagrum.Application.Services;

public class ProfileService : IProfileService
{
    private const string Steam32 = @"SOFTWARE\VALVE\Steam";
    private const string Steam64 = @"SOFTWARE\Wow6432Node\Valve\Steam";

    private readonly EbonyArchiveManager _archiveManager = new();
    private readonly IConfiguration _configuration;

    public ProfileService(IConfiguration configuration)
    {
        _configuration = configuration;

        if (_configuration.ShouldMigratePreProfilesData)
        {
            MigratePreProfilesData();
        }

        // Setup current profile
        Current = _configuration.Profiles.First(p => p.Id == _configuration.CurrentProfile);

        // Ensure required directories exist
        IOHelper.EnsureDirectoryExists(FlagrumDirectory);
        IOHelper.EnsureDirectoriesExistForFilePath(DatabasePath);
        IOHelper.EnsureDirectoryExists(ImagesDirectory);
        IOHelper.EnsureDirectoryExists(ModThumbnailWebDirectory);
        IOHelper.EnsureDirectoryExists(CacheDirectory);
        IOHelper.EnsureDirectoryExists(ModStagingDirectory);
        IOHelper.EnsureDirectoryExists(EarcModThumbnailDirectory);
        IOHelper.EnsureDirectoryExists(ModFilesDirectory);
        IOHelper.EnsureDirectoryExists(EarcModBackupsDirectory);
        IOHelper.EnsureDirectoryExists(TemporaryDirectory);

        // Try setup other paths automatically
        TrySetDefaultGamePath();
        TrySetDefaultBinmodListPath();
        TrySetSteamExePath();
    }

    private string WorkshopPath
    {
        get
        {
            if (string.IsNullOrEmpty(Current.GamePath))
            {
                return null;
            }

            var ffxvDirectory = Path.GetDirectoryName(Current.GamePath);
            var commonFolder = Path.GetDirectoryName(ffxvDirectory);
            var steamAppsFolder = Path.GetDirectoryName(commonFolder);

            return $@"{steamAppsFolder}\workshop\appworkshop_637650.acf";
        }
    }

    public List<IProfileViewModel> Profiles => _configuration.Profiles.Select(p => new FlagrumProfile
    {
        Id = p.Id.ToString(),
        Type = p.Type,
        Name = p.Name,
        GamePath = p.GamePath,
        BinmodListPath = p.BinmodListPath
    }).Cast<IProfileViewModel>().ToList();

    public string ClientId => _configuration.ClientId.ToString();

    public IProfile Current { get; }

    public string LastVersionNotes
    {
        get => _configuration.LatestVersionNotes;
        set => _configuration.LatestVersionNotes = value;
    }

    public Version LastVersion
    {
        get
        {
            var lastVersion = LastVersionNotes;
            if (lastVersion == null)
            {
                return null;
            }

            var tokens = LastVersionNotes.Split('.');
            return new Version(int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(tokens[2]));
        }
    }

    public bool DidMigrateThisSession { get; private set; }

    public bool IsReady
    {
        get
        {
            if (Current?.Type == LuminousGame.FFXV &&
                Current.GamePath?.Contains("forspoken", StringComparison.OrdinalIgnoreCase) == true)
            {
                return false;
            }

            if (Current?.GamePath == null || !Directory.Exists(GameDataDirectory))
            {
                return false;
            }

            return Current?.Type == LuminousGame.Forspoken || Current?.BinmodListPath != null;
        }
    }

    public string FlagrumDirectory =>
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Flagrum";

    public string DatabasePath => $@"{FlagrumDirectory}\profiles\{Current.Id}\flagrum.db";
    public string FileIndexPath => Path.Combine(FlagrumDirectory, "profiles", Current.Id.ToString(), "file_index.zstd");
    public string ImagesDirectory => $@"{IOHelper.GetWebRoot()}\images\{Current.Id}";
    public string ModThumbnailWebDirectory => $@"{IOHelper.GetWebRoot()}\EarcMods\{Current.Id}";

    /// <inheritdoc />
    public string TemporaryDirectory => Path.Combine(IOHelper.LocalApplicationData, "Temp", "Flagrum");
    
    /// <inheritdoc />
    public string CacheDirectory => Path.Combine(TemporaryDirectory, Current.Id.ToString(), "cache");
    
    /// <inheritdoc />
    public string ModStagingDirectory => Path.Combine(TemporaryDirectory, Current.Id.ToString(), "staging");

    /// <inheritdoc />
    public string PatchDirectory => Path.Combine(GameDataDirectory, "patch");

    /// <inheritdoc />
    public string ModFilesDirectory => $@"{FlagrumDirectory}\earc\{Current.Id}";

    public string EarcModThumbnailDirectory => $@"{ModFilesDirectory}\thumbnails";
    public string EarcModBackupsDirectory => $@"{ModFilesDirectory}\backup";

    /// <inheritdoc />
    public string SteamExePath { get; private set; }

    public string BinmodDirectory => $"{Path.GetDirectoryName(Current.BinmodListPath)}";
    public string WorkshopDirectory => $@"{Path.GetDirectoryName(WorkshopPath)}\content\637650";
    public string GameDataDirectory => $@"{Path.GetDirectoryName(Current.GamePath)}\datas";
    public string GameDirectory => Path.GetDirectoryName(Current.GamePath);
    public string ModStatePath => Path.Combine(FlagrumDirectory, "Profiles", Current.Id.ToString(), "mod_state.zstd");

    public void Dispose()
    {
        _archiveManager?.Dispose();
        GC.SuppressFinalize(this);
    }

    public bool IsGamePathAvailable(string path)
    {
        var allPaths = _configuration.Profiles
            .Where(p => p.GamePath != null)
            .Select(p => p.GamePath)
            .ToList();

        return !allPaths.Any(p => IOHelper.AreInSameDirectory(p, path));
    }

    public void SetPatreonToken(string token, string refreshToken, DateTime expiry)
    {
        _configuration.PatreonToken = token;
        _configuration.PatreonRefreshToken = refreshToken;
        _configuration.PatreonTokenExpiry = expiry;
    }

    public void SetGiftToken(string token)
    {
        _configuration.GiftToken = new Guid(token);
    }

    public void SetCurrentProfile(string id)
    {
        _configuration.CurrentProfile = new Guid(id);
    }

    public void Add(IProfileViewModel profile)
    {
        _configuration.AddProfile(new Profile(_configuration)
        {
            Id = new Guid(profile.Id),
            Type = profile.Type,
            Name = profile.Name,
            GamePath = profile.GamePath,
            BinmodListPath = profile.BinmodListPath
        });
    }

    public void Update(IProfileViewModel profile)
    {
        _configuration.UpdateProfile(new Profile(_configuration)
        {
            Id = new Guid(profile.Id),
            Type = profile.Type,
            Name = profile.Name,
            GamePath = profile.GamePath,
            BinmodListPath = profile.BinmodListPath
        });
    }

    public void Delete(IProfileViewModel profile)
    {
        _configuration.DeleteProfile(new Guid(profile.Id));
        DeleteDirectoryIfExists(profile, Path.GetDirectoryName(DatabasePath));
        DeleteDirectoryIfExists(profile, ImagesDirectory);
        DeleteDirectoryIfExists(profile, ModThumbnailWebDirectory);
        DeleteDirectoryIfExists(profile, CacheDirectory);
        DeleteDirectoryIfExists(profile, ModStagingDirectory);
        DeleteDirectoryIfExists(profile, ModFilesDirectory);
    }

    public bool IsGameRunning()
    {
        if (!string.IsNullOrWhiteSpace(Current.GamePath))
        {
            var directory = Path.GetDirectoryName(Current.GamePath);
            var fileName = Path.GetFileNameWithoutExtension(Current.GamePath);

            if (directory != null && fileName != null)
            {
                try
                {
                    return Process.GetProcessesByName(fileName)
                        .Any(p => p.MainModule?.FileName
                                      .StartsWith(directory, StringComparison.OrdinalIgnoreCase) == true);
                }
                catch
                {
                    // Failure should not prevent Flagrum from doing what it needs to, so assume the game is closed
                    return false;
                }
            }
        }

        return false;
    }

    public EbonyArchive OpenArchive(string absolutePath)
    {
        if (Current.Type == LuminousGame.Forspoken)
        {
            // Don't cache c000.earc otherwise the mod manager can't alter it
            return absolutePath.EndsWith("c000.earc")
                ? new EbonyArchive(absolutePath)
                : _archiveManager.Open(absolutePath);
        }

        // FFXV has a ton of earcs, so we don't want to hold them all in memory
        return new EbonyArchive(absolutePath);
    }

    private void TrySetDefaultGamePath()
    {
        if (Current.GamePath == null && Current.Type == LuminousGame.FFXV)
        {
            var gamePath =
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Steam\steamapps\common\Final Fantasy XV\ffxv_s.exe";

            if (File.Exists(gamePath) && IsGamePathAvailable(gamePath))
            {
                Current.GamePath = gamePath;
            }
        }
        else if (Current.GamePath == null && Current.Type == LuminousGame.Forspoken)
        {
            var gamePath =
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Steam\steamapps\common\FORSPOKEN\FORSPOKEN.exe";

            if (File.Exists(gamePath) && IsGamePathAvailable(gamePath))
            {
                Current.GamePath = gamePath;
            }
        }
    }

    private void TrySetDefaultBinmodListPath()
    {
        if (Current.BinmodListPath == null && Current.Type == LuminousGame.FFXV)
        {
            var basePath =
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\Documents\My Games\FINAL FANTASY XV\Steam";

            if (Directory.Exists(basePath))
            {
                foreach (var directory in Directory.EnumerateDirectories(basePath))
                {
                    foreach (var subdirectory in Directory.EnumerateDirectories(directory))
                    {
                        if (subdirectory.EndsWith("\\mod"))
                        {
                            var binmodList = Directory.EnumerateFiles(subdirectory)
                                .FirstOrDefault(f => f.ToLower().EndsWith("\\binmod.list"));

                            if (binmodList != null)
                            {
                                Current.BinmodListPath = binmodList;
                            }
                        }
                    }
                }
            }
        }
    }

    private void TrySetSteamExePath()
    {
        try
        {
            var key64 = Registry.LocalMachine.OpenSubKey(Steam64);
            if (key64 == null)
            {
                var key32 = Registry.LocalMachine.OpenSubKey(Steam32);
                SteamExePath = key32?.GetValue("InstallPath")?.ToString();
            }
            else
            {
                SteamExePath = key64.GetValue("InstallPath")?.ToString();
            }

            if (SteamExePath != null)
            {
                SteamExePath += @"\steam.exe";
            }
        }
        catch
        {
            // Don't want a failed Steam path to take out the whole app
            // It's not that important
        }
    }

    /// <summary>
    /// Moves the data from pre-1.4 installations into the default profile
    /// </summary>
    private void MigratePreProfilesData()
    {
        if (File.Exists($@"{FlagrumDirectory}\flagrum.db") && !File.Exists(DatabasePath))
        {
            IOHelper.EnsureDirectoriesExistForFilePath(DatabasePath);
            File.Move($@"{FlagrumDirectory}\flagrum.db", DatabasePath);
            MoveDirectoryIfExists($@"{FlagrumDirectory}\earc", ModFilesDirectory);

            try
            {
                MoveDirectoryIfExists($@"{IOHelper.GetWebRoot()}\images", ImagesDirectory);
                MoveDirectoryIfExists($@"{IOHelper.GetWebRoot()}\EarcMods", ModThumbnailWebDirectory);
                MoveDirectoryIfExists(
                    $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Temp\Flagrum\cache",
                    CacheDirectory);
                MoveDirectoryIfExists(
                    $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Temp\Flagrum\staging",
                    ModStagingDirectory);
            }
            catch { }

            DidMigrateThisSession = true;
        }
    }

    private void MoveDirectoryIfExists(string oldPath, string newPath)
    {
        if (Directory.Exists(oldPath))
        {
            var tempPath = $@"{FlagrumDirectory}\temp";
            Directory.Move(oldPath, tempPath);
            IOHelper.EnsureDirectoryExists(Path.GetDirectoryName(newPath));
            Directory.Move(tempPath, newPath);
        }
    }

    private void DeleteDirectoryIfExists(IProfileViewModel profile, string path)
    {
        path = path.Replace(Current.Id.ToString(), profile.Id);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}