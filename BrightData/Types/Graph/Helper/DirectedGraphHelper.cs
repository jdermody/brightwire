using System;
using System.Collections.Generic;

namespace BrightData.Types.Graph.Helper
{
    internal static class DirectedGraphHelper<GT>
        where GT: struct, IReadOnlyDirectedGraph
    {
        public static IEnumerable<uint> TopologicalSort(ref GT graph)
        {
            var inDegree = new uint[graph.Size];
            for (var i = 0U; i < graph.Size; i++)
                inDegree[i] = graph.GetInDegree(i);

            var queue = new Queue<uint>();
            for (var i = 0U; i < graph.Size; i++)
                if (inDegree[i] == 0)
                    queue.Enqueue(i);

            var count = 0U;
            var sorted = new uint[graph.Size];
            while (queue.Count > 0) {
                var node = queue.Dequeue();
                sorted[count++] = node;

                foreach (var successor in graph.GetConnectedNodes(node)) {
                    inDegree[successor]--;
                    if (inDegree[successor] == 0)
                        queue.Enqueue(successor);
                }
            }

            // If sorted count != node count then there's a cycle
            return count == graph.Size 
                ? sorted[..(int)count] 
                : []
            ;
        }
    }
}