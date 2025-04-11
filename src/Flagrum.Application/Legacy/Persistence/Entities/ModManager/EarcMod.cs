using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Application.Features.ModManager.Mod;
using Flagrum.Application.Resources;

namespace Flagrum.Application.Persistence.Entities.ModManager;

public class EarcMod
{
    public int Id { get; set; }

    [Display(Name = nameof(DisplayNameResource.ModName), ResourceType = typeof(DisplayNameResource))]
    [Required(ErrorMessageResourceName = nameof(ErrorMessageResource.RequiredError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    [StringLength(37, ErrorMessageResourceName = nameof(ErrorMessageResource.MaxLengthError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    public string Name { get; set; }

    [Display(Name = nameof(DisplayNameResource.Author), ResourceType = typeof(DisplayNameResource))]
    [Required(ErrorMessageResourceName = nameof(ErrorMessageResource.RequiredError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    [StringLength(32, ErrorMessageResourceName = nameof(ErrorMessageResource.MaxLengthError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    public string Author { get; set; }

    [Display(Name = nameof(DisplayNameResource.Description), ResourceType = typeof(DisplayNameResource))]
    [Required(ErrorMessageResourceName = nameof(ErrorMessageResource.RequiredError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    [StringLength(1000, ErrorMessageResourceName = nameof(ErrorMessageResource.MaxLengthError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    public string Description { get; set; }

    public string Readme { get; set; }

    public bool IsActive { get; set; }
    public ModCategory Category { get; set; }
    public ModFlags Flags { get; set; }
    public Guid? Identifier { get; set; }
    public bool IsFavourite { get; set; }

    public ICollection<EarcModEarc> Earcs { get; set; } = new List<EarcModEarc>();
    public ICollection<EarcModLooseFile> LooseFiles { get; set; } = new List<EarcModLooseFile>();
}