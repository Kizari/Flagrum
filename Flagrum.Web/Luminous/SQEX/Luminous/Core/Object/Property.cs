namespace SQEX.Luminous.Core.Object
{
    public class Property
    {
        public enum PrimitiveType
        {
            INVALID = 0,
            ClassField = 1,
            Int8 = 2,
            Int16 = 3,
            Int32 = 4,
            Int64 = 5,
            UInt8 = 6,
            UInt16 = 7,
            UInt32 = 8,
            UInt64 = 9,
            INVALID2 = 10,
            Bool = 11,
            Float = 12,
            Double = 13,
            String = 14,
            Pointer = 15,
            INVALID3 = 16,
            Array = 17,
            PointerArray = 18,
            Fixid = 19,
            Vector4 = 20,
            Color = 21,
            Buffer = 22,
            Enum = 23,
            IntrusivePointerArray = 24,
            DoubleVector4 = 25
        }

        public string Name { get; }
        private uint NameHash { get; }
        private string TypeName { get; }
        private uint Offset { get; }
        private uint Size { get; }
        private ushort ItemCount { get; }
        public PrimitiveType Type { get; }
        private PrimitiveType ItemPrimitiveType { get; }
        private char Attr { get; }

        public Property(string name, uint nameHash, string typeName, uint offset, uint size, ushort itemCount, PrimitiveType type, PrimitiveType itemPrimitiveType, char attr)
        {
            this.Name = name;
            this.NameHash = nameHash;
            this.TypeName = typeName;
            this.Offset = offset;
            this.Size = size;
            this.ItemCount = itemCount;
            this.Type = type;
            this.ItemPrimitiveType = itemPrimitiveType;
            this.Attr = attr;
        }
    }
}
