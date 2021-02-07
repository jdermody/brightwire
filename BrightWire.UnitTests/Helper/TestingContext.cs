using System;
using System.Collections.Generic;
using BrightData;
using BrightWire.Models;

namespace BrightWire.UnitTests.Helper
{
    internal class TestingContext : IGraphSequenceContext
    {
        public List<(IExecutionHistory, IBackpropagation)> Forward { get; } = new List<(IExecutionHistory, IBackpropagation)>();
        public List<(IGraphData, INode, INode)> Backward { get; } = new List<(IGraphData, INode, INode)>();

        public TestingContext(ILinearAlgebraProvider lap)
        {
            LinearAlgebraProvider = lap;
            LearningContext = new MockLearningContext();
        }

        public void Dispose()
        {
            // nop
        }

        public INode Source { get; }
        public IGraphData Data { get; set; }
        public IGraphExecutionContext ExecutionContext { get; }
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

        public void SetOutput(IGraphData data, int channel = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphData GetOutput(int channel = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphData[] Output { get; set; }
        public void StoreExecutionResult()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExecutionResult> Results { get; }
    }
}
