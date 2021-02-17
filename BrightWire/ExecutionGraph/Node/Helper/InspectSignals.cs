using System;

namespace BrightWire.ExecutionGraph.Node.Helper
{
    internal class InspectSignals : NodeBase
	{
		class Backpropagation : SingleBackpropagationBase<InspectSignals>
		{
			public Backpropagation(InspectSignals source) : base(source)
			{
			}

            protected override IGraphData Backpropagate(IGraphData errorSignal, IGraphSequenceContext context)
            {
                _source._backwardCallback?.Invoke(errorSignal);
                return errorSignal;
            }
        }

		readonly Action<IGraphData>? _forwardCallback;
		readonly Action<IGraphData>? _backwardCallback;

		public InspectSignals(Action<IGraphData>? forwardCallback, Action<IGraphData>? backwardCallback = null, string? name = null, string? id = null) : base(name, id)
		{
			_forwardCallback = forwardCallback;
			_backwardCallback = backwardCallback;
		}

        public override (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphSequenceContext context, NodeBase? source)
        {
            _forwardCallback?.Invoke(signal);
            if(_backwardCallback != null)
                return (this, signal, () => new Backpropagation(this));
            else
                return (this, signal, null);
        }
    }
}
