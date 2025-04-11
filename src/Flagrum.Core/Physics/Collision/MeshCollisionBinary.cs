using System.IO;
using System.Linq;
using Flagrum.Core.Data;

namespace Flagrum.Core.Physics.Collision;

public class MeshCollisionBinary : IResourceBinaryItem
{
    public DataIndex Index { get; set; }
    
    public char[] Magic { get; set; } = "MCB ".ToCharArray();
    public uint Version { get; set; }
    public uint LayerOffsetListOffset { get; set; }
    public uint AttributesOffset { get; set; }
    public uint EdgeBinaryOffset { get; set; }
    public uint AttributeSetOffsetListOffset { get; set; }
    public uint CollisionPriorityListOffset { get; set; }
    public uint TargetSizeListOffset { get; set; }
    public uint MovingStatesListOffset { get; set; }
    
    public uint LayerOffsetListCount { get; set; }
    public uint[] LayerOffsetList { get; set; }
    public MeshCollisionLayer[] Layers { get; set; }
    
    public uint AttributesCount { get; set; }
    public uint[] Attributes { get; set; }
    
    public uint AttributeSetOffsetListCount { get; set; }
    public uint[] AttributeSetOffsetList { get; set; }
    public MeshCollisionAttributeSet[] AttributeSets { get; set; }
    
    public uint CollisionPriorityListCount { get; set; }
    public uint[] CollisionPriorityList { get; set; }
    public uint[] CollisionPriorities { get; set; }
    
    public uint TargetSizeListCount { get; set; }
    public uint[] TargetSizeList { get; set; }
    public uint[] TargetSizes { get; set; }
    
    public uint MovingStatesListCount { get; set; }
    public uint[] MovingStatesList { get; set; }
    public uint[] MovingStates { get; set; }
    
    public void Read(BinaryReader reader)
    {
        Magic = reader.ReadChars(4);
        Version = reader.ReadUInt32();
        LayerOffsetListOffset = reader.ReadUInt32();
        AttributesOffset = reader.ReadUInt32();
        EdgeBinaryOffset = reader.ReadUInt32();
        AttributeSetOffsetListOffset = reader.ReadUInt32();
        CollisionPriorityListOffset = reader.ReadUInt32();
        TargetSizeListOffset = reader.ReadUInt32();
        MovingStatesListOffset = reader.ReadUInt32();

        LayerOffsetListCount = reader.ReadUInt32();
        LayerOffsetList = new uint[LayerOffsetListCount];
        for (var i = 0; i < LayerOffsetListCount; i++)
        {
            LayerOffsetList[i] = reader.ReadUInt32();
        }

        Layers = new MeshCollisionLayer[LayerOffsetListCount];
        for (var i = 0; i < LayerOffsetListCount; i++)
        {
            var layer = new MeshCollisionLayer();
            layer.Read(reader);
            Layers[i] = layer;
        }

        AttributesCount = reader.ReadUInt32();
        Attributes = new uint[AttributesCount];
        for (var i = 0; i < AttributesCount; i++)
        {
            Attributes[i] = reader.ReadUInt32();
        }

        AttributeSetOffsetListCount = reader.ReadUInt32();
        AttributeSetOffsetList = new uint[AttributeSetOffsetListCount];
        for (var i = 0; i < AttributeSetOffsetListCount; i++)
        {
            AttributeSetOffsetList[i] = reader.ReadUInt32();
        }

        AttributeSets = new MeshCollisionAttributeSet[AttributeSetOffsetListCount];
        for (var i = 0; i < AttributeSetOffsetListCount; i++)
        {
            var attributeSet = new MeshCollisionAttributeSet();
            attributeSet.Read(reader);
            AttributeSets[i] = attributeSet;
        }

        CollisionPriorityListCount = reader.ReadUInt32();
        CollisionPriorityList = new uint[CollisionPriorityListCount];
        for (var i = 0; i < CollisionPriorityListCount; i++)
        {
            CollisionPriorityList[i] = reader.ReadUInt32();
        }

        CollisionPriorities = new uint[CollisionPriorityListCount];
        for (var i = 0; i < CollisionPriorityListCount; i++)
        {
            CollisionPriorities[i] = reader.ReadUInt32();
        }

        TargetSizeListCount = reader.ReadUInt32();
        TargetSizeList = new uint[TargetSizeListCount];
        for (var i = 0; i < TargetSizeListCount; i++)
        {
            TargetSizeList[i] = reader.ReadUInt32();
        }

        TargetSizes = new uint[TargetSizeListCount];
        for (var i = 0; i < TargetSizeListCount; i++)
        {
            TargetSizes[i] = reader.ReadUInt32();
        }

        MovingStatesListCount = reader.ReadUInt32();
        MovingStatesList = new uint[MovingStatesListCount];
        for (var i = 0; i < MovingStatesListCount; i++)
        {
            MovingStatesList[i] = reader.ReadUInt32();
        }

        MovingStates = new uint[MovingStatesListCount];
        for (var i = 0; i < MovingStatesListCount; i++)
        {
            MovingStates[i] = reader.ReadUInt32();
        }
    }

    public void Write(BinaryWriter writer)
    {
        var start = writer.BaseStream.Position;
        writer.Write(Magic);
        writer.Write(Version);
        
        // Skip offsets for now
        var offsetsAddress = writer.BaseStream.Position;
        writer.BaseStream.Seek(7 * sizeof(uint), SeekOrigin.Current);

        LayerOffsetListOffset = (uint)(writer.BaseStream.Position - start);
        // TODO: Calculate this properly instead of based on one item
        LayerOffsetList = new[] {LayerOffsetListOffset + 8}; // + count size + one offset item + offset 0
        LayerOffsetListCount = (uint)LayerOffsetList.Length;
        writer.Write(LayerOffsetListCount);

        foreach (var item in LayerOffsetList)
        {
            writer.Write(item);
        }

        foreach (var layer in Layers)
        {
            layer.Write(writer);
        }

        AttributesOffset = (uint)(writer.BaseStream.Position - start);
        AttributesCount = (uint)Attributes.Length;
        writer.Write(AttributesCount);
        foreach (var attribute in Attributes)
        {
            writer.Write(attribute);
        }

        AttributeSetOffsetListOffset = (uint)(writer.BaseStream.Position - start);
        // TODO: Calculate this properly instead of based on one item
        AttributeSetOffsetList = new[] {AttributeSetOffsetListOffset + 8}; // + count size + one offset item + offset 0
        AttributeSetOffsetListCount = (uint)AttributeSetOffsetList.Length;
        writer.Write(AttributeSetOffsetListCount);
        
        foreach (var attributeSetOffset in AttributeSetOffsetList)
        {
            writer.Write(attributeSetOffset);
        }
        
        foreach (var attributeSet in AttributeSets)
        {
            attributeSet.Write(writer);
        }

        CollisionPriorityListOffset = (uint)(writer.BaseStream.Position - start);
        // TODO: Calculate this properly instead of based on one item
        CollisionPriorityList = new[] {CollisionPriorityListOffset + 8}; // + count size + one offset item + offset 0
        CollisionPriorityListCount = (uint)CollisionPriorityList.Length;
        writer.Write(CollisionPriorityListCount);
        
        foreach (var item in CollisionPriorityList)
        {
            writer.Write(item);
        }

        foreach (var item in CollisionPriorities)
        {
            writer.Write(item);
        }

        TargetSizeListOffset = (uint)(writer.BaseStream.Position - start);
        // TODO: Calculate this properly instead of based on one item
        TargetSizeList = new[] {TargetSizeListOffset + 8}; // + count size + one offset item + offset 0
        TargetSizeListCount = (uint)TargetSizeList.Length;
        writer.Write(TargetSizeListCount);
        
        foreach (var item in TargetSizeList)
        {
            writer.Write(item);
        }

        foreach (var item in TargetSizes)
        {
            writer.Write(item);
        }

        MovingStatesListOffset = (uint)(writer.BaseStream.Position - start);
        // TODO: Calculate this properly instead of based on one item
        MovingStatesList = new[] {MovingStatesListOffset + 8}; // + count size + one offset item + offset 0
        MovingStatesListCount = (uint)MovingStates.Length;
        writer.Write(MovingStatesListCount);
        
        foreach (var item in MovingStatesList)
        {
            writer.Write(item);
        }

        foreach (var item in MovingStates)
        {
            writer.Write(item);
        }
        
        // Go back and write the offsets
        var returnAddress = writer.BaseStream.Position;
        writer.BaseStream.Seek(offsetsAddress, SeekOrigin.Begin);
        writer.Write(LayerOffsetListOffset);
        writer.Write(AttributesOffset);
        writer.Write(EdgeBinaryOffset);
        writer.Write(AttributeSetOffsetListOffset);
        writer.Write(CollisionPriorityListOffset);
        writer.Write(TargetSizeListOffset);
        writer.Write(MovingStatesListOffset);
        writer.BaseStream.Seek(returnAddress, SeekOrigin.Begin);
    }
}