namespace BrightWire.ExecutionGraph.Helper
{
    /// <summary>
    /// A wire to a graph node
    /// </summary>
    class WireToNode : IWire
    {
        readonly INode _node;
        readonly int _channel;

        public WireToNode(INode node, int channel = 0) { _node = node; _channel = channel; }

        public INode SendTo => _node;
        public int Channel => _channel;
    }
}
