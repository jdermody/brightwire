using System.Collections.Generic;
using System.Linq;

namespace BrightData.LinearAlgebra.Clustering
{
    /// <summary>
    /// Hierarchical clustering
    /// </summary>
    internal class Hierarchical : IClusteringStrategy
    {
        class Node
        {
            readonly List<uint> _indices = [];

            public Node(IReadOnlyVector<float> vector, uint index)
            {
                Vector = vector;
                Count = 1;
                _indices.Add(index);
            }

            public Node(Node left, Node right)
            {
                _indices.AddRange(left._indices);
                _indices.AddRange(right._indices);
                Count = left.Count + right.Count;
                Vector = left.Vector.Multiply(left.Count).Add(right.Vector.Multiply(right.Count))
                    .Multiply(1f / Count);
            }

            public IReadOnlyVector<float> Vector { get; }
            public int Count { get; }
            public uint[] GetIndices() => [.._indices];
        }
        public uint[][] Cluster(IReadOnlyVector<float>[] vectors, uint numClusters, DistanceMetric metric)
        {
            var nodes = new LinkedList<Node>(vectors.Select((x, i) => new Node(x, (uint)i)));
            while (nodes.Count > numClusters)
            {
                var closest = FindMinimumDistance(nodes, metric);
                if (closest is null)
                    break;

                var (left, right) = closest.Value;
                nodes.AddAfter(left, new Node(left.Value, right.Value));
                nodes.Remove(left);
                nodes.Remove(right);
            }
            return nodes.Select(x => x.GetIndices()).ToArray();
        }

        static (LinkedListNode<Node> Left, LinkedListNode<Node> Right)? FindMinimumDistance(
            LinkedList<Node> nodes, DistanceMetric metric)
        {
            (LinkedListNode<Node> Left, LinkedListNode<Node> Right)? best = null;
            var node = nodes.First;
            var shortestDistance = float.MaxValue;

            while (node is not null)
            {
                var next = node.Next;
                if (next is null)
                    break;

                var p = next;
                while (p is not null)
                {
                    var distance = node.Value.Vector.FindDistance(p.Value.Vector, metric);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        best = (node, p);
                    }
                    p = p.Next;
                }
                node = next;
            }

            return best;
        }
    }
}
