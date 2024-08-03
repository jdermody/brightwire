using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using BrightData.Distribution;

namespace BrightData.Types.Graph
{
    public class HierarchicalNavigationSmallWorldGraph<T, W, AT, BLAT>(BrightDataContext context, int maxLayers)
        where T : unmanaged, IHaveSingleIndex
        where W : unmanaged, INumber<W>, IMinMaxValue<W>, IBinaryFloatingPointIeee754<W>
        where AT : unmanaged, IFixedSizeSortedArray<uint, W>
        where BLAT : unmanaged, IFixedSizeSortedArray<uint, W>
    {
        public record struct NodeIndex(T Value, uint LayerIndex) : IHaveSingleIndex
        {
            public uint Index => Value.Index;
        }
        readonly IContinuousDistribution<W> _distribution = context.CreateExponentialDistribution<W>(W.CreateSaturating(maxLayers));
        readonly IWeightedDynamicGraph<NodeIndex, W>[] _layers = Enumerable.Range(0, maxLayers)
            .Select(i => (IWeightedDynamicGraph<NodeIndex, W>)(i == 0 ? new FixedSizeWeightedDynamicGraph<NodeIndex, W, BLAT>() : new FixedSizeWeightedDynamicGraph<NodeIndex, W, AT>()))
            .ToArray()
        ;
        NodeIndex? _entryPoint = null;

        public void Add(IEnumerable<T> values, ICalculateNodeWeights<W> distanceCalculator)
        {
            Span<(uint, W)> newNodeNeighbours = stackalloc (uint, W)[32];

            foreach (var value in values) {
                var entryPoint = _entryPoint;
                var level = GetRandomLevel();

                // zoom in
                uint? entryPointLevel = null;
                if (entryPoint is not null) {
                    entryPointLevel = entryPoint.Value.LayerIndex;
                    for (var i = entryPointLevel.Value; i > level; i--) {
                        var layer = _layers[i];
                        var w = layer.Search<FixedSizeSortedAscending1Array<uint, W>, AT>(value.Index, entryPoint.Value.Index, distanceCalculator);
                        entryPoint = layer.Find(w.MinValue);
                    }
                }

                // add to levels
                var from = Math.Min(level, entryPoint?.LayerIndex ?? int.MaxValue);
                for(var i = level; i > from; i--)
                    _layers[i].Add(new NodeIndex(value, (uint)i));
                for (var i = from; i >= 0; i--) {
                    var layer = _layers[i];
                    var newNodeIndex = 0;
                    if (entryPoint is not null) {
                        var w = layer.Search<AT, BLAT>(value.Index, entryPoint.Value.Index, distanceCalculator);
                        foreach (var (ni, nw) in w.Elements) {
                            layer.AddNeighbour(ni, value.Index, nw);
                            newNodeNeighbours[newNodeIndex++] = (ni, nw);
                        }
                    }
                    layer.Add(new NodeIndex(value, (uint)i), newNodeNeighbours[..newNodeIndex]);
                }

                if(!entryPointLevel.HasValue || level > entryPointLevel.Value)
                    _entryPoint = _layers[level].Find(value.Index);
            }
        }

        public AT KnnSearch(uint q, ICalculateNodeWeights<W> distanceCalculator)
        {
            var entryPoint = _entryPoint ?? throw new Exception("No nodes in graph");
            for (var i = (int)entryPoint.LayerIndex; i > 0; i--) {
                var layer = _layers[i];
                var w = layer.Search<AT, BLAT>(q, entryPoint.Index, distanceCalculator);
                entryPoint = layer.Find(w.MinValue);
            }
            return _layers[0].Search<AT, BLAT>(q, entryPoint.Index, distanceCalculator);
        }

        public IEnumerable<uint> BreadthFirstSearch(uint index)
        {
            var queue = new Queue<uint>();
            var visited = new HashSet<uint> { index};
            var layer = _layers[0];
            queue.Enqueue(index);

            while (queue.Count > 0) {
                foreach (var neighbour in layer.GetNeighbours(queue.Dequeue()).ToArray()) {
                    if(!visited.Add(neighbour))
                        continue;
                    yield return neighbour;
                }
            }
        }

        int GetRandomLevel()
        {
            int ret;
            do {
                ret = int.CreateTruncating(_distribution.Sample());
            } while (ret >= maxLayers);
            return ret;
        }
    }
}
