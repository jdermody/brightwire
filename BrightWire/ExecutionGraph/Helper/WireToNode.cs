using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Helper
{
    /// <summary>
    /// Wires connect nodes in the graph
    /// </summary>
	public class WireToNode 
    {
	    /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="node">The destination node</param>
		/// <param name="channel">The input channel on the node</param>
        public WireToNode(NodeBase node, uint channel = 0) { SendTo = node; Channel = channel; }

        /// <summary>
        /// The node to send a signal to
        /// </summary>
        public NodeBase SendTo { get; }

        /// <summary>
        /// The channel to send the signal on
        /// </summary>
	    public uint Channel { get; }
    }
}
