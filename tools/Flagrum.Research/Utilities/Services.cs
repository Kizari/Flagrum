using System;
using System.Runtime.CompilerServices;
using Flagrum.Abstractions;
using Flagrum.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flagrum.Research.Utilities;

/// <summary>
/// Helper class for dependency injection within this application.
/// </summary>
public static class Services
{
    private static readonly Lazy<IServiceProvider> _provider = new(() => new ServiceCollection()
        .AddLogging(builder => builder.AddConsole())
        .AddSingleton<IProfileService, ProfileService>()
        .AddSingleton<AppStateService>()
        .AddFlagrumResearch()
        .AddFlagrumApplicationManual()
        .BuildServiceProvider());

    /// <summary>
    /// Resolves a service from the dependency injection container.
    /// </summary>
    /// <typeparam name="TService">Type of the service to resolve.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TService Get<TService>() where TService : notnull =>
        _provider.Value.GetRequiredService<TService>();
}