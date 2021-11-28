namespace Flagrum.Web.Components.Controls.Data;

public class InMemoryFile
{
    public InMemoryFile(string fileName, byte[] data)
    {
        Data = data;
        FileName = fileName;
    }

    public byte[] Data { get; }
    public string FileName { get; }
}