using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Flagrum.Core.Entities.Xml;

public partial class XmlBinary
{
    private const bool EnableIeeeHack = true;
    private static readonly bool OptimizeSize = true;
    private static Crc64 crc = new();
    private Dictionary<ulong, int> dicAttributes;

    private Dictionary<ulong, int> dicElements;
    private Dictionary<ulong, int> dicVariants;
    private Dictionary<string, StringEntry> strings;

    public XmlBinary(XDocument document) : this()
    {
        ReadRootElement(document.Root);
        EvaluateOffsetsAndData();
    }

    public static byte[] ToXml(byte[] xmb)
    {
        using var stream = new MemoryStream(xmb);
        var document = new XmlBinary(new BinaryReader(stream)).Document;
        using var outputStream = new MemoryStream();
        document.Save(outputStream);
        return outputStream.ToArray();
    }

    private void ReadRootElement(XElement element)
    {
        dicElements = new Dictionary<ulong, int>();
        dicAttributes = new Dictionary<ulong, int>();
        dicVariants = new Dictionary<ulong, int>();
        strings = new Dictionary<string, StringEntry>();
        crc = new Crc64();
        RootElementIndex = ReadElement(element);
    }

    private int ReadElement(XElement xmlElement)
    {
        var element = new Element
        {
            NameStringOffset = AddString(xmlElement.Name.ToString()),
            Name = xmlElement.Name.ToString()
        };

        var elementStrType = string.Empty;
        if (xmlElement.HasAttributes)
        {
            element.AttributeTableIndex = _attributeIndexTable.Count;

            var attributeIndexList = new List<int>();
            foreach (var xmlAttribute in xmlElement.Attributes())
            {
                var valueType = GuessTypeFromValue(xmlAttribute.Value);

                var attribute = new Attribute
                {
                    NameStringOffset = AddString(xmlAttribute.Name.ToString()),
                    Name = xmlAttribute.Name.ToString(),
                    VariantOffset = GetOrAddVariantIndex(valueType, xmlAttribute.Value)
                };

                if (!xmlElement.HasElements && xmlAttribute.Name == "type")
                {
                    elementStrType = xmlAttribute.Value;
                }

                attributeIndexList.Add(AddComparable(dicAttributes, _attributes, attribute));
                element.AttributeCount++;
            }

            element.AttributeTableIndex = OptimizeSize ? FindMatch(attributeIndexList, _attributeIndexTable) : -1;
            if (element.AttributeTableIndex < 0)
            {
                element.AttributeTableIndex = _attributeIndexTable.Count;
                _attributeIndexTable.AddRange(attributeIndexList);
            }
        }

        if (xmlElement.HasElements)
        {
            var elementIndexList = xmlElement.Elements()
                .Select(x => ReadElement(x))
                .ToList();

            element.ElementCount = elementIndexList.Count;
            element.ElementTableIndex = OptimizeSize ? FindMatch(elementIndexList, _elementIndexTable) : -1;
            if (element.ElementTableIndex < 0)
            {
                element.ElementTableIndex = _elementIndexTable.Count;
                _elementIndexTable.AddRange(elementIndexList);
            }

            element.VariantOffset = GetOrAddVariantIndex(ValueType.Unknown, string.Empty);
        }
        else
        {
            var type = GetTypeFromAttribute(elementStrType, xmlElement.Value);
            element.VariantOffset = GetOrAddVariantIndex(type, xmlElement.Value);
        }

        if (OptimizeSize)
        {
            return AddComparable(dicElements, _elements, element);
        }

        _elements.Add(element);
        return _elements.Count - 1;
    }

    private void EvaluateOffsetsAndData()
    {
        StringData = WriteChunk(strings, (w, x) =>
        {
            x.Value.Offset = (int)w.BaseStream.Position;
            w.BaseStream.WriteCString(x.Key);
        });

        foreach (var element in _elements)
        {
            element.NameStringOffset = strings[element.Name].Offset;
        }

        foreach (var attribute in _attributes)
        {
            attribute.NameStringOffset = strings[attribute.Name].Offset;
        }

        foreach (var variant in _variants)
        {
            variant.NameStringOffset = strings[variant.Name].Offset;
            ProcessVariant(variant);
        }
    }

    public void Write(BinaryWriter writer)
    {
        const int TotalHeaderSize = 0x4C;

        var dataElements = WriteChunk(_elements, (w, x) => x.Write(w));
        var dataAttributes = WriteChunk(_attributes, (w, x) => x.Write(w));
        var dataVariants = WriteChunk(_variants, (w, x) => x.Write(w));
        var dataElementsTable = WriteChunk(_elementIndexTable, (w, x) => w.Write(x));
        var dataAttributesTable = WriteChunk(_attributeIndexTable, (w, x) => w.Write(x));

        var header = new Header
        {
            MagicCode = 0x00424D58,
            Elements = new HeaderEntry {Count = _elements.Count},
            Attributes = new HeaderEntry {Count = _attributes.Count},
            StringTable = new HeaderEntry {Count = StringData.Length},
            ElementIndexTable = new HeaderEntry {Count = _elementIndexTable.Count},
            AttributeIndexTable = new HeaderEntry {Count = _attributeIndexTable.Count},
            Variants = new HeaderEntry {Count = _variants.Count},
            RootElementIndex = RootElementIndex
        };

        var offset = TotalHeaderSize;
        header.Elements.Offset = offset;

        offset += dataElements.Length;
        header.Attributes.Offset = offset;

        offset += dataAttributes.Length;
        header.Variants.Offset = offset;

        offset += dataVariants.Length;
        header.StringTable.Offset = offset;

        offset += StringData.Length;
        header.ElementIndexTable.Offset = offset;

        offset += dataElementsTable.Length;
        header.AttributeIndexTable.Offset = offset;

        header.Write(writer);
        writer.Write(0); // reserved data
        writer.Write(0); // reserved data
        writer.Write(0); // reserved data
        writer.Write(0); // reserved data
        Debug.Assert(writer.BaseStream.Position == TotalHeaderSize);

        writer.Write(dataElements);
        writer.Write(dataAttributes);
        writer.Write(dataVariants);
        writer.Write(StringData);
        writer.Write(dataElementsTable);
        writer.Write(dataAttributesTable);
    }

    private byte[] WriteChunk<T>(IEnumerable<T> collection, Action<BinaryWriter, T> writeFunc)
    {
        var stream = new MemoryStream(0x10000);
        var writer = new BinaryWriter(stream);
        foreach (var item in collection)
        {
            writeFunc(writer, item);
        }

        stream.Position = 0;
        return new BinaryReader(stream).ReadBytes((int)stream.Length);
    }

    private int GetOrAddVariantIndex(ValueType type, string value)
    {
        return AddComparable(dicVariants, _variants, new Variant
        {
            Type = type,
            NameStringOffset = AddString(value),
            Name = value
        });
    }

    private int AddString(string str)
    {
        int value;
        if (!strings.TryGetValue(str, out var entry))
        {
            strings[str] = new StringEntry
            {
                Index = value = strings.Count
            };
        }
        else
        {
            value = entry.Index;
        }

        return value;
    }

    public static int FindMatch(List<int> pattern, List<int> list)
    {
        // Unroll the nested for loop to achieve more performance
        switch (pattern.Count)
        {
            case 0:
                return 0;
            case 1:
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i] == pattern[0])
                    {
                        return i;
                    }
                }

                break;
            case 2:
                for (var i = 0; i < list.Count - 1; i++)
                {
                    if (list[i] == pattern[0] &&
                        list[i + 1] == pattern[1])
                    {
                        return i;
                    }
                }

                break;
            case 3:
                for (var i = 0; i < list.Count - 2; i++)
                {
                    if (list[i] == pattern[0] &&
                        list[i + 1] == pattern[1] &&
                        list[i + 2] == pattern[2])
                    {
                        return i;
                    }
                }

                break;
            case 4:
                for (var i = 0; i < list.Count - 3; i++)
                {
                    if (list[i] == pattern[0] &&
                        list[i + 1] == pattern[1] &&
                        list[i + 2] == pattern[2] &&
                        list[i + 3] == pattern[3])
                    {
                        return i;
                    }
                }

                break;
            case 8: // used often
                for (var i = 0; i < list.Count - 3; i++)
                {
                    if (list[i] == pattern[0] &&
                        list[i + 1] == pattern[1] &&
                        list[i + 2] == pattern[2] &&
                        list[i + 3] == pattern[3] &&
                        list[i + 4] == pattern[4] &&
                        list[i + 5] == pattern[5] &&
                        list[i + 6] == pattern[6] &&
                        list[i + 7] == pattern[7])
                    {
                        return i;
                    }
                }

                break;
            default:
                for (int i = 0, j, k; i <= list.Count - pattern.Count; i++)
                {
                    if (list[i] != pattern[0])
                    {
                        continue;
                    }

                    j = 1;
                    k = i + 1;
                    do
                    {
                        if (j >= pattern.Count)
                        {
                            return i;
                        }
                    } while (list[k++] == pattern[j++]);
                }

                break;
        }

        return -1;
    }

    private static int AddComparable<T>(Dictionary<ulong, int> dictionary, List<T> collection, T item)
        where T : IComparable<T>, IHashable
    {
        AddComparable(dictionary, collection, item, out var result);
        return result;
    }

    private static bool AddComparable<T>(Dictionary<ulong, int> dictionary, List<T> collection, T item, out int index)
        where T : IComparable<T>, IHashable
    {
        var hash = item.GetHash(crc);
        if (!dictionary.TryGetValue(hash, out index))
        {
            dictionary[hash] = index = collection.Count;
            collection.Add(item);
            return true;
        }

        return false;
    }

    private static ValueType GetTypeFromAttribute(string type, string value)
    {
        switch (type)
        {
            case "bool":
                return ValueType.Bool;
            default:
                return GuessTypeFromValue(value);
        }
    }

    public static ValueType GuessTypeFromValue(string value)
    {
        switch (value)
        {
            case null:
                return ValueType.Unknown;
            case "true":
            case "false":
            case "True":
            case "False":
                return ValueType.Bool;
            default:
                if (value.Length > 0)
                {
                    if (value.Contains(",") || value.Contains("."))
                    {
                        // During an acceptance test on debug_wm.exml I found that
                        // the value -8.651422E-06,-145,8.651422E-06,1 is recognized
                        // as Float4 by my parser, but the actual file has Unknown as
                        // a type, which is odd. My opinion is that the parser in SQEX
                        // house is so stupid that recognize the f.ffE-XX as a string.
                        // This is why there is this awkard hack...
                        if (EnableIeeeHack && value.Contains("E"))
                        {
                            return ValueType.Unknown;
                        }

                        var values = value.Split(',');
                        if (float.TryParse(values[0], out var v))
                        {
                            switch (values.Length)
                            {
                                case 1: return ValueType.Float;
                                case 2: return ValueType.Float2;
                                case 3: return ValueType.Float3;
                                case 4: return ValueType.Float4;
                            }
                        }
                    }
                    else
                    {
                        if (value[0] == '-')
                        {
                            if (int.TryParse(value, out var v))
                            {
                                return ValueType.Signed;
                            }
                        }
                        else
                        {
                            if (uint.TryParse(value, out var v))
                            {
                                return ValueType.Unsigned;
                            }
                        }
                    }
                }

                return ValueType.Unknown;
        }
    }

    private static void ProcessVariant(Variant variant)
    {
        switch (variant.Type)
        {
            case ValueType.Bool:
                ProcessBoolean(variant);
                break;
            case ValueType.Signed:
                ProcessInteger(variant);
                break;
            case ValueType.Unsigned:
                ProcessInteger(variant);
                break;
            case ValueType.Float:
            case ValueType.Float2:
            case ValueType.Float3:
            case ValueType.Float4:
                ProcessFloat(variant);
                break;
        }
    }

    private static void ProcessBoolean(Variant variant)
    {
        if (!bool.TryParse(variant.Name.ToLower(), out var b))
        {
            throw new ArgumentException($"Unable to convert {variant.Name} for type {variant.Type}");
        }

        variant.Value1 = b ? 1 : 0;
    }

    private static void ProcessInteger(Variant variant)
    {
        if (!int.TryParse(variant.Name, out var v))
        {
            throw new ArgumentException($"Unable to convert {variant.Name} for type {variant.Type}");
        }

        variant.Value1 = v;
    }

    private static void ProcessFloat(Variant variant)
    {
        FromFloatArray(variant, ConvertFloatsFromString(variant.Name));
    }

    private static float[] ConvertFloatsFromString(string value)
    {
        var values = value.Split(',');
        var fArray = new float[values.Length];
        for (var i = 0; i < fArray.Length; i++)
        {
            if (!float.TryParse(values[i], out var v))
            {
                throw new ArgumentException($"Unable to convert {value}");
            }

            fArray[i] = v;
        }

        return fArray;
    }

    private static void FromFloatArray(Variant variant, float[] f)
    {
        if (f.Length > 0)
        {
            variant.Value1 = FromFloat(f[0]);
        }

        if (f.Length > 1)
        {
            variant.Value2 = FromFloat(f[1]);
        }

        if (f.Length > 2)
        {
            variant.Value3 = FromFloat(f[2]);
        }

        if (f.Length > 3)
        {
            variant.Value4 = FromFloat(f[3]);
        }
    }

    private static int FromFloat(float f)
    {
        var data = BitConverter.GetBytes(f);
        var v = (data[0] & 0xFF) << 0;
        v |= (data[1] & 0xFF) << 8;
        v |= (data[2] & 0xFF) << 16;
        v |= (data[3] & 0xFF) << 24;

        return v;
    }

    private class StringEntry
    {
        public int Index { get; set; }

        public int Offset { get; set; }
    }
}