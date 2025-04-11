using Flagrum.Core.Scripting.Ebex.Configuration;
using Flagrum.Core.Scripting.Ebex.Type;

namespace Flagrum.Core.Scripting.Ebex.Data;

public class DocumentInterface
{
    public static IDocumentContainer DocumentContainer { get; set; }

    public static IDocumentAction DocumentAction { get; set; }

    public static ModuleContainer ModuleContainer { get; set; }

    public static Configuration.Configuration Configuration { get; set; }

    public static JenovaApplicationConfiguration ApplicationConfiguration { get; set; }

    public static IMenuItemCommandExecutor MenuItemCommandExecutor { get; set; }
}