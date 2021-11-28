    using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Luminous.Xml
{
    /// <summary>
    /// An XMB2 attribute.
    /// </summary>
    [DebuggerDisplay("{GetTextValue()}")]
    public sealed class Xmb2Variant
    {
        internal enum Type
        {
            ElementValue = 0,
            String = 1,
            Bool = 2,
            Signed = 3,
            Unsigned = 4,
            Float = 5,
            Double = 6,
            Float2 = 7,
            Float3 = 8,
            Float4 = 9
        }

        /// <summary>
        /// The type of the attribute.
        /// </summary>
        internal Type ValueType { get; private set; }

        /// <summary>
        /// The FNV-1A hash of the attribute name.
        /// </summary>
        internal uint NameHash { get; }

        /// <summary>
        /// The value of the attribute.
        /// </summary>
        internal object Value { get; private set; }

        private Xmb2Variant(Type valueType, uint nameHash, object value)
        {
            this.ValueType = valueType;
            this.NameHash = nameHash;
            this.Value = value;
        }

        /// <summary>
        /// Parse an attribute from an XMB2 document.
        /// </summary>
        /// <param name="xmb2">The XMB2 document to read from.</param>
        /// <param name="startIndex">The index at which the attribute starts.</param>
        /// <returns>The parsed attribute.</returns>
        public static Xmb2Variant FromByteArray(byte[] xmb2, long startIndex)
        {
            var valueType = (Type)xmb2[startIndex];
            var nameHash = BitConverter.ToUInt32(xmb2, (int)startIndex + 1);
            object value;

            var valueOffset = startIndex + 1 + sizeof(uint);
            switch (valueType)
            {
                case Type.String:
                    {
                        var realValueRelativeOffset = BitConverter.ToInt32(xmb2, (int)valueOffset);
                        var realValueOffset = (int)valueOffset + realValueRelativeOffset;
                        value = ReadCString(xmb2, realValueOffset);
                    }

                    break;
                case Type.Bool:
                    value = xmb2[valueOffset] != 0;
                    break;
                case Type.Signed:
                    value = BitConverter.ToInt32(xmb2, (int)valueOffset);
                    break;
                case Type.Unsigned:
                    value = BitConverter.ToUInt32(xmb2, (int)valueOffset);
                    break;
                case Type.Float:
                    value = BitConverter.ToSingle(xmb2, (int)valueOffset);
                    break;
                case Type.Double:
                    {
                        var realValueRelativeOffset = BitConverter.ToInt32(xmb2, (int)valueOffset);
                        value = BitConverter.ToDouble(xmb2, (int)valueOffset + realValueRelativeOffset);
                    }

                    break;
                case Type.Float2:
                    {
                        var realValueRelativeOffset = BitConverter.ToInt32(xmb2, (int)valueOffset);
                        var realValueOffset = (int)valueOffset + realValueRelativeOffset;
                        var float2 = new float[2];
                        float2[0] = BitConverter.ToSingle(xmb2, realValueOffset);
                        float2[1] = BitConverter.ToSingle(xmb2, realValueOffset + sizeof(float));
                        value = float2;
                    }

                    break;
                case Type.Float3:
                    {
                        var realValueRelativeOffset = BitConverter.ToInt32(xmb2, (int)valueOffset);
                        var realValueOffset = (int)valueOffset + realValueRelativeOffset;
                        var float3 = new float[3];
                        float3[0] = BitConverter.ToSingle(xmb2, realValueOffset);
                        float3[1] = BitConverter.ToSingle(xmb2, realValueOffset + sizeof(float));
                        float3[2] = BitConverter.ToSingle(xmb2, realValueOffset + sizeof(float) * 2);
                        value = float3;
                    }
                    break;
                case Type.Float4:
                    {
                        var realValueRelativeOffset = BitConverter.ToInt32(xmb2, (int)valueOffset);
                        var realValueOffset = (int)valueOffset + realValueRelativeOffset;
                        var float4 = new float[4];
                        float4[0] = BitConverter.ToSingle(xmb2, realValueOffset);
                        float4[1] = BitConverter.ToSingle(xmb2, realValueOffset + sizeof(float));
                        float4[2] = BitConverter.ToSingle(xmb2, realValueOffset + sizeof(float) * 2);
                        float4[3] = BitConverter.ToSingle(xmb2, realValueOffset + sizeof(float) * 2);
                        value = float4;
                    }
                    break;
                default:
                    throw new ArgumentException($"Unrecognized attribute type {valueType}");
            }

            return new Xmb2Variant(valueType, nameHash, value);
        }

        /// <summary>
        /// Read a null-terminated string.
        /// </summary>
        /// <param name="xmb2">The XMB2 document.</param>
        /// <param name="offset">The offset to read from.</param>
        /// <returns>The string.</returns>
        private static string ReadCString(byte[] xmb2, long offset)
        {
            var chars = new List<byte>();
            while (true)
            {
                var nextChar = xmb2[offset];
                if (nextChar == 0)
                {
                    break;
                }

                chars.Add(nextChar);
                offset++;
            }

            return Encoding.UTF8.GetString(chars.ToArray());
        }

        /// <summary>
        /// Return a string representation of this attribute's value.
        /// </summary>
        /// <returns>The string representation.</returns>
        public string GetTextValue()
        {
            switch (this.ValueType)
            {
                case Type.String:
                    return this.Value as string;
                case Type.Bool:
                    if ((bool)this.Value)
                    {
                        return "True";
                    }

                    return "False";
                case Type.Signed:
                case Type.Unsigned:
                case Type.Float:
                case Type.Double:
                    return this.Value.ToString();
                case Type.Float2:
                    var vector = this.Value as float[];
                    return $"{vector[0]},{vector[1]}";
                case Type.Float3:
                    vector = this.Value as float[];
                    return $"{vector[0]},{vector[1]},{vector[2]}";
                case Type.Float4:
                    vector = this.Value as float[];
                    return $"{vector[0]},{vector[1]},{vector[2]},{vector[3]}";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Return a boolean representation of this attribute's value.
        /// </summary>
        /// <returns>The boolean representation.</returns>
        public bool ToBool(bool defaultValue = false)
        {
            switch (this.ValueType)
            {
                case Type.String:
                    return XmlUtility.ToBool(this.Value as string);
                case Type.Bool:
                    return (bool)this.Value;
                case Type.Signed:
                case Type.Unsigned:
                    return (uint)this.Value != 0;
                case Type.Float:
                    return (float)this.Value != 0;
                case Type.Double:
                    return (double)this.Value != 0;
                default:
                    return defaultValue;
            }
        }

        /// <summary>
        /// Return an integer representation of this attribute's value.
        /// </summary>
        /// <returns>The integer representation.</returns>
        public int ToInt(int defaultValue = 0)
        {
            switch (this.ValueType)
            {
                case Type.String:
                    return Int32.Parse(this.Value as string);
                case Type.Bool:
                    return (!(bool)this.Value) ? 0 : 1;
                case Type.Signed:
                    return (int)this.Value;
                case Type.Unsigned:
                    return (int)(uint)this.Value;
                case Type.Float:
                    return (int)(float)this.Value;
                case Type.Double:
                    return (int)(double)this.Value;
                default:
                    return defaultValue;
            }
        }

        /// <summary>
        /// Return a uint representation of this attribute's value.
        /// </summary>
        /// <returns>The uint representation.</returns>
        public uint ToUInt(uint defaultValue = 0)
        {
            switch (this.ValueType)
            {
                case Type.String:
                    return UInt32.Parse(this.Value as string);
                case Type.Bool:
                    return (!(bool)this.Value) ? 0u : 1u;
                case Type.Signed:
                    return (uint)(int)this.Value;
                case Type.Unsigned:
                    return (uint)this.Value;
                case Type.Float:
                    return (uint)(float)this.Value;
                case Type.Double:
                    return (uint)(double)this.Value;
                default:
                    return defaultValue;
            }
        }

        /// <summary>
        /// Return a float representation of this attribute's value.
        /// </summary>
        /// <returns>The float representation.</returns>
        public float ToFloat(int defaultValue = 0)
        {
            switch (this.ValueType)
            {
                case Type.String:
                    return Single.Parse(this.Value as string);
                case Type.Bool:
                    return (!(bool)this.Value) ? 0.0f : 1.0f;
                case Type.Signed:
                    return (int)this.Value;
                case Type.Unsigned:
                    return (uint)this.Value;
                case Type.Float:
                    return (float)this.Value;
                case Type.Double:
                    return (float)(double)this.Value;
                default:
                    return defaultValue;
            }
        }

        /// <summary>
        /// Return a double representation of this attribute's value.
        /// </summary>
        /// <returns>The double representation.</returns>
        public double ToDouble(int defaultValue = 0)
        {
            switch (this.ValueType)
            {
                case Type.String:
                    return (double)Single.Parse(this.Value as string);
                case Type.Bool:
                    return (!(bool)this.Value) ? 0.0 : 1.0;
                case Type.Signed:
                    return (int)this.Value;
                case Type.Unsigned:
                    return (uint)this.Value;
                case Type.Float:
                    return (double)(float)this.Value;
                case Type.Double:
                    return (double)this.Value;
                default:
                    return defaultValue;
            }
        }

        /// <summary>
        /// Return a float4 representation of this attribute's value.
        /// </summary>
        /// <returns>The float4 representation.</returns>
        public float[] TryGetFloat4()
        {
            var result = new float[4];
            switch (this.ValueType)
            {
                case Type.Bool:
                    if ((bool)this.Value)
                    {
                        result[0] = 1.0f;
                    }

                    break;
                case Type.Signed:
                    result[0] = (float)(int)this.Value;
                    break;
                case Type.Unsigned:
                    result[0] = (float)(uint)this.Value;
                    break;
                case Type.Float:
                    result[0] = (float)this.Value;
                    break;
                case Type.Double:
                    result[0] = (float)(double)this.Value;
                    break;
                case Type.Float2:
                    result[0] = ((float[])this.Value)[0];
                    result[1] = ((float[])this.Value)[1];
                    break;
                case Type.Float3:
                    result[0] = ((float[])this.Value)[0];
                    result[1] = ((float[])this.Value)[1];
                    result[2] = ((float[])this.Value)[2];
                    break;
                case Type.Float4:
                    result[0] = ((float[])this.Value)[0];
                    result[1] = ((float[])this.Value)[1];
                    result[2] = ((float[])this.Value)[2];
                    result[3] = ((float[])this.Value)[3];
                    break;
            }

            return result;
        }
    }
}
