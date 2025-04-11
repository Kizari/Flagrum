using System.IO;

namespace Flagrum.Core.Data;

public class ResourceArchive<TResource> : ResourceArchive
    where TResource : BlackResourceBinary, new()
{
    public TResource Resource { get; set; }

    public override void Read(Stream stream)
    {
        base.Read(stream);
        Resource = new TResource();
        Resource.Read(stream);
    }

    public override void Write(Stream stream)
    {
        var start = stream.Position;

        // Skip the header temporarily
        stream.Seek(256, SeekOrigin.Current);

        // Write the resource
        Resource.Write(stream);

        DataOffset = (ulong)stream.Position;
        var returnAddress = stream.Position;
        stream.Seek(start, SeekOrigin.Begin);

        // Write the header
        base.Write(stream);

        stream.Seek(returnAddress, SeekOrigin.Begin);
    }
}