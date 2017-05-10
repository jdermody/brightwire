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
        readonly IReadOnlyList<IIndexableVector> _output;

        public ExecutionResult(IMiniBatchSequence miniBatch, IReadOnlyList<IIndexableVector> output)
        {
            _miniBatch = miniBatch;
            _output = output;
        }

        public IReadOnlyList<IIndexableVector> Output => _output;
        public IReadOnlyList<IIndexableVector> Target => _miniBatch.Target?.AsIndexable().Rows.ToList();
        public IReadOnlyList<IIndexableVector> Input => _miniBatch.Input.AsIndexable().Rows.ToList();

        public float CalculateError(IErrorMetric errorMetric) => Output.Zip(Target, (o, t) => errorMetric.Compute(o, t)).Average();
    }
}
