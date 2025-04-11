using System.ComponentModel.DataAnnotations;
using Flagrum.Application.Resources;

namespace Flagrum.Application.Features.ModManager.Modals;

public class ModCardViewModel
{
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
}