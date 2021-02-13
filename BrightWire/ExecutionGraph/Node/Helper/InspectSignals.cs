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

		public override void ExecuteForward(IGraphSequenceContext context)
		{
			_forwardCallback?.Invoke(context.Data);
			if(_backwardCallback != null)
				AddNextGraphAction(context, context.Data, () => new Backpropagation(this));
			else
				AddNextGraphAction(context, context.Data, null);
		}

        public override (INode FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            _forwardCallback?.Invoke(signal);
            if(_backwardCallback != null)
                return (this, signal, () => new Backpropagation(this));
            else
                return (this, signal, null);
        }
    }
}
