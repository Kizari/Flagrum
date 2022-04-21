using System;
using System.Collections.Generic;
using Blazor.Diagrams.Core.Models;
using SQEX.Ebony.Framework.Node;

namespace Flagrum.Web.Components.Graph;

public class StandardPort : PortModel
{
    public const string Control = "#e1d9b7";
    public const string Rose = "#783b4a";

    public StandardPort(GraphPin pin, NodeModel parent, PortAlignment alignment, Type type, string name) : base(parent,
        alignment)
    {
        Pin = pin;
        Type = type;
        Name = name;
        Color = GetColorByType();
    }

    public Dictionary<Type, string> TypeColors { get; set; } = new()
    {
        {typeof(GraphInputPin), Control},
        {typeof(GraphOutputPin), Control},
        {typeof(GraphTriggerInputPin), Control},
        {typeof(GraphTriggerOutputPin), Control},
        {typeof(GraphVariableInputPin), Rose},
        {typeof(GraphVariableOutputPin), Rose}
    };

    public GraphPin Pin { get; set; }
    public Type Type { get; set; }
    public string Color { get; set; }
    public string Name { get; set; }

    public override bool CanAttachTo(PortModel port)
    {
        return true;
    }

    private string GetColorByType()
    {
        if (TypeColors.TryGetValue(Type, out var color))
        {
            return color;
        }

        return "grey";
    }
}