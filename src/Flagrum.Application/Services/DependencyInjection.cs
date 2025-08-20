using Flagrum.Abstractions;
using Flagrum.Abstractions.Utilities;
using Flagrum.Components;
using Flagrum.Core.Archive;
using Flagrum.Application.Features.AssetExplorer.Indexing;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Project;
using Flagrum.Application.Features.ModManager.Services;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Application.Features.Shared;
using Flagrum.Application.Features.WorkshopMods.Services;
using Flagrum.Application.Persistence;
using Flagrum.Application.Utilities;
using MemoryPack;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Application.Services;

public static class DependencyInjection
{
    /// <summary>
    /// Adds <c>Flagrum.Application</c> services to the dependency injection container.
    /// </summary>
    /// <param name="services">Collection to add the services to.</param>
    /// <returns>Same collection as the input parameter.</returns>
    /// <remarks>
    /// This is separate to <c>AddFlagrumApplication</c>, which is created automatically by source generators
    /// due to legacy code. Ideally, these should all be refactored to be registered via annotations.
    /// </remarks>
    public static IServiceCollection AddFlagrumApplicationManual(this IServiceCollection services)
    {
        // Associate MemoryPack entities with their respective interfaces
        MemoryPackFormatterProvider.Register(new Profile.Formatter());
        MemoryPackFormatterProvider.Register(new Configuration.Formatter());
        MemoryPackFormatterProvider.Register(new FileIndex.Formatter());
        MemoryPackFormatterProvider.Register(new FlagrumProjectArchive.Formatter());
        MemoryPackFormatterProvider.Register(new FlagrumProject.Formatter());
        MemoryPackFormatterProvider.Register(new ModBuildInstructionFormatter());
        MemoryPackFormatterProvider.Register(new PackedBuildInstructionFormatter());

        // Register services
        services
            .AddLocalization()
            .AddDbContext<FlagrumDbContext>(ServiceLifetime.Transient)
            .AddFlagrumComponents()
            .AddScoped<SteamWorkshopService>()
            .AddScoped<JSInterop>()
            .AddScoped<BinmodBuilder>()
            .AddScoped<BinmodTypeHelper>()
            .AddScoped<EntityPackageBuilder>()
            .AddScoped<Modmeta>()
            .AddScoped<TerrainPacker>()
            .AddScoped<TextureConverter>()
            .AddSingleton<IConfiguration, Configuration>()
            .AddSingleton<IUriHelper, UriHelper>(_ => UriHelper.Instance)
            .AddBlazorContextMenu()
            .AddScoped<LegacyModManagerServiceBase, LegacyFFXVModManager>()
            .AddFlagrumApplication()
            .AddSingleton<ModManagerServiceBase>(provider =>
            {
                var profile = provider.GetRequiredService<IProfileService>();
                return profile.Current.Type == LuminousGame.Forspoken
                    ? ActivatorUtilities.CreateInstance<ForspokenModManager>(provider)
                    : ActivatorUtilities.CreateInstance<FFXVModManager>(provider);
            })
            .AddScoped<INavigationManager>(provider => new NavigationManagerWrapper(
                provider.GetRequiredService<NavigationManager>()));

        // Register premium services
        PremiumHelper.Instance.AddServices(services);

        return services;
    }
}