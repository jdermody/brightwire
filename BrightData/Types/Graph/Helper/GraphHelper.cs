using System;
using System.Collections.Generic;

namespace BrightData.Types.Graph.Helper
{
    internal static class GraphHelper<GT>
        where GT : struct, IReadOnlyGraph
    {
        public static IEnumerable<uint> DepthFirstSearch(ref GT graph, uint startNode, Func<uint, bool>? shouldVisit = null)
        {
            var ret = new List<uint>();
            var visited = new HashSet<uint>();
            var stack = new Stack<uint>();
            stack.Push(startNode);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (visited.Add(node) && (shouldVisit?.Invoke(node) ?? true))
                {
                    ret.Add(node);
                    foreach (var successor in graph.GetConnectedNodes(node))
                        stack.Push(successor);
                }
            }

            return ret;
        }

        public static IEnumerable<uint> BreadthFirstSearch(ref GT graph, uint startNode, Func<uint, bool>? shouldVisit = null)
        {
            var ret = new List<uint>();
            var visited = new HashSet<uint>();
            var queue = new Queue<uint>();
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (visited.Add(node) && (shouldVisit?.Invoke(node) ?? true))
                {
                    ret.Add(node);
                    foreach (var successor in graph.GetConnectedNodes(node))
                        queue.Enqueue(successor);
                }
            }
            return ret;
        }
    }
}
