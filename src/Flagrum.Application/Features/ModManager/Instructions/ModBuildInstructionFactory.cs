using System;
using System.Linq;
using System.Reflection;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Application.Utilities;
using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Application.Features.ModManager.Instructions;

/// <inheritdoc />
[RegisterScoped<IModBuildInstructionFactory>]
public partial class ModBuildInstructionFactory(IServiceProvider provider) : IModBuildInstructionFactory
{
    /// <inheritdoc />
    public TInstruction Create<TInstruction>() where TInstruction : IModBuildInstruction
    {
        var type = typeof(TInstruction);
        if (type.IsAbstract)
        {
            type = Assembly.GetAssembly(typeof(ModBuildInstructionFactory))!
                .GetTypes()
                .Single(t => !t.IsAbstract && t.IsAssignableTo(type));
        }
        
        var instruction = (TInstruction)ActivatorUtilities.CreateInstance(provider, type);
        foreach (var property in instruction.GetType()
                     .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                     .Where(p => p.GetCustomAttribute<FactoryInjectAttribute>() != null))
        {
            property.SetValue(instruction, provider.GetRequiredService(property.PropertyType));
        }

        return instruction;
    }

    /// <inheritdoc />
    public void Inject(IModBuildInstruction instruction)
    {
        foreach (var property in instruction.GetType()
                     .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                     .Where(p => p.GetCustomAttribute<FactoryInjectAttribute>() != null))
        {
            property.SetValue(instruction, provider.GetRequiredService(property.PropertyType));
        }
    }
}