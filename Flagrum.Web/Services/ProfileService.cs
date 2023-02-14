using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Types;
using Flagrum.Services.Vendor.Patreon;
using Flagrum.Web.Features.Settings.Data;
using Flagrum.Web.Persistence.Configuration;
using Flagrum.Web.Persistence.Configuration.Entities;
using Flagrum.Web.Services.Vendor.Patreon;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Flagrum.Web.Services;

public class ProfileService : IDisposable
{
    private const string Steam32 = @"SOFTWARE\VALVE\Steam";
    private const string Steam64 = @"SOFTWARE\Wow6432Node\Valve\Steam";
    private readonly EbonyArchiveManager _archiveManager = new();
    private readonly ConfigurationDbContext _configuration;

    public ProfileService()
    {
        _configuration = new ConfigurationDbContext();
        
        // Migrate from profiles.json to config.fcg
        if (File.Exists(ProfilesPath))
        {
            var json = File.ReadAllText(ProfilesPath);
            var container = JsonConvert.DeserializeObject<FlagrumProfileContainer>(json)!;

            var profiles = container.Profiles
                .Select(p => new ProfileEntity
                {
                    Id = p.Id,
                    Type = p.Type,
                    Name = p.Name,
                    GamePath = p.GamePath,
                    BinmodListPath = p.BinmodListPath
                });

            _configuration.ProfileEntities.AddRange(profiles);
            _configuration.SaveChanges();
            
            _configuration.SetString(ConfigurationKey.CurrentProfile, container.Current);
            _configuration.SetString(ConfigurationKey.LatestVersionNotes, container.LastVersionNotes);
            _configuration.SetString(ConfigurationKey.PatreonToken, container.PatreonToken);
            _configuration.SetString(ConfigurationKey.PatreonRefreshToken, container.PatreonRefreshToken);
            _configuration.SetDateTime(ConfigurationKey.PatreonTokenExpiry, container.PatreonTokenExpiry);
            
            File.Delete(ProfilesPath);
        }

        // Generate default profiles if none exist
        var needsMigrating = false;
        if (!_configuration.ProfileEntities.Any())
        {
            var defaultProfileId = Guid.NewGuid().ToString();
            
            _configuration.ProfileEntities.AddRange(new List<ProfileEntity>
            {
                new()
                {
                    Id = defaultProfileId,
                    Name = "Final Fantasy XV Windows Edition",
                    Type = LuminousGame.FFXV
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Forspoken",
                    Type = LuminousGame.Forspoken
                }
            });

            _configuration.SaveChanges();
            _configuration.SetString(ConfigurationKey.CurrentProfile, defaultProfileId);
            needsMigrating = true;
        }
        
        // Setup current profile
        var id = _configuration.GetString(ConfigurationKey.CurrentProfile);
        var current = _configuration.ProfileEntities.Find(id);
        Current = new ConcurrentProfileEntity(_configuration, current);

        if (needsMigrating)
        {
            MigratePreProfilesData();
        }

        // Ensure required directories exist
        IOHelper.EnsureDirectoryExists(FlagrumDirectory);
        IOHelper.EnsureDirectoriesExistForFilePath(DatabasePath);
        IOHelper.EnsureDirectoryExists(ImagesDirectory);
        IOHelper.EnsureDirectoryExists(EarcImagesDirectory);
        IOHelper.EnsureDirectoryExists(CacheDirectory);
        IOHelper.EnsureDirectoryExists(ModStagingDirectory);
        IOHelper.EnsureDirectoryExists(EarcModThumbnailDirectory);
        IOHelper.EnsureDirectoryExists(EarcModsDirectory);
        IOHelper.EnsureDirectoryExists(EarcModBackupsDirectory);

        // Try setup other paths automatically
        TrySetDefaultGamePath();
        TrySetDefaultBinmodListPath();
        TrySetSteamExePath();
    }

    public List<FlagrumProfile> Profiles => _configuration.ProfileEntities.Select(p => new FlagrumProfile
    {
        Id = p.Id,
        Type = p.Type,
        Name = p.Name,
        GamePath = p.GamePath,
        BinmodListPath = p.BinmodListPath
    }).ToList();

    #if DEBUG
    public bool IsEarlyAccessEnabled
    {
        get => true;
        set
        {
            
        }
    }
    #else
    public bool IsEarlyAccessEnabled { get; set; }
    #endif

    public ConcurrentProfileEntity Current { get; }

    public string LastVersionNotes
    {
        get => _configuration.GetString(ConfigurationKey.LatestVersionNotes);
        set => _configuration.SetString(ConfigurationKey.LatestVersionNotes, value);
    }

    public string PatreonToken => _configuration.GetString(ConfigurationKey.PatreonToken);
    private string PatreonRefreshToken => _configuration.GetString(ConfigurationKey.PatreonRefreshToken);
    private DateTime PatreonTokenExpiry => _configuration.GetDateTime(ConfigurationKey.PatreonTokenExpiry);

    public bool DidMigrateThisSession { get; private set; }

    public bool IsReady
    {
        get
        {
            if (Current?.Type == LuminousGame.FFXV && Current.GamePath?.Contains("forspoken", StringComparison.OrdinalIgnoreCase) == true)
            {
                return false;
            }
            
            return Current?.GamePath != null && (Current?.Type == LuminousGame.Forspoken || Current?.BinmodListPath != null);
        }
    }

    public string FlagrumDirectory =>
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Flagrum";

    public string DatabasePath => $@"{FlagrumDirectory}\profiles\{Current.Id}\flagrum.db";
    public string ImagesDirectory => $@"{IOHelper.GetWebRoot()}\images\{Current.Id}";
    public string EarcImagesDirectory => $@"{IOHelper.GetWebRoot()}\EarcMods\{Current.Id}";

    public string CacheDirectory =>
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Temp\Flagrum\{Current.Id}\cache";

    public string ModStagingDirectory =>
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Temp\Flagrum\{Current.Id}\staging";

    public string EarcModsDirectory => $@"{FlagrumDirectory}\earc\{Current.Id}";
    public string EarcModThumbnailDirectory => $@"{EarcModsDirectory}\thumbnails";
    public string EarcModBackupsDirectory => $@"{EarcModsDirectory}\backup";

    private string ProfilesPath => $@"{FlagrumDirectory}\profiles.json";

    public string SteamExePath { get; private set; }

    public string ModDirectory => $"{Path.GetDirectoryName(Current.BinmodListPath)}";
    public string WorkshopDirectory => $@"{Path.GetDirectoryName(WorkshopPath)}\content\637650";
    public string GameDataDirectory => $@"{Path.GetDirectoryName(Current.GamePath)}\datas";

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

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    public void Dispose()
    {
        _archiveManager?.Dispose();
        _configuration?.Dispose();
    }

    public void SetPatreonToken(string token, string refreshToken, DateTime expiry)
    {
        _configuration.SetString(ConfigurationKey.PatreonToken, token);
        _configuration.SetString(ConfigurationKey.PatreonRefreshToken, refreshToken);
        _configuration.SetDateTime(ConfigurationKey.PatreonTokenExpiry, expiry);
    }

    public void SetCurrentProfile(string id)
    {
        _configuration.SetString(ConfigurationKey.CurrentProfile, id);
    }

    public void Add(FlagrumProfile profile)
    {
        _configuration.ProfileEntities.Add(new ProfileEntity
        {
            Id = profile.Id,
            Type = profile.Type,
            Name = profile.Name,
            GamePath = profile.GamePath,
            BinmodListPath = profile.BinmodListPath,
        });

        _configuration.SaveChanges();
    }

    public void Update(FlagrumProfile profile)
    {
        _configuration.ProfileEntities.Update(new ProfileEntity
        {
            Id = profile.Id,
            Type = profile.Type,
            Name = profile.Name,
            GamePath = profile.GamePath,
            BinmodListPath = profile.BinmodListPath,
        });
        
        _configuration.SaveChanges();
    }

    public void Delete(FlagrumProfile profile)
    {
        var profileEntity = _configuration.ProfileEntities.Find(profile.Id)!;
        _configuration.Remove(profileEntity);
        _configuration.SaveChanges();

        var databasePath = DatabasePath.Replace(Current.Id, profile.Id);
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
            Directory.Delete(Path.GetDirectoryName(databasePath)!);
        }

        DeleteDirectoryIfExists(profile, ImagesDirectory);
        DeleteDirectoryIfExists(profile, EarcImagesDirectory);
        DeleteDirectoryIfExists(profile, CacheDirectory);
        DeleteDirectoryIfExists(profile, ModStagingDirectory);
        DeleteDirectoryIfExists(profile, EarcModsDirectory);
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

    public bool IsGameRunning()
    {
        if (!string.IsNullOrWhiteSpace(Current.GamePath))
        {
            var directory = Path.GetDirectoryName(Current.GamePath);
            var fileName = Path.GetFileNameWithoutExtension(Current.GamePath);

            if (directory != null && fileName != null)
            {
                return Process.GetProcessesByName(fileName)
                    .Any(p => p.MainModule?.FileName?.StartsWith(directory, StringComparison.OrdinalIgnoreCase) ==
                              true);
            }
        }

        return false;
    }

    private void TrySetDefaultGamePath()
    {
        if (Current.GamePath == null && Current.Type == LuminousGame.FFXV)
        {
            var gamePath =
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Steam\steamapps\common\Final Fantasy XV\ffxv_s.exe";

            if (File.Exists(gamePath))
            {
                Current.GamePath = gamePath;
            }
        }
        else if (Current.GamePath == null && Current.Type == LuminousGame.Forspoken)
        {
            var gamePath =
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Steam\steamapps\common\FORSPOKEN\FORSPOKEN.exe";

            if (File.Exists(gamePath))
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

    public async Task<PatreonStatus> CheckPatreonStatus()
    {
        if (PatreonToken != null)
        {
            // Token is expired, return status so the caller can notify the user
            if ((PatreonTokenExpiry - DateTime.Now).TotalMinutes < 1)
            {
                // Clear the Patreon auth information as it is no use anymore
                // This will prevent these calls and warnings recurring as well
                SetPatreonToken(null, null, default);
                return PatreonStatus.TokenExpired;
            }
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PatreonToken);

            try
            {
                // Refresh token if it is less than two weeks from expiry to prevent the user getting signed out
                if ((PatreonTokenExpiry - DateTime.Now).TotalDays < 14)
                {
                    var refreshUri = $"https://ek19cc.deta.dev/patreon/refreshtoken?refreshToken={PatreonRefreshToken}";
                    var refreshResponse = await client.GetFromJsonAsync<PatreonTokenResponse>(refreshUri);
                    SetPatreonToken(refreshResponse!.AccessToken, refreshResponse.RefreshToken,
                        DateTime.Now.AddSeconds(refreshResponse.SecondsUntilExpiry));
                }
            }
            catch
            {
                // This can try again another time, it's not important
            }

            try
            {
                const string uri = "https://www.patreon.com/api/oauth2/v2/identity?include=memberships&fields%5Bmember%5D=currently_entitled_amount_cents,patron_status";
                var response = await client.GetFromJsonAsync<PatreonIdentityResponse>(uri);

                if (response?.Memberships?.Any(m => m.Attributes.PatronStatus == "active_patron") == true)
                {
                    IsEarlyAccessEnabled = true;
                    return PatreonStatus.AuthorisationStateChanged;
                }
            }
            catch
            {
                return PatreonStatus.AuthorisationFailed;
            }
        }

        return PatreonStatus.AllOkay;
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
            MoveDirectoryIfExists($@"{FlagrumDirectory}\earc", EarcModsDirectory);

            try
            {
                MoveDirectoryIfExists($@"{IOHelper.GetWebRoot()}\images", ImagesDirectory);
                MoveDirectoryIfExists($@"{IOHelper.GetWebRoot()}\EarcMods", EarcImagesDirectory);
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

    private void DeleteDirectoryIfExists(FlagrumProfile profile, string path)
    {
        path = path.Replace(Current.Id, profile.Id);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}