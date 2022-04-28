using System.Collections.Generic;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using SQEX.Ebony.Framework.Node;
using SQEX.Ebony.Framework.Sequence.Tray;

namespace Flagrum.Web.Components.Graph;

public class StandardGroup : GroupModel
{
    public SequenceTrayBase Tray { get; set; }
    
    public StandardGroup(SequenceTrayBase tray) : base(new List<NodeModel>(), 50)
    {
        Tray = tray;
        Size = new Size(600, 450);
        
        foreach (var port in tray.triInPorts_)
        {
            var newPort = new StandardPort(port, this, PortAlignment.Left, typeof(GraphTriggerInputPin), port.pinName_);
            port.Port = newPort;
            AddPort(newPort);
        }
        
        foreach (var port in tray.triOutPorts_)
        {
            var newPort = new StandardPort(port, this, PortAlignment.Right, typeof(GraphTriggerOutputPin), port.pinName_);
            port.Port = newPort;
            AddPort(newPort);
        }
        
        foreach (var port in tray.refInPorts_)
        {
            var newPort = new StandardPort(port, this, PortAlignment.Left, typeof(GraphVariableInputPin), port.pinName_);
            port.Port = newPort;
            AddPort(newPort);
        }
        
        foreach (var port in tray.refOutPorts_)
        {
            var newPort = new StandardPort(port, this, PortAlignment.Right, typeof(GraphVariableOutputPin), port.pinName_);
            port.Port = newPort;
            AddPort(newPort);
        }
    }

    public string Title { get; set; } = "Sequence Tray";
}