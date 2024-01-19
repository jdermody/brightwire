using System;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.ExecutionGraph.Helper
{
    /// <summary>
    /// Record of graph execution
    /// </summary>
    public class ExecutionHistory
    {
		/// <summary>
		/// Creates a training action history from a single optional parent
		/// </summary>
		/// <param name="source">The node that executed</param>
		/// <param name="data">The output of the node</param>
		/// <param name="parent">The single parent that contributed to the output (optional)</param>
	    public ExecutionHistory(NodeBase source, IGraphData data, NodeBase? parent = null)
        {
            Parents = parent != null 
                ? [parent]
                : Array.Empty<NodeBase>();

            Source = source;
            Data = data;
        }

		/// <summary>
		/// Creates a training action history from multiple parents
		/// </summary>
		/// <param name="source">The node that executed</param>
		/// <param name="data">The output of the node</param>
		/// <param name="parents">The parent nodes that contributed to the output</param>
        public ExecutionHistory(NodeBase source, IGraphData data, NodeBase[] parents)
        {
            Parents = parents;
            Source = source;
            Data = data;
        }

        /// <summary>
        /// Node that was executed
        /// </summary>
        public NodeBase Source { get; }
        
        /// <summary>
        /// Node output signal
        /// </summary>
        public IGraphData Data { get; }

        /// <summary>
        /// Optional backpropagation
        /// </summary>
        public IBackpropagate? Backpropagation { get; set; } = null;

        /// <summary>
        /// The node's ancestors
        /// </summary>
        public NodeBase[] Parents { get; }

        /// <inheritdoc />
        public override string ToString() => $"{Source} {Data} ({Parents.Length})";
    }
}
