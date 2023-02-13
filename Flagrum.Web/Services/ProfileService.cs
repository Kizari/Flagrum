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
using Flagrum.Web.Services.Vendor.Patreon;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Flagrum.Web.Services;

public class ProfileService : IDisposable
{
    private const string Steam32 = @"SOFTWARE\VALVE\Steam";
    private const string Steam64 = @"SOFTWARE\Wow6432Node\Valve\Steam";
    private static readonly object _persistLock = new();
    private readonly EbonyArchiveManager _archiveManager = new();

    private FlagrumProfile _current;

    private string _lastVersionNotes;

    public ProfileService()
    {
        // Setup profiles
        if (File.Exists(ProfilesPath))
        {
            string json;

            lock (_persistLock)
            {
                json = File.ReadAllText(ProfilesPath);
            }

            var container = JsonConvert.DeserializeObject<FlagrumProfileContainer>(json)!;
            Profiles = container.Profiles;
            _lastVersionNotes = container.LastVersionNotes;
            PatreonToken = container.PatreonToken;
            PatreonRefreshToken = container.PatreonRefreshToken;
            PatreonTokenExpiry = container.PatreonTokenExpiry;
            _current = Profiles.First(p => p.Id == container.Current);
        }
        else
        {
            var container = FlagrumProfileContainer.GetDefault();
            Profiles = container.Profiles;
            Current = Profiles.First(p => p.Id == container.Current);
            Persist();
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

    public List<FlagrumProfile> Profiles { get; }

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
    
    public FlagrumProfile Current
    {
        get => _current;
        private set
        {
            _current = value;
            Persist();
        }
    }

    public string LastVersionNotes
    {
        get => _lastVersionNotes;
        set
        {
            if (_lastVersionNotes != value)
            {
                _lastVersionNotes = value;
                Persist();
            }
        }
    }

    public string PatreonToken { get; private set; }
    public string PatreonRefreshToken { get; private set; }
    public DateTime PatreonTokenExpiry { get; private set; }

    public bool DidMigrateThisSession { get; private set; }

    public bool IsReady
    {
        get
        {
            if (Current?.Type == LuminousGame.FFXV && GamePath?.Contains("forspoken", StringComparison.OrdinalIgnoreCase) == true)
            {
                return false;
            }
            
            return GamePath != null && (Current?.Type == LuminousGame.Forspoken || BinmodListPath != null);
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

    public string ProfilesPath => $@"{FlagrumDirectory}\profiles.json";

    public string SteamExePath { get; private set; }

    public string ModDirectory => $"{Path.GetDirectoryName(BinmodListPath)}";
    public string WorkshopDirectory => $@"{Path.GetDirectoryName(WorkshopPath)}\content\637650";
    public string GameDataDirectory => $@"{Path.GetDirectoryName(Current.GamePath)}\datas";

    public string WorkshopPath
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

    public string GamePath
    {
        get => Current.GamePath;
        set
        {
            Current.GamePath = value;
            Persist();
        }
    }

    public string BinmodListPath
    {
        get => Current.BinmodListPath;
        set
        {
            if (Current.BinmodListPath != value)
            {
                Current.BinmodListPath = value;
                Persist();
            }
        }
    }

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    public void Dispose()
    {
        _archiveManager?.Dispose();
    }

    public void SetPatreonToken(string token, string refreshToken, DateTime expiry)
    {
        PatreonToken = token;
        PatreonRefreshToken = refreshToken;
        PatreonTokenExpiry = expiry;
        Persist();
    }

    public void SetCurrentById(string id)
    {
        Current = Profiles.First(p => p.Id == id);
    }

    public void Add(FlagrumProfile profile)
    {
        Profiles.Add(profile);
        Persist();
    }

    public void Update(FlagrumProfile profile)
    {
        var match = Profiles.First(p => p.Id == profile.Id);
        match.Name = profile.Name;
        match.Type = profile.Type;
        Persist();
    }

    public void Delete(FlagrumProfile profile)
    {
        Profiles.Remove(profile);
        Persist();

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

    private void Persist()
    {
        var container = new FlagrumProfileContainer
        {
            Current = Current.Id,
            Profiles = Profiles,
            LastVersionNotes = LastVersionNotes,
            PatreonToken = PatreonToken,
            PatreonRefreshToken = PatreonRefreshToken,
            PatreonTokenExpiry = PatreonTokenExpiry
        };

        lock (_persistLock)
        {
            File.WriteAllText(ProfilesPath, JsonConvert.SerializeObject(container));
        }
    }

    private void TrySetDefaultGamePath()
    {
        if (GamePath == null && Current.Type == LuminousGame.FFXV)
        {
            var gamePath =
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Steam\steamapps\common\Final Fantasy XV\ffxv_s.exe";

            if (File.Exists(gamePath))
            {
                GamePath = gamePath;
            }
        }
        else if (GamePath == null && Current.Type == LuminousGame.Forspoken)
        {
            var gamePath =
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Steam\steamapps\common\FORSPOKEN\FORSPOKEN.exe";

            if (File.Exists(gamePath))
            {
                GamePath = gamePath;
            }
        }
    }

    private void TrySetDefaultBinmodListPath()
    {
        if (BinmodListPath == null && Current.Type == LuminousGame.FFXV)
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
                                BinmodListPath = binmodList;
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