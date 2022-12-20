namespace Flagrum.Console.Ps4.Porting;

public static class Ps4PorterConfiguration
{
    public const string GamePath = @"C:\Modding\Chocomog\Final Fantasy XV - RAW PS4\dummy.exe";
    public const string ReleaseGamePath = @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\ffxv_s.exe";
    public const string OutputDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas";
    public const string StagingDirectory = @"C:\Modding\Chocomog\Testing";

    private static string _gameDirectory;
    public static string GameDirectory => _gameDirectory ??= GamePath[..GamePath.LastIndexOf('\\')] + "\\datas";
}