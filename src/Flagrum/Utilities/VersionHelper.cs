using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Timers;
using Flagrum.Core.Utilities.Extensions;
using Microsoft.Win32;

namespace Flagrum.Utilities;

public class VersionHelper
{
    private const string VersionKey =
        @"HKEY_CURRENT_USER\Software\Flagrum\CLASSES\CLSID\{926FEA7F-C202-4984-A67C-23FB540BE8D3}";
    private const string VersionTimeKey =
        @"HKEY_CURRENT_USER\Software\Flagrum\CLASSES\CLSID\{8D96FAAC-9E52-4C08-BE73-6BCD845F834D}";

    private readonly Timer _longTimer = new(TimeSpan.FromHours(24));

    private readonly Timer _timer = new(TimeSpan.FromSeconds(60));

    public VersionHelper()
    {
        _longTimer.Elapsed += OnLongTimerElapsed;
        _longTimer.Start();

        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
        OnTimerElapsed(null, null);
    }

    public bool IsCurrent()
    {
        var selfVersion = typeof(VersionHelper).Assembly.GetName().Version!;
        var registryVersionString = GetVersionFromRegistry();
        if (registryVersionString != null)
        {
            var time = GetVersionTimeFromRegistry();
            
            // Allow a 6-hour grace period (see https://github.com/Kizari/Flagrum/issues/179)
            if (time.HasValue && (DateTime.UtcNow - time.Value).TotalHours <= 6)
            {
                return true;
            }
            
            var registryVersion = StringToVersion(registryVersionString);
            return selfVersion >= registryVersion;
        }

        // Couldn't verify, so let the user continue
        return true;
    }

    private void OnLongTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // It has been 24 hours, start the short timer again to get a new read on the latest version
        _timer.Stop();
        _timer.Start();
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs? e)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            var response =
                await client.GetFromJsonAsync<GitHubLatestResponse>(
                    "https://api.github.com/repos/Kizari/Flagrum/releases/latest");
            if (response != null)
            {
                var current = GetVersionFromRegistry();
                if (current != response.TagName)
                {
                    SetVersionInRegistry(response.TagName);
                    SetVersionTimeInRegistry(DateTime.UtcNow);
                }
                
                _timer.Stop();
            }
        }
        catch
        {
            // Couldn't get the version, try again in a minute
        }
    }

    private void SetVersionInRegistry(string version)
    {
        Registry.SetValue(VersionKey, "", version.ToBase64());
    }

    private string? GetVersionFromRegistry()
    {
        var result = (string?)Registry.GetValue(VersionKey, "", null);
        return result?.FromBase64();
    }

    private void SetVersionTimeInRegistry(DateTime time)
    {
        Registry.SetValue(VersionTimeKey, "", time.Ticks.ToString());
    }

    private DateTime? GetVersionTimeFromRegistry()
    {
        var time = (string?)Registry.GetValue(VersionTimeKey, "", null);
        if (time != null && long.TryParse(time, out var ticks))
        {
            return new DateTime(ticks);
        }

        return null;
    }

    private Version StringToVersion(string versionString)
    {
        var tokens = versionString[1..].Split('.');
        return new Version(int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(tokens[2]));
    }
}