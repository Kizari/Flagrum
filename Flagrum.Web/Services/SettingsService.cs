using System;
using System.IO;
using System.Linq;
using Flagrum.Core.Utilities;
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

        SettingsPath = $"{FlagrumDirectory}\\settings.json";

        if (File.Exists(SettingsPath))
        {
            var json = File.ReadAllText(SettingsPath);
            var settingsData = JsonConvert.DeserializeObject<SettingsData>(json);
            GamePath = settingsData.GamePath;
            BinmodListPath = settingsData.BinmodListPath;
            WorkshopPath = settingsData.WorkshopPath;
            LastVersionNotes = settingsData.LastVersionNotes;
        }

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

        if (WorkshopPath == null)
        {
            var workshopPath =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\\Steam\\steamapps\\workshop\\appworkshop_637650.acf";

            if (File.Exists(workshopPath))
            {
                WorkshopPath = workshopPath;
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
        Save();
    }

    public string SteamExePath { get; set; }

    public bool IsReady { get; set; }
    public string FlagrumDirectory { get; }
    public string SettingsPath { get; }
    public string TempDirectory { get; }

    public string GamePath { get; private set; }
    public string BinmodListPath { get; private set; }
    public string WorkshopPath { get; private set; }
    public string LastVersionNotes { get; private set; }

    public string ModDirectory => $"{Path.GetDirectoryName(BinmodListPath)}";
    public string WorkshopDirectory => $"{Path.GetDirectoryName(WorkshopPath)}\\content\\637650";
    public string GameDataDirectory => $"{Path.GetDirectoryName(GamePath)}\\datas";
    public string ReplacementsFilePath => $"{FlagrumDirectory}\\replacements.json";
    public string StatePath => $"{FlagrumDirectory}\\state.json";

    public void SetGamePath(string path)
    {
        GamePath = path;
        Save();
        CheckIsReady();
    }

    public void SetBinmodListPath(string path)
    {
        BinmodListPath = path;
        Save();
        CheckIsReady();
    }

    public void SetWorkshopPath(string path)
    {
        WorkshopPath = path;
        Save();
        CheckIsReady();
    }

    public void SetLastVersionNotes(string version)
    {
        LastVersionNotes = version;
        Save();
    }

    private void CheckIsReady()
    {
        if (!IsReady)
        {
            IsReady = GamePath != null && BinmodListPath != null && WorkshopPath != null;
        }
    }

    public void Save()
    {
        var settingsData = new SettingsData
        {
            GamePath = GamePath,
            WorkshopPath = WorkshopPath,
            BinmodListPath = BinmodListPath,
            LastVersionNotes = LastVersionNotes
        };

        File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(settingsData));
    }

    public string GetTempFile()
    {
        return $"{TempDirectory}\\{Guid.NewGuid().ToString()}.tmp";
    }
}