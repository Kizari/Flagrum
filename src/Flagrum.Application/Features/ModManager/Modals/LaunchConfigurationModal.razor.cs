using Flagrum.Application.Features.Settings.Data;
using Flagrum.Components.Modals;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Flagrum.Application.Features.ModManager.Modals;

public partial class LaunchConfigurationModal
{
    [Inject] private LaunchConfiguration Configuration { get; set; } = null!;
    
    private AutosizeModal Modal { get; set; } = null!;
    private EditContext EditContext { get; set; } = null!;
    
    private string? SteamRoot { get; set; }
    private string? ProtonPrefix { get; set; }
    private string? SteamLaunchWrapper { get; set; }
    private string? SteamReaper { get; set; }
    private string? SteamRuntime { get; set; }
    private string? Proton { get; set; }
    private string? LaunchCommand { get; set; }

    public void Open(bool shouldValidateOnOpen)
    {
        if (shouldValidateOnOpen)
        {
            EditContext.Validate();
        }
        
        Modal.Open();
    }

    protected override void OnInitialized()
    {
        // Copy the launch configuration for editing
        SteamRoot = Configuration.SteamRoot;
        ProtonPrefix = Configuration.ProtonPrefix;
        SteamLaunchWrapper = Configuration.SteamLaunchWrapper;
        SteamReaper = Configuration.SteamReaper;
        SteamRuntime = Configuration.SteamRuntime;
        Proton = Configuration.Proton;
        LaunchCommand = Configuration.LaunchCommand;

        // Set form validation state
        EditContext = new EditContext(this);
    }

    private void Save()
    {
        // Copy the edited values back into the launch configuration
        Configuration.SteamRoot = SteamRoot;
        Configuration.ProtonPrefix = ProtonPrefix;
        Configuration.SteamLaunchWrapper = SteamLaunchWrapper;
        Configuration.SteamReaper = SteamReaper;
        Configuration.SteamRuntime = SteamRuntime;
        Configuration.Proton = Proton;
        Configuration.LaunchCommand = LaunchCommand;
        
        Modal.Close();
    }
}