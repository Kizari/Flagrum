using System.IO;

namespace Flagrum.Core.Data;

public interface ISubresource
{
    void Read(Stream stream);
    void Write(Stream stream);
}