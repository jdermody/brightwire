using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph
{
    class BatchContext : IBatchContext
    {
        readonly ILearningContext _context;
        readonly IExecutionContext _executionContext;
        readonly Dictionary<int, Stack<IBackpropagation>> _backProp;
        readonly List<double> _miniBatchError;
        IMatrix _target, _output;
        int _batchSize;

        public BatchContext(IExecutionContext executionContext, int batchSize, IMatrix target = null)
        {
            _executionContext = executionContext;
            _batchSize = batchSize;
            _target = target;
            IsTraining = false;
        }

        public BatchContext(IExecutionContext executionContext, ILearningContext context, int batchSize, IMatrix target)
        {
            _executionContext = executionContext;
            _context = context;
            _batchSize = batchSize;
            _target = target;
            _backProp = new Dictionary<int, Stack<IBackpropagation>>();
            _miniBatchError = new List<double>();
            IsTraining = true;
        }

        public int BatchSize => _batchSize;
        public double? TrainingError => _miniBatchError?.Any() == true ? (double?)_miniBatchError.Average() : null;
        public void SetOutput(IMatrix output) => _output = output;
        public bool IsTraining { get; private set; }
        public ILinearAlgebraProvider LinearAlgebraProvider => _executionContext.LinearAlgebraProvider;
        public IExecutionContext ExecutionContext => _executionContext;
        public ILearningContext LearningContext => _context;
        public IMatrix Target => _target;
        public IMatrix Output => _output;

        public void AddBackpropagation(IBackpropagation backProp, int channel)
        {
            Stack<IBackpropagation> stack;
            if (!_backProp.TryGetValue(channel, out stack))
                _backProp.Add(channel, stack = new Stack<IBackpropagation>());
            stack.Push(backProp);
        }
        public void CalculateTrainingError(IMatrix delta)
        {
            if (_context.CalculateTrainingError)
                _miniBatchError.Add(delta.AsIndexable().Values.Select(v => Math.Pow(v, 2)).Average() / 2);
        }
        public void Backpropagate(IMatrix delta, int channel)
        {
            if (delta != null) {
                Stack<IBackpropagation> stack;
                if (_backProp.TryGetValue(channel, out stack) && stack.Any()) {
                    var next = stack.Pop();
                    next.Backward(delta, channel, this, stack.Any());
                }
            }
        }
    }
}
