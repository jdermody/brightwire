using BrightWire.ExecutionGraph.Engine;
using BrightWire.ExecutionGraph.Node.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Helper
{
    class TrainingEngineContext : IContext
    {
        readonly FlowThrough _input;
        readonly IExecutionContext _executionContext;
        readonly IMiniBatchSequence _miniBatch;
        readonly ILearningContext _learningContext;
        readonly List<IExecutionHistory> _forward = new List<IExecutionHistory>();
        readonly Stack<(IGraphData, INode)> _backward = new Stack<(IGraphData, INode)>();
        readonly Dictionary<INode, List<IExecutionHistory>> _history = new Dictionary<INode, List<IExecutionHistory>>();
        INode _sourceNode;
        IGraphData _errorSignal = null;
        double _trainingError = 0;
        IMatrix _output = null;

        public TrainingEngineContext(IExecutionContext executionContext, IMiniBatchSequence miniBatch, ILearningContext learningContext, FlowThrough input)
        {
            _input = input;
            _miniBatch = miniBatch;
            _executionContext = executionContext;
            _learningContext = learningContext;
            _executionContext.Data = miniBatch.Input.ToGraphData();
        }
        public TrainingEngineContext(IExecutionContext executionContext, IGraphData data, ILearningContext learningContext, FlowThrough input)
        {
            _input = input;
            _miniBatch = null;
            _executionContext = executionContext;
            _learningContext = learningContext;
            _executionContext.Data = data;
        }

        public bool IsTraining => _learningContext != null;
        public ILinearAlgebraProvider LinearAlgebraProvider => _executionContext.LinearAlgebraProvider;
        public IExecutionContext ExecutionContext => _executionContext;
        public ILearningContext LearningContext => _learningContext;
        public IMiniBatchSequence BatchSequence => _miniBatch;
        public bool HasNext => _forward.Any();
        public double TrainingError => _trainingError;
        public INode Source => _sourceNode;
        public IGraphData ErrorSignal => _errorSignal;
        public IGraphData Data => _executionContext.Data;
        public IMatrix Output { get => _output; set => _output = value; }

        public void AddForward(IExecutionHistory action, Func<IBackpropagation> callback)
        {
            if (callback != null && IsTraining)
                action.Backpropagation = callback();
            _forward.Add(action);

            List<IExecutionHistory> temp;
            if (!_history.TryGetValue(action.Source, out temp))
                _history.Add(action.Source, temp = new List<IExecutionHistory>());
            temp.Add(action);
        }

        public void AddBackward(IGraphData error, INode source)
        {
            _backward.Push((error, source));
        }

        public bool ExecuteNext()
        {
            if (HasNext) {
                var next = _forward.ElementAt(0);
                _forward.RemoveAt(0);

                _executionContext.Data = next.Data;
                _sourceNode = next.Source;
                if (next.Source.Output != null) {
                    foreach (var output in next.Source.Output)
                        output.SendTo?.ExecuteForward(this, output.Channel);
                }

                return true;
            }
            return false;
        }

        public IGraphData Backpropagate(IGraphData delta)
        {
            // calculate training error
            if (_learningContext?.CalculateTrainingError == true)
                _trainingError = Math.Sqrt(delta.Decompose().Average(m => m.AsIndexable().Values.Select(v => Math.Pow(v, 2)).Average()));

            // initialise backpropagation stack
            _backward.Clear();
            _backward.Push((delta, _sourceNode));

            // backpropagate the error through the graph
            List<IExecutionHistory> history;
            _errorSignal = null;
            while (_backward.Any()) {
                var next = _backward.Pop();
                _errorSignal = next.Item1;
                if (next.Item2 != null && _history.TryGetValue(next.Item2, out history)) {
                    foreach (var item in history) {
                        if (item.Backpropagation != null)
                            item.Backpropagation.Backward(_errorSignal, this, item.Parents);
                        else {
                            foreach (var parent in item.Parents)
                                _backward.Push((_errorSignal, parent));
                        }
                    }
                }
            }
            return _input.ErrorSignal.ToGraphData();
        }
    }
}
