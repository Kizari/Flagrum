using Flagrum.Core.Scripting.Ebex.Data;

namespace Flagrum.Core.Scripting.Ebex.Type;

public interface IMenuItemCommandExecutor
{
    bool Execute(DataItem dataItem, DataItem fieldItem, string command);
}