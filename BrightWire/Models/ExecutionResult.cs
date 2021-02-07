using System.Linq;
using BrightData.LinearAlgebra;

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
		public ExecutionResult(IMiniBatchSequence miniBatch, Vector<float>[] output, uint index)
        {
            _miniBatch = miniBatch;
            Index = index;
            Output = output;
			Target = _miniBatch.Target?.GetMatrix().Data.Rows.ToArray();
			Input = _miniBatch.Input?.GetMatrix().Data.Rows.ToArray();
		}

		/// <summary>
		/// Output index
		/// </summary>
		public uint Index { get; }

		/// <summary>
		/// The list of output rows
		/// </summary>
		public Vector<float>[] Output { get; }

		/// <summary>
		/// The list of target rows
		/// </summary>
		public Vector<float>[]? Target { get; }

		/// <summary>
		/// The list of input rows
		/// </summary>
		public Vector<float>[]? Input { get; }

		/// <summary>
		/// The mini batch
		/// </summary>
		public IMiniBatchSequence MiniBatchSequence => _miniBatch;

		/// <summary>
		/// Calculates the error of the output against the target
		/// </summary>
		/// <param name="errorMetric">The error metric to calculate with</param>
		/// <returns></returns>
		public float CalculateError(IErrorMetric errorMetric) => Output.Zip(Target!, errorMetric.Compute).Average();
	}
}
