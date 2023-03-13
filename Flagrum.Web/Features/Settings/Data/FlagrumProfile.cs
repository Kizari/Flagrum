using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Flagrum.Core.Utilities.Types;

namespace Flagrum.Web.Features.Settings.Data;

public class FlagrumProfile
{
    public string Id { get; set; }
    public LuminousGame Type { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; }

    public string GamePath { get; set; }
    public string BinmodListPath { get; set; }
}

public class FlagrumProfileContainer
{
    public string Current { get; set; }
    public List<FlagrumProfile> Profiles { get; set; }
    public string LastVersionNotes { get; set; }
    public string PatreonToken { get; set; }
    public string PatreonRefreshToken { get; set; }
    public DateTime PatreonTokenExpiry { get; set; }

    public static FlagrumProfileContainer GetDefault()
    {
        var profiles = new List<FlagrumProfile>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Final Fantasy XV Windows Edition",
                Type = LuminousGame.FFXV
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Forspoken",
                Type = LuminousGame.Forspoken
            }
        };

        return new FlagrumProfileContainer
        {
            Current = profiles[0].Id,
            Profiles = profiles
        };
    }
}