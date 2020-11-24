namespace BrightWire.ExecutionGraph.Node
{
    /// <summary>
    /// Base class for nodes that back propagate to a single parent
    /// </summary>
    /// <typeparam name="T">The node type</typeparam>
    public abstract class SingleBackpropagationBase<T> : BackpropagationBase<T>
        where T : INode
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The node that generated the forward signal</param>
        protected SingleBackpropagationBase(T source) : base(source) { }

        /// <summary>
        /// Called when a valid error signal has been received
        /// </summary>
        /// <param name="fromNode">The node that sent the backpropagation signal</param>
        /// <param name="errorSignal">The backpropagating error</param>
        /// <param name="context">Graph context</param>
        /// <param name="parents">Parents of the current node</param>
        public override void _Backward(INode fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents)
        {
            IGraphData nextError = null;
            if(errorSignal != null)
                nextError = _Backpropagate(fromNode, errorSignal, context, parents);
            if(nextError != null)
				_SendErrorTo(nextError, context, parents);
        }

        /// <summary>
        /// Backpropagation implementation
        /// </summary>
        /// <param name="fromNode">The node that sent the backpropagation signal</param>
        /// <param name="errorSignal">The backpropagating error</param>
        /// <param name="context">Graph context</param>
        /// <param name="parents">Parents of the current node</param>
        /// <returns></returns>
        protected abstract IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents);
    }
}
