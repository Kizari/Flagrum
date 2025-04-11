using System;
using Newtonsoft.Json;

namespace Flagrum.Application.Features.WorkshopMods.Data;

/// <summary>
/// This class is just here to allow the old binmod stuff to work until it is made obsolete and replaced
/// </summary>
public class HalfJsonConverter : JsonConverter<Half>
{
    public override void WriteJson(JsonWriter writer, Half value, JsonSerializer serializer)
    {
        throw new NotImplementedException("Not necessary because we don't serialise materials to JSON anymore");
    }

    public override Half ReadJson(JsonReader reader, Type objectType, Half existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        _ = reader.Value;
        
        // Halves are broken in the material templates, so just use the default value as it doesn't matter anyway
        return (Half)0.0f;
    }
}