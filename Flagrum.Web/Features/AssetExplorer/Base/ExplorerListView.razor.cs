using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Web.Features.AssetExplorer.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Flagrum.Web.Features.AssetExplorer.Base;

public partial class ExplorerListView
{
    [CascadingParameter(Name = "AssetExplorer")]
    public AssetExplorer Parent { get; set; }

    [Parameter] public IAssetExplorerNode Node { get; set; }

    private ICollection<IAssetExplorerNode> Children => Parent.CurrentLayout == FileListLayout.TreeView
        ? Node.Children.Where(n => n.Type != ExplorerItemType.Directory).ToList()
        : Node.Children ?? Node.Parent.Children;

    protected override async Task OnAfterRenderAsync(bool first)
    {
        if (first)
        {
            await JSInterop.SetFocusToElement("directoryView");
        }
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
        // var currentNode = Parent.FileList.CurrentNode;
        // if (currentNode != null)
        // {
        //     var children = currentNode.Parent.Children.ToList();
        //     var index = children.IndexOf(currentNode);
        //     if (up)
        //     {
        //         index--;
        //         if (index >= 0)
        //         {
        //             Parent.FileList.SetCurrentNode(children[index]);
        //         }
        //     }
        //     else
        //     {
        //         index++;
        //         if (index < children.Count)
        //         {
        //             Parent.FileList.SetCurrentNode(children[index]);
        //         }
        //     }
        // }
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

    private void SetContextNode(IAssetExplorerNode node)
    {
        Parent.ContextNode = node;
    }
}