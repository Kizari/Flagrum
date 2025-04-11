using System.Reflection;
using Flagrum.Blender.Commands;

namespace Flagrum.Blender;

public static class Program
{
    public static int Main(string[] args)
    {
        var commandName = args[0];
        var arguments = args[1..];

        var commands = Assembly.GetAssembly(typeof(Program))!.GetTypes()
            .Where(t => t is {IsInterface: false, IsAbstract: false} && t.IsAssignableTo(typeof(IConsoleCommand)))
            .Select(t => (IConsoleCommand)Activator.CreateInstance(t)!)
            .ToList();

        var command = commands.FirstOrDefault(c => c.Command == commandName);

        if (command == null)
        {
            Console.Error.WriteLine($"Command not found: {commandName}");
            return 404;
        }

        command.Execute(arguments);

        return 0;
    }
}