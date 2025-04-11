using Flagrum.Core.Mathematics;

namespace Flagrum.Core.Animation.Model;

public class RawDataInformation
{
    public RawType Type { get; set; }
    public uint EntriesCount { get; set; }
    public Quaternion[] Values { get; set; }
}