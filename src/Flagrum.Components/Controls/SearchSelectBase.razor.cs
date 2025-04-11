using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Flagrum.Components.Controls;

public partial class SearchSelectBase<TItem>
{
    private string _baseQuery;
    [Parameter] public ICollection<TItem> Items { get; set; }
    [Parameter] public RenderFragment<TItem> DisplayTemplate { get; set; }
    [Parameter] public string Placeholder { get; set; }
    [Parameter] public Func<TItem, string> SearchValue { get; set; }
    [Parameter] public bool RemoveOnSelect { get; set; }
    [Parameter] public virtual Action<TItem> OnSelect { get; set; }
    [Parameter] public bool CloseOnSelect { get; set; }

    private string Id { get; set; }
    protected List<SearchSelectItem<TItem>> EncapsulatedItems { get; set; }
    private string WrapperId { get; set; }
    private string SearchId { get; set; }
    protected bool IsSearching { get; set; }

    protected virtual string Query
    {
        get => _baseQuery;
        set
        {
            if (!EqualityComparer<string>.Default.Equals(_baseQuery, value))
            {
                _baseQuery = value;
                Search();
            }
        }
    }

    protected TItem SelectedItem { get; set; }
    protected bool Remote { get; set; }
    protected bool HasNextPage { get; set; }
    protected int Page { get; set; } = 1;

    private RenderFragment PlaceholderFragment { get; set; }

    protected override Task OnInitializedAsync()
    {
        Id = Guid.NewGuid().ToString();
        WrapperId = $"{Id}Wrapper";
        SearchId = $"{Id}Search";

        if (Items != null)
        {
            EncapsulatedItems = Items.Select(i => new SearchSelectItem<TItem>
            {
                IsVisible = true,
                Item = i
            }).OrderBy(i => SearchValue(i.Item)).ToList();
        }

        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("interop.monitorSearchSelect", Id, WrapperId, CloseOnSelect);
            await Search(true);
        }
    }

    protected virtual Task Search(bool fromInitialized = false)
    {
        if (EncapsulatedItems == null)
        {
            return Task.CompletedTask;
        }

        if (string.IsNullOrEmpty(Query))
        {
            EncapsulatedItems.ForEach(i => i.IsVisible = true);
        }
        else
        {
            var matches = EncapsulatedItems.Where(i => SearchValue(i.Item).ToLower().Contains(Query.ToLower()));
            foreach (var item in EncapsulatedItems.Except(matches))
            {
                item.IsVisible = false;
            }
        }

        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task Toggle()
    {
        await JSRuntime.InvokeVoidAsync("interop.toggleSearchSelect", Id);
        await JSRuntime.InvokeVoidAsync("interop.focusElement", SearchId);
        Query = null;
        Page = 1;
    }

    protected virtual Task SelectItem(SearchSelectItem<TItem> item)
    {
        EncapsulatedItems.ForEach(i => i.IsSelected = false);
        item.IsSelected = true;

        if (RemoveOnSelect)
        {
            EncapsulatedItems.Remove(item);
        }

        OnSelect?.Invoke(item.Item);
        StateHasChanged();
        return Task.CompletedTask;
    }

    public void AddItem(TItem item)
    {
        EncapsulatedItems.Add(new SearchSelectItem<TItem> {IsVisible = true, Item = item});
        EncapsulatedItems.Sort((x, y) => SearchValue(x.Item).CompareTo(SearchValue(y.Item)));
        //StateHasChanged();
    }

    protected virtual Task LoadMore() => Task.CompletedTask;
}

public class SearchSelectItem<TItem>
{
    public bool IsVisible { get; set; } = true;
    public bool IsSelected { get; set; }
    public TItem Item { get; set; }
}