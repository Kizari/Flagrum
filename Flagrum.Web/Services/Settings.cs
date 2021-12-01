using System;
using System.IO;

namespace Flagrum.Web.Services;

public class Settings
{
    private string _binmodListPath;
    private string _flagrumDirectory;
    private string _modDirectory;
    private string _tempDirectory;
    private string _workshopDirectory;

    public string TempDirectory
    {
        get
        {
            if (_tempDirectory == null)
            {
                _tempDirectory = $"{FlagrumDirectory}\\tmp";

                if (!Directory.Exists(_tempDirectory))
                {
                    Directory.CreateDirectory(_tempDirectory);
                }
            }

            return _tempDirectory;
        }
    }

    public string FlagrumDirectory
    {
        get
        {
            if (_flagrumDirectory == null)
            {
                _flagrumDirectory =
                    $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Flagrum";

                if (!Directory.Exists(_flagrumDirectory))
                {
                    Directory.CreateDirectory(_flagrumDirectory);
                }
            }

            return _flagrumDirectory;
        }
    }

    public string ModDirectory
    {
        get
        {
            if (_modDirectory == null)
            {
                var basePath =
                    $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Documents\\My Games\\FINAL FANTASY XV\\Steam";
                foreach (var directory in Directory.EnumerateDirectories(basePath))
                {
                    foreach (var subdirectory in Directory.EnumerateDirectories(directory))
                    {
                        if (subdirectory.EndsWith("\\mod"))
                        {
                            _modDirectory = subdirectory;
                        }
                    }
                }
            }

            return _modDirectory;
        }
    }

    // TODO: Probably isn't always at this path
    public string WorkshopDirectory => _workshopDirectory ??=
        $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\\Steam\\steamapps\\workshop\\content\\637650";

    public string BinmodListPath => _binmodListPath ??=
        $"{ModDirectory}\\binmod.list";

    public string GetTempFile()
    {
        return $"{TempDirectory}\\{Guid.NewGuid().ToString()}.tmp";
    }
}