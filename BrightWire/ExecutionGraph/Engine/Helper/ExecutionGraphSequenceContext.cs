﻿using System;
using System.Collections.Generic;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Execution engine context
    /// </summary>
    internal class ExecutionGraphSequenceContext : SequenceContextBase, IGraphContext
    {
        public ExecutionGraphSequenceContext(GraphExecutionContext executionContext, MiniBatch.Sequence miniBatch) : base(miniBatch)
        {
            ExecutionContext = executionContext;
            BatchSequence.GraphContext = this;
        }

        public void Dispose()
        {
            // nop
        }

        public bool IsTraining => false;
        public GraphExecutionContext ExecutionContext { get; }
        public ILearningContext? LearningContext => null;
        public LinearAlgebraProvider<float> LinearAlgebraProvider => ExecutionContext.LinearAlgebraProvider;
        
        public IGraphData Backpropagate(IGraphData? delta) => throw new NotImplementedException();
        public void AddForwardHistory(NodeBase source, IGraphData data, Func<IBackpropagate>? callback, params NodeBase[] prev) { /* nop */ }
        public IGraphData ErrorSignal => throw new NotImplementedException();

        public IEnumerable<ExecutionResult> Results 
        {
            get
            {
                if (Data.HasValue)
                    yield return new ExecutionResult(BatchSequence, Data.GetMatrix(), ExecutionContext.WantInputInExecutionResults);
            }
        }

        public void ClearForBackpropagation()
        {
            // nop
        }
    }
}
