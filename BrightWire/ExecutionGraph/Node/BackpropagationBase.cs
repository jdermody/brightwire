using System;
using System.Linq;

namespace BrightWire.ExecutionGraph.Node
{
    /// <summary>
    /// Base class for node backpropagation
    /// </summary>
    /// <typeparam name="T">The node type</typeparam>
    public abstract class BackpropagationBase<T> : IBackpropagation
        where T : INode
    {
        /// <summary>
        /// The node that generated the forward signal
        /// </summary>
        protected readonly T _source;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The node that generated the forward signal</param>
        protected BackpropagationBase(T source) { _source = source; }

        #region Disposal
        /// <summary>
        /// Destructor
        /// </summary>
        ~BackpropagationBase()
        {
            _Dispose(false);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="isDisposing"></param>
        protected virtual void _Dispose(bool isDisposing) { }
        #endregion

        /// <summary>
        /// Called when backpropagating
        /// </summary>
        /// <param name="fromNode">The node that sent the backpropagation signal</param>
        /// <param name="errorSignal">The backpropagating error</param>
        /// <param name="context">Graph context</param>
        /// <param name="parents">Parents of the current node</param>
        public void Backward(INode fromNode, IGraphData errorSignal, IContext context, INode[] parents)
        {
            if (errorSignal == null) {
                if (parents?.Any() == true) {
                    foreach (var parent in parents)
                        context.AddBackward(null, parent, _source);
                }
            } else
                _Backward(fromNode, errorSignal, context, parents);
        }

        /// <summary>
        /// Called when a valid error signal has been received
        /// </summary>
        /// <param name="fromNode">>The node that sent the backpropagation signal</param>
        /// <param name="errorSignal">The backpropagating error</param>
        /// <param name="context">Graph context</param>
        /// <param name="parents">Parents of the current node</param>
        public abstract void _Backward(INode fromNode, IGraphData errorSignal, IContext context, INode[] parents);

        /// <summary>
        /// Sends a backpropagation signal further up the graph
        /// </summary>
        /// <param name="errorSignal">The backpropagating error</param>
        /// <param name="context">Graph context</param>
        /// <param name="parents">Parents of the current node</param>
        protected void _SendErrorTo(IGraphData errorSignal, IContext context, INode[] parents)
        {
            if (parents?.Any() == true) {
                foreach (var parent in parents)
                    context.AddBackward(errorSignal, parent, _source);
            }
        }
    }
}
