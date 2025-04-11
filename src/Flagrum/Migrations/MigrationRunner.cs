using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Generators;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Migrations;

[InjectableDependency(ServiceLifetime.Singleton)]
public partial class MigrationRunner
{
    [Inject] private readonly IProfileService _profile;
    
    public async Task RunMigrationsAsync()
    {
        var migrations = Assembly.GetAssembly(typeof(MigrationRunner))!.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IDataMigration)) && !t.IsInterface)
            .Select(migrationClass => (IDataMigration)Program.Services.GetRequiredService(migrationClass))
            .OrderBy(m => m.Order)
            .ToList();
        
        foreach (var migration in migrations.Where(migration => migration.ShouldRun))
        {
            await migration.RunAsync();
        }
        
        // This is okay to do here since it will only get this far if the migrations run to an acceptable point
        _profile.Current.LastSeenVersion = Assembly.GetAssembly(typeof(MigrationRunner))!.GetName().Version!;
    }
}