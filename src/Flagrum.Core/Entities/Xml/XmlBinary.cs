using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Flagrum.Core.Entities.Xml2;

namespace Flagrum.Core.Entities.Xml;

public partial class XmlBinary
{
    public enum ValueType
    {
        Unknown,
        Bool,
        Signed,
        Unsigned,
        Float,
        Float2Alt,
        Float2,
        Float3,
        Float4
    }

    private readonly List<int> _attributeIndexTable;
    private readonly List<Attribute> _attributes;
    private readonly List<int> _elementIndexTable;

    private readonly List<Element> _elements;
    private readonly List<Variant> _variants;

    private XmlBinary()
    {
        _elements = new List<Element>();
        _attributes = new List<Attribute>();
        _variants = new List<Variant>();
        _elementIndexTable = new List<int>();
        _attributeIndexTable = new List<int>();
    }

    public XmlBinary(BinaryReader reader)
    {
        var header = Header.Read(reader);
        RootElementIndex = header.RootElementIndex;

        _elements = ReadTable(header.Elements, reader, r => Element.Read(r));
        _attributes = ReadTable(header.Attributes, reader, r => Attribute.Read(r));
        _variants = ReadTable(header.Variants, reader, r => Variant.Read(r));
        _elementIndexTable = ReadTable(header.ElementIndexTable, reader, r => r.ReadInt32());
        _attributeIndexTable = ReadTable(header.AttributeIndexTable, reader, r => r.ReadInt32());

        reader.BaseStream.Position = header.StringTable.Offset;
        StringData = reader.ReadBytes(header.StringTable.Count);

        ReadStrings(_elements, StringData, (x, s) => x.Name = s, x => x.NameStringOffset);
        ReadStrings(_attributes, StringData, (x, s) => x.Name = s, x => x.NameStringOffset);
        ReadStrings(_variants, StringData, (x, s) => x.Name = s, x => x.NameStringOffset);
    }

    public int RootElementIndex { get; private set; }

    public IEnumerable<Element> Elements => _elements;

    public IEnumerable<Attribute> Attributes => _attributes;

    public IEnumerable<Variant> Variants => _variants;

    public IEnumerable<int> ElementIndexTable => _elementIndexTable;

    public IEnumerable<int> AttributeIndexTable => _attributeIndexTable;

    public byte[] StringData { get; private set; }

    public XDocument Document => new(ReadRootNode());

    private XElement ReadNode(Element xmbElement)
    {
        var element = new XElement(xmbElement.Name);

        var variantValue = _variants[xmbElement.VariantOffset];
        element.Value = variantValue.Name;

        for (var i = 0; i < xmbElement.AttributeCount; i++)
        {
            var attributeIndex = _attributeIndexTable[xmbElement.AttributeTableIndex + i];
            var attribute = _attributes[attributeIndex];
            var value = _variants[attribute.VariantOffset];
            element.Add(new XAttribute(attribute.Name, value.Name));
        }

        for (var i = 0; i < xmbElement.ElementCount; i++)
        {
            var elementIndex = _elementIndexTable[xmbElement.ElementTableIndex + i];
            var node = ReadNode(_elements[elementIndex]);
            element.Add(node);
        }

        return element;
    }

    private XElement ReadRootNode()
    {
        return ReadNode(_elements[RootElementIndex]);
    }

    private static List<T> ReadTable<T>(HeaderEntry entry, BinaryReader reader, Func<BinaryReader, T> funcRead)
    {
        var list = (List<T>)Activator.CreateInstance(typeof(List<>).MakeGenericType(typeof(T)));

        reader.BaseStream.Position = entry.Offset;
        for (var i = 0; i < entry.Count; i++)
        {
            list.Add(funcRead(reader));
        }

        return list;
    }

    private static void ReadStrings<T>(IEnumerable<T> table, byte[] strData, Action<T, string> funcSetString,
        Func<T, int> funcGetStrIndex)
    {
        foreach (var item in table)
        {
            funcSetString(item, ReadString(item, strData, funcGetStrIndex));
        }
    }

    private static string ReadString<T>(T entry, byte[] strData, Func<T, int> funcGetStrIndex)
    {
        var offset = funcGetStrIndex(entry);
        var count = 0;

        while (strData[offset + count] != 0)
        {
            count++;
        }

        return Encoding.UTF8.GetString(strData, offset, count);
    }

    private static float ToFloat(int n)
    {
        return BitConverter.ToSingle(BitConverter.GetBytes(n), 0);
    }

    public interface IHashable
    {
        ulong GetHash(IHashing<ulong> hashing);
    }

    private class HeaderEntry
    {
        public int Offset { get; set; }
        public int Count { get; set; }

        public static HeaderEntry Read(BinaryReader reader)
        {
            return new HeaderEntry
            {
                Offset = reader.ReadInt32(),
                Count = reader.ReadInt32()
            };
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Offset);
            writer.Write(Count);
        }
    }

    private class Header
    {
        public uint MagicCode { get; internal set; }
        public uint RESERVED { get; internal set; }
        public HeaderEntry Elements { get; internal set; }
        public HeaderEntry Attributes { get; internal set; }
        public HeaderEntry StringTable { get; internal set; }
        public HeaderEntry ElementIndexTable { get; internal set; }
        public HeaderEntry AttributeIndexTable { get; internal set; }
        public HeaderEntry Variants { get; internal set; }
        public int RootElementIndex { get; internal set; }

        public static Header Read(BinaryReader reader)
        {
            var magicCode = reader.ReadUInt32();
            if (magicCode != 0x00424D58)
            {
                throw new ArgumentException("Not a XMB file");
            }

            return new Header
            {
                MagicCode = magicCode,
                RESERVED = reader.ReadUInt32(),
                Elements = HeaderEntry.Read(reader),
                Attributes = HeaderEntry.Read(reader),
                StringTable = HeaderEntry.Read(reader),
                ElementIndexTable = HeaderEntry.Read(reader),
                AttributeIndexTable = HeaderEntry.Read(reader),
                Variants = HeaderEntry.Read(reader),
                RootElementIndex = reader.ReadInt32()
            };
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(MagicCode);
            writer.Write(RESERVED);
            Elements.Write(writer);
            Attributes.Write(writer);
            StringTable.Write(writer);
            ElementIndexTable.Write(writer);
            AttributeIndexTable.Write(writer);
            Variants.Write(writer);
            writer.Write(RootElementIndex);
        }
    }

    public class Element : IComparable<Element>, IHashable
    {
        public long Reserved { get; set; }
        public int AttributeTableIndex { get; set; }
        public int AttributeCount { get; set; }
        public int ElementTableIndex { get; set; }
        public int ElementCount { get; set; }
        public int NameStringOffset { get; set; }
        public int VariantOffset { get; set; }

        public string Name { get; set; }

        public int CompareTo(Element other)
        {
            return AttributeTableIndex == other.AttributeTableIndex &&
                   ElementTableIndex == other.ElementTableIndex &&
                   AttributeCount == other.AttributeCount &&
                   ElementCount == other.ElementCount &&
                   NameStringOffset == other.NameStringOffset &&
                   VariantOffset == other.VariantOffset
                ? 0
                : 1;
        }

        public ulong GetHash(IHashing<ulong> hashing)
        {
            return hashing.GetDigest(AttributeTableIndex, AttributeCount, ElementTableIndex, ElementCount,
                NameStringOffset, VariantOffset);
        }

        public override string ToString()
        {
            return Name;
        }

        public static Element Read(BinaryReader reader)
        {
            return new Element
            {
                Reserved = reader.ReadInt64(),
                AttributeTableIndex = reader.ReadInt32(),
                AttributeCount = reader.ReadInt32(),
                ElementTableIndex = reader.ReadInt32(),
                ElementCount = reader.ReadInt32(),
                NameStringOffset = reader.ReadInt32(),
                VariantOffset = reader.ReadInt32()
            };
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((long)0);
            writer.Write(AttributeTableIndex);
            writer.Write(AttributeCount);
            writer.Write(ElementTableIndex);
            writer.Write(ElementCount);
            writer.Write(NameStringOffset);
            writer.Write(VariantOffset);
        }
    }

    public class Attribute : IComparable<Attribute>, IHashable
    {
        public long Reserved { get; internal set; }
        public int NameStringOffset { get; internal set; }
        public int VariantOffset { get; internal set; }

        public string Name { get; internal set; }

        public int CompareTo(Attribute other)
        {
            return NameStringOffset == other.NameStringOffset &&
                   VariantOffset == other.VariantOffset
                ? 0
                : 1;
        }

        public ulong GetHash(IHashing<ulong> hashing)
        {
            return hashing.GetDigest(NameStringOffset, VariantOffset);
        }

        public override string ToString()
        {
            return Name;
        }

        public static Attribute Read(BinaryReader reader)
        {
            return new Attribute
            {
                Reserved = reader.ReadInt64(),
                NameStringOffset = reader.ReadInt32(),
                VariantOffset = reader.ReadInt32()
            };
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((long)0);
            writer.Write(NameStringOffset);
            writer.Write(VariantOffset);
        }
    }

    public class Variant : IComparable<Variant>, IHashable
    {
        public ValueType Type { get; internal set; }
        public int NameStringOffset { get; internal set; }
        public int Value1 { get; internal set; }
        public int Value2 { get; internal set; }
        public int Value3 { get; internal set; }
        public int Value4 { get; internal set; }

        public string Name { get; internal set; }

        public int CompareTo(Variant other)
        {
            return NameStringOffset == other.NameStringOffset &&
                   Type == other.Type
                ? 0
                : 1;
        }

        public ulong GetHash(IHashing<ulong> hashing)
        {
            return hashing.GetDigest(NameStringOffset, (int)Type);
        }

        public override string ToString()
        {
            return Name;
        }

        public static Variant Read(BinaryReader reader)
        {
            return new Variant
            {
                Type = (ValueType)reader.ReadInt32(),
                NameStringOffset = reader.ReadInt32(),
                Value1 = reader.ReadInt32(),
                Value2 = reader.ReadInt32(),
                Value3 = reader.ReadInt32(),
                Value4 = reader.ReadInt32()
            };
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((int)Type);
            writer.Write(NameStringOffset);
            writer.Write(Value1);
            writer.Write(Value2);
            writer.Write(Value3);
            writer.Write(Value4);
        }
    }
}