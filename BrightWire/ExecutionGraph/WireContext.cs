using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph
{
    class WireContext : IWireContext
    {
        readonly ILearningContext _context;
        readonly Stack<IBackpropagation> _backProp;
        readonly IMatrix _target;
        readonly List<double> _miniBatchError = new List<double>();
        readonly List<IMatrix> _output = new List<IMatrix>();

        public WireContext(ILearningContext context, IMatrix target, bool shouldBackpropagate)
        {
            _context = context;
            _target = target;
            _backProp = shouldBackpropagate ? new Stack<IBackpropagation>() : null;
        }

        public ILearningContext Context { get { return _context; } }
        public Stack<IBackpropagation> Backpropagation { get { return _backProp; } }
        public IMatrix Target { get { return _target; } }
        public void AddOutput(IMatrix output)
        {
            _output.Add(output);
        }
        public void AddBackpropagation(IBackpropagation backProp)
        {
            _backProp?.Push(backProp);
        }
        public void Backpropagate(IMatrix delta)
        {
            if (_context.CalculateTrainingError)
                _miniBatchError.Add(delta.AsIndexable().Values.Select(v => Math.Pow(v, 2)).Average() / 2);

            if (_backProp != null) {
                while (_backProp.Any() == true) {
                    var next = _backProp.Pop();
                    delta = next.Backward(delta, _context, _backProp.Any());
                }
            }
        }
        public double TrainingError { get { return _miniBatchError.Any() ? _miniBatchError.Average() : double.MaxValue; } }
        public IReadOnlyList<IMatrix> Output { get { return _output; } }
    }
}
