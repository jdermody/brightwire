using System;
using System.Collections.Generic;

namespace BrightData.Types.Graph.Helper
{
    internal static class GraphHelper<GT>
        where GT : struct, IReadOnlyGraph
    {
        public static IEnumerable<uint> DepthFirstSearch(ref GT graph, uint startNode, Func<uint, bool>? shouldVisit = null)
        {
            var stack = new Stack<uint>();
            stack.Push(startNode);
            return Search(ref graph, shouldVisit, stack, s => s.Count, (s, n) => s.Push(n));
        }

        public static IEnumerable<uint> BreadthFirstSearch(ref GT graph, uint startNode, Func<uint, bool>? shouldVisit = null)
        {
            var queue = new Queue<uint>();
            queue.Enqueue(startNode);
            return Search(ref graph, shouldVisit, queue, q => q.Count, (q, n) => q.Enqueue(n));
        }

        static IEnumerable<uint> Search<TCollection>(
            ref GT graph,
            Func<uint, bool>? shouldVisit,
            TCollection collection,
            Func<TCollection, int> getCount,
            Action<TCollection, uint> enqueue)
        {
            var ret = new uint[graph.Size];
            var visited = new bool[graph.Size];
            var count = 0;

            // Initial node already enqueued by caller
            while (getCount(collection) > 0)
            {
                var node = GetNextNode(collection);
                if (!visited[node] && (shouldVisit?.Invoke(node) ?? true))
                {
                    visited[node] = true;
                    ret[count++] = node;
                    foreach (var successor in graph.GetConnectedNodes(node))
                        if (!visited[successor])
                            enqueue(collection, successor);
                }
            }

            return ret[..count];
        }

        static uint GetNextNode<TCollection>(TCollection collection)
        {
            if (collection is Stack<uint> stack)
                return stack.Pop();
            if (collection is Queue<uint> queue)
                return queue.Dequeue();
            throw new ArgumentException("Collection must be Stack<uint> or Queue<uint>");
        }
    }
}
