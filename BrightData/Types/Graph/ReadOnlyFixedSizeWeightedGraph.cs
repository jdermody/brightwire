using System;
using System.Buffers;
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
    /// Fixed size weighted graph.
    /// </summary>
    /// <typeparam name="W"></typeparam>
    /// <typeparam name="AT"></typeparam>
    /// <typeparam name="T"></typeparam>
    public readonly struct ReadOnlyFixedSizeWeightedGraph<T, W, AT> : IReadOnlyWeightedGraph<T, W>, IEquatable<ReadOnlyFixedSizeWeightedGraph<T, W, AT>>
        where T : unmanaged, IEquatable<T>, IHaveSingleIndex
        where W : unmanaged, IBinaryFloatingPointIeee754<W>, IMinMaxValue<W>
        where AT : unmanaged, IFixedSizeSortedArray<uint, W>
    {
        readonly FixedSizeWeightedGraphNode<T, W, AT>[] _nodes;

        /// <summary>
        /// Creates a graph from an array of nodes.
        /// </summary>
        /// <param name="nodes"></param>
        public ReadOnlyFixedSizeWeightedGraph(FixedSizeWeightedGraphNode<T, W, AT>[] nodes)
        {
            _nodes = nodes;
        }

        /// <summary>
        /// Number of nodes in the graph.
        /// </summary>
        public uint Size => (uint)_nodes.Length;

        /// <inheritdoc />
        public RAT ProbabilisticSearch<RAT, CAT>(uint q, uint entryPoint, ICalculateNodeWeights<W> distanceCalculator)
            where RAT : struct, IFixedSizeSortedArray<uint, W>
            where CAT : struct, IFixedSizeSortedArray<uint, W>
        {
            return WeightedGraphHelper.SearchFixedSize<W, RAT, CAT>(q, entryPoint, distanceCalculator, GetNeighbours);
        }

        /// <summary>
        /// Gets the neighbours for a node, sorted by distance.
        /// </summary>
        /// <param name="vectorIndex"></param>
        /// <returns></returns>
        public ReadOnlySpan<uint> GetNeighbours(uint vectorIndex) => _nodes[vectorIndex].NeighbourIndices;

        /// <inheritdoc />
        public IEnumerable<(uint NeighbourIndex, W Weight)> EnumerateNeighbours(uint vectorIndex) => _nodes[vectorIndex].WeightedNeighbours;

        /// <summary>
        /// Gets the weights for the node's neighbours.
        /// </summary>
        /// <param name="vectorIndex"></param>
        /// <returns></returns>
        public ReadOnlySpan<W> GetNeighbourWeights(uint vectorIndex) => _nodes[vectorIndex].NeighbourWeights;

        /// <summary>
        /// Enumerates the neighbour indices and their weights in ascending order.
        /// </summary>
        /// <param name="vectorIndex"></param>
        /// <returns></returns>
        public IEnumerable<(uint NeighbourIndex, W NeighbourWeight)> GetWeightedNeighbours(uint vectorIndex) => _nodes[vectorIndex].WeightedNeighbours;

        /// <inheritdoc />
        public bool AddNeighbour(uint nodeIndex, uint neighbourIndex, W weight) => _nodes[nodeIndex].TryAddNeighbour(neighbourIndex, weight);

        /// <inheritdoc />
        public IEnumerable<uint> DepthFirstSearch(uint startNodeIndex) => GraphHelper<ReadOnlyFixedSizeWeightedGraph<T, W, AT>>.DepthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public IEnumerable<uint> BreadthFirstSearch(uint startNodeIndex) => GraphHelper<ReadOnlyFixedSizeWeightedGraph<T, W, AT>>.BreadthFirstSearch(ref Unsafe.AsRef(in this), startNodeIndex);

        /// <inheritdoc />
        public T Get(uint nodeIndex) => _nodes[nodeIndex].Value;

        /// <inheritdoc />
        public uint GetNodeIndex(T node) => node.Index;

        /// <inheritdoc />
        public IEnumerable<uint> GetConnectedNodes(uint nodeIndex) => _nodes[nodeIndex].Neighbours;

        /// <summary>
        /// Builds a fixed-size weighted graph from a set of vectors by computing pairwise distances
        /// and connecting each node to its N closest neighbours.
        /// </summary>
        /// <param name="vectors">The source vectors to build the graph from.</param>
        /// <param name="distanceMetric">The distance metric to use for computing distances between vectors.</param>
        /// <param name="shortCircuitIfNodeNeighboursAreFull">If true, skip distance computation for nodes that already have full neighbour slots.</param>
        /// <param name="onNode">Optional callback invoked after each node is processed.</param>
        /// <returns>A new graph connecting each vector to its closest neighbours.</returns>
        [SkipLocalsInit]
        public static unsafe ReadOnlyFixedSizeWeightedGraph<GraphNodeIndex, W, AT> Build(
            IStoreVectors<W> vectors,
            DistanceMetric distanceMetric,
            bool shortCircuitIfNodeNeighboursAreFull = true,
            Action<uint>? onNode = null)
        {
            var size = vectors.Size;
            var distance = ArrayPool<W>.Shared.Rent((int)size);

            try
            {
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
                            if (currentIndex != j)
                                destPtr[j] = W.Abs(x.FindDistance(vectors[currentIndex], distanceMetric));
                        });
                    }

                    // find top N closest neighbours
                    var maxHeap = new FixedSizeWeightedGraphNode<GraphNodeIndex, W, FixedSizeSortedAscending8Array<uint, W>>();
                    for (var j = 0; j < size; j++)
                    {
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
            finally
            {
                ArrayPool<W>.Shared.Return(distance);
            }
        }

        /// <summary>
        /// Writes the graph to disk.
        /// </summary>
        /// <param name="filePath"></param>
        public async Task WriteToDisk(string filePath)
        {
            using var fileHandle = File.OpenHandle(filePath, FileMode.Create, FileAccess.Write, FileShare.None, FileOptions.Asynchronous);
            await RandomAccess.WriteAsync(fileHandle, _nodes.AsMemory().AsBytes(), 0);
        }

        /// <summary>
        /// Loads a graph from disk.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<ReadOnlyFixedSizeWeightedGraph<T, W, AT>> LoadFromDisk(string filePath)
        {
            using var fileHandle = File.OpenHandle(filePath);
            var ret = GC.AllocateUninitializedArray<FixedSizeWeightedGraphNode<T, W, AT>>((int)(RandomAccess.GetLength(fileHandle) / Unsafe.SizeOf<FixedSizeWeightedGraphNode<T, W, AT>>()));
            await RandomAccess.ReadAsync(fileHandle, ret.AsMemory().AsBytes(), 0);
            return new(ret);
        }

        /// <inheritdoc />
        public bool Equals(ReadOnlyFixedSizeWeightedGraph<T, W, AT> other) => _nodes.AsSpan().SequenceEqual(other._nodes);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is ReadOnlyFixedSizeWeightedGraph<T, W, AT> other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => _nodes.GetHashCode();

        /// <summary>
        /// Compares two graphs for equality.
        /// </summary>
        public static bool operator ==(ReadOnlyFixedSizeWeightedGraph<T, W, AT> left, ReadOnlyFixedSizeWeightedGraph<T, W, AT> right) => left.Equals(right);

        /// <summary>
        /// Compares two graphs for inequality.
        /// </summary>
        public static bool operator !=(ReadOnlyFixedSizeWeightedGraph<T, W, AT> left, ReadOnlyFixedSizeWeightedGraph<T, W, AT> right) => !left.Equals(right);
    }
}
