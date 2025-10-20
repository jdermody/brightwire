using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using System.Threading.Tasks;
using BrightData.Types.Graph.Helper;
using BrightData.Types.Helper;

namespace BrightData.Types.Graph
{
    /// <summary>
    /// Fixed size weighted graph
    /// </summary>
    /// <typeparam name="W"></typeparam>
    /// <typeparam name="AT"></typeparam>
    public readonly struct FixedSizeWeightedGraph<W, AT> : IReadOnlyWeightedGraph<GraphNodeIndex, W>
        where W : unmanaged, IBinaryFloatingPointIeee754<W>, IMinMaxValue<W>
        where AT : unmanaged, IFixedSizeSortedArray<uint, W>
    {
        readonly FixedSizeWeightedGraphNode<GraphNodeIndex, W, AT>[] _nodes;

        /// <summary>
        /// Creates a vector graph from an array of nodes
        /// </summary>
        /// <param name="nodes"></param>
        public FixedSizeWeightedGraph(FixedSizeWeightedGraphNode<GraphNodeIndex, W, AT>[] nodes)
        {
            _nodes = nodes;
        }

        /// <summary>
        /// Number of nodes in the graph
        /// </summary>
        public uint Size => (uint)_nodes.Length;

        /// <inheritdoc />
        public RAT ProbabilisticSearch<RAT, CAT>(uint q, uint entryPoint, ICalculateNodeWeights<W> distanceCalculator) where RAT : struct, IFixedSizeSortedArray<uint, W> where CAT : struct, IFixedSizeSortedArray<uint, W>
        {
            return WeightedGraphHelper.SearchFixedSize<W, RAT, CAT>(q, entryPoint, distanceCalculator, GetNeighbours);
        }

        /// <summary>
        /// Gets the neighbours for a node, sorted by distance
        /// </summary>
        /// <param name="vectorIndex"></param>
        /// <returns></returns>
        public ReadOnlySpan<uint> GetNeighbours(uint vectorIndex) => _nodes[vectorIndex].NeighbourIndices;

        /// <inheritdoc />
        public IEnumerable<(uint NeighbourIndex, W Weight)> EnumerateNeighbours(uint vectorIndex) => _nodes[vectorIndex].WeightedNeighbours;

        /// <summary>
        /// Gets the weights for the node's neighbours
        /// </summary>
        /// <param name="vectorIndex"></param>
        /// <returns></returns>
        public ReadOnlySpan<W> GetNeighbourWeights(uint vectorIndex) => _nodes[vectorIndex].NeighbourWeights;

        /// <summary>
        /// Enumerates the neighbour indices and their weights in ascending order
        /// </summary>
        /// <param name="vectorIndex"></param>
        /// <returns></returns>
        public IEnumerable<(uint NeighbourIndex, W NeighbourWeight)> GetWeightedNeighbours(uint vectorIndex) => _nodes[vectorIndex].WeightedNeighbours;

        /// <inheritdoc />
        public bool AddNeighbour(uint nodeIndex, uint neighbourIndex, W weight)
        {
            ref var node = ref _nodes[nodeIndex];
            if (!Unsafe.IsNullRef(ref node)) {
                return node.TryAddNeighbour(neighbourIndex, weight);
            }

            return false;
        }

        /// <inheritdoc />
        public IEnumerable<uint> DepthFirstSearch(uint startNodeIndex) => GraphHelper<FixedSizeWeightedGraph<W, AT>>.DepthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public IEnumerable<uint> BreadthFirstSearch(uint startNodeIndex) => GraphHelper<FixedSizeWeightedGraph<W, AT>>.BreadthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public GraphNodeIndex Get(uint nodeIndex)
        {
            ref var node = ref _nodes[nodeIndex];
            if (!Unsafe.IsNullRef(ref node))
                return node.Value;
            throw new ArgumentException($"Node with index {nodeIndex} was not found");
        }


        /// <inheritdoc />
        public IEnumerable<uint> GetConnectedNodes(uint nodeIndex)
        {
            ref var node = ref _nodes[nodeIndex];
            return !Unsafe.IsNullRef(ref node) ? node.Neighbours : [];
        }


        /// <summary>
        /// Creates 
        /// </summary>
        /// <param name="vectors"></param>
        /// <param name="distanceMetric"></param>
        /// <param name="shortCircuitIfNodeNeighboursAreFull"></param>
        /// <param name="onNode"></param>
        /// <returns></returns>
        [SkipLocalsInit]
        public static unsafe FixedSizeWeightedGraph<W, AT> Build(
            IStoreVectors<W> vectors, 
            DistanceMetric distanceMetric, 
            bool shortCircuitIfNodeNeighboursAreFull = true,
            Action<uint>? onNode = null)
        {
            var size = vectors.Size;
            var distance = size <= 1024 
                ? stackalloc W[(int)size] 
                : new W[size];

            var ret = GC.AllocateUninitializedArray<FixedSizeWeightedGraphNode<GraphNodeIndex, W, AT>>((int)size);
            for (var i = 0U; i < size; i++)
                ret[i] = new(new(i));

            for (var i = 0U; i < size; i++)
            {
                if (shortCircuitIfNodeNeighboursAreFull && ret[i].NeighbourCount == FixedSizeSortedAscending8Array<uint, W>.MaxSize)
                    continue;

                // find the distance between this node and each of its neighbours
                fixed (W* dest = distance)
                {
                    var destPtr = dest;
                    var currentIndex = i;
                    vectors.ForEach((x, j) =>
                    {
                        if(currentIndex != j)
                            destPtr[j] = W.Abs(x.FindDistance(vectors[currentIndex], distanceMetric));
                    });
                }

                // find top N closest neighbours
                var maxHeap = new FixedSizeWeightedGraphNode<GraphNodeIndex, W, FixedSizeSortedAscending8Array<uint, W>>();
                for (var j = 0; j < size; j++) {
                    if (i == j)
                        continue;
                    var d = distance[j];
                    maxHeap.TryAddNeighbour((uint)j, d);
                }

                // connect the closest nodes
                foreach (var (index, d) in maxHeap.WeightedNeighbours)
                {
                    ret[index].TryAddNeighbour(i, d);
                    ret[i].TryAddNeighbour(index, d);
                }
                onNode?.Invoke(i);
            }

            return new(ret);
        }

        /// <summary>
        /// Writes the graph to disk
        /// </summary>
        /// <param name="filePath"></param>
        public async Task WriteToDisk(string filePath)
        {
            using var fileHandle = File.OpenHandle(filePath, FileMode.Create, FileAccess.Write, FileShare.None, FileOptions.Asynchronous);
            await RandomAccess.WriteAsync(fileHandle, _nodes.AsMemory().AsBytes(), 0);
        }

        /// <summary>
        /// Loads a vector graph from disk
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<FixedSizeWeightedGraph<W, AT>> LoadFromDisk(string filePath)
        {
            using var fileHandle = File.OpenHandle(filePath);
            var ret = GC.AllocateUninitializedArray<FixedSizeWeightedGraphNode<GraphNodeIndex, W, AT>>((int)(RandomAccess.GetLength(fileHandle) / Unsafe.SizeOf<FixedSizeWeightedGraphNode<GraphNodeIndex, W, AT>>()));
            await RandomAccess.ReadAsync(fileHandle, ret.AsMemory().AsBytes(), 0);
            return new(ret);
        }
    }
}
