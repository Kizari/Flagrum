using System.Linq;
using Flagrum.Core.Ebex.Types;

namespace Flagrum.Core.Ebex.Entities;

public class EbexValue : IEbexComponent
{
    public EbexValue()
    {
        Type = PrimitiveType.None;
        Data = null;
    }
    
    public EbexValue(PrimitiveType type, object data)
    {
        Type = type;
        Data = data;
    }

    public EbexValue(EbexEnum @enum, string value)
    {
        if (@enum.Items.All(i => i.Name != value) && @enum.Items.FirstOrDefault() != null)
        {
            value = @enum.Items.First().Name;
        }

        Type = PrimitiveType.Enum;
        Data = value;
    }

    public PrimitiveType Type { get; set; }
    public object Data { get; set; }
}