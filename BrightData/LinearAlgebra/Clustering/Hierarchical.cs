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

            public Node(IReadOnlyVector vector, uint index)
            {
                Vector = vector;
                _indices.Add(index);
            }

            public Node(Node left, Node right)
            {
                _indices.AddRange(left._indices);
                _indices.AddRange(right._indices);
                using var segment = left.Vector.Add(right.Vector);
                segment.ApplySpan(true, x => x.MultiplyInPlace(0.5f));
                Vector = segment.ToNewArray().ToReadOnlyVector();
            }
            public IReadOnlyVector Vector { get; }
            public uint[] GetIndices() => _indices.ToArray();
        }
        record NodePair(LinkedListNode<Node> Left, LinkedListNode<Node> Right);

        public uint[][] Cluster(IReadOnlyVector[] vectors, uint numClusters, DistanceMetric metric)
        {
            var nodes = new LinkedList<Node>(vectors.Select((x, i) => new Node(x, (uint)i)));
            var cache = new Dictionary<NodePair, float>();
            while (nodes.Count > numClusters)
            {
                var closest = FindMinimumDistance(nodes, metric, cache);
                if (closest is null)
                    break;

                nodes.AddAfter(closest.Left, new Node(closest.Left.Value, closest.Right.Value));
                nodes.Remove(closest.Left);
                nodes.Remove(closest.Right);
            }
            return nodes.Select(x => x.GetIndices()).ToArray();
        }

        static NodePair? FindMinimumDistance(LinkedList<Node> nodes, DistanceMetric metric, Dictionary<NodePair, float> cache)
        {
            NodePair? ret = null;
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
                    var pair = new NodePair(node, p);
                    var distance = GetDistance(pair, metric, cache);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        ret = pair;
                    }
                    p = p.Next;
                }
                node = next;
            }

            return ret;
        }

        static float GetDistance(NodePair pair, DistanceMetric metric, Dictionary<NodePair, float> cache)
        {
            if (!cache.TryGetValue(pair, out var distance))
                cache.Add(pair, distance = pair.Left.Value.Vector.FindDistance(pair.Right.Value.Vector, metric));
            return distance;
        }
    }
}
