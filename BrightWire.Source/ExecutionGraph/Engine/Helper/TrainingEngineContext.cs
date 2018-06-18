using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Training engine context
    /// </summary>
    class TrainingEngineContext : IContext
    {
        readonly IExecutionContext _executionContext;
        readonly IMiniBatchSequence _miniBatch;
        readonly ILearningContext _learningContext;
        readonly List<IExecutionHistory> _forward = new List<IExecutionHistory>();
        readonly Stack<(IGraphData ErrorSignal, INode Target, INode Source)> _backward = new Stack<(IGraphData, INode, INode)>();
        readonly Dictionary<INode, List<IExecutionHistory>> _history = new Dictionary<INode, List<IExecutionHistory>>();
        readonly Dictionary<INode, List<IGraphData>> _nodeErrorSignal = new Dictionary<INode, List<IGraphData>>();
        INode _sourceNode;
        IGraphData _errorSignal = null, _data;
        double? _trainingError;

        public TrainingEngineContext(IExecutionContext executionContext, IMiniBatchSequence miniBatch, ILearningContext learningContext)
        {
            _miniBatch = miniBatch;
            _executionContext = executionContext;
            _learningContext = learningContext;
            _data = null;
        }
        public TrainingEngineContext(IExecutionContext executionContext, IGraphData data, ILearningContext learningContext)
        {
            _miniBatch = null;
            _executionContext = executionContext;
            _learningContext = learningContext;
            _data = data;
        }

        public void Dispose()
        {
            foreach (var item in _forward)
                item.Backpropagation?.Dispose();
            _forward.Clear();

            _ClearBackward();
            _nodeErrorSignal.Clear();

            foreach (var item in _history) {
                foreach (var item2 in item.Value) {
                    //item2.Data.Dispose();
                    item2.Backpropagation?.Dispose();
                }
            }
            _history.Clear();
        }

        public bool IsTraining => _learningContext != null;
        public ILinearAlgebraProvider LinearAlgebraProvider => _executionContext.LinearAlgebraProvider;
        public IExecutionContext ExecutionContext => _executionContext;
        public ILearningContext LearningContext => _learningContext;
        public IMiniBatchSequence BatchSequence => _miniBatch;
        public bool HasNext => _forward.Any();
        public double? TrainingError => _trainingError;
        public INode Source => _sourceNode;
        public IGraphData ErrorSignal => _errorSignal;
        public IGraphData Data => _data;

        public void AddForward(IExecutionHistory action, Func<IBackpropagation> callback)
        {
            if (callback != null && IsTraining)
                action.Backpropagation = callback();
            _forward.Add(action);

            if (!_history.TryGetValue(action.Source, out List<IExecutionHistory> temp))
                _history.Add(action.Source, temp = new List<IExecutionHistory>());
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
                if (next.Source.Output != null) {
                    foreach (var output in next.Source.Output)
                        output.SendTo?.ExecuteForward(this, output.Channel);
                }

                return true;
            }
            return false;
        }

        void _ClearBackward()
        {
            _backward.Clear();
        }

        public void Backpropagate(IGraphData delta)
        {
            // calculate training error
            if (delta != null) {
                if (_learningContext?.TrainingErrorCalculation == TrainingErrorCalculation.Fast)
                    _trainingError = Math.Sqrt(delta.GetMatrix().AsIndexable().Values.Select(v => Math.Pow(v, 2)).Average());
            }

            // initialise backpropagation stack
            _ClearBackward();
            AddBackward(delta, _sourceNode, null);

            // backpropagate the error through the graph
            _errorSignal = null;
            while (_backward.Any()) {
                var next = _backward.Pop();
                _errorSignal = _GetErrorSignal(next.ErrorSignal, next.Target);

                if (next.Target != null && _history.TryGetValue(next.Target, out List<IExecutionHistory> history)) {
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

        IGraphData _GetErrorSignal(IGraphData errorSignal, INode node)
        {
            var list = new List<IGraphData>();

            if (errorSignal != null)
                list.Add(errorSignal);
            if (_nodeErrorSignal.TryGetValue(node, out List<IGraphData> temp)) {
                foreach (var item in temp) {
                    if (item != null)
                        list.Add(item);
                }
                _nodeErrorSignal.Remove(node);
            }

            if (list.Count == 1)
                return list.First();
            else if(list.Count > 1) {
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
            if (!_nodeErrorSignal.TryGetValue(forNode, out List<IGraphData> temp))
                _nodeErrorSignal.Add(forNode, temp = new List<IGraphData>());
            temp.Add(errorSignal);
        }
    }
}
