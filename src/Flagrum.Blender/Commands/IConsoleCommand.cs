namespace Flagrum.Blender.Commands;

public interface IConsoleCommand
{
    public string Command { get; }
    public void Execute(string[] args);
}