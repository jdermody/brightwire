using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Training engine context
    /// </summary>
    internal class TrainingEngineContext : SequenceContextBase, IGraphSequenceContext
    {
        readonly IGraphExecutionContext _executionContext;
        readonly ILearningContext? _learningContext;
        readonly List<ExecutionHistory> _forward = new List<ExecutionHistory>();
        readonly Stack<(IGraphData ErrorSignal, INode Target, INode? Source)> _backward = new Stack<(IGraphData, INode, INode?)>();
        readonly Dictionary<INode, List<ExecutionHistory>> _history = new Dictionary<INode, List<ExecutionHistory>>();
        readonly Dictionary<INode, List<IGraphData>> _nodeErrorSignal = new Dictionary<INode, List<IGraphData>>();
	    readonly Dictionary<int, IGraphData> _output = new Dictionary<int, IGraphData>();
        INode? _sourceNode;
        IGraphData? _errorSignal;
        IGraphData _data;

        public TrainingEngineContext(IGraphExecutionContext executionContext, IMiniBatchSequence miniBatch, ILearningContext? learningContext)
        {
            BatchSequence = miniBatch;
            _executionContext = executionContext;
            _learningContext = learningContext;
            _data = new NullGraphData();
        }

        public void Dispose()
        {
            foreach (var item in _forward)
                item.Backpropagation?.Dispose();
            _forward.Clear();

            ClearBackward();
            _nodeErrorSignal.Clear();

            foreach (var item in _history) {
                foreach (var item2 in item.Value) {
                    //item2.Segment.Dispose();
                    item2.Backpropagation?.Dispose();
                }
            }
            _history.Clear();
        }

        public bool IsTraining => _learningContext != null;
        public ILinearAlgebraProvider LinearAlgebraProvider => _executionContext.LinearAlgebraProvider;
        public IGraphExecutionContext ExecutionContext => _executionContext;
        public ILearningContext? LearningContext => _learningContext;
        public IMiniBatchSequence BatchSequence { get; }
        public bool HasNext => _forward.Any();
        public INode? Source => _sourceNode;
        public IGraphData? ErrorSignal => _errorSignal;
        public IGraphData Data => _data;
        protected override IGraphSequenceContext Context => this;

        public void AddForward(ExecutionHistory action, Func<IBackpropagate>? callback)
        {
            if (callback != null && IsTraining)
                action.Backpropagation = callback();
            _forward.Add(action);

            if (!_history.TryGetValue(action.Source, out var temp))
                _history.Add(action.Source, temp = new List<ExecutionHistory>());
            temp.Add(action);
        }

        public void AddBackward(IGraphData error, INode target, INode source)
        {
            _backward.Push((error, target, source));
        }

        public bool ExecuteNext()
        {
            if (HasNext) {
                var next = _forward.ElementAt(0);
                _forward.RemoveAt(0);

                _data = next.Data;
                _sourceNode = next.Source;
                foreach (var output in next.Source.Output)
                    output.SendTo.ExecuteForward(this, output.Channel);

                return true;
            }
            return false;
        }

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

	    void ClearBackward()
        {
            _backward.Clear();
        }

        public void Backpropagate(IGraphData? delta)
        {
            // initialise backpropagation stack
            ClearBackward();
            AddBackward(delta, _sourceNode ?? throw new Exception("No backpropagation target"), null);

            // backpropagate the error through the graph
            _errorSignal = null;
            while (_backward.Any()) {
                var next = _backward.Pop();
                _errorSignal = GetErrorSignal(next.ErrorSignal, next.Target);

                if (_history.TryGetValue(next.Target, out var history)) {
                    foreach (var item in history) {
                        if (item.Backpropagation != null) {
                             item.Backpropagation.Backward(next.Source, _errorSignal, this, item.Parents);
                            item.Backpropagation.Dispose();
                        } else {
                            foreach (var parent in item.Parents)
                                AddBackward(_errorSignal, parent, next.Target);
                        }
                    }
                }
            }
        }

        IGraphData? GetErrorSignal(IGraphData? errorSignal, INode node)
        {
            var list = new List<IGraphData>();

            if (errorSignal != null)
                list.Add(errorSignal);
            if (_nodeErrorSignal.TryGetValue(node, out var temp)) {
                foreach (var item in temp) {
                    if (item != null)
                        list.Add(item);
                }
                _nodeErrorSignal.Remove(node);
            }

            if (list.Count == 1)
                return list.First();
            if(list.Count > 1) {
                var first = list.First().GetMatrix();
                foreach (var item in list.Skip(1)) {
                    var next = item.GetMatrix();
                    if (next.RowCount == first.RowCount && next.ColumnCount == first.ColumnCount)
                        first.AddInPlace(next);
                }
                return errorSignal?.ReplaceWith(first);
            }
            return null;
        }

        public void AppendErrorSignal(IGraphData errorSignal, INode forNode)
        {
            if (!_nodeErrorSignal.TryGetValue(forNode, out var temp))
                _nodeErrorSignal.Add(forNode, temp = new List<IGraphData>());
            temp.Add(errorSignal);
        }
    }
}
