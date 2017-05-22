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

        public ExecutionResult(IMiniBatchSequence miniBatch, IReadOnlyList<FloatVector> output)
        {
            _miniBatch = miniBatch;
            _output = output;
        }

        public IReadOnlyList<FloatVector> Output => _output;
        public IReadOnlyList<FloatVector> Target => _miniBatch.Target?.Data.Row;
        public IReadOnlyList<FloatVector> Input => _miniBatch.Input.Data.Row;
        public IMiniBatchSequence MiniBatchSequence => _miniBatch;

        public float CalculateError(IErrorMetric errorMetric) => Output.Zip(Target, (o, t) => errorMetric.Compute(o, t)).Average();
    }
}
