namespace Flagrum.Core.Serialization.MessagePack;

/// <summary>
/// Some items in Luminous' MessagePack implementation have a slightly different structure for the first item
/// in an array. This interface is used to mark an item as such when reading arrays.
/// </summary>
public interface IMessagePackDifferentFirstItem
{
    bool IsFirst { get; set; }
}