using System.IO;
using System.Reflection;
using Flagrum.Abstractions;
using Flagrum.Application.Features.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Application.Utilities;

/// <summary>
/// Helper class for interfacing with the premium DLL, if present.
/// </summary>
public class PremiumHelper
{
    private const string LibraryName = "Flagrum.Premium.dll";

    /// <summary>
    /// Dynamically loads the premium library, if present.
    /// </summary>
    private PremiumHelper()
    {
        if (File.Exists(LibraryName))
        {
            Assembly = Assembly.LoadFrom(LibraryName);
        }
    }

    /// <summary>
    /// Singleton instance of this helper class.
    /// </summary>
    public static PremiumHelper Instance { get; } = new();

    /// <summary>
    /// Reference to the dynamically loaded premium library.
    /// </summary>
    /// <remarks>
    /// Returns <c>null</c> if the library was not present on disk.
    /// </remarks>
    public Assembly Assembly { get; }

    /// <summary>
    /// Adds premium services based on the presence of the <c>Flagrum.Premium</c>
    /// DLL in the application directory. This DLL is distributed with Flagrum releases,
    /// so should always be present. However, if the DLL is not present, such as if
    /// compiling a community fork without the premium DLL, equivalent free services
    /// will be added instead, allowing the application to function correctly without
    /// premium features.
    /// </summary>
    /// <param name="services">Service collection for the application.</param>
    /// <returns>The same service collection that was passed in.</returns>
    /// <exception cref="FileLoadException">
    /// Thrown if something goes wrong loading the premium DLL.
    /// </exception>
    public void AddServices(IServiceCollection services)
    {
        if (Assembly == null)
        {
            services.AddSingleton<IPremiumService, FreeService>();
            services.AddSingleton<IAuthenticationService, NoAuthenticationService>();
        }
        else
        {
            var extensions = Assembly.GetType("Flagrum.Premium.Architecture.ServiceCollectionExtensions");
            if (extensions == null)
            {
                throw new FileLoadException("Assembly was loaded, but ServiceCollectionExtensions class was not found");
            }

            var method = extensions.GetMethod("AddFlagrumPremium",
                BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                throw new FileLoadException(
                    "ServiceCollectionExtensions class was loaded, but AddFlagrumPremium method was not found");
            }

            method.Invoke(null, [services]);
        }
    }
}