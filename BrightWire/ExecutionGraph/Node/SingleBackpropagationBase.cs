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
        /// Backpropagates the error
        /// </summary>
        /// <param name="errorSignal"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context);

        /// <inheritdoc />
        public override IEnumerable<(IGraphData Signal, IGraphContext Context, NodeBase? ToNode)> Backward(IGraphData errorSignal, IGraphContext context, NodeBase[] parents)
        {
            var ret = Backpropagate(errorSignal, context);
            var returnedSomething = false;
            foreach (var parent in parents) {
                yield return (ret, context, parent);
                returnedSomething = true;
            }

            if (!returnedSomething)
                yield return (ret, context, null);
        }
    }
}
