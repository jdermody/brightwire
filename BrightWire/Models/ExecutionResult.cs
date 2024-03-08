using System.Linq;
using BrightData;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.Models
{
	/// <summary>
	/// The output from a mini batch
	/// </summary>
	public class ExecutionResult
	{
		readonly MiniBatch.Sequence _miniBatch;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="miniBatch">The mini batch sequence</param>
        /// <param name="output">The mini batch output</param>
        /// <param name="wantInputInExecutionResults">True to save graph inputs in the execution results</param>
        public ExecutionResult(MiniBatch.Sequence miniBatch, IMatrix<float> output, bool wantInputInExecutionResults)
        {
            _miniBatch = miniBatch;

            Output = output.AllRowsAsReadOnly(true);
            Target = _miniBatch.Target?.GetMatrix().AllRowsAsReadOnly(true);
			if(wantInputInExecutionResults)
                Input = _miniBatch.Input?.GetMatrix().AllRowsAsReadOnly(true);
        }

        /// <summary>
		/// The list of output rows
		/// </summary>
		public IReadOnlyVector<float>[] Output { get; }

		/// <summary>
		/// The list of target rows
		/// </summary>
		public IReadOnlyVector<float>[]? Target { get; }

		/// <summary>
		/// The list of input rows
		/// </summary>
		public IReadOnlyVector<float>[]? Input { get; }

        /// <summary>
        /// Optional list of errors
        /// </summary>
        public IReadOnlyVector<float>[]? Error { get; set; } = null;

		/// <summary>
		/// The mini batch
		/// </summary>
		public MiniBatch.Sequence MiniBatchSequence => _miniBatch;

		/// <summary>
		/// Calculates the error of the output against the target
		/// </summary>
		/// <param name="errorMetric">The error metric to calculate with</param>
		/// <returns></returns>
		public float CalculateError(IErrorMetric errorMetric) => Output.Zip(Target!, errorMetric.Compute).Average();
    }
}
