namespace Flagrum.Core.Serialization;

public static class SerializationHelper
{
    public static uint Align(uint offset, uint blockSize)
    {
        return blockSize + blockSize * (offset / blockSize) - offset;
    }

    /// <summary>
    /// Aligns current offset to the given block size
    /// </summary>
    /// <param name="offset">The offset of the end of the data that needs to be aligned</param>
    /// <param name="blockSize">The size to align to</param>
    /// <returns>The offset of the end of the alignment</returns>
    public static uint GetAlignment(uint offset, uint blockSize)
    {
        return blockSize + blockSize * (offset / blockSize);
    }

    public static ulong GetAlignment(ulong offset, ulong blockSize)
    {
        return blockSize + blockSize * (offset / blockSize);
    }
}