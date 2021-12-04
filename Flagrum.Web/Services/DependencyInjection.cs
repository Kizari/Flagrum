using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Web.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddFlagrum(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorage();
        services.AddSingleton<Settings>();
        services.AddScoped<SteamWorkshopService>();
        services.AddSingleton<AppStateService>();
        services.AddScoped<JSInterop>();
        return services;
    }
}