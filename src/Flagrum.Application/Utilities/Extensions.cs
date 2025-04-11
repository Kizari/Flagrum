using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Persistence;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Project;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flagrum.Application.Utilities;

public static class Extensions
{
    public static FlagrumProject LoadFlagrumProject(this IServiceProvider provider, string path)
    {
        var project = MemoryPackHelper.DeserializeCompressed<FlagrumProject>(path);
        InjectDependenciesForInstructions(provider, project);
        return project;
    }

    public static FlagrumProject CloneFlagrumProject(this IServiceProvider provider, IFlagrumProject project)
    {
        var newProject = ((FlagrumProject)project).DeepClone();
        InjectDependenciesForInstructions(provider, newProject);
        return newProject;
    }

    private static void InjectDependenciesForInstructions(IServiceProvider provider, FlagrumProject project)
    {
        var instructions = project.Archives.SelectMany(a => a.Instructions)
            .Cast<PackedBuildInstruction>()
            .Union(project.Instructions);

        foreach (var instruction in instructions)
        {
            foreach (var property in instruction.GetType()
                         .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                         .Where(p => p.GetCustomAttribute<FactoryInjectAttribute>() != null))
            {
                property.SetValue(instruction, provider.GetRequiredService(property.PropertyType));
            }
        }
    }

    public static IDictionary<string, string> GetQueryParameters(this NavigationManager navigationManager)
    {
        var queryStrings = navigationManager.Uri.Split('?').Last().Split('&');
        return queryStrings
            .Select(q => q.Split('='))
            .ToDictionary(r => r[0], r => r[1]);
    }

    /// <summary>
    /// Executes an action, and attempts to retry it a given number of times with a set delay if it fails
    /// </summary>
    /// <param name="logger">The logger that will log any debug information associated with this method</param>
    /// <param name="actionName">Human-readable name to describe this action</param>
    /// <param name="attempts">The number of times to attempt executing this action</param>
    /// <param name="delay">The amount of time to wait between attempts</param>
    /// <param name="action">The action to execute</param>
    public static async Task ExecuteWithRetryAsync(this ILogger logger, string actionName, int attempts, TimeSpan delay,
        Action action)
    {
        var attemptNumber = 0;
        var exception = new Exception("This should never show as it will be overwritten in the catch block");

        while (attemptNumber < attempts)
        {
            try
            {
                action();
                return;
            }
            catch (Exception e)
            {
                exception = e;
            }

            attemptNumber++;
            await Task.Delay(delay);
        }

        logger.LogError(exception, "{Action} failed to complete after {Attempts} attempts",
            actionName, attempts);

        throw exception;
    }

    public static string ToDisplayString(this Enum enumeration)
    {
        if (enumeration.GetType().GetCustomAttribute<FlagsAttribute>() == null)
        {
            return enumeration.ToString();
        }

        var values = Enum.GetValues(enumeration.GetType());
        var flags = new List<string>();
        foreach (Enum flag in values)
        {
            if (enumeration.HasFlag(flag))
            {
                flags.Add(flag.ToString());
            }
        }

        return string.Join(" | ", flags);
    }
}