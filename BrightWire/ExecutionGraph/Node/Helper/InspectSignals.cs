using System;

namespace BrightWire.ExecutionGraph.Node.Helper
{
	class InspectSignals : NodeBase
	{
		class Backpropagation : SingleBackpropagationBase<InspectSignals>
		{
			public Backpropagation(InspectSignals source) : base(source)
			{
			}

			protected override IGraphData _Backpropagate(INode fromNode, IGraphData errorSignal, IGraphContext context, INode[] parents)
			{
				_source._backwardCallback(errorSignal);
				return errorSignal;
			}
		}

		readonly Action<IGraphData> _forwardCallback;
		readonly Action<IGraphData> _backwardCallback;

		public InspectSignals(Action<IGraphData> forwardCallback, Action<IGraphData> backwardCallback = null, string name = null, string id = null) : base(name, id)
		{
			_forwardCallback = forwardCallback;
			_backwardCallback = backwardCallback;
		}

		public override void ExecuteForward(IGraphContext context)
		{
			_forwardCallback?.Invoke(context.Data);
			if(_backwardCallback != null)
				_AddNextGraphAction(context, context.Data, () => new Backpropagation(this));
			else
				_AddNextGraphAction(context, context.Data, null);
		}
	}
}
