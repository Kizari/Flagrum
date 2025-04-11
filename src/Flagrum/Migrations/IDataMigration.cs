using System.Threading.Tasks;

namespace Flagrum.Migrations;

public interface IDataMigration
{
    int Order { get; }
    bool ShouldRun { get; }
    Task RunAsync();
}