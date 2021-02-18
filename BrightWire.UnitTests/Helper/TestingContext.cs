using System;
using System.Collections.Generic;
using BrightData;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using BrightWire.Models;

namespace BrightWire.UnitTests.Helper
{
    internal class TestingContext : IGraphSequenceContext
    {
        public List<(ExecutionHistory, IBackpropagate)> Forward { get; } = new List<(ExecutionHistory, IBackpropagate)>();

        public TestingContext(ILinearAlgebraProvider lap)
        {
            LinearAlgebraProvider = lap;
            LearningContext = new MockLearningContext();
        }

        public void Dispose()
        {
            // nop
        }

        public NodeBase Source { get; }
        public IGraphData Data { get; set; }
        public IGraphExecutionContext ExecutionContext { get; }
        public ILearningContext LearningContext { get; }
        public ILinearAlgebraProvider LinearAlgebraProvider { get; }

        public IMiniBatchSequence BatchSequence { get; }


        public void AddForwardHistory(NodeBase source, IGraphData data, Func<IBackpropagate>? callback, params NodeBase[] prev)
        {
            Forward.Add((new ExecutionHistory(source, data), callback!()));
        }

        public IGraphData? Backpropagate(IGraphData? delta)
        {
            throw new NotImplementedException();
        }

        public IGraphData ErrorSignal { get; }

        public void SetOutput(IGraphData data, int channel = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphData GetOutput(int channel = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphData[] Output { get; set; }

        public IEnumerable<ExecutionResult> Results { get; }
        public void ClearForBackpropagation()
        {
            throw new NotImplementedException();
        }

        public void SetData(string name, string type, IGraphData data)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<(string Name, IGraphData Data)> GetData(string type)
        {
            throw new NotImplementedException();
        }
    }
}
