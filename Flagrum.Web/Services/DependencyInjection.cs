using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Web.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddFlagrum(this IServiceCollection services)
    {
        services.AddSingleton<SettingsService>();
        services.AddScoped<SteamWorkshopService>();
        services.AddSingleton<AppStateService>();
        services.AddScoped<JSInterop>();
        services.AddScoped<BinmodBuilder>();
        services.AddScoped<BinmodTypeHelper>();
        services.AddScoped<EntityPackageBuilder>();
        services.AddScoped<ModelReplacementPresets>();
        services.AddScoped<Modmeta>();
        return services;
    }
}