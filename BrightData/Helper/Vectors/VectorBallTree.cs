using BrightData.Types;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BrightData.Helper.Vectors
{
    /// <summary>
    /// Creates a ball tree for vectors
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VectorBallTree<T> : IHaveSize, ISupportKnnSearch<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Node within tree
        /// </summary>
        /// <param name="data"></param>
        /// <param name="node"></param>
        public readonly ref struct ReadOnlyNode(ReadOnlySpan<T> data, VectorKDTree<T>.ReadOnlyNode node)
        {
            readonly ReadOnlySpan<T> _data = data;
            readonly VectorKDTree<T>.ReadOnlyNode _node = node;

            /// <summary>
            /// Vector index
            /// </summary>
            public uint VectorIndex => _node.VectorIndex;

            /// <summary>
            /// Ball radius
            /// </summary>
            public T Radius => _data.Length > 0 ? _data[0] : T.Zero;

            /// <summary>
            /// Ball centroid
            /// </summary>
            public ReadOnlySpan<T> Centroid => _data.Length > 0 ? _data[1..] : [];

            /// <summary>
            /// Left node index
            /// </summary>
            public uint? LeftNodeIndex => _node.LeftNodeIndex;

            /// <summary>
            /// Right node index
            /// </summary>
            public uint? RightNodeIndex => _node.RightNodeIndex;

            /// <summary>
            /// True if the node has a left branch
            /// </summary>
            public bool HasLeftBranch => _node.HasLeftBranch;

            /// <summary>
            /// True if the node has a right branch
            /// </summary>
            public bool HasRightBranch => _node.HasRightBranch;

            /// <summary>
            /// Deconstructs the node
            /// </summary>
            /// <param name="left"></param>
            /// <param name="right"></param>
            /// <param name="vectorIndex"></param>
            /// <param name="radius"></param>
            /// <param name="centroid"></param>
            public void Deconstruct(out uint vectorIndex, out T radius, out ReadOnlySpan<T> centroid, out uint? left, out uint? right)
            {
                vectorIndex = VectorIndex;
                radius = Radius;
                centroid = Centroid;
                left = LeftNodeIndex;
                right = RightNodeIndex;
            }
        }

        readonly ReadOnlyMemory<T> _vectorBuffer;
        readonly Dictionary<uint, uint> _kdTreeNodeIndexToVectorOffset;
        readonly int _vectorDataSize;

        /// <summary>
        /// Creates a ball tree from a store of vectors
        /// </summary>
        /// <param name="vectors"></param>
        /// <param name="distanceMetric"></param>
        public VectorBallTree(IReadOnlyVectorStore<T> vectors, DistanceMetric distanceMetric)
            : this(new VectorKDTree<T>(vectors), distanceMetric)
        {
        }

        /// <summary>
        /// Creates a ball tree from a KD tree
        /// </summary>
        /// <param name="kdTree"></param>
        /// <param name="distanceMetric"></param>
        public VectorBallTree(VectorKDTree<T> kdTree, DistanceMetric distanceMetric)
        {
            DistanceMetric = distanceMetric;
            KDTree = kdTree;
            var vectors = kdTree.Vectors;
            _vectorDataSize = (int)vectors.VectorSize + 1;
            var vectorBuffer = new ArrayBufferWriter<T>(/*(int)vectors.Size * _vectorDataSize*/);
            _kdTreeNodeIndexToVectorOffset = new((int)vectors.Size);

            // recursively add corresponding ball nodes
            var queue = new Queue<uint>();
            queue.Enqueue(KDTree.RootNodeIndex);

            while (queue.Count > 0) {
                var nodeIndex = queue.Dequeue();
                var vectorIndices = KDTree.GetAllVectorIndices(nodeIndex).ToArray();

                // find the centroid and distance from that centroid
                if (vectorIndices.Length > 1) {
                    // find centroid
                    var centroidBuilder = SpanAggregator<T>.GetOnlineAverage(vectors.VectorSize);
                    foreach (var index in vectorIndices)
                        centroidBuilder.Add(vectors[index]);
                    var centroid = centroidBuilder.Span;

                    // calculate max distance to centroid
                    using var distances = vectors.FindDistancesFromVectorsToQuery(vectorIndices, centroid, distanceMetric);
                    var bestDistance = distances.Span.AsReadOnly().Maximum().Value;

                    //var maxDistance = T.MinValue;
                    //foreach (var vectorIndex in vectorIndices) {
                    //    var distance = centroid.FindDistance(vectors[vectorIndex], distanceMetric);
                    //    if (distance > maxDistance)
                    //        maxDistance = distance;
                    //}

                    // write the vector node
                    _kdTreeNodeIndexToVectorOffset.Add(nodeIndex, (uint)vectorBuffer.WrittenCount);
                    var vectorSpan = vectorBuffer.GetSpan(_vectorDataSize);
                    vectorSpan[0] = bestDistance;
                    centroid.CopyTo(vectorSpan[1..]);
                    vectorBuffer.Advance(_vectorDataSize);
                }

                // add children nodes
                var node = KDTree.GetNodeByIndex(nodeIndex);
                if (node.HasLeftBranch)
                    queue.Enqueue(node.LeftNodeIndex!.Value);
                if (node.HasRightBranch)
                    queue.Enqueue(node.RightNodeIndex!.Value);
            }

            _vectorBuffer = vectorBuffer.WrittenMemory;
        }

        /// <summary>
        /// Distance metric
        /// </summary>
        public DistanceMetric DistanceMetric { get; }

        /// <summary>
        /// Vector store
        /// </summary>
        public IReadOnlyVectorStore<T> Vectors => KDTree.Vectors;

        /// <inheritdoc />
        public uint Size => Vectors.Size;

        /// <summary>
        /// The node index of the root node
        /// </summary>
        public uint RootNodeIndex => KDTree.RootNodeIndex;

        /// <summary>
        /// Underlying KD tree
        /// </summary>
        public VectorKDTree<T> KDTree { get; }

        /// <summary>
        /// Returns a node from a node index
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        public ReadOnlyNode GetNodeByIndex(uint nodeIndex)
        {
            return _kdTreeNodeIndexToVectorOffset.TryGetValue(nodeIndex, out var vectorOffset) 
                ? new ReadOnlyNode(_vectorBuffer.Span.Slice((int)vectorOffset, _vectorDataSize), KDTree.GetNodeByIndex(nodeIndex)) 
                : new ReadOnlyNode([], KDTree.GetNodeByIndex(nodeIndex))
            ;
        }

        /// <summary>
        /// Searches for the best vector index relative to a query vector
        /// </summary>
        /// <param name="query">Query vector</param>
        /// <returns></returns>
        public (uint BestVectorIndex, T Distance) Search(ReadOnlySpan<T> query)
        {
            var temp = new FixedSizeSortedAscending1Array<uint, T>();
            KnnSearch(query, KDTree.RootNodeIndex, ref temp);
            return (temp.MinValue, temp.MinWeight);
        }

        /// <summary>
        /// Searches for N best results relative to a query vector
        /// </summary>
        /// <typeparam name="AT"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public AT KnnSearch<AT>(ReadOnlySpan<T> query)
            where AT : IFixedSizeSortedArray<uint, T>, new()
        {
            var ret = new AT();
            KnnSearch(query, KDTree.RootNodeIndex, ref ret);
            return ret;
        }

        [SkipLocalsInit]
        void KnnSearch<AT>(ReadOnlySpan<T> query, uint fromNodeIndex, ref AT results)
            where AT : IFixedSizeSortedArray<uint, T>
        {
            const int MaxDepth = 128;
            Span<uint> stack = stackalloc uint[MaxDepth];
            stack[0] = fromNodeIndex;
            var stackSize = 1;

            while (stackSize > 0) {
                var nodeIndex = stack[--stackSize];
                var node = GetNodeByIndex(nodeIndex);

                // check if leaf node
                bool hasLeft = node.HasLeftBranch, hasRight = node.HasRightBranch;
                if (!hasLeft && !hasRight) {
                    results.TryAdd(node.VectorIndex, Vectors[node.VectorIndex].FindDistance(query, DistanceMetric));
                    continue;
                }

                // check ball radius against query distance from centroid
                if (results.Size > 0 && node.Centroid.FindDistance(query, DistanceMetric) - node.Radius > results.MaxWeight)
                    continue;

                // query is within the ball
                results.TryAdd(node.VectorIndex, Vectors[node.VectorIndex].FindDistance(query, DistanceMetric));
                if (hasLeft && stackSize < MaxDepth)
                    stack[stackSize++] = node.LeftNodeIndex!.Value;
                if (hasRight && stackSize < MaxDepth)
                    stack[stackSize++] = node.RightNodeIndex!.Value;
            }
        }
    }
}
