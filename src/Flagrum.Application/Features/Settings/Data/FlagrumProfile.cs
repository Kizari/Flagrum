using System.ComponentModel.DataAnnotations;
using Flagrum.Abstractions;

namespace Flagrum.Application.Features.Settings.Data;

public class FlagrumProfile : IProfileViewModel
{
    public string Id { get; set; }
    public LuminousGame Type { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; }

    public string GamePath { get; set; }
    public string BinmodListPath { get; set; }
}