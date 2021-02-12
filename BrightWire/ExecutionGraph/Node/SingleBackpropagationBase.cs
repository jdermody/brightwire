using System.Collections.Generic;

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
        /// Backpropagation implementation
        /// </summary>
        /// <param name="fromNode">The node that sent the backpropagation signal</param>
        /// <param name="errorSignal">The backpropagating error</param>
        /// <param name="context">Graph context</param>
        /// <param name="parents">Parents of the current node</param>
        /// <returns></returns>
        protected abstract IGraphData Backpropagate(INode? fromNode, IGraphData errorSignal, IGraphSequenceContext context, INode[] parents);

        protected abstract IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context);

        public override IEnumerable<(IGraphData Signal, INode ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, INode[] parents)
        {
            foreach (var parent in parents)
                yield return (Backpropagate(errorSignal, context), parent);
        }
    }
}
