namespace Flagrum.Core.Serialization.MessagePack;

public interface IMessagePackItem
{
    void Read(MessagePackReader reader);
    void Write(MessagePackWriter writer);
}