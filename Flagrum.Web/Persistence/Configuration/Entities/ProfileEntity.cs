using System.ComponentModel.DataAnnotations;
using System.Linq;
using Flagrum.Core.Utilities.Types;

namespace Flagrum.Web.Persistence.Configuration.Entities;

public class ProfileEntity
{
    public string Id { get; set; }
    public LuminousGame Type { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; }

    public string GamePath { get; set; }
    public string BinmodListPath { get; set; }
}

public class ConcurrentProfileEntity
{
    private readonly object _lock = new();
    private readonly ConfigurationDbContext _configuration;
    
    public ConcurrentProfileEntity(ConfigurationDbContext configuration, ProfileEntity profileEntity)
    {
        _configuration = configuration;

        _gamePath = profileEntity.GamePath;
        _binmodListPath = profileEntity.BinmodListPath;

        Id = profileEntity.Id;
        Type = profileEntity.Type;
    }

    public string Id { get; }
    public LuminousGame Type { get; }
    
    
    private string _gamePath;

    public string GamePath 
    {
        get
        {
            lock (_lock)
            {
                return _gamePath;
            }
        }
        
        set
        {
            lock (_lock)
            {
                _gamePath = value;
                var profile = _configuration.ProfileEntities.Find(Id)!;
                profile.GamePath = _gamePath;
                profile.BinmodListPath = _binmodListPath;
                _configuration.SaveChanges();
            }
        }
    }
    
    private string _binmodListPath;

    public string BinmodListPath
    {
        get
        {
            lock (_lock)
            {
                return _binmodListPath;
            }
        }
        
        set
        {
            lock (_lock)
            {
                _binmodListPath = value;
                var profile = _configuration.ProfileEntities.Find(Id)!;
                profile.GamePath = _gamePath;
                profile.BinmodListPath = _binmodListPath;
                _configuration.SaveChanges();
            }
        }
    }
}