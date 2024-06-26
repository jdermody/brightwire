﻿using System;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using BrightWire.Models;

namespace BrightWire.UnitTests.Helper
{
    internal class MockGraphEngine(LinearAlgebraProvider<float> lap) : IGraphEngine
    {
        public IGraphContext Create(GraphExecutionContext executionContext, MiniBatch.Sequence sequence, ILearningContext? learningContext)
        {
            throw new NotImplementedException();
        }

        public LinearAlgebraProvider<float> LinearAlgebraProvider { get; } = lap;
        public ExecutionGraphModel Graph { get; } = null!;
        public IDataSource? DataSource { get; } = null!;
        public NodeBase Start { get; } = null!;
    }
}
