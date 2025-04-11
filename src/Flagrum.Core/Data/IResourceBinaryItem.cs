using System.IO;

namespace Flagrum.Core.Data;

public interface IResourceBinaryItem
{
    DataIndex Index { get; set; }
    void Read(BinaryReader reader);
    void Write(BinaryWriter writer);
}