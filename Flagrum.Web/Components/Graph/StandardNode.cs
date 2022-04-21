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

    public StandardNode(GraphNode node, Point position = null)
        : base(position)
    {
        Node = node;
        Title = node.GetType().Name;

        var tokens = node.GetType().Namespace.Split('.');
        foreach (var token in tokens)
        {
            if (Title.StartsWith(token))
            {
                Title = Title[token.Length..];
            }
        }

        var fields = node.GetType().GetFields();

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
            var pin = (GraphPin)field.GetValue(node);
            var port = new StandardPort(pin, this, PortAlignment.Left, field.FieldType, field.Name);
            pin.Port = port;

            AddPort(port);
        }

        foreach (var field in outputFields)
        {
            var pin = (GraphPin)field.GetValue(node);
            var port = new StandardPort(pin, this, PortAlignment.Right, field.FieldType, field.Name);
            pin.Port = port;

            AddPort(port);
        }

        var fieldsToExclude = fields.Where(f => FieldsToExclude.Contains(f.Name));
        DataFields = fields.Except(inputFields).Except(outputFields).Except(fieldsToExclude);
    }

    public GraphNode Node { get; set; }
    public IEnumerable<FieldInfo> DataFields { get; set; }
}