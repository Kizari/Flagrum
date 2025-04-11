using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Containers;

/// <summary>
/// Interface for types that are compatible as children for the <see cref="GraphicsBinary{TData}" /> container
/// </summary>
public interface IGraphicsBinaryData
{
    void Read(MessagePackReader reader);
    void Write(MessagePackWriter writer);
}