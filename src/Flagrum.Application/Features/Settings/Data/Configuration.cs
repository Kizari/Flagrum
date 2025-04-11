using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Abstractions;
using Flagrum.Core.Persistence;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Generators;
using Flagrum.Application.Legacy.Migration;
using Flagrum.Application.Utilities;
using Injectio.Attributes;
using MemoryPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using ZstdSharp;

namespace Flagrum.Application.Features.Settings.Data;

[RegisterScoped]
public partial class DummyService;

[MemoryPackable]
public partial class Configuration : IConfiguration
{
    private const string ClientIdKey =
        @"HKEY_CURRENT_USER\Software\Lucent\CLASSES\CLSID\{D81BA2C5-A176-4EE5-B8CB-3AAAE258AC8A}";
    
    [MemoryPackInclude] [ConcurrentProperty]
    private Guid _clientId;

    [MemoryPackInclude] [ConcurrentProperty]
    private Guid _currentProfile;

    [MemoryPackInclude] [ConcurrentProperty]
    private string _latestVersionNotes;

    [MemoryPackInclude] [ConcurrentProperty]
    private Guid _giftToken;

    [MemoryPackInclude] [ConcurrentProperty]
    private string _patreonToken;

    [MemoryPackInclude] [ConcurrentProperty]
    private string _patreonRefreshToken;

    [MemoryPackInclude] [ConcurrentProperty]
    private DateTime _patreonTokenExpiry;

    [MemoryPackConstructor]
    public Configuration() { }
    
    public Configuration(DummyService dummyService)
    {
        // Migrate from the old configuration DB if applicable
        ConfigurationMigration.RunAsync().AwaitSynchronous();

        // Load the persisted configuration from disk
        if (File.Exists(Path))
        {
            var buffer = File.ReadAllBytes(Path);
            var decompressor = new Decompressor();
            var configuration = this;
            MemoryPackSerializer.Deserialize(decompressor.Unwrap(buffer), ref configuration,
                MemoryPackSerializerOptions.Utf8);
        }

        // Ensure a client ID exists
        if (ClientId == Guid.Empty)
        {
            ClientId = Guid.NewGuid();
        }

        // Generate default profiles if none exist
        if (!Profiles.Any())
        {
            var defaultProfileId = Guid.NewGuid();

            Profiles.AddRange(new List<Profile>
            {
                new(this)
                {
                    Id = defaultProfileId,
                    Name = "Final Fantasy XV Windows Edition",
                    Type = LuminousGame.FFXV
                },
                new(this)
                {
                    Id = Guid.NewGuid(),
                    Name = "Forspoken",
                    Type = LuminousGame.Forspoken
                }
            });

            CurrentProfile = defaultProfileId;
            ShouldMigratePreProfilesData = true;
        }
    }

    [MemoryPackIgnore]
    public bool ShouldMigratePreProfilesData { get; set; }

    public static string Path => System.IO.Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "configuration.fcg");

    public List<IProfile> Profiles { get; set; } = [];

    [MemoryPackInclude] private Dictionary<StateKey, string> _statePairs = new();
    [MemoryPackInclude] private HashSet<Guid> _migrations = [];

    [MemoryPackInclude] [ConcurrentProperty]
    private AuthenticationType _authenticationType;
    
    [MemoryPackIgnore]
    public Guid LucentClientId
    {
        get
        {
            var clientIdString = (string)Registry.GetValue(ClientIdKey, "", null);
            if (clientIdString == null)
            {
                clientIdString = Guid.NewGuid().ToString();
                Registry.SetValue(ClientIdKey, "", clientIdString);
            }

            return new Guid(clientIdString);
        }
    }

    public void AddProfile(IProfile profile)
    {
        lock (_lock)
        {
            Profiles.Add(profile);
            Save();
        }
    }

    public void UpdateProfile(IProfile profile)
    {
        lock (_lock)
        {
            var match = Profiles.First(p => p.Id == profile.Id);
            var index = Profiles.IndexOf(match);
            Profiles[index] = profile;
            Save();
        }
    }

    public void DeleteProfile(Guid id)
    {
        lock (_lock)
        {
            var match = Profiles.First(p => p.Id == id);
            Profiles.Remove(match);
            Save();
        }
    }

    public void Save()
    {
        Repository.Save(this, Path);
    }

    [MemoryPackOnDeserialized]
    private void OnDeserialized()
    {
        foreach (var profile in Profiles)
        {
            profile.SetConfiguration(this);
        }
    }

    public bool ContainsKey(StateKey key)
    {
        return _statePairs.ContainsKey(key);
    }

    public TValue Get<TValue>(StateKey key)
    {
        lock (_lock)
        {
            _statePairs.TryGetValue(key, out var value);

            // If the state pair isn't in the dictionary, return -1 for integers, or the default for TValue otherwise
            if (value == null)
            {
                return typeof(TValue) == typeof(int) ? (TValue)(object)-1 : default;
            }

            // Enumerations need to be handled differently, as those that are unsigned won't cast nicely
            if (typeof(TValue).IsEnum)
            {
                var numeric = Convert.ToInt32(value);
                return (TValue)Enum.ToObject(typeof(TValue), numeric);
            }

            // Convert the string to the required data type
            return typeof(TValue) == typeof(string)
                ? (TValue)(object)value
                : (TValue)Convert.ChangeType(value, typeof(TValue));
        }
    }

    public void Set<TValue>(StateKey key, TValue value)
    {
        lock (_lock)
        {
            if (typeof(TValue).IsEnum)
            {
                // Need to store enum as its integer representation so enum members can be renamed during refactors
                _statePairs[key] = Convert.ToInt32(value).ToString();
            }
            else
            {
                _statePairs[key] = value?.ToString();
            }

            Save();
        }
    }
    
    public bool HasMigrated(Guid migration) => _migrations.Contains(migration);

    public void SetMigrated(Guid migration)
    {
        _migrations.Add(migration);
        Save();
    }

    public void SetMigratedNoSave(IEnumerable<Guid> migrations) => _migrations.AddRange(migrations);
    
    /// <summary>
    /// Should be run when Flagrum is first installed to prevent previous version migrations from running
    /// </summary>
    public void OnFreshInstall(IEnumerable<Guid> applicationSteps, IEnumerable<Guid> profileSteps)
    {
        // Mark all application-level migrations as completed
        _migrations.AddRange(applicationSteps);
        
        // Mark all profile-level migrations as completed for every profile
        foreach (var profile in Profiles)
        {
            profile.SetMigratedNoSave(profileSteps);
        }
        
        Save();
    }

    public sealed class Formatter : MemoryPackFormatter<IConfiguration>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, 
            scoped ref IConfiguration value)
        {
            writer.WritePackable((Configuration)value);
        }
        
        public override void Deserialize(ref MemoryPackReader reader, scoped ref IConfiguration value)
        {
            value = reader.ReadPackable<Configuration>();
        }
    }
}