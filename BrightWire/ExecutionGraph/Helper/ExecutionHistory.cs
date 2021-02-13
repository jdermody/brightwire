namespace BrightWire.ExecutionGraph.Helper
{
    public class ExecutionHistory
    {
		/// <summary>
		/// Creates a training action history from a single optional parent
		/// </summary>
		/// <param name="source">The node that executed</param>
		/// <param name="data">The output of the node</param>
		/// <param name="parent">The single parent that contributed to the output (optional)</param>
	    public ExecutionHistory(INode source, IGraphData data, INode? parent = null)
        {
            Parents = parent != null 
                ? new[] { parent } 
                : new INode[0];

            Source = source;
            Data = data;
        }

		/// <summary>
		/// Creates a training action history from multiple parents
		/// </summary>
		/// <param name="source">The node that executed</param>
		/// <param name="data">The output of the node</param>
		/// <param name="parents">The parent nodes that contributed to the output</param>
        public ExecutionHistory(INode source, IGraphData data, INode[] parents)
        {
            Parents = parents;
            Source = source;
            Data = data;
        }

        /// <summary>
        /// Node that was executed
        /// </summary>
        public INode Source { get; }
        
        /// <summary>
        /// Node output signal
        /// </summary>
        public IGraphData Data { get; }

        /// <summary>
        /// Optional backpropagation
        /// </summary>
        public IBackpropagate? Backpropagation { get; set; } = null;

        /// <summary>
        /// The node's parents
        /// </summary>
        public INode[] Parents { get; }

        public override string ToString() => $"{Source} {Data} ({Parents.Length})";
    }
}
