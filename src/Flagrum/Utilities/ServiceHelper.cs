using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Flagrum.Abstractions;
using Flagrum.Core.Utilities;
using Flagrum.Generators;
using Flagrum.Migrations;
using Flagrum.Services;
using Flagrum.Application.Services;
using Flagrum.ApplicationHost;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;

namespace Flagrum.Utilities;

public static class ServiceHelper
{
    public static IServiceProvider ConfigureServices()
    {
        // Set up Serilog to write to log files that roll over daily
        var logDirectory = Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "logs");
        IOHelper.EnsureDirectoryExists(logDirectory);
        var path = Path.Combine(logDirectory, "log-.txt");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(path, LogEventLevel.Information, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Populate the service collection
        var services = new ServiceCollection()
            .AddLogging(l => l.AddSerilog())
            .AddSingleton<IProfileService, ProfileService>()
            .AddSingleton<AppStateService>()
            .AddSingleton<IPlatformService, PlatformService>()
            .AddSingleton<JSComponentConfigurationStore>()
            .AddSingleton<IFileProvider>(_ => new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")))
            .AddBlazorWebView()
            .AddFlagrum()
            .AddFlagrumApplicationManual();

        // Automatically add all ViewModels to the DI container via reflection
        var viewModels = Assembly.GetAssembly(typeof(App))!.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(ObservableObject)));

        foreach (var viewModel in viewModels)
        {
            services.AddTransient(viewModel);
        }

        // Automatically add all data migration classes to the DI container via reflection
        var migrations = Assembly.GetAssembly(typeof(MigrationRunner))!.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IDataMigration)) && !t.IsInterface);

        foreach (var migration in migrations)
        {
            services.AddTransient(migration);
        }

        return services.BuildServiceProvider();
    }
}