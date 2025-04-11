using System.Collections.Generic;
using Flagrum.Application.Features.ModManager.Project;

namespace Flagrum.Application.Features.ModManager.Installer;

public class ModInstallationResult
{
    public ModInstallationStatus Status { get; set; }
    public string ErrorTitle { get; set; }
    public string ErrorHeading { get; set; }
    public string ErrorText { get; set; }
    public List<FlagrumProject> Projects { get; set; } = new();

    public ModInstallationResult(ModInstallationStatus status)
    {
        Status = status;
    }

    public ModInstallationResult(FlagrumProject project)
    {
        Status = ModInstallationStatus.Success;
        Projects.Add(project);
    }

    public ModInstallationResult(string errorTitle, string errorHeading, string errorText)
    {
        Status = ModInstallationStatus.Error;
        ErrorTitle = errorTitle;
        ErrorHeading = errorHeading;
        ErrorText = errorText;
    }
}

public enum ModInstallationStatus
{
    Error,
    Success,
    Cancelled
}