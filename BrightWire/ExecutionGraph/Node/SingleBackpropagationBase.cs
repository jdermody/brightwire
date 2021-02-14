using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Node
{
    /// <summary>
    /// Base class for nodes that back propagate to a single parent
    /// </summary>
    /// <typeparam name="T">The node type</typeparam>
    public abstract class SingleBackpropagationBase<T> : BackpropagationBase<T>
        where T : NodeBase
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
        //protected abstract IGraphData Backpropagate(NodeBase? fromNode, IGraphData errorSignal, IGraphSequenceContext context, NodeBase[] parents);

        protected abstract IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context);

        public override IEnumerable<(IGraphData Signal, IGraphSequenceContext Context, NodeBase ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, NodeBase[] parents)
        {
            foreach (var parent in parents)
                yield return (Backpropagate(errorSignal, context), context, parent);
        }
    }
}
