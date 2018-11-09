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

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="miniBatch">The mini batch sequence</param>
		/// <param name="output">The mini batch output</param>
		/// <param name="index">Output index</param>
		public ExecutionResult(IMiniBatchSequence miniBatch, IReadOnlyList<FloatVector> output, int index)
		{
			Index = index;
			_miniBatch = miniBatch;
			Output = output;
			Target = _miniBatch.Target?.GetMatrix().Data.Row;
			Input = _miniBatch.Input.Select(input => input.GetMatrix().Data.Row).ToList();
		}

		/// <summary>
		/// Output index
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// The list of output rows
		/// </summary>
		public IReadOnlyList<FloatVector> Output { get; }

		/// <summary>
		/// The list of target rows
		/// </summary>
		public IReadOnlyList<FloatVector> Target { get; }

		/// <summary>
		/// The list of input rows
		/// </summary>
		public IReadOnlyList<IReadOnlyList<FloatVector>> Input { get; }

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
