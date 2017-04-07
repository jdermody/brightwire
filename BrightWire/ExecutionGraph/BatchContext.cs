using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph
{
    class BatchContext : IBatchContext
    {
        public class SequenceData : ISequenceResult
        {
            public int Channel { get; set; }
            public IMatrix Delta { get; set; }
            public IMatrix Output { get; set; }
            public IMatrix Target { get; set; }
            public double TrainingError => Delta != null ? Math.Sqrt(Delta.AsIndexable().Values.Select(v => Math.Pow(v, 2)).Average()) : 0;
        }

        readonly ILearningContext _context;
        readonly IExecutionContext _executionContext;
        readonly IMiniBatch _batch;
        readonly Dictionary<int, Stack<IBackpropagation>> _backProp;
        readonly Dictionary<IMiniBatchSequence, SequenceData> _output = new Dictionary<IMiniBatchSequence, SequenceData>();

        public BatchContext(IExecutionContext executionContext, IMiniBatch batch)
        {
            _executionContext = executionContext;
            _batch = batch;
            IsTraining = false;
        }

        public BatchContext(IExecutionContext executionContext, ILearningContext context, IMiniBatch batch) :
            this(executionContext, batch)
        {
            _context = context;
            _backProp = new Dictionary<int, Stack<IBackpropagation>>();
            IsTraining = true;
        }

        public IMiniBatch Batch => _batch;
        public bool IsTraining { get; private set; }
        public ILinearAlgebraProvider LinearAlgebraProvider => _executionContext.LinearAlgebraProvider;
        public IExecutionContext ExecutionContext => _executionContext;
        public ILearningContext LearningContext => _context;
        public double TrainingError => _output.Any() ? _output.Select(kv => kv.Value.TrainingError).Average() : 0;
        public IEnumerable<ISequenceResult> Results => _output.Select(kv => kv.Value);

        public void SetOutput(IMatrix output, IMatrix target, IMatrix delta, int channel)
        {
            _output[_batch.CurrentSequence] = new SequenceData {
                Channel = channel,
                Delta = delta,
                Output = output,
                Target = target
            };
            Backpropagate(delta, channel);
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

        public void RegisterBackpropagation(IBackpropagation backProp, int channel)
        {
            Stack<IBackpropagation> stack;
            if (!_backProp.TryGetValue(channel, out stack))
                _backProp.Add(channel, stack = new Stack<IBackpropagation>());
            stack.Push(backProp);
        }
    }
}
