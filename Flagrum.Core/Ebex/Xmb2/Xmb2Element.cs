using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Ebex.Xmb2;

public class Xmb2Element
{
    private Xmb2Element(int elementStartOffset, int attributeStartOffset, uint elementCount, uint attributeCount,
        byte[] xmb2, long offset)
    {
        ElementStartOffset = elementStartOffset;
        AttributeStartOffset = attributeStartOffset;
        ElementCount = elementCount;
        AttributeCount = attributeCount;
        Xmb2 = xmb2;
        Offset = offset;
        Name = GetName();
    }

    /// <summary>
    /// Offset to the child element relative offset table, relative to this element.
    /// </summary>
    private int ElementStartOffset { get; }

    /// <summary>
    /// Offset to the attribute relative offset table, relative to this field.
    /// </summary>
    private int AttributeStartOffset { get; }

    /// <summary>
    /// Number of child elements.
    /// </summary>
    public uint ElementCount { get; }

    /// <summary>
    /// Number of attributes.
    /// </summary>
    private uint AttributeCount { get; }

    /// <summary>
    /// The XMB2 document.
    /// </summary>
    private byte[] Xmb2 { get; }

    /// <summary>
    /// This element's absolute offset.
    /// </summary>
    private long Offset { get; }

    /// <summary>
    /// This element's name.
    /// </summary>
    public string Name { get; }
    
    private static readonly Dictionary<uint, string> Names = new string[]
    {
        "",
        "name",
        "objectIndex",
        "type",
        "value",
        "path",
        "checked",
        "reference",
        "object",
        "original_type",
        "owner",
        "ownerIndex",
        "ownerPath",
        "fixid",
        "relativePath",
        "dynamic",
        "connectorIndex",
        "connectorRelativePath",
        "connectorObject",
        "connectorObjectIndex",
        "PrefabOwnerPackageSource",
        "PrefabConnectionSourceItemPath",
        "ProxyConnectionOwnerPackageName",
        "UseTemplateConnection",
        "UnresolvedPointerPackageSource",
        "UseUnresolvedPointerReference",
        "FarReference",
        "ReferenceSourceItemPath",
        "UsePrefabConnectionV20",
        "CharaEntryRef",
        "csharp_type",
        "deserialize_type",
        "csharp",
        "import_component_class_name",
        "import_component_class_hash",
        "import_component_member_hash"
    }.ToDictionary((Func<string, uint>) (x => Cryptography.Hash32(x)), (Func<string, string>) (x => x));

    public void Dump(StringBuilder output, int indent)
    {
        for (var i = 0; i < indent; i++)
        {
            output.Append(' ');
        }

        output.Append('<').Append(Name);

        // TODO attributes
        var variantRelativeOffsetTableOffset = Offset + AttributeStartOffset + 8;
        for (var i = 0; i < AttributeCount; i++)
        {
            var variantRelativeOffsetOffset = variantRelativeOffsetTableOffset + i * sizeof(int);
            var variantRelativeOffset = BitConverter.ToInt32(Xmb2, (int)variantRelativeOffsetOffset);
            var variantOffset = variantRelativeOffsetOffset + variantRelativeOffset;
            var variant = Xmb2Variant.FromByteArray(Xmb2, variantOffset);

            //output.Append(" 0x").Append(variant.NameHash.ToString("x")).Append("=\"").Append(variant.GetTextValue())
            //    .Append('\"');
            output.Append(' ').Append(Names[variant.NameHash]).Append("=\"").Append(variant.GetTextValue())
                .Append('\"');
        }

        if (ElementCount != 0)
        {
            output.Append(">\n");
            foreach (var element in GetElements())
            {
                element.Dump(output, indent + 2);
            }

            for (var i = 0; i < indent; i++)
            {
                output.Append(" ");
            }

            output.Append("</").Append(Name).Append(">\n");
            return;
        }

        if (ElementStartOffset == 0)
        {
            output.Append(" />\n");
            return;
        }

        output.Append('>').Append(GetTextValue());
        output.Append("</").Append(Name).Append(">\n");
    }

    /// <summary>
    /// Read an Xmb2Element from a byte array.
    /// </summary>
    /// <param name="xmb2">The XMB2 document byte array.</param>
    /// <param name="startIndex">The index to start reading from.</param>
    /// <returns>The parsed Xmb2Element.</returns>
    public static Xmb2Element FromByteArray(byte[] xmb2, long startIndex)
    {
        var elementStartOffset = BitConverter.ToInt32(xmb2, (int)startIndex);
        var attributeStartOffset = BitConverter.ToInt32(xmb2, (int)startIndex + sizeof(int));

        // ElementCount is a 24-bit integer, so we need to manually parse it.
        var elementCount = xmb2[startIndex + 2 * sizeof(int)]
                           + (xmb2[startIndex + (2 * sizeof(int) + 1)] << 8)
                           + (xmb2[startIndex + (2 * sizeof(int) + 2)] << 16);

        var attributeCount = xmb2[startIndex + (2 * sizeof(int) + 3)];
        return new Xmb2Element(elementStartOffset, attributeStartOffset, (uint)elementCount, attributeCount, xmb2,
            startIndex);
    }

    /// <summary>
    /// Read the child element with a given index.
    /// </summary>
    /// <param name="index">The index of the child element to get.</param>
    /// <returns>The parsed child element.</returns>
    public Xmb2Element GetElementByIndex(int index)
    {
        if (ElementCount == 0)
        {
            return null;
        }

        // This is an array of relative offsets.
        var childElementRelativeOffsetTableOffset = Offset + ElementStartOffset;

        // Absolute offset to the child element's relative offset. Yes, this is an offset to an offset.
        var childElementRelativeOffsetOffset = childElementRelativeOffsetTableOffset + index * sizeof(int);

        // The child element offset is relative to the offset's offset itself. May be, and in fact usually is, a negative value.
        var childElementRelativeOffset = BitConverter.ToInt32(Xmb2, (int)childElementRelativeOffsetOffset);
        var childElementOffset = childElementRelativeOffsetOffset + childElementRelativeOffset;

        return FromByteArray(Xmb2, childElementOffset);
    }

    /// <summary>
    /// Read the child element with a given name.
    /// </summary>
    /// <param name="name">The name of the child element to get.</param>
    /// <returns>The parsed child element.</returns>
    public Xmb2Element GetElementByName(string name)
    {
        if (ElementCount == 0)
        {
            return null;
        }

        for (var i = 0; i < ElementCount; i++)
        {
            var childElement = GetElementByIndex(i);
            var childName = childElement.GetName();
            if (name == childName)
            {
                return childElement;
            }
        }

        return null;
    }

    public Xmb2Variant GetAttributeByName(uint nameHash)
    {
        var variantRelativeOffsetTableOffset = Offset + AttributeStartOffset + 8;
        for (var i = 0; i < AttributeCount; i++)
        {
            var variantRelativeOffsetOffset = variantRelativeOffsetTableOffset + i * sizeof(int);
            var variantRelativeOffset = BitConverter.ToInt32(Xmb2, (int)variantRelativeOffsetOffset);
            var variantOffset = variantRelativeOffsetOffset + variantRelativeOffset;
            var variant = Xmb2Variant.FromByteArray(Xmb2, variantOffset);

            if (variant.NameHash == nameHash)
            {
                return variant;
            }
        }

        return null;
    }

    /// <summary>
    /// Read the attribute element with a given name.
    /// </summary>
    /// <param name="name">The name of the attribute to get.</param>
    /// <returns>The parsed attribute.</returns>
    public Xmb2Variant GetAttributeByName(string name)
    {
        var nameHash = Cryptography.Hash32(name);
        return GetAttributeByName(nameHash);
    }

    /// <summary>
    /// Read the list of child elements sharing a given name.
    /// </summary>
    /// <param name="name">The name of the elements to read.</param>
    /// <param name="results">List into which to store the results.</param>
    public void GetElements(string name, IList<Xmb2Element> results)
    {
        for (var i = 0; i < ElementCount; i++)
        {
            var element = GetElementByIndex(i);
            if (element == null)
            {
                continue;
            }

            if (element.Name == name)
            {
                results.Add(element);
            }
        }
    }

    public IEnumerable<Xmb2Element> GetElements(string name)
    {
        for (var i = 0; i < ElementCount; i++)
        {
            var element = GetElementByIndex(i);
            if (element == null)
            {
                continue;
            }

            if (element.Name == name)
            {
                yield return element;
            }
        }
    }

    /// <summary>
    /// Read the list of child elements.
    /// </summary>
    public IList<Xmb2Element> GetElements()
    {
        var result = new List<Xmb2Element>();
        for (var i = 0; i < ElementCount; i++)
        {
            var element = GetElementByIndex(i);
            if (element == null)
            {
                continue;
            }

            result.Add(element);
        }

        return result;
    }

    /// <summary>
    /// Get the absolute offset to this element's name attribute.
    /// </summary>
    /// <returns>The absolute offset to this element's name attribute.</returns>
    private long GetNameAttributeOffset()
    {
        var attributeRelativeOffsetTableOffset = Offset + sizeof(int) + AttributeStartOffset;
        var attributeRelativeOffset = BitConverter.ToInt32(Xmb2, (int)attributeRelativeOffsetTableOffset);
        return attributeRelativeOffsetTableOffset + attributeRelativeOffset;
    }

    /// <summary>
    /// Get the value of this element as a string.
    /// </summary>
    /// <returns>The element's value as a string.</returns>
    public string GetTextValue()
    {
        var attribute = GetValueAttribute();
        return attribute?.GetTextValue();
    }

    /// <summary>
    /// Get the value of this element as a bool.
    /// </summary>
    /// <returns>The element's value as a bool.</returns>
    public bool GetBoolValue()
    {
        var attribute = GetValueAttribute();
        return attribute.ToBool();
    }

    /// <summary>
    /// Get the value of this element as an integer.
    /// </summary>
    /// <returns>The element's value as an integer.</returns>
    public int GetIntValue()
    {
        var attribute = GetValueAttribute();
        return attribute.ToInt();
    }

    /// <summary>
    /// Get the value of this element as an unsigned integer.
    /// </summary>
    /// <returns>The element's value as an unsigned integer.</returns>
    public uint GetUIntValue()
    {
        var attribute = GetValueAttribute();
        return attribute.ToUInt();
    }

    /// <summary>
    /// Get the value of this element as a float.
    /// </summary>
    /// <returns>The element's value as a float.</returns>
    public float GetFloatValue()
    {
        var attribute = GetValueAttribute();
        return attribute.ToFloat();
    }

    /// <summary>
    /// Get the value of this element as a double.
    /// </summary>
    /// <returns>The element's value as a double.</returns>
    public double GetDoubleValue()
    {
        var attribute = GetValueAttribute();
        return attribute.ToDouble();
    }

    /// <summary>
    /// Get the value of this element as a float4.
    /// </summary>
    /// <returns>The element's value as a float4.</returns>
    public float[] GetFloat4Value()
    {
        var attribute = GetValueAttribute();
        return attribute.TryGetFloat4();
    }

    /// <summary>
    /// Get this element's value attribute.
    /// </summary>
    /// <returns>The value attribute.</returns>
    private Xmb2Variant GetValueAttribute()
    {
        if (ElementCount != 0)
        {
            return null;
        }

        if (ElementStartOffset == 0)
        {
            return null;
        }

        var attributeOffset = Offset + ElementStartOffset;
        if (attributeOffset == 0)
        {
            return null;
        }

        return Xmb2Variant.FromByteArray(Xmb2, attributeOffset);
    }

    /// <summary>
    /// Get the name of the element.
    /// </summary>
    /// <returns>The element's name.</returns>
    private string GetName()
    {
        var nameAttributeOffset = GetNameAttributeOffset();
        var nameAttribute = Xmb2Variant.FromByteArray(Xmb2, nameAttributeOffset);

        if (nameAttribute == null)
        {
            return null;
        }

        if (nameAttribute.ValueType == Xmb2Variant.Type.String)
        {
            return nameAttribute.Value as string;
        }

        if (nameAttribute.ValueType != Xmb2Variant.Type.Bool)
        {
            return null;
        }

        var boolValue = (bool)nameAttribute.Value;
        return boolValue ? "True" : "False";
    }
}