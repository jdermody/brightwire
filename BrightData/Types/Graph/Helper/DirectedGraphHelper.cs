using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightData.Types.Graph.Helper
{
    internal static class DirectedGraphHelper<GT, NT>
        where GT: struct, IReadOnlyDirectedGraph<NT>
        where NT : unmanaged, IEquatable<NT>
    {
        public static IEnumerable<uint> TopologicalSort(ref GT graph)
        {
            var inDegree = new Dictionary<uint, uint>();
            for (var i = 0U; i < graph.Size; i++)
                inDegree.Add(i, graph.GetInDegree(i));
            var queue = new Queue<uint>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
            var sorted = new List<uint>();

            while (queue.Count > 0) {
                var node = queue.Dequeue();
                sorted.Add(node);

                foreach (var successor in graph.GetConnectedNodes(node)) {
                    inDegree[successor]--;
                    if (inDegree[successor] == 0)
                        queue.Enqueue(successor);
                }
            }

            // If sorted count != node count then there's a cycle
            return sorted.Count == graph.Size ? sorted : [];
        }
    }
}
