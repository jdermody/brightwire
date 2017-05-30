using BrightWire.Models;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Helper
{
    public class ExecutionResult
    {
        readonly IMiniBatchSequence _miniBatch;
        readonly IReadOnlyList<FloatVector> _output;
        readonly IReadOnlyList<FloatVector> _target;
        readonly IReadOnlyList<FloatVector> _input;

        public ExecutionResult(IMiniBatchSequence miniBatch, IReadOnlyList<FloatVector> output)
        {
            _miniBatch = miniBatch;
            _output = output;
            _target = _miniBatch.Target?.GetMatrix().Data.Row;
            _input = _miniBatch.Input.GetMatrix().Data.Row;
        }

        public IReadOnlyList<FloatVector> Output => _output;
        public IReadOnlyList<FloatVector> Target => _target;
        public IReadOnlyList<FloatVector> Input => _input;
        public IMiniBatchSequence MiniBatchSequence => _miniBatch;

        public float CalculateError(IErrorMetric errorMetric) => Output.Zip(Target, (o, t) => errorMetric.Compute(o, t)).Average();
    }
}
