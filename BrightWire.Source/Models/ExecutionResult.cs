using System.Collections.Generic;
using System.Linq;

namespace BrightWire.Models
{
    /// <summary>
    /// The output from a mini batch
    /// </summary>
    public class ExecutionResult
    {
        readonly IMiniBatchSequence _miniBatch;
        readonly IReadOnlyList<FloatVector> _output;
        readonly IReadOnlyList<FloatVector> _target;
        readonly IReadOnlyList<IReadOnlyList<FloatVector>> _input;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="miniBatch">The mini batch sequence</param>
        /// <param name="output">The mini batch output</param>
        public ExecutionResult(IMiniBatchSequence miniBatch, IReadOnlyList<FloatVector> output)
        {
            _miniBatch = miniBatch;
            _output = output;
            _target = _miniBatch.Target?.GetMatrix().Data.Row;
            _input = _miniBatch.Input.Select(input => input.GetMatrix().Data.Row).ToList();
        }

        /// <summary>
        /// The list of output rows
        /// </summary>
        public IReadOnlyList<FloatVector> Output => _output;

        /// <summary>
        /// The list of target rows
        /// </summary>
        public IReadOnlyList<FloatVector> Target => _target;

        /// <summary>
        /// The list of input rows
        /// </summary>
        public IReadOnlyList<IReadOnlyList<FloatVector>> Input => _input;

        /// <summary>
        /// The mini batch
        /// </summary>
        public IMiniBatchSequence MiniBatchSequence => _miniBatch;

        /// <summary>
        /// Calculates the error of the output against the target
        /// </summary>
        /// <param name="errorMetric">The error metric to calculate with</param>
        /// <returns></returns>
        public float CalculateError(IErrorMetric errorMetric) => Output.Zip(Target, (o, t) => errorMetric.Compute(o, t)).Average();
    }
}
