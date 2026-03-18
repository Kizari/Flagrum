using Flagrum.Components.Modals;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Components;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFlagrumComponents(this IServiceCollection services) => services
        .AddSingleton<ObservedTaskScheduler>()
        .AddSingleton<IAlertService, AlertService>()
        .AddSingleton<ComponentStateService>();
}