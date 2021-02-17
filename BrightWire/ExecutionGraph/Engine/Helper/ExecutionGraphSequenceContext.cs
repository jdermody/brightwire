using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Execution engine context
    /// </summary>
    internal class ExecutionGraphSequenceContext : IGraphSequenceContext
    {
        readonly IGraphExecutionContext _executionContext;
        readonly Dictionary<int, IGraphData> _output = new Dictionary<int, IGraphData>();

        public ExecutionGraphSequenceContext(IGraphExecutionContext executionContext, IMiniBatchSequence miniBatch)
        {
            _executionContext = executionContext;
            BatchSequence = miniBatch;
            Data = GraphData.Null;
        }

        public void Dispose()
        {
            // nop
        }

        public bool IsTraining => false;
        public IGraphExecutionContext ExecutionContext => _executionContext;
        public ILearningContext? LearningContext => null;
        public ILinearAlgebraProvider LinearAlgebraProvider => _executionContext.LinearAlgebraProvider;
        public IMiniBatchSequence BatchSequence { get; }
        public IGraphData Backpropagate(IGraphData? delta) => throw new NotImplementedException();
        public void AddForwardHistory(NodeBase source, IGraphData data, Func<IBackpropagate>? callback, params NodeBase[] prev) { /* nop */ }
        public IGraphData ErrorSignal => throw new NotImplementedException();
        public IGraphData Data { get; set; }

        public void SetOutput(IGraphData data, int channel = 0)
	    {
		    _output[channel] = data;
	    }

	    public IGraphData? GetOutput(int channel = 0)
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

        public IEnumerable<ExecutionResult> Results 
        {
            get
            {
                if (Data.HasValue)
                    yield return  new ExecutionResult(BatchSequence, Data.GetMatrix().Data.Rows.ToArray());
            }
        }

        public void ClearForBackpropagation()
        {
            // nop
        }
    }
}
