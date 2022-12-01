using System;
using System.IO;
using System.Linq;
using Flagrum.Core.Utilities;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Flagrum.Web.Services;

public class SettingsData
{
    public string GamePath { get; set; }
    public string BinmodListPath { get; set; }
    public string WorkshopPath { get; set; }
    public string LastVersionNotes { get; set; }
}

public class SettingsService
{
    private const string Steam32 = @"SOFTWARE\VALVE\Steam";
    private const string Steam64 = @"SOFTWARE\Wow6432Node\Valve\Steam";

    public SettingsService()
    {
        var imagesDirectory = $"{IOHelper.GetWebRoot()}\\images";
        if (!Directory.Exists(imagesDirectory))
        {
            Directory.CreateDirectory(imagesDirectory);
        }

        FlagrumDirectory =
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Flagrum";

        if (!Directory.Exists(FlagrumDirectory))
        {
            Directory.CreateDirectory(FlagrumDirectory);
        }

        CacheDirectory =
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Temp\Flagrum\cache";

        if (!Directory.Exists(CacheDirectory))
        {
            Directory.CreateDirectory(CacheDirectory);
        }
        
        EarcModThumbnailDirectory = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Flagrum\earc\thumbnails";

        if (!Directory.Exists(EarcModThumbnailDirectory))
        {
            Directory.CreateDirectory(EarcModThumbnailDirectory);
        }

        ModStagingDirectory =
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Temp\Flagrum\staging";

        if (!Directory.Exists(ModStagingDirectory))
        {
            Directory.CreateDirectory(ModStagingDirectory);
        }

        EarcModsDirectory = $@"{FlagrumDirectory}\earc";
        if (!Directory.Exists(EarcModsDirectory))
        {
            Directory.CreateDirectory(EarcModsDirectory);
        }

        var earcImagesDirectory = $@"{IOHelper.GetWebRoot()}\EarcMods";
        if (!Directory.Exists(earcImagesDirectory))
        {
            Directory.CreateDirectory(earcImagesDirectory);
        }

        SettingsPath = $"{FlagrumDirectory}\\settings.json";
        using var context = new FlagrumDbContext();

        // Migrate old settings file into local DB
        if (File.Exists(SettingsPath))
        {
            var json = File.ReadAllText(SettingsPath);
            var settingsData = JsonConvert.DeserializeObject<SettingsData>(json)!;

            context.SetString(StateKey.GamePath, settingsData.GamePath);
            context.SetString(StateKey.BinmodListPath, settingsData.BinmodListPath);
            context.SetString(StateKey.LastSeenVersionNotes, settingsData.LastVersionNotes);

            File.Delete(SettingsPath);
        }

        // Read persisted settings
        GamePath = context.GetString(StateKey.GamePath);
        BinmodListPath = context.GetString(StateKey.BinmodListPath);
        LastVersionNotes = context.GetString(StateKey.LastSeenVersionNotes);

        TempDirectory = $"{FlagrumDirectory}\\tmp";
        if (!Directory.Exists(TempDirectory))
        {
            Directory.CreateDirectory(TempDirectory);
        }

        if (BinmodListPath == null)
        {
            var basePath =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Documents\\My Games\\FINAL FANTASY XV\\Steam";

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

        if (GamePath == null)
        {
            var gamePath =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\\Steam\\steamapps\\common\\Final Fantasy XV\\ffxv_s.exe";

            if (File.Exists(gamePath))
            {
                GamePath = gamePath;
            }
        }

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

        CheckIsReady();
    }

    public string SteamExePath { get; set; }

    public bool IsReady { get; set; }
    public string FlagrumDirectory { get; }
    public string CacheDirectory { get; }
    public string EarcModThumbnailDirectory { get; }
    public string ModStagingDirectory { get; }
    public string EarcModsDirectory { get; }
    public string SettingsPath { get; }
    public string TempDirectory { get; }

    public string GamePath { get; private set; }
    public string BinmodListPath { get; private set; }

    public string WorkshopPath
    {
        get
        {
            if (string.IsNullOrEmpty(GamePath))
            {
                return null;
            }

            var ffxvDirectory = Path.GetDirectoryName(GamePath);
            var commonFolder = Path.GetDirectoryName(ffxvDirectory);
            var steamAppsFolder = Path.GetDirectoryName(commonFolder);

            return $@"{steamAppsFolder}\workshop\appworkshop_637650.acf";
        }
    }

    public string LastVersionNotes { get; private set; }

    public string ModDirectory => $"{Path.GetDirectoryName(BinmodListPath)}";
    public string WorkshopDirectory => $"{Path.GetDirectoryName(WorkshopPath)}\\content\\637650";
    public string GameDataDirectory => $"{Path.GetDirectoryName(GamePath)}\\datas";
    public string StatePath => $"{FlagrumDirectory}\\state.json";

    public void SetGamePath(string path)
    {
        GamePath = path;
        using var context = new FlagrumDbContext();
        context.SetString(StateKey.GamePath, path);
        CheckIsReady();
    }

    public void SetBinmodListPath(string path)
    {
        BinmodListPath = path;
        using var context = new FlagrumDbContext();
        context.SetString(StateKey.BinmodListPath, path);
        CheckIsReady();
    }

    public void SetLastVersionNotes(string version)
    {
        LastVersionNotes = version;
        using var context = new FlagrumDbContext();
        context.SetString(StateKey.LastSeenVersionNotes, version);
    }

    private void CheckIsReady()
    {
        if (!IsReady)
        {
            IsReady = GamePath != null && BinmodListPath != null && WorkshopPath != null;
        }
    }
}