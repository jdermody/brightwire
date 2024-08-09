using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using BrightData.Types;

namespace BrightData.Helper
{
    /// <summary>
    /// KD Tree for vectors
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VectorKDTree<T>
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        readonly ref struct Node(uint nodeIndex, Span<uint> indices)
        {
            readonly Span<uint> _indices = indices;

            public uint NodeIndex => nodeIndex;

            public uint? LeftNodeIndex
            {
                set => _indices[1] = value ?? uint.MaxValue;
            }
            public uint? RightNodeIndex
            {
                set => _indices[2] = value ?? uint.MaxValue;
            }
        }

        /// <summary>
        /// Node in the tree
        /// </summary>
        /// <param name="indices">Span that contains the node data</param>
        public readonly ref struct ReadOnlyNode(ReadOnlySpan<uint> indices)
        {
            readonly ReadOnlySpan<uint> _indices = indices;

            /// <summary>
            /// Vector index
            /// </summary>
            public uint VectorIndex => _indices[0];

            /// <summary>
            /// Left node index
            /// </summary>
            public uint? LeftNodeIndex => _indices[1] == uint.MaxValue ? null : _indices[1];

            /// <summary>
            /// Right node index
            /// </summary>
            public uint? RightNodeIndex => _indices[2] == uint.MaxValue ? null : _indices[2];

            /// <summary>
            /// True if the node has a left branch
            /// </summary>
            public bool HasLeftBranch => _indices[1] != uint.MaxValue;

            /// <summary>
            /// True if the node has a right branch
            /// </summary>
            public bool HasRightBranch => _indices[2] != uint.MaxValue;
        }

        readonly IStoreVectors<T> _storage;
        readonly ArrayBufferWriter<uint> _indexBuffer;

        /// <summary>
        /// Creates a vector kd tree from all vectors in the vector storage
        /// </summary>
        /// <param name="storage"></param>
        public VectorKDTree(IStoreVectors<T> storage)
        {
            _storage = storage;
            _indexBuffer = new((int)storage.Size * 3);
            BuildTree(storage.Size.AsRange().ToArray(), 0);
        }

        uint? BuildTree(Span<uint> vectorIndices, int depth)
        {
            if (vectorIndices.IsEmpty)
                return null;
            if (vectorIndices.Length == 1)
                return CreateNode(vectorIndices[0]).NodeIndex;

            var axis = depth % (int)_storage.VectorSize;
            vectorIndices.Sort((x, y) => _storage[x][axis].CompareTo(_storage[y][axis]));

            var medianIndex = vectorIndices.Length / 2;
            var vectorIndex = vectorIndices[medianIndex];
            var ret = CreateNode(vectorIndex);
            var leftSpan = vectorIndices[..medianIndex];
            var rightSpan = vectorIndices[(medianIndex + 1)..];
            ret.LeftNodeIndex = BuildTree(leftSpan, depth + 1);
            ret.RightNodeIndex = BuildTree(rightSpan, depth + 1);
            return ret.NodeIndex;
        }

        [SkipLocalsInit]
        Node CreateNode(uint vectorIndex)
        {
            Span<uint> nodeData = stackalloc uint[] {vectorIndex, uint.MaxValue, uint.MaxValue};
            var nodeIndex = (uint)_indexBuffer.WrittenCount;
            var ret = _indexBuffer.GetSpan(3);
            nodeData.CopyTo(ret);
            _indexBuffer.Advance(3);
            return new Node(nodeIndex, ret);
        }

        /// <summary>
        /// Returns a node by index
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        public ReadOnlyNode GetNodeByIndex(uint nodeIndex) => new(_indexBuffer.WrittenSpan.Slice((int)nodeIndex, 3));

        /// <summary>
        /// Finds a node by vector index
        /// </summary>
        /// <param name="vectorIndex"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public ReadOnlyNode GetNodeByVectorIndex(uint vectorIndex)
        {
            var stack = new Stack<uint>();
            stack.Push(0);

            while (stack.Count > 0) {
                var node = GetNodeByIndex(stack.Pop());
                if(node.VectorIndex == vectorIndex)
                    return node;
                var left = node.LeftNodeIndex;
                var right = node.RightNodeIndex;
                if (left.HasValue)
                    stack.Push(left.Value);
                if (right.HasValue)
                    stack.Push(right.Value);
            }

            throw new ArgumentException($"Node with vector index {vectorIndex} was not found");
        }

        /// <summary>
        /// Finds the closest node relative to the query vector
        /// </summary>
        /// <param name="query">Query vector</param>
        /// <returns></returns>
        public (uint BestVectorIndex, T Distance) Search(ReadOnlySpan<T> query)
        {
            var temp = new FixedSizeSortedAscending1Array<uint, T>();
            KnnSearch(query, 0, 0, ref temp);
            return (temp.MinValue, temp.MinWeight);
        }

        /// <summary>
        /// Finds the closest N nodes relative to the query vector
        /// </summary>
        /// <typeparam name="AT"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public AT KnnSearch<AT>(ReadOnlySpan<T> query)
            where AT: IFixedSizeSortedArray<uint, T>, new()
        {
            var ret = new AT();
            KnnSearch(query, 0, 0, ref ret);
            return ret;
        }

        void KnnSearch<AT>(ReadOnlySpan<T> query, uint nodeIndex, int depth, ref AT results) 
            where AT : IFixedSizeSortedArray<uint, T>
        {
            while (true) {
                var node = GetNodeByIndex(nodeIndex);
                var nodeVector = _storage[node.VectorIndex];
                bool hasLeft = node.HasLeftBranch, hasRight = node.HasRightBranch;

                // compare to node vector
                results.TryAdd(node.VectorIndex, query.EuclideanDistance(nodeVector));

                // two branches
                if (hasLeft && hasRight) {
                    var axis = depth % (int)_storage.VectorSize;
                    var qv = query[axis];
                    var nv = nodeVector[axis];
                    var (bestBranch, otherBranch) = qv.CompareTo(nv) >= 0
                        ? (node.RightNodeIndex, node.LeftNodeIndex)
                        : (node.LeftNodeIndex, node.RightNodeIndex);

                    // check the best branch
                    KnnSearch(query, bestBranch!.Value, depth + 1, ref results);

                    // optionally check the other branch
                    if (T.Abs(qv - nv) <= results.MaxWeight) {
                        nodeIndex = otherBranch!.Value;
                        depth += 1;
                        continue;
                    }
                }

                // single branch
                else if (hasLeft || hasRight) {
                    nodeIndex = hasLeft ? node.LeftNodeIndex!.Value : node.RightNodeIndex!.Value;
                    depth += 1;
                    continue;
                }

                break;
            }
        }
    }
}
