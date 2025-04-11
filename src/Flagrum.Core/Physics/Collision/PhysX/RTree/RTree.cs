using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Flagrum.Core.Mathematics;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Physics.Collision.PhysX.RTree;

public class RTree
{
    private const float Epsilon = 5e-4f;
    private const uint Sentinel = 0xABCDEF01;
    private const int NodePointerMultiplier = 28; // Size of RTreeNodeQuantized
    
    public const uint PAGE_SIZE = 4;
    
    public Vector4 InvDiagonal { get; set; }
    public uint TotalNodes { get; set; }
    public uint TotalPages { get; set; }
    public List<RTreePage> Pages { get; set; }
    public Vector4 BoundsMin { get; set; }
    public Vector4 BoundsMax { get; set; }
    public Vector4 DiagonalScaler { get; set; }
    public uint PageSize { get; set; }
    public uint LevelsCount { get; set; }
    public uint Unused { get; set; }
    public uint RootPagesCount { get; set; }

    public static RTree BuildFromTriangles(List<Vector3> vertices, ushort[] triangles,
        out uint[] faceRemap)
    {
        var allBounds = new List<RTreeBoundingBox>();
        var allMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var allMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        var epsilon = new Vector3(Epsilon, Epsilon, Epsilon);
        
        for (var i = 0; i < triangles.Length / 3; i++)
        {
            var v0 = vertices[triangles[i * 3]];
            var v1 = vertices[triangles[i * 3 + 1]];
            var v2 = vertices[triangles[i * 3 + 2]];

            var min = Vector3.Min(Vector3.Min(v0, v1), v2) - epsilon; // min over 3 verts, subtract eps to inflate
            var max = Vector3.Max(Vector3.Max(v0, v1), v2) + epsilon; // max over 3 verts, add eps to inflate
            allMin = Vector3.Min(allMin, min);
            allMax = Vector3.Max(allMax, max);
            
            allBounds.Add(new RTreeBoundingBox(min, max));
        }

        return BuildFromBounds(allBounds, allMin, allMax, out faceRemap);
    }

    private static RTree BuildFromBounds(List<RTreeBoundingBox> allBounds, Vector3 allMin, Vector3 allMax,
        out uint[] faceRemap)
    {
        var tree = new List<RTreeNodeNonQuantized>();
        var treeBounds = new RTreeBoundingBox(allMin, allMax);
        var permute = Enumerable.Range(0, allBounds.Count).Select(i => (uint)i).ToList();

        var xRanks = new uint[allBounds.Count];
        var yRanks = new uint[allBounds.Count];
        var zRanks = new uint[allBounds.Count];

        var xOrder = new List<uint>(permute);
        var yOrder = new List<uint>(permute);
        var zOrder = new List<uint>(permute);

        xOrder.Sort((first, second) =>
        {
            var center1 = allBounds[(int)first].Min.X + allBounds[(int)first].Max.X;
            var center2 = allBounds[(int)second].Min.X + allBounds[(int)second].Max.X;
            return center1 < center2 ? -1 : 1;
        });
        
        yOrder.Sort((first, second) =>
        {
            var center1 = allBounds[(int)first].Min.Y + allBounds[(int)first].Max.Y;
            var center2 = allBounds[(int)second].Min.Y + allBounds[(int)second].Max.Y;
            return center1 < center2 ? -1 : 1;
        });
        
        zOrder.Sort((first, second) =>
        {
            var center1 = allBounds[(int)first].Min.Z + allBounds[(int)first].Max.Z;
            var center2 = allBounds[(int)second].Min.Z + allBounds[(int)second].Max.Z;
            return center1 < center2 ? -1 : 1;
        });

        for (var i = 0; i < allBounds.Count; i++)
        {
            xRanks[(int)xOrder[i]] = (uint)i;
        }
        
        for (var i = 0; i < allBounds.Count; i++)
        {
            yRanks[(int)yOrder[i]] = (uint)i;
        }
        
        for (var i = 0; i < allBounds.Count; i++)
        {
            zRanks[(int)zOrder[i]] = (uint)i;
        }

        var maxLevels = 0u;
        permute.Add(Sentinel);
        var sorter = new RTreeSorter(permute.ToArray(), allBounds, xOrder, yOrder, zOrder, xRanks, yRanks, zRanks);
        sorter.Sort(permute.ToArray(), (uint)allBounds.Count, tree, ref maxLevels);
        
        // Discard the sentinel value
        permute.RemoveAt(permute.Count - 1);
        faceRemap = permute.ToArray();
        
        // Quantize the tree
        var treeNodes = new List<RTreeNodeQuantized>();
        var firstEmptyIndex = -1;
        var resultCount = tree.Count;

        for (var i = 0; i < resultCount; i++)
        {
            var unquantized = tree[i];
            var quantized = new RTreeNodeQuantized();
            quantized.SetLeaf(unquantized.LeafCount > 0);

            if (unquantized.ChildPageFirstNodeIndex == -1)
            {
                if (firstEmptyIndex == -1)
                {
                    firstEmptyIndex = treeNodes.Count;
                }

                quantized.MinX = quantized.MinY = quantized.MinZ = float.MaxValue;
                quantized.MaxX = quantized.MaxY = quantized.MaxZ = float.MinValue;

                quantized.Pointer = (uint)(firstEmptyIndex * NodePointerMultiplier);
                quantized.SetLeaf(true);
            }
            else
            {
                quantized.MinX = unquantized.Bounds.Min.X;
                quantized.MinY = unquantized.Bounds.Min.Y;
                quantized.MinZ = unquantized.Bounds.Min.Z;
                quantized.MaxX = unquantized.Bounds.Max.X;
                quantized.MaxY = unquantized.Bounds.Max.Y;
                quantized.MaxZ = unquantized.Bounds.Max.Z;

                if (unquantized.LeafCount > 0)
                {
                    quantized.Pointer = (uint)unquantized.ChildPageFirstNodeIndex;
                    quantized.Pointer = Remap(quantized.Pointer, (uint)unquantized.LeafCount);
                }
                else
                {
                    quantized.Pointer = (uint)(unquantized.ChildPageFirstNodeIndex * NodePointerMultiplier);
                    quantized.SetLeaf(false);
                }
            }
            
            treeNodes.Add(quantized);
        }
        
        // Build the final RTree image
        var result = new RTree
        {
            InvDiagonal = new Vector4(1.0f),
            TotalNodes = (uint)treeNodes.Count,
            TotalPages = (uint)treeNodes.Count / PAGE_SIZE,
            Pages = new List<RTreePage>(),
            BoundsMin = new Vector4(treeBounds.Min, 0.0f),
            BoundsMax = new Vector4(treeBounds.Max, 0.0f),
            DiagonalScaler = new Vector4(treeBounds.Extents, 0.0f) / 65535.0f,
            PageSize = PAGE_SIZE,
            LevelsCount = maxLevels,
            Unused = 0,
            RootPagesCount = 1
        };

        for (var i = 0; i < result.TotalPages; i++)
        {
            var page = new RTreePage();
            
            var node = treeNodes[(int)(i * PAGE_SIZE)];
            page.MaxX.X = node.MaxX;
            page.MaxY.X = node.MaxY;
            page.MaxZ.X = node.MaxZ;
            page.MinX.X = node.MinX;
            page.MinY.X = node.MinY;
            page.MinZ.X = node.MinZ;
            page.Pointers[0] = node.Pointer;
            
            node = treeNodes[(int)(i * PAGE_SIZE + 1)];
            page.MaxX.Y = node.MaxX;
            page.MaxY.Y = node.MaxY;
            page.MaxZ.Y = node.MaxZ;
            page.MinX.Y = node.MinX;
            page.MinY.Y = node.MinY;
            page.MinZ.Y = node.MinZ;
            page.Pointers[1] = node.Pointer;
            
            node = treeNodes[(int)(i * PAGE_SIZE + 2)];
            page.MaxX.Z = node.MaxX;
            page.MaxY.Z = node.MaxY;
            page.MaxZ.Z = node.MaxZ;
            page.MinX.Z = node.MinX;
            page.MinY.Z = node.MinY;
            page.MinZ.Z = node.MinZ;
            page.Pointers[2] = node.Pointer;
            
            node = treeNodes[(int)(i * PAGE_SIZE + 3)];
            page.MaxX.W = node.MaxX;
            page.MaxY.W = node.MaxY;
            page.MaxZ.W = node.MaxZ;
            page.MinX.W = node.MinX;
            page.MinY.W = node.MinY;
            page.MinZ.W = node.MinZ;
            page.Pointers[3] = node.Pointer;
            
            result.Pages.Add(page);
        }

        return result;
    }

    private static uint Remap(uint start, uint leafCount)
    {
        var leafTriangles = new RTreeLeafTriangles();
        leafTriangles.SetData(leafCount, start);
        return leafTriangles.Data;
    }

    public float CalculateGeometryEpsilon()
    {
        var epsilon = 0.0f;
        for (var i = 0; i < 3; i++)
        {
            epsilon = Math.Max(epsilon, Math.Max(Math.Abs(BoundsMax.GetValue(i)), Math.Abs(BoundsMin.GetValue(i))));
        }

        epsilon *= MathF.Pow(2.0f, -22.0f);
        return epsilon;
    }
}