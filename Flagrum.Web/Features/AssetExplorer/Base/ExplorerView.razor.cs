using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Web.Persistence.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Flagrum.Web.Features.AssetExplorer.Base;

public partial class ExplorerView
{
    private AssetExplorerNode _node;

    [CascadingParameter] public System.Index Parent { get; set; }

    [Parameter]
    public AssetExplorerNode Node
    {
        get => _node;
        set
        {
            if (_node != value)
            {
                _node = value;
                GetNodes();
            }
        }
    }

    private List<AssetExplorerNode> Nodes { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool first)
    {
        if (first)
        {
            await JSInterop.SetFocusToElement("directoryView");
        }
    }

    private void GetNodes()
    {
        Nodes = Context.AssetExplorerNodes
            .Where(n => n.ParentId == Node.Id)
            .OrderByDescending(n => n.Children.Any())
            .ThenBy(n => n.Name)
            .ToList();
    }

    private void OnKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowUp":
                ShiftNode(true);
                break;
            case "ArrowDown":
                ShiftNode(false);
                break;
        }
    }

    private void ShiftNode(bool up)
    {
        // var currentNode = Nodes.FirstOrDefault(n => n.GetUri(Context) == Parent.SelectedItem?.Uri);
        // if (currentNode == null && Nodes.Any())
        // {
        //     currentNode = up ? Nodes[Nodes.Count > 1 ? 1 : 0] : Nodes[0];
        // }
        //
        // if (currentNode != null)
        // {
        //     var index = Nodes.IndexOf(currentNode);
        //     if (up)
        //     {
        //         index--;
        //         if (index >= 0)
        //         {
        //             Parent.SetSelectedItem(AssetExplorerItem.FromNode(Nodes[index], Context));
        //         }
        //     }
        //     else
        //     {
        //         index++;
        //         if (index < Nodes.Count)
        //         {
        //             Parent.SetSelectedItem(AssetExplorerItem.FromNode(Nodes[index], Context));
        //         }
        //     }
        // }
    }
}