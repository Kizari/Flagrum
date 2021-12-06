using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using SQEX.Ebony.Framework.Node;

namespace Flagrum.Web.Components.Graph;

public class StandardNode : NodeModel
{
    public static IEnumerable<Type> InputTypes = new List<Type>
    {
        typeof(GraphTriggerInputPin),
        typeof(GraphVariableInputPin),
        typeof(GraphInputPin)
    };

    public static IEnumerable<Type> OutputTypes = new List<Type>
    {
        typeof(GraphTriggerOutputPin),
        typeof(GraphVariableOutputPin),
        typeof(GraphOutputPin)
    };

    public static IEnumerable<string> FieldsToExclude = new List<string>
    {
        "triInPorts_",
        "triOutPorts_",
        "refInPorts_",
        "refOutPorts_",
        "Isolated_"
    };

    public StandardNode(Type type, Point position = null)
        : base(position)
    {
        Title = type.Name;
        Type = type;

        var fields = type.GetFields();

        var comparer = Comparer<Type>.Create((first, second) =>
        {
            var firstIsTrigger = first == typeof(GraphTriggerInputPin) || first == typeof(GraphTriggerOutputPin);
            var secondIsTrigger = second == typeof(GraphTriggerInputPin) || second == typeof(GraphTriggerOutputPin);

            if (firstIsTrigger && secondIsTrigger)
            {
                return 0;
            }

            if (firstIsTrigger && !secondIsTrigger)
            {
                return -1;
            }

            if (!firstIsTrigger && secondIsTrigger)
            {
                return 1;
            }

            return 0;
        });

        var inputFields = fields.Where(f => InputTypes.Contains(f.FieldType)).OrderBy(f => f.FieldType, comparer);
        var outputFields = fields.Where(f => OutputTypes.Contains(f.FieldType)).OrderBy(f => f.FieldType, comparer);

        foreach (var field in inputFields)
        {
            AddPort(new StandardPort(this, PortAlignment.Left, field.FieldType, field.Name));
        }

        foreach (var field in outputFields)
        {
            AddPort(new StandardPort(this, PortAlignment.Right, field.FieldType, field.Name));
        }

        var fieldsToExclude = fields.Where(f => FieldsToExclude.Contains(f.Name));
        DataFields = fields.Except(inputFields).Except(outputFields).Except(fieldsToExclude);
    }

    public Type Type { get; set; }
    public IEnumerable<FieldInfo> DataFields { get; set; }
}