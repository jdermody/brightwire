namespace BrightWire.ExecutionGraph.Helper
{
	/// <inheritdoc />
	public class WireToNode : IWire
    {
	    /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="node">The destination node</param>
		/// <param name="channel">The input channel on the node</param>
        public WireToNode(INode node, uint channel = 0) { SendTo = node; Channel = channel; }
	    /// <inheritdoc />
        public INode SendTo { get; }
	    /// <inheritdoc />
	    public uint Channel { get; }
    }
}
