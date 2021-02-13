using System;
using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Node
{
    /// <summary>
    /// Base class for node backpropagation
    /// </summary>
    /// <typeparam name="T">The node type</typeparam>
    public abstract class BackpropagationBase<T> : IBackpropagate
        where T : NodeBase
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
            DisposeMemory(false);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            DisposeMemory(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="isDisposing"></param>
        protected virtual void DisposeMemory(bool isDisposing) { }
        #endregion

        protected IEnumerable<(IGraphData signal, NodeBase toNode)> ErrorTo(IGraphData errorSignal, NodeBase[] parents)
        {
            foreach (var parent in parents)
                yield return (errorSignal, parent);
        }

        public abstract IEnumerable<(IGraphData Signal, NodeBase ToNode)> Backward(IGraphData errorSignal, IGraphSequenceContext context, NodeBase[] parents);
    }
}
