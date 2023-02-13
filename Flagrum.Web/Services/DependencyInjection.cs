using Flagrum.Web.Features.ModManager.Services;
using Flagrum.Web.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Web.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddFlagrum(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddDbContext<FlagrumDbContext>(ServiceLifetime.Transient);
        services.AddSingleton<UriMapper>();
        services.AddScoped<SteamWorkshopService>();
        services.AddScoped<JSInterop>();
        services.AddScoped<BinmodBuilder>();
        services.AddScoped<BinmodTypeHelper>();
        services.AddScoped<EntityPackageBuilder>();
        services.AddScoped<ModelReplacementPresets>();
        services.AddScoped<Modmeta>();
        services.AddScoped<EnvironmentPacker>();
        services.AddScoped<TerrainPacker>();
        services.AddScoped<AssetConverter>();
        services.AddBlazorContextMenu();

        return services;
    }
}