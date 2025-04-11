using System.Collections.Generic;
using System.IO;
using System.Text;
using Flagrum.Core.Data.Msgbin;
using Flagrum.Core.Serialization;

namespace Flagrum.Core.Data;

public class MessageBinary : BlackResourceBinary, IResourceBinaryItem, ISubresource
{
    public uint MessageCount { get; set; }
    public List<Message> Messages { get; set; } = new();
    public StringBuffer StringBuffer { get; set; }

    public List<Message> NewMessages { get; set; } = new();
    public List<byte[]> NewMessagesData { get; set; } = new();

    public override void Read(Stream stream)
    {
        base.Read(stream);

        using var reader = new BinaryReader(stream, Encoding.UTF8, true);

        MessageCount = reader.ReadUInt32();

        for (var i = 0; i < MessageCount; i++)
        {
            var message = new Message
            {
                Fixid = reader.ReadUInt32(),
                Offset = reader.ReadUInt32()
            };

            Messages.Add(message);
        }

        var returnAddress = stream.Position;
        var counter = -1;

        while (true)
        {
            stream.Seek(counter, SeekOrigin.End);
            var b = reader.ReadByte();
            if (b != 0x00)
            {
                break;
            }

            counter--;
        }

        var size = stream.Position + 1 - returnAddress;
        var stringBuffer = new byte[(int)size];

        stream.Seek(returnAddress, SeekOrigin.Begin);
        _ = reader.Read(stringBuffer);

        StringBuffer = new StringBuffer(stringBuffer);
    }

    public override void Write(Stream stream)
    {
        var start = stream.Position;

        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

        // Skip header for now
        stream.Seek(32, SeekOrigin.Current);

        // Write total message count
        writer.Write((uint)(Messages.Count + NewMessages.Count));

        // Write the message metadata for the original messages
        foreach (var message in Messages)
        {
            writer.Write(message.Fixid);
            writer.Write((uint)(message.Offset + NewMessages.Count * 8));
        }

        // Leave space for the metadata for the new messages
        var messagesStart = stream.Position;
        stream.Seek(8 * NewMessages.Count, SeekOrigin.Current);

        // Write the original strings
        writer.Write(StringBuffer.ToArray());

        // Write the new strings
        for (var i = 0; i < NewMessagesData.Count; i++)
        {
            NewMessages[i].Offset = (uint)(stream.Position - (start + 32));
            writer.Write(NewMessagesData[i]);
        }

        // Update the size
        Size = (int)(stream.Position - start);

        // Return to the start and write the Bdevresource header
        var returnAddress = stream.Position;
        stream.Seek(start, SeekOrigin.Begin);
        base.Write(stream);

        // Return to the messages and write the new metadata
        stream.Seek(messagesStart, SeekOrigin.Begin);
        foreach (var message in NewMessages)
        {
            writer.Write(message.Fixid);
            writer.Write(message.Offset);
        }

        // Return to the end of this resource
        stream.Seek(returnAddress, SeekOrigin.Begin);
    }

    public DataIndex Index { get; set; }
    
    public void Read(BinaryReader reader)
    {
        throw new System.NotImplementedException();
    }

    public void Write(BinaryWriter writer)
    {
        throw new System.NotImplementedException();
    }
}