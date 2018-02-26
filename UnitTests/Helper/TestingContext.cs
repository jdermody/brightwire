using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire;

namespace UnitTests.Helper
{
	class TestingContext : IContext
	{
		public List<(IExecutionHistory, IBackpropagation)> Forward { get; } = new List<(IExecutionHistory, IBackpropagation)>();
		public List<(IGraphData, INode, INode)> Backward { get; } = new List<(IGraphData, INode, INode)>();

		public TestingContext(ILinearAlgebraProvider lap)
		{
			LinearAlgebraProvider = lap;
		}

		public void Dispose()
		{
			// nop
		}

		public bool IsTraining { get; set;  }
		public INode Source { get; }
		public IGraphData Data { get; set; }
		public IExecutionContext ExecutionContext { get; }
		public ILearningContext LearningContext { get; }
		public ILinearAlgebraProvider LinearAlgebraProvider { get; }

		public IMiniBatchSequence BatchSequence { get; }
		public void AddForward(IExecutionHistory action, Func<IBackpropagation> callback)
		{
			Forward.Add((action, callback()));
		}

		public void AddBackward(IGraphData errorSignal, INode target, INode source)
		{
			Backward.Add((errorSignal, target, source));
		}

		public void AppendErrorSignal(IGraphData errorSignal, INode forNode)
		{
			throw new NotImplementedException();
		}

		public void Backpropagate(IGraphData delta)
		{
			throw new NotImplementedException();
		}

		public IGraphData ErrorSignal { get; }
		public bool HasNext { get; }
		public bool ExecuteNext()
		{
			throw new NotImplementedException();
		}
	}
}
