using System;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    internal class InspectSignals(Action<IGraphData>? forwardCallback, Action<IGraphData>? backwardCallback = null, string? name = null, string? id = null)
        : NodeBase(name, id)
    {
		class Backpropagation(InspectSignals source) : SingleBackpropagationBase<InspectSignals>(source)
        {
            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphContext context)
            {
                _source._backwardCallback?.Invoke(errorSignal);
                return errorSignal;
            }
        }

        readonly Action<IGraphData>? _backwardCallback = backwardCallback;

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source)
        {
            forwardCallback?.Invoke(signal);
            if(_backwardCallback != null)
                return (this, signal, () => new Backpropagation(this));
            else
                return (this, signal, null);
        }
    }
}
