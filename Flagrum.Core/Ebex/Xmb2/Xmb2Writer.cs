using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Flagrum.Core.Ebex.Xmb2;

public class Xmb2Writer
{
    private readonly Element RootElement;
    private static string[] _compiledExpressions;
    private static int _compiledExpressionsCounter;

    public Xmb2Writer(byte[] xml)
    {
        var xmlString = Encoding.UTF8.GetString(xml);
        _compiledExpressions = new Regex("<compiledExpression_ type=\"string\">(.+?)</compiledExpression_>")
            .Matches(xmlString)
            .Select(m => m.Groups[1].Value)
            .ToArray();

        xmlString = Regex.Replace(xmlString,
            "(<compiledExpression_ type=\"string\">)(.+?)(</compiledExpression_>)",
            m => m.Groups[1].Value + m.Groups[3].Value);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
        var root = XDocument.Load(stream).Root;
        EVEAList = new List<List<int>>();
        OffsetList = new List<int>();
        VariantList = new List<int>();
        ColorFloat4List = new List<List<float>>();
        CStringList = new List<string>();
        VariantTable = new List<Variant>();
        RootElement = ReadElement(root);
        PositionOfTHOs = 16 + ElementCount * 12;
        var num = ElementCount * 2 * 9 + AttributeCount * 9;
        var padding = GetPadding(PositionOfTHOs + num, 4);
        PositionOfElementTable = PositionOfTHOs + num + padding;
        PositionOfVariantTable = PositionOfElementTable + (ElementCount - 1) * 4;
        PositionOfNums = PositionOfVariantTable + ElementCount * 4 + AttributeCount * 4;
        PositionOfCStrings = PositionOfNums + ColorFloat4List.Count * 16;
        PostOrderTraverse(RootElement);
    }

    private List<Variant> VariantTable { get; }

    private List<List<float>> ColorFloat4List { get; }

    private List<List<int>> EVEAList { get; }

    private List<int> OffsetList { get; }

    private List<int> VariantList { get; }

    private List<string> CStringList { get; }

    private int PositionOfTHOs { get; }

    private int PositionOfElementTable { get; }

    private int PositionOfVariantTable { get; }

    private int PositionOfNums { get; }

    private int PositionOfCStrings { get; }

    private string FileName { get; }

    private int ElementCount { get; set; }

    private int AttributeCount { get; set; }

    private int ElementIndex { get; set; }

    private int NextCString { get; set; }

    private int VariantIndex { get; set; }

    private int VarTableIndex { get; set; }

    private int ElTableIndex { get; set; }

    private int JumpOffsetIndex { get; set; }

    private Element ReadElement(XElement node)
    {
        var el = Element.Read(node);
        ++ElementCount;
        el.ValueType = (string)node.Attribute((XName)"type");
        if (el.Count > 0)
        {
            foreach (var element1 in node.Elements())
            {
                var element2 = ReadElement(element1);
                el.ChildElementList.Add(element2);
            }
        }

        foreach (var xattribute in node.Attributes().Select((Func<XAttribute, XAttribute>)(at => at)))
        {
            var attribute = new Attribute();
            ++AttributeCount;
            attribute.Name = xattribute.Name.ToString();
            attribute.Value = xattribute.Value;
            attribute.Hash = xattribute.ToString().GetHashCode();
            el.AttributeList.Add(attribute);
            if (attribute.Value == "float4" || attribute.Value == "Color")
            {
                ColorFloat4(el);
            }
        }

        el.AttributeCount = el.AttributeList.Count();
        GetCStrings(el);
        return el;
    }

    private void GetCStrings(Element el)
    {
        var utF8 = Encoding.UTF8;
        CStringList.Add(el.Name);
        el.CStringNameOffset = NextCString;
        NextCString += utF8.GetByteCount(el.Name) + 1;
        for (var index = el.AttributeList.Count - 1; index >= 0; --index)
        {
            int result;
            if ((el.AttributeList[index].Value == "string" || el.AttributeList[index].Value == "enum" ||
                 el.AttributeList[index].Value == "Fixid") && el.Value != "" && el.Value != "True" &&
                el.Value != "False" && !int.TryParse(el.Value, out result))
            {
                CStringList.Add(el.Value);
                el.CStringValueOffset = NextCString;
                NextCString += utF8.GetByteCount(el.Value) + 1;
            }

            if (el.AttributeList[index].Value != "True" && el.AttributeList[index].Value != "False" &&
                !int.TryParse(el.AttributeList[index].Value, out result))
            {
                CStringList.Add(el.AttributeList[index].Value);
                el.AttributeList[index].CStringValueOffset = NextCString;
                NextCString += utF8.GetByteCount(el.AttributeList[index].Value) + 1;
            }
        }
    }

    public int GetPadding(int ending, int mod)
    {
        return ending % mod == 0 ? 0 : mod - (ending + mod) % mod;
    }

    public uint GetNameHash(string NameHash)
    {
        return new Fnv1a().GetDigest(NameHash);
    }

    private void PostOrderTraverse(Element el)
    {
        var intList = new List<int>();
        var elementList = new List<Element>();
        foreach (var childElement in el.ChildElementList)
        {
            PostOrderTraverse(childElement);
            childElement.ThirdOffset = 16 + ElTableIndex * 12;
            intList.Add(childElement.ThirdOffset);
            elementList.Add(childElement);
            childElement.JumpOffset = ElTableIndex;
            ++ElTableIndex;
        }

        for (var index = 0; index < elementList.Count; ++index)
        {
            elementList[index].JumpOffset = JumpOffsetIndex;
            ++JumpOffsetIndex;
        }

        foreach (var num in intList)
        {
            OffsetList.Add(num);
        }

        if (el.Value == "" && el.ChildElementList.Count() == 0)
        {
            el.ElementTableOffset = 0;
        }
        else if (el.ChildElementList.Count() > 0)
        {
            el.ElementTableOffset = PositionOfElementTable + el.ChildElementList[0].JumpOffset * 4;
        }
        else
        {
            el.ElementTableOffset += PositionOfTHOs + ElementIndex * 9;
        }

        el.VariantTableOffset = PositionOfVariantTable + VariantIndex * 4;
        VariantList.Add(PositionOfTHOs + 9 + VarTableIndex * 9);
        foreach (var attribute in el.AttributeList)
        {
            ++VarTableIndex;
            VariantList.Add(PositionOfTHOs + 9 + VarTableIndex * 9);
        }

        VarTableIndex += 2;
        ElementIndex += 2 + el.AttributeCount;
        VariantIndex += 1 + el.AttributeCount;
        EVEAList.Add(new List<int>
        {
            el.ElementTableOffset,
            el.VariantTableOffset,
            el.ChildElementList.Count(),
            0,
            el.AttributeCount
        });
        CallSetVariant(el);
    }

    private void ColorFloat4(Element el)
    {
        var floatList = new List<float>();
        var str = el.Value;
        var chArray = new char[1] {','};
        foreach (var s in str.Split(chArray))
        {
            var num = float.Parse(s, NumberStyles.Float);
            floatList.Add(num);
        }

        ColorFloat4List.Add(floatList);
        el.ColorFloat4Offset = (ColorFloat4List.Count() - 1) * 16;
    }

    private void CallSetVariant(Element el)
    {
        SetVariant(el, "elValue", el.Value, el.CStringValueOffset);
        SetVariant(el, "elName", el.Value, el.CStringValueOffset);
        foreach (var attribute in el.AttributeList)
        {
            SetVariant(el, attribute.Name, attribute.Value, attribute.CStringValueOffset);
        }
    }

    private void SetVariant(Element el, string variantType, string value, int offset)
    {
        var variant = new Variant();
        variant.Hash = GetNameHash(variantType);
        if (variantType == "elName")
        {
            variant.Type = 1;
            variant.Hash = GetNameHash(el.Name);
            variant.OffsetOrVal = el.CStringNameOffset + PositionOfCStrings;
        }
        else
        {
            bool result1;
            if (bool.TryParse(value, out result1))
            {
                variant.Type = 2;
                variant.OffsetOrVal = Convert.ToInt32(result1);
            }
            else
            {
                uint result2;
                if (uint.TryParse(value, out result2))
                {
                    variant.Type = 4;
                    variant.OffsetOrVal = result2;
                }
                else
                {
                    int result3;
                    if (int.TryParse(value, out result3))
                    {
                        variant.Type = 3;
                        variant.OffsetOrVal = Convert.ToInt32(result3);
                    }
                    else
                    {
                        float result4;
                        if (float.TryParse(value.Split(',')[0], out result4))
                        {
                            switch (value.Split(',').Length)
                            {
                                case 1:
                                    variant.Type = 5;
                                    variant.OffsetOrVal = result4;
                                    break;
                                case 2:
                                    variant.Type = 7;
                                    variant.OffsetOrVal = el.ColorFloat4Offset + PositionOfNums;
                                    break;
                                case 3:
                                    variant.Type = 8;
                                    variant.OffsetOrVal = el.ColorFloat4Offset + PositionOfNums;
                                    break;
                                case 4:
                                    variant.Type = 9;
                                    variant.OffsetOrVal = el.ColorFloat4Offset + PositionOfNums;
                                    break;
                            }
                        }
                        else
                        {
                            variant.Type = 1;
                            variant.OffsetOrVal = offset + PositionOfCStrings;
                        }
                    }
                }
            }
        }

        if (variantType == "elValue")
        {
            variant.Hash = 0U;
        }

        VariantTable.Add(variant);
    }

    public byte[] Write()
    {
        const string tag = "XMB2";

        using var memoryStream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(memoryStream);

        binaryWriter.Write(tag.ToCharArray());
        binaryWriter.Write(0);
        binaryWriter.Write(0);
        binaryWriter.Write(PositionOfTHOs - 24);
        foreach (var evea in EVEAList)
        {
            if (evea[0] == 0)
            {
                binaryWriter.Write(0);
            }
            else
            {
                binaryWriter.Write(evea[0] - (int)binaryWriter.BaseStream.Position);
            }

            binaryWriter.Write(evea[1] - (int)binaryWriter.BaseStream.Position);
            binaryWriter.Write((ushort)evea[2]);
            binaryWriter.Write((byte)evea[3]);
            binaryWriter.Write((byte)evea[4]);
        }

        foreach (var variant in VariantTable)
        {
            binaryWriter.Write((byte)variant.Type);
            binaryWriter.Write(variant.Hash);
            if (variant.Type == 2 || variant.Type == 3)
            {
                binaryWriter.Write(Convert.ToInt32(variant.OffsetOrVal));
            }
            else if (variant.Type == 4)
            {
                binaryWriter.Write(Convert.ToUInt32(variant.OffsetOrVal));
            }
            else if (variant.Type == 5)
            {
                binaryWriter.Write(Convert.ToSingle(variant.OffsetOrVal));
            }
            else
            {
                binaryWriter.Write(Convert.ToInt32(variant.OffsetOrVal) - (int)binaryWriter.BaseStream.Position);
            }
        }

        binaryWriter.BaseStream.Position = PositionOfElementTable;
        foreach (var offset in OffsetList)
        {
            binaryWriter.Write(offset - (int)binaryWriter.BaseStream.Position);
        }

        foreach (var variant in VariantList)
        {
            binaryWriter.Write(variant - (int)binaryWriter.BaseStream.Position);
        }

        foreach (var colorFloat4 in ColorFloat4List)
        {
            foreach (var num2 in colorFloat4)
            {
                binaryWriter.Write(num2);
            }
        }

        foreach (var cstring in CStringList)
        {
            binaryWriter.Write(cstring.ToCharArray());
            binaryWriter.Write((byte)0);
        }

        var padding = GetPadding((int)binaryWriter.BaseStream.Position, 16);
        var buffer = new byte[padding];
        binaryWriter.Write(buffer, 0, padding);

        return memoryStream.ToArray();
    }

    private class Variant
    {
        public int Type { get; set; }

        public uint Hash { get; set; }

        public object OffsetOrVal { get; set; }
    }

    private class Element
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public List<Element> ChildElementList { get; set; }

        public List<Attribute> AttributeList { get; set; }

        public int ElementTableOffset { get; set; }

        public int VariantTableOffset { get; set; }

        public int Count { get; set; }

        public int AttributeCount { get; set; }

        public string ValueType { get; set; }

        public int ThirdOffset { get; set; }

        public int CStringValueOffset { get; set; }

        public int CStringNameOffset { get; set; }

        public int ColorFloat4Offset { get; set; }

        public int JumpOffset { get; set; }

        public static Element Read(XElement node)
        {
            return new Element
            {
                Name = node.Name.ToString(),
                Value = node.Name.ToString() == "compiledExpression_"
                    ? _compiledExpressions[_compiledExpressionsCounter++]
                    : node.ShallowValue()
                    .Replace("&amp;", "&")
                    .Replace("&lt;", "<")
                    .Replace("&gt;", ">")
                    .Replace("&quot;", "&#34;")
                    .Replace("&apos;", "'")
                    .Replace("\"", "\""),
                ChildElementList = new List<Element>(),
                AttributeList = new List<Attribute>(),
                Count = node.Elements().Count()
            };
        }
    }

    private class Attribute
    {
        public int Hash { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public int Type { get; set; }

        public int Offset { get; set; }

        public int CStringValueOffset { get; set; }
    }
}

public class Fnv1a : IHashing<uint>
{
    private uint hval;

    public uint GetDigest()
    {
        return hval;
    }

    public void Init()
    {
        hval = 2166136261U;
    }

    public void Write(byte[] data, uint offset, uint size)
    {
        for (var index = 0; index < size; ++index)
        {
            WriteByte(data[offset + index]);
        }
    }

    public void WriteByte(byte b)
    {
        hval ^= b;
        hval *= 16777619U;
    }
}

public interface IHashing<T>
{
    void Init();

    void WriteByte(byte b);

    void Write(byte[] data, uint offset, uint size);

    T GetDigest();
}

public static class HashingExtension
{
    public static T GetDigest<T>(this IHashing<T> hashing, string text)
    {
        return hashing.GetDigest(text, Encoding.UTF8);
    }

    public static T GetDigest<T>(this IHashing<T> hashing, string text, Encoding encoding)
    {
        hashing.Init();
        var bytes = encoding.GetBytes(text);
        hashing.Write(bytes, 0U, (uint)bytes.Length);
        return hashing.GetDigest();
    }

    public static T GetDigest<T>(this IHashing<T> hashing, params int[] values)
    {
        hashing.Init();
        hashing.Write(values);
        return hashing.GetDigest();
    }

    public static void Write<T>(this IHashing<T> hashing, params int[] values)
    {
        for (var index = 0; index < values.Length; ++index)
        {
            var num = values[index];
            hashing.WriteByte((byte)num);
            hashing.WriteByte((byte)(num >> 8));
            hashing.WriteByte((byte)(num >> 16));
            hashing.WriteByte((byte)(num >> 24));
        }
    }
}

public static class XElementExtensions
{
    public static string ShallowValue(this XElement xe)
    {
        return xe.Nodes().OfType<XText>().Aggregate(new StringBuilder(),
            (Func<StringBuilder, XText, StringBuilder>)((s, c) => s.Append(c)),
            (Func<StringBuilder, string>)(s => s.ToString()));
    }
}