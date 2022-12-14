using System;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Node;

namespace BrightWire.UnitTests.Helper
{
    internal class MockLearningContext : ILearningContext
    {
#pragma warning disable 8618
        public MockLearningContext()
#pragma warning restore 8618
        {
        }

        public double EpochSeconds { get; set; }
        public long EpochMilliseconds { get; set; }
        public uint CurrentEpoch { get; set; }
        public float LearningRate { get; set; }
        public float BatchLearningRate { get; set; }
        public uint BatchSize { get; set; }
        public uint RowCount { get; set; }
        public void StoreUpdate(NodeBase fromNode, IMatrix update, Action<IMatrix> updater)
        {

        }

        public void StoreUpdate(NodeBase fromNode, IVector update, Action<IVector> updater)
        {
            
        }

        public bool DeferUpdates { get; set; }
        public void AddError(NodeErrorType errorType, NodeBase fromNode, ITensor error)
        {
        }

        public void ApplyUpdates()
        {
            throw new NotImplementedException();
        }

        public void StartEpoch()
        {
            throw new NotImplementedException();
        }

        public void EndEpoch()
        {
            throw new NotImplementedException();
        }

        public void SetRowCount(uint rowCount)
        {
            throw new NotImplementedException();
        }

        public void DeferBackpropagation(IGraphData? errorSignal, Func<IGraphData?, IGraphData?> update)
        {
            throw new NotImplementedException();
        }

        public IGraphData BackpropagateThroughTime(IGraphData? signal, int maxDepth = 2147483647)
        {
            throw new NotImplementedException();
        }

        public void ScheduleLearningRate(uint atEpoch, float newLearningRate)
        {
            throw new NotImplementedException();
        }

        public void EnableNodeUpdates(NodeBase node, bool enableUpdates)
        {
            throw new NotImplementedException();
        }


        public Action<string> MessageLog { get; set; }
        public event Action<ILearningContext> BeforeEpochStarts;
        public event Action<ILearningContext> AfterEpochEnds;
        public IErrorMetric ErrorMetric { get; set; }
        public GraphFactory GraphFactory { get; set; }
        public void ResetEpoch()
        {
            throw new NotImplementedException();
        }
    }
}
