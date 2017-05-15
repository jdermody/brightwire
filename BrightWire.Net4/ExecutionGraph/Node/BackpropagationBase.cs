using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node
{
    abstract class BackpropagationBase<T> : IBackpropagation
        where T : INode
    {
        protected readonly T _source;

        protected BackpropagationBase(T source) { _source = source; }

        #region Disposal
        ~BackpropagationBase()
        {
            _Dispose(false);
        }
        public void Dispose()
        {
            _Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void _Dispose(bool isDisposing) { }
        #endregion

        public void Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
        {
            if (errorSignal == null) {
                if (parents?.Any() == true) {
                    foreach (var parent in parents)
                        context.AddBackward(errorSignal, parent, _source);
                }
            } else
                _Backward(fromNode, errorSignal, context, parents);
        }

        public abstract void _Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents);

        protected void _SendErrorTo(IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
        {
            if (parents?.Any() == true) {
                foreach (var parent in parents)
                    context.AddBackward(errorSignal, parent, _source);
            }
        }
    }
}
