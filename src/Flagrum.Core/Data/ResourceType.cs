using System;
using System.Collections.Generic;
using Flagrum.Core.Data.Bins;

namespace Flagrum.Core.Data;

public class ResourceType
{
    public static Dictionary<uint, Type> Map { get; } = new()
    {
        {67271937, typeof(DataIndexBinary)},
        {67110385, typeof(ParameterTableCollection)},
        {67111838, typeof(ResourceArchive<MessageBinary>)}
    };
}