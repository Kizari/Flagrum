using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
// ReSharper disable IntVariableOverflowInUncheckedContext

namespace Flagrum.Core.Physics.Collision.PhysX.RTree;

public class RTreeSorter
{
    private float[] _metricL;
    private float[] _metricR;
    private uint[] _tempPermute;
    private uint[] _tempRanks;

    private uint[] _permuteStart;
    private List<RTreeBoundingBox> _allBounds;
    private List<uint> _xOrder;
    private List<uint> _yOrder;
    private List<uint> _zOrder;
    private uint[] _xRanks;
    private uint[] _yRanks;
    private uint[] _zRanks;

    public RTreeSorter(uint[] permuteStart, List<RTreeBoundingBox> allBounds, List<uint> xOrder, List<uint> yOrder,
        List<uint> zOrder, uint[] xRanks, uint[] yRanks, uint[] zRanks)
    {
        _permuteStart = permuteStart;
        _allBounds = allBounds;
        _xOrder = xOrder;
        _yOrder = yOrder;
        _zOrder = zOrder;
        _xRanks = xRanks;
        _yRanks = yRanks;
        _zRanks = zRanks;

        _metricL = new float[allBounds.Count];
        _metricR = new float[allBounds.Count];
        _tempPermute = new uint[allBounds.Count * 2 + 1];
        _tempRanks = new uint[allBounds.Count];
    }

    public void Sort(uint[] permute, uint clusterSize, List<RTreeNodeNonQuantized> tree, ref uint maxLevels, uint level = 0, RTreeNodeNonQuantized parentNode = null)
    {
        maxLevels = level == 0 ? 1 : Math.Max(maxLevels, level + 1);
        var splitPos = Enumerable.Range(1, (int)RTree.PAGE_SIZE).Select(i => (uint)i).ToList();

        if (clusterSize >= RTree.PAGE_SIZE)
        {
            // split into RT_PAGESIZE number of regions via RT_PAGESIZE-1 subsequent splits
            // each split is represented as a current interval
            // we iterate over currently active intervals and compute it's surface area
            // then we split the interval with maximum surface area
            // AP scaffold: possible optimization - seems like computeSA can be cached for unchanged intervals

            var splits = new List<RTreeInterval> {new(0, clusterSize)};

            for (var split = 0; split < RTree.PAGE_SIZE - 1; split++)
            {
                var maxSurfaceAreaHeuristic = float.MinValue;
                var maxSplit = uint.MaxValue;

                for (var i = 0; i < splits.Count; i++)
                {
                    if (splits[i].Count == 1)
                    {
                        continue;
                    }

                    var surfaceAreaHeuristic = ComputeSurfaceArea(permute, splits[i]) * splits[i].Count;

                    if (surfaceAreaHeuristic > maxSurfaceAreaHeuristic)
                    {
                        maxSurfaceAreaHeuristic = surfaceAreaHeuristic;
                        maxSplit = (uint)i;
                    }
                }
                
                // maxSplit is now the index of the interval in splits array with maximum surface area
                // we now split it into 2 using the split() function
                var old = splits[(int)maxSplit];
                var splitLocal = Split(permute[(int)old.Start..], old.Count);
                
                splits.Add(new RTreeInterval(old.Start, splitLocal));
                splits.Add(new RTreeInterval(old.Start + splitLocal, old.Count - splitLocal));

                // Replace element at i with last element in list
                var temp = splits.Last();
                splits.RemoveAt((int)maxSplit);
                splits.Remove(temp);
                splits.Insert((int)maxSplit, temp);
                
                splitPos[split] = old.Start + splitLocal;
            }
        }
        else
        {
            for (var i = clusterSize; i < RTree.PAGE_SIZE - 1; i++)
            {
                splitPos[(int)i] = clusterSize;
            }
        }
        
        splitPos.Sort();
        splitPos[(int)RTree.PAGE_SIZE - 1] = clusterSize; // splitCount[n] is computed as splitPos[n+1]-splitPos[n], so we need to add this last value

        // now compute splitStarts and splitCounts from splitPos[] array
        var splitStarts = new uint[RTree.PAGE_SIZE];
        var splitCounts = new uint[RTree.PAGE_SIZE];
        splitCounts[0] = splitPos[0];
        var sumCounts = splitCounts[0];
        for (var j = 1; j < RTree.PAGE_SIZE; j++)
        {
            splitStarts[j] = splitPos[j - 1];
            splitCounts[j] = splitPos[j] - splitPos[j - 1];
            sumCounts += splitCounts[j];
        }
        
        // mark this cluster as terminal based on clusterSize <= stopAtTrisPerPage parameter
        var terminalClusterByTotalCount = clusterSize <= 64;
        
        // iterate over splitCounts for the current cluster, if any of counts exceed 16 (which is the maximum supported by LeafTriangles
        // we cannot mark this cluster as terminal (has to be split more)
        for (var s = 0; s < RTree.PAGE_SIZE; s++)
        {
            if (splitCounts[s] > 16) // LeafTriangles doesn't support > 16 tris
            {
                terminalClusterByTotalCount = false;
            }
        }
        
        // Iterate over all the splits
        for (var s = 0; s < RTree.PAGE_SIZE; s++)
        {
            var result = new RTreeNodeNonQuantized();
            var splitCount = splitCounts[s];
            if (splitCount > 0)
            {
                // sweep left to right and compute min and max SAH for each individual bound in current split
                var bounds = _allBounds[(int)permute[(int)splitStarts[s]]];
                var sahMin = SurfaceAreaHueristic(bounds.Extents);
                var sahMax = sahMin;

                for (var i = 1; i < splitCount; i++)
                {
                    var localIndex = i + (int)splitStarts[s];
                    var bounds2 = _allBounds[(int)permute[localIndex]];
                    var sah = SurfaceAreaHueristic(bounds2.Extents);
                    sahMin = Math.Min(sahMin, sah);
                    sahMax = Math.Max(sahMax, sah);
                    bounds.Include(bounds2);
                }

                result.Bounds.Min = bounds.Min;
                result.Bounds.Max = bounds.Max;
                
                // if bounds differ widely (according to some heuristic preset), we continue splitting
                // this is important for a mixed cluster with large and small triangles
                var isOkay = sahMax / sahMin < 40.0f;
                if (!isOkay)
                {
                    terminalClusterByTotalCount = false;
                }

                var stopSplitting = splitCount <= 2 
                                    || (isOkay && splitCount <= 3) 
                                    || terminalClusterByTotalCount 
                                    || splitCount <= 16;

                if (stopSplitting)
                {
                    // this is a terminal page then, mark as such
                    // first node index is relative to the top level input array beginning
                    result.ChildPageFirstNodeIndex = (int)(splitStarts[s] + (permute[0] - _permuteStart[0]));
                    result.LeafCount = (int)splitCount;
                }
                else
                {
                    result.ChildPageFirstNodeIndex = -1;
                    result.LeafCount = 0;
                }
            }
            else // splitCount == 0 at this point, this is an empty paddding node (with current presets it's very rare)
            {
                result.Bounds.SetEmpty();
                result.ChildPageFirstNodeIndex = -1;
                result.LeafCount = -1;
            }

            tree.Add(result);
        }

        // Abort recursion if terminal cluster
        if (terminalClusterByTotalCount)
        {
            return;
        }
        
        // Recurse on subpages
        var parentIndex = tree.Count - (int)RTree.PAGE_SIZE;
        for (var s = 0; s < RTree.PAGE_SIZE; s++)
        {
            var parent = tree[parentIndex + s];
            
            // Only split pages that were marked as non-terminal during splitting
            if (parent.LeafCount == 0)
            {
                // all child nodes will be pushed inside of this recursive call,
                // so we set the child pointer for parent node to resultTree.size()
                parent.ChildPageFirstNodeIndex = tree.Count;
                Sort(permute[(int)splitStarts[s]..], splitCounts[s], tree, ref maxLevels, level + 1, parent);
            }
        }
    }

    private float ComputeSurfaceArea(uint[] permute, RTreeInterval split)
    {
        var min = _allBounds[(int)permute[(int)split.Start]].Min;
        var max = _allBounds[(int)permute[(int)split.Start]].Max;

        for (var i = 1; i < split.Count; i++)
        {
            var bounds = _allBounds[(int)permute[(int)split.Start + i]];
            min = Vector3.Min(min, bounds.Min);
            max = Vector3.Max(max, bounds.Max);
        }

        var center = max - min;
        return SurfaceAreaHueristic(center);
    }

    private float SurfaceAreaHueristic(Vector3 vector)
    {
        return Vector3.Dot(vector, new Vector3(vector.Z, vector.X, vector.Y));
    }

    public uint Split(uint[] permute, uint clusterSize)
    {
        if (clusterSize <= 1)
        {
            return 0;
        }

        if (clusterSize == 2)
        {
            return 1;
        }

        var minCount = clusterSize >= 4 ? 2 : 1;
        var splitStartL = minCount;
        var splitEndL = (int)(clusterSize - minCount);
        var splitStartR = (int)(clusterSize - splitStartL);
        var splitEndR = (int)(clusterSize - splitEndL);

        var minMetric = new float[3];
        var minMetricSplit = new uint[3];

        var ranks = new[] {_xRanks, _yRanks, _zRanks};
        var orders = new[] {_xOrder, _yOrder, _zOrder};

        for (var coordIndex = 0; coordIndex <= 2; coordIndex++)
        {
            var rank = ranks[coordIndex];
            var order = orders[coordIndex];

            // build ranks in tempPermute
            if (clusterSize == _allBounds.Count)
            {
                for (var i = 0; i < clusterSize; i++)
                {
                    _tempPermute[i] = order[i];
                }
            }
            else
            {
                for (var i = 0; i < clusterSize; i++)
                {
                    _tempRanks[i] = rank[(int)permute[i]];
                }

                Array.Sort(_tempRanks, 0, (int)clusterSize);

                for (var i = 0; i < clusterSize; i++)
                {
                    _tempPermute[i] = order[(int)_tempRanks[i]];
                }
            }
            
            // we consider overlapping intervals for minimum sum of metrics
            // left interval is from splitStartL up to splitEndL
            // right interval is from splitStartR down to splitEndR

            // first compute the array metricL
            var boundsLMin = _allBounds[(int)_tempPermute[0]].Min;
            var boundsLMax = _allBounds[(int)_tempPermute[0]].Max;

            for (var i = 1; i < splitStartL; i++) // sweep right to include all bounds up to splitStartL-1
            {
                boundsLMin = Vector3.Min(boundsLMin, _allBounds[(int)_tempPermute[i]].Min);
                boundsLMax = Vector3.Max(boundsLMax, _allBounds[(int)_tempPermute[i]].Max);
            }

            var countL0 = 0;
            for (var i = splitStartL; i <= splitEndL; i++) // compute metric for inclusive bounds from splitStartL to splitEndL
            {
                boundsLMin = Vector3.Min(boundsLMin, _allBounds[(int)_tempPermute[i]].Min);
                boundsLMax = Vector3.Max(boundsLMax, _allBounds[(int)_tempPermute[i]].Max);
                _metricL[countL0++] = SurfaceAreaHueristic(boundsLMax - boundsLMin);
            }
            
            // now compute the array metricR
            var boundsRMin = _allBounds[(int)_tempPermute[clusterSize - 1]].Min;
            var boundsRMax = _allBounds[(int)_tempPermute[clusterSize - 1]].Max;

            for (var i = clusterSize - 2; i > splitStartR; i--) // include bounds to the left of splitEndR down to splitStartR
            {
                boundsRMin = Vector3.Min(boundsRMin, _allBounds[(int)_tempPermute[i]].Min);
                boundsRMax = Vector3.Max(boundsRMax, _allBounds[(int)_tempPermute[i]].Max);
            }

            var countR0 = 0;
            for (var i = splitStartR; i >= splitEndR; i--) // continue sweeping left, including bounds and recomputing the metric
            {
                boundsRMin = Vector3.Min(boundsRMin, _allBounds[(int)_tempPermute[i]].Min);
                boundsRMax = Vector3.Max(boundsRMax, _allBounds[(int)_tempPermute[i]].Max);
                _metricR[countR0++] = SurfaceAreaHueristic(boundsRMax - boundsRMin);
            }
            
            // now iterate over splitRange and compute the minimum sum of SAHLeft*countLeft + SAHRight*countRight
            var minMetricSplitPosition = 0;
            var minMetricLocal = float.MaxValue;
            var hs = clusterSize / 2;
            var splitRange = splitEndL - splitStartL + 1;

            for (var i = 0; i < splitRange; i++)
            {
                float countL = i + minCount;
                float countR = splitRange - i - 1 + minCount;

                var metric = countL * _metricL[i] + countR * _metricR[splitRange - i - 1];
                var splitPos = i + splitStartL;

                if (metric < minMetricLocal 
                    || (metric <= minMetricLocal && Math.Abs(splitPos - hs) < Math.Abs(minMetricSplitPosition - hs)))
                {
                    minMetricLocal = metric;
                    minMetricSplitPosition = splitPos;
                }
            }

            minMetric[coordIndex] = minMetricLocal;
            minMetricSplit[coordIndex] = (uint)minMetricSplitPosition;
        }

        var winIndex = 2;
        if (minMetric[0] <= minMetric[1] && minMetric[0] <= minMetric[2])
        {
            winIndex = 0;
        }
        else if (minMetric[1] <= minMetric[2])
        {
            winIndex = 1;
        }

        var oRank = ranks[winIndex];
        var oOrder = ranks[winIndex];

        if (clusterSize == _allBounds.Count)
        {
            for (var i = 0; i < clusterSize; i++)
            {
                permute[i] = oOrder[i];
            }
        }
        else
        {
            for (var i = 0; i < clusterSize; i++)
            {
                _tempRanks[i] = oRank[(int)permute[i]];
            }
            
            Array.Sort(_tempRanks, 0, (int)clusterSize);

            for (var i = 0; i < clusterSize; i++)
            {
                permute[i] = oOrder[(int)_tempRanks[i]];
            }
        }

        var splitPoint = minMetricSplit[winIndex];
        if (clusterSize == 3 && splitPoint == 0)
        {
            splitPoint = 1;
        }

        return splitPoint;
    }
}