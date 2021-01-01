﻿using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Execution engine context
    /// </summary>
    class ExecutionEngineContext : IGraphContext
    {
        readonly IGraphExecutionContext _executionContext;
        readonly List<IExecutionHistory> _forward = new List<IExecutionHistory>();
	    readonly Dictionary<int, IGraphData> _output = new Dictionary<int, IGraphData>();
        INode _sourceNode = null;
        IGraphData _data;

        public ExecutionEngineContext(IGraphExecutionContext executionContext, IMiniBatchSequence miniBatch)
        {
            _executionContext = executionContext;
            BatchSequence = miniBatch;
            _data = null;
        }

        public void Dispose()
        {
            // nop
        }

        public bool IsTraining => false;
        public INode Source => _sourceNode;
        public IGraphExecutionContext ExecutionContext => _executionContext;
        public ILearningContext LearningContext => null;
        public ILinearAlgebraProvider LinearAlgebraProvider => _executionContext.LinearAlgebraProvider;
        public IMiniBatchSequence BatchSequence { get; }
        public void AddBackward(IGraphData errorSignal, INode target, INode source) => throw new NotImplementedException();
        public void Backpropagate(IGraphData delta) => throw new NotImplementedException();
        public void AppendErrorSignal(IGraphData errorSignal, INode forNode) => throw new NotImplementedException();
        public void AddForward(IExecutionHistory action, Func<IBackpropagation> callback) => _forward.Add(action);
        public IGraphData ErrorSignal => throw new NotImplementedException();
        public bool HasNext => _forward.Any();
        public IGraphData Data => _data;

        public bool ExecuteNext()
        {
            if (HasNext) {
                var next = _forward.ElementAt(0);
                _forward.RemoveAt(0);

                _data = next.Data;

                _sourceNode = next.Source;
                if (next.Source.Output != null) {
                    foreach (var output in next.Source.Output)
                        output.SendTo?.ExecuteForward(this, output.Channel);
                }

                return true;
            }
            return false;
        }

	    public void SetOutput(IGraphData data, int channel = 0)
	    {
		    _output[channel] = data;
	    }

	    public IGraphData GetOutput(int channel = 0)
	    {
		    if (_output.TryGetValue(channel, out var ret))
			    return ret;
		    return null;
	    }

	    public IGraphData[] Output => _output
            .OrderBy(kv => kv.Key)
            .Select(kv => kv.Value)
            .ToArray()
        ;
    }
}