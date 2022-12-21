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
        var currentItem = Parent.Preview.Item;
        if (currentItem != null)
        {
            var currentNode = currentItem.Parent;
            var children = currentNode.Children.ToList();
            var index = children.IndexOf(currentItem);
            if (up)
            {
                index--;
                if (index >= 0)
                {
                    var item = children[index];
                    Parent.AddressBar.SetCurrentPath(item.Path);
                    Parent.Preview.SetItem(item);
                }
            }
            else
            {
                index++;
                if (index < children.Count)
                {
                    var item = children[index];
                    Parent.AddressBar.SetCurrentPath(item.Path);
                    Parent.Preview.SetItem(item);
                }
            }
        }
    }

    private void SetContextNode(IAssetExplorerNode node)
    {
        Parent.ContextNode = node;
    }

    private void ContextMenuMouseUp(MouseEventArgs e, IAssetExplorerNode node)
    {
        if (e.Button == 2)
        {
            SetContextNode(node);
        }
    }
}