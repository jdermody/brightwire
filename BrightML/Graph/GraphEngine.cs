using System.Collections.Generic;
using System.Linq;

namespace BrightML.Graph
{
    class GraphEngine
    {
        class NodeContext
        {
            public NodeContext Previous { get; }
            public INode Node { get; }
            public IGraphData Data { get; }
            public uint Channel { get; }

            public NodeContext(NodeContext previousContext, INode node, IGraphData data, uint channel)
            {
                Previous = previousContext;
                Node = node;
                Data = data;
                Channel = channel;
            }
        }

        private readonly Graph _graph;
        List<NodeContext> _context;

        public GraphEngine(Graph graph, IEnumerable<IGraphData> input)
        {
            _graph = graph;
            _context = graph.Input
                .Zip(input, (n, d) => new NodeContext(null, n, d, 0))
                .ToList();
        }

        public bool Forward()
        {
            var nextContext = new List<NodeContext>();
            foreach (var item in _context) {
                var output = item.Node.Forward(item.Data, item.Channel);
                foreach(var wire in item.Node.Output)
                    nextContext.Add(new NodeContext(item, wire.Destination, output, wire.Channel));
            }

            _context = nextContext;
            return _context.Any();
        }

        public IReadOnlyList<(INode, IGraphData)> Backward()
        {
            var context = _context
                .Where(c => c.Previous != null)
                .Select(c => new NodeContext(c.Previous.Previous, c.Previous.Node, c.Data, c.Channel))
                .ToList();

            var visited = new HashSet<INode>();
            List<(INode, IGraphData)> backwardOutput = null;
            while (context.Any()) {
                var byNode = context.ToLookup(c => c.Node);
                var nodeOutput = new Dictionary<INode, IGraphData>();

                // group by node and channel
                foreach (var node in byNode) {
                    if (visited.Add(node.Key)) {
                        foreach (var group in node.GroupBy(d => d.Channel)) {
                            var output = node.Key.Backward(_Combine(group.Select(d => d.Data)), group.Key);
                            if (output != null)
                                nodeOutput.Add(node.Key, output);
                        }
                    }
                }

                var nextContext = new List<NodeContext>();
                foreach (var item in context.Where(c => c.Previous != null)) {
                    if(nodeOutput.TryGetValue(item.Node, out var output))
                        nextContext.Add(new NodeContext(item.Previous.Previous, item.Previous.Node, output, item.Channel));
                }

                context = nextContext;
                backwardOutput = nodeOutput.Select(kv => (kv.Key, kv.Value)).ToList();
            }

            return backwardOutput;
        }

        IGraphData _Combine(IEnumerable<IGraphData> data) => data.Single();
    }
}
