using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using SQEX.Ebony.Framework.Node;
using SQEX.Ebony.Framework.Sequence.Group;
using ZLibNet;

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
        
        var inputPins = new List<(GraphPin pin, string name, Type type)>();
        var outputPins = new List<(GraphPin pin, string name, Type type)>();
        
        inputPins.AddRange(fields
            .Where(f => f.FieldType.IsAssignableTo(typeof(GraphInputPin)))
            .Select(f => ((GraphPin)f.GetValue(node), f.Name, f.FieldType)));
        
        outputPins.AddRange(fields
            .Where(f => f.FieldType.IsAssignableTo(typeof(GraphOutputPin)))
            .Select(f => ((GraphPin)f.GetValue(node), f.Name, f.FieldType)));
        
        var pinListFields = fields.Where(f => f.FieldType.GetInterface("IList") != null && f.FieldType.GenericTypeArguments.Any(t => t.IsAssignableTo(typeof(GraphPin))));

        foreach (var pinList in pinListFields)
        {
            var pins = (List<GraphPin>)pinList.GetValue(node);
            foreach (var pin in pins)
            {
                if (pin.GetType().IsAssignableTo(typeof(GraphInputPin)))
                {
                    inputPins.Add((pin, pin.pinName_, pin.GetType()));
                }
                else if (pin.GetType().IsAssignableTo(typeof(GraphOutputPin)))
                {
                    outputPins.Add((pin, pin.pinName_, pin.GetType()));
                }
                else
                {
                    throw new InvalidOperationException($"Unknown pin type {pin.GetType()}");
                }
            }
        }
        
        inputPins.Sort((first, second) => comparer.Compare(first.type, second.type));
        outputPins.Sort((first, second) => comparer.Compare(first.type, second.type));

        foreach (var (pin, name, type) in inputPins)
        {
            var port = new StandardPort(pin, this, PortAlignment.Left, type, name);
            pin.Port = port;
            AddPort(port);
        }

        foreach (var (pin, name, type) in outputPins)
        {
            var port = new StandardPort(pin, this, PortAlignment.Right, type, name);
            pin.Port = port;
            AddPort(port);
        }

        var fieldsToExclude = fields.Where(f => FieldsToExclude.Contains(f.Name));
        DataFields = fields.Where(f => !f.FieldType.IsAssignableTo(typeof(GraphPin))).Except(fieldsToExclude);
    }

    public int ObjectIndex { get; set; }
    public GraphNode Node { get; set; }
    public IEnumerable<FieldInfo> DataFields { get; set; }
}