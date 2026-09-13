using System.Threading.Tasks;
using Flagrum.Components.Modals;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Application.Features.Information.News;

/// <summary>
/// Base class for modals that present news to the user.
/// </summary>
public abstract class NewsModal : ComponentBase
{
    /// <summary>
    /// Modal component that displays the news content.
    /// </summary>
    protected AutosizeModal Modal { get; set; } = null!;

    /// <summary>
    /// Context associated with this news modal.
    /// </summary>
    [Parameter]
    public NewsModalContext? Context { get; set; }

    /// <summary>
    /// Logic to execute immediately before presenting the modal.
    /// </summary>
    protected virtual Task OnBeforePresent() => Task.CompletedTask;

    /// <summary>
    /// Presents this modal if applicable, otherwise presents the next modal in the news flow.
    /// </summary>
    public async Task PresentAsync()
    {
        // Skip to next modal if condition to show this one is unmet
        if (!Context!.Predicate(Context.Profile, Context.Application))
        {
            await Context.Next!.Reference!.PresentAsync();
            return;
        }

        // Present this modal
        await OnBeforePresent();
        Modal.Open();
    }

    /// <summary>
    /// Dismisses this modal, and presents the next modal in the news flow if applicable.
    /// </summary>
    protected Task DismissAsync()
    {
        Modal.Close();

        // End here if there is no next modal
        if (Context?.Next?.Reference == null)
        {
            // Update the latest seen version to prevent showing again
            var v = Context!.Application.Version;
            Context.Profile.LastVersionNotes = $"{v.Major}.{v.Minor}.{v.Build}";
            return Task.CompletedTask;
        }

        // Show the next modal in the flow
        return Context.Next.Reference.PresentAsync();
    }
}