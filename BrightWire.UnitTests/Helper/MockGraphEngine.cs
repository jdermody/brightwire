using System;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Node;
using BrightWire.Models;

namespace BrightWire.UnitTests.Helper
{
    internal class MockGraphEngine : IGraphEngine
    {
        public MockGraphEngine(LinearAlgebraProvider lap)
        {
            LinearAlgebraProvider = lap;
        }

        public IGraphContext Create(GraphExecutionContext executionContext, IMiniBatchSequence sequence, ILearningContext? learningContext)
        {
            throw new NotImplementedException();
        }

        public LinearAlgebraProvider LinearAlgebraProvider { get; }
        public ExecutionGraphModel Graph { get; } = null!;
        public IDataSource? DataSource { get; } = null!;
        public NodeBase Start { get; } = null!;
    }
}
