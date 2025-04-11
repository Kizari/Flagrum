namespace Flagrum.Core.Scripting.Ebex.Data;

public interface IDocumentAction
{
    string GetAvailableName(DataItem item, string name);

    bool IsUniqueName(DataItem item, string name);

    void Delete(DataItem item);
}