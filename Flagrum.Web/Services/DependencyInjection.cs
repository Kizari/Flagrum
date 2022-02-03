using Flagrum.Web.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Web.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddFlagrum(this IServiceCollection services)
    {
        services.AddDbContext<FlagrumDbContext>();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<UriMapper>();
        services.AddScoped<SteamWorkshopService>();
        services.AddSingleton<AppStateService>();
        services.AddScoped<JSInterop>();
        services.AddScoped<BinmodBuilder>();
        services.AddScoped<BinmodTypeHelper>();
        services.AddScoped<EntityPackageBuilder>();
        services.AddScoped<ModelReplacementPresets>();
        services.AddScoped<Modmeta>();

        services.AddBlazorContextMenu(options =>
        {
            // options.ConfigureTemplate(defaultTemplate =>
            // {
            //     defaultTemplate.MenuCssClass = "context-menu";
            //     defaultTemplate.MenuItemCssClass = "context-menu-item";
            // });
        });

        return services;
    }
}