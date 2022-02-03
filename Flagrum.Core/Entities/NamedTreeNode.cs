using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Flagrum.Core.Entities;

[DataContract(IsReference = true)]
public class NamedTreeNode
{
    public NamedTreeNode(string name, NamedTreeNode parent)
    {
        Name = name;
        Parent = parent;
        Children = new LinkedList<NamedTreeNode>();
    }

    [DataMember] public string Name { get; set; }

    [DataMember] public NamedTreeNode Parent { get; set; }

    [DataMember] public LinkedList<NamedTreeNode> Children { get; set; }

    public NamedTreeNode AddChild(string name)
    {
        var node = new NamedTreeNode(name, this);
        Children.AddFirst(new LinkedListNode<NamedTreeNode>(node));
        return node;
    }

    public void TraverseAscending(Action<NamedTreeNode> visitor)
    {
        visitor(this);
        foreach (var child in Children)
        {
            child.TraverseAscending(visitor);
        }
    }

    public void TraverseDescending(Action<NamedTreeNode> visitor)
    {
        visitor(this);
        Parent?.TraverseDescending(visitor);
    }
}