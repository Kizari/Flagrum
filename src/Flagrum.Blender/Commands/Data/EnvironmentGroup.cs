namespace Flagrum.Blender.Commands.Data;

public class EnvironmentGroup : EnvironmentItem
{
    public string Type { get; set; } = "EnvironmentGroup";
    public List<EnvironmentItem> Children { get; set; } = new();

    public void RemoveEmptyGroups()
    {
        // Traverse to the bottom of the tree first before checking this item for empties
        foreach (var group in Children.Where(c => c is EnvironmentGroup))
        {
            ((EnvironmentGroup)group).RemoveEmptyGroups();
        }
        
        for (var i = Children.Count - 1; i >= 0; i--)
        {
            var child = Children[i];
            if (child is EnvironmentGroup group && !group.Children.Any())
            {
                Children.RemoveAt(i);
            }
        }
    }
}