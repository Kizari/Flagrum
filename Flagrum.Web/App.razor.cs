using System.Timers;
using Flagrum.Web.Persistence;
using Flagrum.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Web;

public partial class App
{
    private Timer _timer;
    [Inject] private AppStateService AppState { get; set; }
    [Inject] private FlagrumDbContext Context { get; set; }
    [Inject] private UriMapper UriMapper { get; set; }

    protected override void OnInitialized()
    {
        _timer = new Timer(1000);
        _timer.Elapsed += (_, _) =>
        {
            if (AppState.RootGameViewNode != null)
            {
                InvokeAsync(StateHasChanged);
                _timer.Stop();
            }
        };
        _timer.Start();
    }
}