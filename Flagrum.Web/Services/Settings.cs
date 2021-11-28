using System;
using System.IO;

namespace Flagrum.Web.Services;

public class Settings
{
    private string _binmodListPath;
    private string _modDirectory;

    public string ModDirectory
    {
        get
        {
            if (_modDirectory == null)
            {
                var basePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Documents\\My Games\\FINAL FANTASY XV\\Steam";
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

    public string BinmodListPath => _binmodListPath ??=
        $"{ModDirectory}\\binmod.list";
}