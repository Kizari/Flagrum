namespace Flagrum.Components.Controls.Data;

public class InMemoryFile(string fileName, byte[] data)
{
    public byte[] Data { get; } = data;
    public string FileName { get; } = fileName;
}